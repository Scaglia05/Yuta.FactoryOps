using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Yuta.FactoryOps.Client.Security;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// REGISTRO DOS SERVIÇOS
builder.Services.AddAuthorizationCore();

// 1. Registra o provedor associado ao tipo base do framework
builder.Services.AddScoped<AuthenticationStateProvider, ProvedorAutenticacaoJwt>();

// 2. CORREÇÃO DA CONVERSÃO: Registra o atalho injetando o próprio provedor resolvido de forma segura
builder.Services.AddScoped<ProvedorAutenticacaoJwt>(provider =>
    (ProvedorAutenticacaoJwt)provider.GetRequiredService<AuthenticationStateProvider>());
// Dentro do Program.cs do seu projeto CLIENT:
builder.Services.AddScoped<Yuta.FactoryOps.Client.Security.ProvedorAutenticacaoJwt>();


await builder.Build().RunAsync();