# Estágio de Compilação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copia toda a estrutura do repositório (Domain, Application, Server, Client)
COPY . .

# 2. Restaura as dependências usando o caminho correto a partir da raiz real
RUN dotnet restore "Yuta.FactoryOps/Yuta.FactoryOps/Yuta.FactoryOps.Server.csproj"

# 3. Compila o projeto do servidor
RUN dotnet build "Yuta.FactoryOps/Yuta.FactoryOps/Yuta.FactoryOps.Server.csproj" -c Release -o /app/build

# 4. Publica os binários
FROM build AS publish
RUN dotnet publish "Yuta.FactoryOps/Yuta.FactoryOps/Yuta.FactoryOps.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio Final de Execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Yuta.FactoryOps.Server.dll"]