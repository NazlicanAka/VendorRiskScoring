# 1. Çalışma Zamanı (Runtime) İmajı
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# .NET 8 Docker imajları varsayılan olarak 8080 portunda çalışır
EXPOSE 8080 

# 2. Derleme (Build) İmajı
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Sadece proje (csproj) dosyalarını kopyalayıp paketleri (Nuget) indiriyoruz (Cache avantajı için)
COPY ["src/VendorRiskScoring.API/VendorRiskScoring.API.csproj", "src/VendorRiskScoring.API/"]
COPY ["src/VendorRiskScoring.Application/VendorRiskScoring.Application.csproj", "src/VendorRiskScoring.Application/"]
COPY ["src/VendorRiskScoring.Domain/VendorRiskScoring.Domain.csproj", "src/VendorRiskScoring.Domain/"]
COPY ["src/VendorRiskScoring.Infrastructure/VendorRiskScoring.Infrastructure.csproj", "src/VendorRiskScoring.Infrastructure/"]
RUN dotnet restore "src/VendorRiskScoring.API/VendorRiskScoring.API.csproj"

# Tüm kaynak kodları kopyalayıp projeyi derliyoruz
COPY . .
WORKDIR "/src/src/VendorRiskScoring.API"
RUN dotnet build "VendorRiskScoring.API.csproj" -c Release -o /app/build

# 3. Yayınlama (Publish)
FROM build AS publish
RUN dotnet publish "VendorRiskScoring.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. Son İmaj (Final)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Seed Data klasörünü güvenliğe almak için publish klasörüne manuel kopyalıyoruz
COPY src/VendorRiskScoring.API/Data ./Data

ENTRYPOINT ["dotnet", "VendorRiskScoring.API.dll"]