# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY ./code/WildNatureExplorer.API/WildNatureExplorer.API.csproj ./WildNatureExplorer.API/
COPY ./code/WildNatureExplorer.Application/WildNatureExplorer.Application.csproj ./WildNatureExplorer.Application/
COPY ./code/WildNatureExplorer.Infrastructure/WildNatureExplorer.Infrastructure.csproj ./WildNatureExplorer.Infrastructure/
COPY ./WildNatureExplorer.Tests/WildNatureExplorer.Tests.csproj ./WildNatureExplorer.Tests/
COPY ./code/WildNatureExplorer.Domain/WildNatureExplorer.Domain.csproj WildNatureExplorer.Domain/

RUN dotnet restore ./WildNatureExplorer.API/WildNatureExplorer.API.csproj

COPY ./code/ .

WORKDIR /src/WildNatureExplorer.API

RUN dotnet publish -c Release -o /app/publish --no-self-contained
RUN dotnet nuget locals all --clear

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

COPY --from=build /app/publish .

RUN adduser -D appuser \
    && mkdir -p /home/appuser/.aspnet/DataProtection-Keys \
    && chown -R appuser:appuser /home/appuser/.aspnet/DataProtection-Keys
USER appuser

RUN chown -R appuser:appuser /home/appuser/.aspnet/DataProtection-Keys

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

HEALTHCHECK --interval=120s --timeout=5s --retries=5 \
    CMD wget --spider -q http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "WildNatureExplorer.API.dll"]
