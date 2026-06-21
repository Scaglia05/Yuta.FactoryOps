FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia a estrutura exatamente como está mapeada na sua máquina
COPY *.sln ./
COPY Yuta.FactoryOps.Domain/ ./Yuta.FactoryOps.Domain/
COPY Yuta.FactoryOps.Application/ ./Yuta.FactoryOps.Application/
COPY Yuta.FactoryOps.Client/ ./Yuta.FactoryOps.Client/


# Restaura o projeto Server puxando ele do caminho duplicado
RUN dotnet restore Yuta.FactoryOps.Server/Yuta.FactoryOps.Server.csproj

# Compila o projeto Server
RUN dotnet build Yuta.FactoryOps.Server/Yuta.FactoryOps.Server.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish Yuta.FactoryOps.Server/Yuta.FactoryOps.Server.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Yuta.FactoryOps.Server.dll"]
