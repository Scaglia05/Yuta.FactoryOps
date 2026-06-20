# Estágio de Compilação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copia os arquivos .csproj respeitando EXATAMENTE a estrutura do seu disco
COPY ["Yuta.FactoryOps/Yuta.FactoryOps.Server.csproj", "Yuta.FactoryOps/"]
COPY ["Yuta.FactoryOps.Application/Yuta.FactoryOps.Application.csproj", "Yuta.FactoryOps.Application/"]
COPY ["Yuta.FactoryOps.Domain/Yuta.FactoryOps.Domain.csproj", "Yuta.FactoryOps.Domain/"]
COPY ["Yuta.FactoryOps/Yuta.FactoryOps.Client/Yuta.FactoryOps.Client.csproj", "Yuta.FactoryOps/Yuta.FactoryOps.Client/"]

# 2. Restaura as dependências do projeto principal
RUN dotnet restore "Yuta.FactoryOps/Yuta.FactoryOps.Server.csproj"

# 3. Copia todo o restante dos arquivos fontes
COPY . .

# 4. Entra na pasta correta do servidor para compilar
WORKDIR "/src/Yuta.FactoryOps"
RUN dotnet build "Yuta.FactoryOps.Server.csproj" -c Release -o /app/build

# 5. Publica a aplicação
FROM build AS publish
RUN dotnet publish "Yuta.FactoryOps.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio Final de Execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Yuta.FactoryOps.Server.dll"]