# syntax=docker/dockerfile:1.7-labs

# -------------------- Build stage --------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first to leverage Docker layer caching
COPY ./IPSDataAcquisition.sln ./
COPY ./src/IPSDataAcquisition.Domain/IPSDataAcquisition.Domain.csproj ./src/IPSDataAcquisition.Domain/
COPY ./src/IPSDataAcquisition.Application/IPSDataAcquisition.Application.csproj ./src/IPSDataAcquisition.Application/
COPY ./src/IPSDataAcquisition.Infrastructure/IPSDataAcquisition.Infrastructure.csproj ./src/IPSDataAcquisition.Infrastructure/
COPY ./src/IPSDataAcquisition.Presentation/IPSDataAcquisition.Presentation.csproj ./src/IPSDataAcquisition.Presentation/

# Restore
RUN dotnet restore ./IPSDataAcquisition.sln

# Copy the rest of the source
COPY ./src ./src

# Publish (Release)
RUN dotnet publish ./src/IPSDataAcquisition.Presentation/IPSDataAcquisition.Presentation.csproj -c Release -o /app/publish /p:UseAppHost=false

# -------------------- Runtime stage --------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Set environment defaults (can be overridden by docker-compose or env vars)
ENV ASPNETCORE_URLS=http://+:5000
ENV DOTNET_EnableDiagnostics=0

COPY --from=build /app/publish .

EXPOSE 5000

ENTRYPOINT ["dotnet", "IPSDataAcquisition.Presentation.dll"]

