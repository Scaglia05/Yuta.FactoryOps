FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copia o arquivo de solução moderno (.slnx)
COPY Yuta.FactoryOps.slnx ./

# 2. Copia TODAS as pastas das camadas (Agora incluindo a Infrastructure)
COPY Yuta.FactoryOps.Domain/ ./Yuta.FactoryOps.Domain/
COPY Yuta.FactoryOps.Application/ ./Yuta.FactoryOps.Application/
COPY Yuta.FactoryOps.Infrastructure/ ./Yuta.FactoryOps.Infrastructure/
COPY Yuta.FactoryOps.Client/ ./Yuta.FactoryOps.Client/
COPY Yuta.FactoryOps.Server/ ./Yuta.FactoryOps.Server/

# 3. Restaura as dependências do projeto principal
RUN dotnet restore Yuta.FactoryOps.Server/Yuta.FactoryOps.Server.csproj

# 4. Compila o projeto Server
RUN dotnet build Yuta.FactoryOps.Server/Yuta.FactoryOps.Server.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish Yuta.FactoryOps.Server/Yuta.FactoryOps.Server.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Yuta.FactoryOps.Server.dll"]