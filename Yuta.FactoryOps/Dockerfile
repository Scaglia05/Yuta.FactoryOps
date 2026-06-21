FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copia todos os 4 projetos mantendo a estrutura correta
COPY . .

# 2. Limpa resíduos de compilação locais para evitar conflitos no Docker
RUN dotnet clean $(find . -name "*.slnx" -o -name "*.sln" | head -n 1)

# 3. Restaura as dependências da Solução inteira (as 4 camadas)
RUN dotnet restore $(find . -name "*.slnx" -o -name "*.sln" | head -n 1)

# 4. Compila o Server (que por consequência compila as outras 3 camadas)
RUN dotnet build $(find . -name "Yuta.FactoryOps.Server.csproj") -c Release -o /app/build

FROM build AS publish
RUN dotnet publish $(find . -name "Yuta.FactoryOps.Server.csproj") -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Yuta.FactoryOps.Server.dll"]