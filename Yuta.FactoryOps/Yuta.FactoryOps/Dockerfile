# Estágio de Compilação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copia toda a estrutura do repositório para o container
COPY . .

# 2. Executa o restore apontando exatamente para o arquivo onde você está rodando os comandos
RUN dotnet restore "Yuta.FactoryOps/Yuta.FactoryOps/Yuta.FactoryOps/Yuta.FactoryOps.Server.csproj"

# 3. Compila a aplicação
RUN dotnet build "Yuta.FactoryOps/Yuta.FactoryOps/Yuta.FactoryOps/Yuta.FactoryOps.Server.csproj" -c Release -o /app/build

# 4. Publica os binários compilados
FROM build AS publish
RUN dotnet publish "Yuta.FactoryOps/Yuta.FactoryOps/Yuta.FactoryOps/Yuta.FactoryOps.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio Final de Execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Yuta.FactoryOps.Server.dll"]