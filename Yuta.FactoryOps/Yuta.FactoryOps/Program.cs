using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Yuta.FactoryOps.Components;
using Yuta.FactoryOps.Data;
using Yuta.FactoryOps.Repositories;
using Yuta.FactoryOps.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAÇÕES COMPONENTES BLAZOR PADRÃO ---
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// --- 2. INJEÇÕES DE INFRAESTRUTURA DA YUTA ---
// Configura a conexão com o banco PostgreSQL (Supabase) buscando a string do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FactoryDbContext>(options =>
    options.UseNpgsql(connectionString));

// Injeta o Repositório de Autenticação na arquitetura do sistema
builder.Services.AddScoped<IAuthRepository, AuthRepository>();

// Adiciona o suporte para os Controllers de API (Essencial para receber as requisições de login)
builder.Services.AddControllers();

// --- 3. CONFIGURAÇÃO DE SEGURANÇA (JWT) ---
// Recupera a chave secreta de criptografia para assinar os tokens
var jwtKey = builder.Configuration["Jwt:ChaveSecreta"] ?? "SuaChaveSuperSecretaComMaisDe32CaracteresYutaOps";
var keyBytes = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Defina como true quando subir para produção com SSL
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

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Gerenciamento de segurança contra ataques de falsificação de requisições
app.UseAntiforgery();

app.MapStaticAssets();

// --- 5. MIDDLEWARES DE AUTENTICAÇÃO ---
// ATENÇÃO: Eles precisam vir OBRIGATORIAMENTE antes do MapControllers e das rotas Blazor
app.UseAuthentication();
app.UseAuthorization();

// --- 6. MAPEAMENTO DAS ROTAS DE CONTROLLERS (API) ---
app.MapControllers();

// --- 7. MAPEAMENTO DAS PÁGINAS BLAZOR INTERATIVAS ---
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Yuta.FactoryOps.Client._Imports).Assembly);

app.Run();