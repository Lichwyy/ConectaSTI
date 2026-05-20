FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ConectaSTI/ConectaSTI.Api/ConectaSTI.Api.csproj", "ConectaSTI/ConectaSTI.Api/"]
COPY ["ConectaSTI/ConectaSTI.Dominio/ConectaSTI.Dominio.csproj", "ConectaSTI/ConectaSTI.Dominio/"]
COPY ["ConectaSTI/ConectaSTI.Executor/ConectaSTI.Executor.csproj", "ConectaSTI/ConectaSTI.Executor/"]
COPY ["FGB.Api/FGB.Api.csproj", "FGB.Api/"]
COPY ["FGB/FGB.Dominio.csproj", "FGB/"]

RUN dotnet restore "ConectaSTI/ConectaSTI.Api/ConectaSTI.Api.csproj"

COPY . .
WORKDIR /src/ConectaSTI/ConectaSTI.Api
RUN dotnet publish "ConectaSTI.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ConectaSTI.Api.dll"]
