# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY ./src .

RUN dotnet restore API/API.csproj
RUN dotnet build API/API.csproj --no-restore -c Release


# Publish
FROM build AS publish
WORKDIR /app

RUN dotnet publish API/API.csproj -c Release -o /app/publish /p:UseAppHost=false


# Final
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "API.dll"]