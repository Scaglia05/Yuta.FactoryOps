using Microsoft.EntityFrameworkCore;
using Yuta.FactoryOps.Components;
using Yuta.FactoryOps.Data;
using Yuta.FactoryOps.Repositories;
using Yuta.FactoryOps.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAÇÕES COMPONENTES BLAZOR PADRÃO ---
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// --- 2. ADICIONADO: INJEÇÕES DE INFRAESTRUTURA DA YUTA ---
// Configura a conexão com o banco PostgreSQL (Supabase) buscando a string do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FactoryDbContext>(options =>
    options.UseNpgsql(connectionString));

// Injeta o Repositório de Autenticação na arquitetura do sistema
builder.Services.AddScoped<IAuthRepository, AuthRepository>();

// Adiciona o suporte para os Controllers de API (Essencial para receber as requisições de login)
builder.Services.AddControllers();


var app = builder.Build();

// --- 3. CONFIGURAÇÃO DO PIPELINE DE REQUISIÇÕES (MIDDLEWARES) ---
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

// Gerenciamento de segurança contra ataques de falsificação de requisições
app.UseAntiforgery();

app.MapStaticAssets();

// --- 4. ADICIONADO: MAPEAMENTO DAS ROTAS DE CONTROLLERS (API) ---
// Isso garante que o Blazor consiga achar o endpoint "/api/auth/login-email", por exemplo.
app.MapControllers();

// --- 5. MAPEAMENTO DAS PÁGINAS BLAZOR INTERATIVAS ---
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Yuta.FactoryOps.Client._Imports).Assembly);

app.Run();