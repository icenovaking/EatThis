# syntax=docker/dockerfile:1

# Build stage: restore and publish only the ASP.NET Core API project.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["EatThis.Api.csproj", "./"]
RUN dotnet restore "EatThis.Api.csproj"

COPY . .
RUN dotnet publish "EatThis.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Runtime stage: Cloud Run sends traffic to port 8080 by default.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EatThis.Api.dll"]
