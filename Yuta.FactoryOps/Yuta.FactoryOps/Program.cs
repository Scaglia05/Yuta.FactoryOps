using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Syncfusion.Blazor;
using System.Text;
using Yuta.FactoryOps.Server.Components;
using Yuta.FactoryOps.Server.DbContextBuild;
using Yuta.FactoryOps.Server.Repositories;
using Yuta.FactoryOps.Server.Repositories.Interface;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAÇÕES COMPONENTES BLAZOR PADRÃO ---
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSyncfusionBlazor();

// Registra o HttpClient para o motor de renderização do servidor
// Altere de http://localhost:5000 para a porta real do seu console:
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["BackendUrl"] ?? "https://localhost:7183")
});

// Como deve ficar:
builder.Services.AddCascadingAuthenticationState();

// Registra o provedor de estado de autenticação padrão do ASP.NET Core para servidores
builder.Services.AddScoped<AuthenticationStateProvider, Microsoft.AspNetCore.Components.Server.ServerAuthenticationStateProvider>();

// --- 2. INJEÇÕES DE INFRAESTRUTURA DA YUTA ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<FactoryDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<Yuta.FactoryOps.Client.Security.ProvedorAutenticacaoJwt>();
builder.Services.AddControllers();

// --- 3. CONFIGURAÇÃO DE SEGURANÇA (JWT) ---
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
app.UseBlazorFrameworkFiles(); // 1º: Mapeia os arquivos internos do Blazor Client (.wasm, etc)
app.UseStaticFiles();          // 2º: Serve os arquivos físicos da wwwroot (CSS, JS, Imagens)

app.UseAntiforgery();

// --- 5. MIDDLEWARES DE AUTENTICAÇÃO ---
app.UseAuthentication();
app.UseAuthorization();

// --- 6. MAPEAMENTO DAS ROTAS DE CONTROLLERS (API) ---
app.MapControllers();

// --- 7. MAPEAMENTO DAS PÁGINAS BLAZOR INTERATIVAS ---
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Yuta.FactoryOps.Client._Imports).Assembly);

app.Run();