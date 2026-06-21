FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copia os arquivos de projeto (.csproj e .slnx) primeiro para restaurar as dependências
COPY ["Yuta.FactoryOps.slnx", "./"]
COPY ["Yuta.FactoryOps/Yuta.FactoryOps/Yuta.FactoryOps.Server.csproj", "Yuta.FactoryOps/Yuta.FactoryOps/"]
COPY ["Yuta.FactoryOps.Application/Yuta.FactoryOps.Application.csproj", "Yuta.FactoryOps.Application/"]
COPY ["Yuta.FactoryOps.Domain/Yuta.FactoryOps.Domain.csproj", "Yuta.FactoryOps.Domain/"]

# Restaura os pacotes NuGet baseando-se na solução
RUN dotnet restore Yuta.FactoryOps.slnx

# 2. AGORA copia o restante de todo o código-fonte (essencial para o build encontrar as classes!)
COPY . .

# Move para a pasta do projeto executável e compila tudo junto
WORKDIR "/src/Yuta.FactoryOps/Yuta.FactoryOps"
RUN dotnet build "Yuta.FactoryOps.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Yuta.FactoryOps.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 3. Geração da imagem final de execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Yuta.FactoryOps.Server.dll"]