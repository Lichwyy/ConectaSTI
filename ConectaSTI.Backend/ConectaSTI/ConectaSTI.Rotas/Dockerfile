FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ConectaSTI/ConectaSTI.Rotas/ConectaSTI.Rotas.csproj", "ConectaSTI/ConectaSTI.Rotas/"]
COPY ["ConectaSTI/ConectaSTI.Dominio/ConectaSTI.Dominio.csproj", "ConectaSTI/ConectaSTI.Dominio/"]
COPY ["FGB/FGB.Dominio.csproj", "FGB/"]

RUN dotnet restore "ConectaSTI/ConectaSTI.Rotas/ConectaSTI.Rotas.csproj"

COPY . .
WORKDIR /src/ConectaSTI/ConectaSTI.Rotas
RUN dotnet publish "ConectaSTI.Rotas.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ConectaSTI.Rotas.dll"]
