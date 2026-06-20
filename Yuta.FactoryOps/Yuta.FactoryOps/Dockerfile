# Estágio de Compilação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copia todos os arquivos do repositório para dentro do container Linux
COPY . .

# 2. Executa o restore caçando o .csproj no caminho de tripla pasta revelado pelo PowerShell
RUN dotnet restore "Yuta.FactoryOps/Yuta.FactoryOps/Yuta.FactoryOps/Yuta.FactoryOps.Server.csproj"

# 3. Compila focando no caminho exato
RUN dotnet build "Yuta.FactoryOps/Yuta.FactoryOps/Yuta.FactoryOps/Yuta.FactoryOps.Server.csproj" -c Release -o /app/build

# 4. Publica a aplicação
FROM build AS publish
RUN dotnet publish "Yuta.FactoryOps/Yuta.FactoryOps/Yuta.FactoryOps/Yuta.FactoryOps.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio Final de Execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Yuta.FactoryOps.Server.dll"]