# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Packages.props ./
COPY src/UMS.Domain/UMS.Domain.csproj src/UMS.Domain/
COPY src/UMS.Contracts/UMS.Contracts.csproj src/UMS.Contracts/
COPY src/UMS.Application/UMS.Application.csproj src/UMS.Application/
COPY src/UMS.Infrastructure/UMS.Infrastructure.csproj src/UMS.Infrastructure/
COPY src/UMS.api/UMS.api.csproj src/UMS.api/

RUN dotnet restore src/UMS.api/UMS.api.csproj

COPY src/ ./src/

RUN dotnet publish src/UMS.api/UMS.api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Container-friendly defaults (Azure / Docker)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "UMS.api.dll"]
