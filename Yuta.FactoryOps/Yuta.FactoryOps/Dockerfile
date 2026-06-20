# Estágio de Compilação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copia a estrutura completa primeiro para garantir que o Linux enxergue todos os .csproj
COPY . .

# 2. Executa o restore caçando os projetos automaticamente a partir da Solução (.slnx ou .sln)
RUN dotnet restore

# 3. Entra na pasta onde está o projeto do Servidor para compilar e publicar
# NOTA: O Docker busca a partir da pasta /src criada acima.
WORKDIR "/src"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Estágio Final de Execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# O Render vai varrer a pasta de publicação para achar a DLL do servidor
ENTRYPOINT ["dotnet", "Yuta.FactoryOps.Server.dll"]