using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Syncfusion.Blazor;
using System.Text;
using Yuta.FactoryOps.Server.DbContextBuild;
using Yuta.FactoryOps.Server.Repositories;
using Yuta.FactoryOps.Server.Repositories.Interface;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAÇÕES COMPONENTES BLAZOR PADRÃO (CORRIGIDO) ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()       //  ADICIONADO: Suporte para o modo Server
    .AddInteractiveWebAssemblyComponents();   //  Suporte para o modo WebAssembly

builder.Services.AddSyncfusionBlazor();

// Configuração flexível de HttpClient para rodar tanto local quanto no Render
builder.Services.AddScoped(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    // Se estiver no Render, usa a porta padrão do container (http), caso contrário usa a URL configurada
    var baseUrl = config["BackendUrl"] ?? (builder.Environment.IsDevelopment() ? "https://localhost:7183" : "http://localhost:8080");
    return new HttpClient { BaseAddress = new Uri(baseUrl) };
});

builder.Services.AddCascadingAuthenticationState();

// --- 2. CONFIGURAÇÃO DO BANCO DE DADOS ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FactoryDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(5);
    }));

// --- 3. REPOSITÓRIOS E AUTENTICAÇÃO ---
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<AuthenticationStateProvider, Yuta.FactoryOps.Client.Security.ProvedorAutenticacaoJwt>();
builder.Services.AddScoped<Yuta.FactoryOps.Client.Security.ProvedorAutenticacaoJwt>(sp =>
    (Yuta.FactoryOps.Client.Security.ProvedorAutenticacaoJwt)sp.GetRequiredService<AuthenticationStateProvider>());

builder.Services.AddControllers();

var jwtKey = builder.Configuration["Jwt:ChaveSecreta"] ?? "SuaChaveSuperSecretaComMaisDe32CaracteresYutaOps";
var keyBytes = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
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

var app = builder.Build();

// --- 4. CONFIGURAÇÃO DO PIPELINE DE REQUISIÇÕES (MIDDLEWARES) ---
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

app.Run();