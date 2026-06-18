using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Yuta.FactoryOps.Application.Interfaces;
using Yuta.FactoryOps.Application.Services;
using Yuta.FactoryOps.Client.Security;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// REGISTRO DOS SERVIÇOS
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, ProvedorAutenticacaoJwt>();
builder.Services.AddScoped<ProvedorAutenticacaoJwt>(provider => (ProvedorAutenticacaoJwt)provider.GetRequiredService<AuthenticationStateProvider>());
builder.Services.AddScoped<ProvedorAutenticacaoJwt>();
builder.Services.AddScoped<IDashboardService, DashboardMockService>();


await builder.Build().RunAsync();