# Estágio de Compilação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copia toda a estrutura do repositório para dentro do container
COPY . .

# 2. Faz o restore procurando o arquivo .csproj em qualquer subpasta existente
RUN dotnet restore **/Yuta.FactoryOps.Server.csproj

# 3. Compila a aplicação buscando o projeto automaticamente de forma dinâmica
RUN dotnet build **/Yuta.FactoryOps.Server.csproj -c Release -o /app/build

# 4. Publica a aplicação focando no mesmo arquivo encontrado
FROM build AS publish
RUN dotnet publish **/Yuta.FactoryOps.Server.csproj -c Release -o /app/publish /p:UseAppHost=false

# Estágio Final de Execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Yuta.FactoryOps.Server.dll"]