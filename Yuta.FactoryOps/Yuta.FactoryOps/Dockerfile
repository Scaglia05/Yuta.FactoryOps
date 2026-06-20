# Estágio de Compilação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos de projeto respeitando a estrutura real do seu GitHub
COPY ["Yuta.FactoryOps/Yuta.FactoryOps/Yuta.FactoryOps.Server.csproj", "Yuta.FactoryOps/Yuta.FactoryOps/"]
COPY ["Yuta.FactoryOps.Application/Yuta.FactoryOps.Application.csproj", "Yuta.FactoryOps.Application/"]
COPY ["Yuta.FactoryOps.Domain/Yuta.FactoryOps.Domain.csproj", "Yuta.FactoryOps.Domain/"]
COPY ["Yuta.FactoryOps/Yuta.FactoryOps.Client/Yuta.FactoryOps.Client.csproj", "Yuta.FactoryOps/Yuta.FactoryOps.Client/"]

RUN dotnet restore "Yuta.FactoryOps/Yuta.FactoryOps/Yuta.FactoryOps.Server.csproj"

# Copia o restante do código e compila
COPY . .
WORKDIR "/src/Yuta.FactoryOps/Yuta.FactoryOps"
RUN dotnet build "Yuta.FactoryOps.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Yuta.FactoryOps.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio Final de Execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Yuta.FactoryOps.Server.dll"]