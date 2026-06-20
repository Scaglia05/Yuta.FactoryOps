FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . .

RUN dotnet restore $(find . -name "*.slnx" -o -name "*.sln" | head -n 1)
RUN dotnet build $(find . -name Yuta.FactoryOps.Server.csproj) -c Release -o /app/build

FROM build AS publish
RUN dotnet publish $(find . -name Yuta.FactoryOps.Server.csproj) -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Yuta.FactoryOps.Server.dll"]