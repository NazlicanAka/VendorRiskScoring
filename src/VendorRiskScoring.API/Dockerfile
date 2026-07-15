FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

EXPOSE 8080 

# 2. build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/VendorRiskScoring.API/VendorRiskScoring.API.csproj", "src/VendorRiskScoring.API/"]
COPY ["src/VendorRiskScoring.Application/VendorRiskScoring.Application.csproj", "src/VendorRiskScoring.Application/"]
COPY ["src/VendorRiskScoring.Domain/VendorRiskScoring.Domain.csproj", "src/VendorRiskScoring.Domain/"]
COPY ["src/VendorRiskScoring.Infrastructure/VendorRiskScoring.Infrastructure.csproj", "src/VendorRiskScoring.Infrastructure/"]
RUN dotnet restore "src/VendorRiskScoring.API/VendorRiskScoring.API.csproj"

# get source kod, and build the project
COPY . .
WORKDIR "/src/src/VendorRiskScoring.API"
RUN dotnet build "VendorRiskScoring.API.csproj" -c Release -o /app/build

# publish
FROM build AS publish
RUN dotnet publish "VendorRiskScoring.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Seed data for PostgreSQL database
COPY src/VendorRiskScoring.API/Data ./Data

ENTRYPOINT ["dotnet", "VendorRiskScoring.API.dll"]