# Estágio de Compilação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copia a estrutura completa para o container
COPY . .

# 2. Restaura as dependências baseadas na solução global
RUN dotnet restore

# 3. Compila a partir da raiz usando a solução para manter as referências de projetos intactas
RUN dotnet build -c Release -o /app/build

# 4. Publica focando explicitamente no projeto do Servidor, que arrasta o Client junto
FROM build AS publish
RUN dotnet publish "Yuta.FactoryOps/Yuta.FactoryOps/Yuta.FactoryOps.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio Final de Execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Yuta.FactoryOps.Server.dll"]