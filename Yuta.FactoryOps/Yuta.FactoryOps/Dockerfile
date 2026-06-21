FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia absolutamente tudo para dentro do container primeiro
COPY . .

# Encontra e restaura a solução (.slnx ou .sln) automaticamente
RUN dotnet restore $(find . -name "*.slnx" -o -name "*.sln" | head -n 1)

# Compila o projeto Server encontrando o arquivo .csproj dinamicamente
RUN dotnet build $(find . -name "Yuta.FactoryOps.Server.csproj") -c Release -o /app/build

FROM build AS publish
RUN dotnet publish $(find . -name "Yuta.FactoryOps.Server.csproj") -c Release -o /app/publish /p:UseAppHost=false

# Imagem final de execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Yuta.FactoryOps.Server.dll"]