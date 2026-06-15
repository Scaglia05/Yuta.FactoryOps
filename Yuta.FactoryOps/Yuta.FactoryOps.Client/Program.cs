using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using Yuta.FactoryOps.Client.Security;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// REGISTRO DOS SERVIÇOS
builder.Services.AddAuthorizationCore();

// 1. Registra o provedor associado ao tipo base do framework
builder.Services.AddScoped<AuthenticationStateProvider, ProvedorAutenticacaoJwt>();

// 2. CORREÇÃO DA CONVERSÃO: Registra o atalho injetando o próprio provedor resolvido de forma segura
builder.Services.AddScoped<ProvedorAutenticacaoJwt>(provider =>
    (ProvedorAutenticacaoJwt)provider.GetRequiredService<AuthenticationStateProvider>());

await builder.Build().RunAsync();