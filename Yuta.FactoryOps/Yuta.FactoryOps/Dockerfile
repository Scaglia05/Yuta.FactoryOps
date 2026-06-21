FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia a solução inteira mantendo a estrutura de pastas das camadas
COPY . .

# Restaura as dependências caçando o arquivo de solução (.sln)
RUN dotnet restore $(find . -name "*.slnx" -o -name "*.sln" | head -n 1)

# Compila explicitamente o projeto Server (ponto de entrada do Blazor)
RUN dotnet build $(find . -name "Yuta.FactoryOps.Server.csproj") -c Release -o /app/build

FROM build AS publish
RUN dotnet publish $(find . -name "Yuta.FactoryOps.Server.csproj") -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Yuta.FactoryOps.Server.dll"]