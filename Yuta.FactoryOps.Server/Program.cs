using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Syncfusion.Blazor;
using System.Text;
using Yuta.FactoryOps.Infrastructure.Data;
using Yuta.FactoryOps.Infrastructure.Repositories;
using Yuta.FactoryOps.Domain.Interfaces;
using Yuta.FactoryOps.Application.Services;
using Yuta.FactoryOps.Application.Interfaces;
using Yuta.FactoryOps.Domain.Services;
using Yuta.FactoryOps.Server.Middleware;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Iniciando aplicação Yuta.FactoryOps");

    var builder = WebApplication.CreateBuilder(args);

    // Configurar Serilog
    builder.Host.UseSerilog();

// --- 0. PORTA (Render define a variável de ambiente PORT dinamicamente) ---
var renderPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(renderPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{renderPort}");
}

// --- 1. CONFIGURAÇÕES COMPONENTES BLAZOR PADRÃO (CORRIGIDO) ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()       //  ADICIONADO: Suporte para o modo Server
    .AddInteractiveWebAssemblyComponents();   //  Suporte para o modo WebAssembly

builder.Services.AddSyncfusionBlazor();

// Configuração flexível de HttpClient para rodar tanto local quanto no Render.
// Usada pelos componentes InteractiveServer (Login, Dashboard, Empresas, Usuarios) para
// chamar a própria API do backend. Sempre aponta para o próprio processo (loopback),
// nunca para um host fixo, então funciona igual local e em produção.
builder.Services.AddScoped(sp =>
{
    string baseUrl;
    if (!string.IsNullOrWhiteSpace(renderPort))
    {
        baseUrl = $"http://localhost:{renderPort}";
    }
    else
    {
        var config = sp.GetRequiredService<IConfiguration>();
        baseUrl = config["BackendUrl"] ?? (builder.Environment.IsDevelopment() ? "https://localhost:7183" : "http://localhost:8080");
    }
    return new HttpClient { BaseAddress = new Uri(baseUrl) };
});

builder.Services.AddCascadingAuthenticationState();
// Necessário tanto para [Authorize] nos Controllers quanto para o AuthorizeRouteView/[Authorize]
// dos componentes Blazor (InteractiveServer). Sem isso, IAuthorizationService não é resolvido.
builder.Services.AddAuthorization();

// --- 2. CONFIGURAÇÃO DO BANCO DE DADOS ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FactoryDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(5);
    }));

// --- 3. REPOSITÓRIOS E SERVIÇOS ---
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IEmpresaRepository, EmpresaRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<DatabaseSeeder>();

// Dados de monitoramento de ativos (OEE, vibração, temperatura etc.) ainda não têm uma API real —
// por enquanto usamos um mock em memória por sessão. Ver IFactoryOpsDataService para trocar por uma
// implementação real quando a API/gateway de telemetria existir.
builder.Services.AddScoped<Yuta.FactoryOps.Client.FactoryOps.IFactoryOpsDataService, Yuta.FactoryOps.Client.FactoryOps.MockFactoryOpsDataService>();

builder.Services.AddScoped<AuthenticationStateProvider, Yuta.FactoryOps.Client.Security.ProvedorAutenticacaoJwt>();
builder.Services.AddScoped<Yuta.FactoryOps.Client.Security.ProvedorAutenticacaoJwt>(sp =>
    (Yuta.FactoryOps.Client.Security.ProvedorAutenticacaoJwt)sp.GetRequiredService<AuthenticationStateProvider>());

builder.Services.AddControllers();

// --- CHAVE JWT: NUNCA hardcoded. Deve vir de configuração (env var Jwt__ChaveSecreta,
// ou User Secrets em desenvolvimento). Só usamos uma chave de fallback em ambiente de
// desenvolvimento local, para não travar o "dotnet run" de quem ainda não configurou nada.
var jwtKey = builder.Configuration["Jwt:ChaveSecreta"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    if (builder.Environment.IsDevelopment())
    {
        jwtKey = "ChaveDesenvolvimentoLocalApenasNaoUsarEmProducaoYutaOps32c";
        Log.Warning("Jwt:ChaveSecreta não configurada. Usando chave de DESENVOLVIMENTO. Configure a variável de ambiente Jwt__ChaveSecreta antes de ir para produção.");
    }
    else
    {
        throw new InvalidOperationException("Jwt:ChaveSecreta não configurada. Defina a variável de ambiente Jwt__ChaveSecreta antes de iniciar em produção.");
    }

    // Reescreve a configuração em memória com a chave resolvida (ex.: o fallback de dev),
    // para que o TokenService (que lê Jwt:ChaveSecreta de forma independente ao assinar
    // tokens) use exatamente a mesma chave usada aqui para validar — caso contrário os
    // tokens gerados localmente nunca passariam na validação.
    builder.Configuration["Jwt:ChaveSecreta"] = jwtKey;
}
var keyBytes = Encoding.ASCII.GetBytes(jwtKey);

var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

// Login social (Google/Microsoft): só registrado quando credenciais reais existem
// (via env vars Authentication__Google__ClientId/ClientSecret etc). Sem isso, os botões
// continuam ocultos no front-end e essas rotas simplesmente não existem no pipeline.
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.CallbackPath = "/signin-google";
    });
}

var microsoftClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
var microsoftClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
if (!string.IsNullOrWhiteSpace(microsoftClientId) && !string.IsNullOrWhiteSpace(microsoftClientSecret))
{
    authBuilder.AddMicrosoftAccount(options =>
    {
        options.ClientId = microsoftClientId;
        options.ClientSecret = microsoftClientSecret;
        options.CallbackPath = "/signin-microsoft";
    });
}

var app = builder.Build();

// --- 4. CONFIGURAÇÃO DO PIPELINE DE REQUISIÇÕES (MIDDLEWARES) ---
// Necessário atrás do proxy do Render, que termina o TLS e encaminha o protocolo
// original via cabeçalho — sem isso, UseHttpsRedirection/UseHsts podem entrar em loop.
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
// O proxy do Render não está na rede loopback (padrão confiável do ASP.NET Core),
// então precisamos limpar essas listas para que os cabeçalhos X-Forwarded-* sejam aceitos.
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();

// ORDEM CRUCIAL DA ENGRENAGEM BLAZOR:
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// --- 7. MAPEAMENTO DAS PÁGINAS BLAZOR INTERATIVAS ---
app.MapRazorComponents<Yuta.FactoryOps.Client.Pages.App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();

// --- 8. MIGRAÇÃO E SEED DO BANCO DE DADOS ---
// Aplica migrations pendentes automaticamente e popula empresa/usuário padrão.
// Isso é o que garante que o Supabase tenha a estrutura e o usuário admin sem
// precisar rodar `dotnet ef` manualmente.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FactoryDbContext>();
    try
    {
        await db.Database.MigrateAsync();
        Log.Information("Migrações do banco de dados aplicadas com sucesso");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Não foi possível aplicar as migrações do banco de dados. Verifique ConnectionStrings__DefaultConnection. A aplicação vai continuar subindo, mas login/dashboard não vão funcionar até o banco estar acessível.");
    }

    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

Log.Information("Aplicação iniciada com sucesso");
app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação terminou inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}