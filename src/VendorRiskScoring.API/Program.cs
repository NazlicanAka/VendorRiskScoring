using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VendorRiskScoring.Application.Configuration;
using VendorRiskScoring.Application.Factories;
using VendorRiskScoring.Application.Interfaces;
using VendorRiskScoring.Application.Rules;
using VendorRiskScoring.Application.Services;
using VendorRiskScoring.Infrastructure.Data;
using VendorRiskScoring.Infrastructure.Providers;
using VendorRiskScoring.Infrastructure.Repositories;

// 1. Serilog Başlangıç Ayarları
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog'u appsettings.json'dan okuyacak şekilde sisteme entegre et
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration));

    // 2. DbContext Kaydı (PostgreSQL Bağlantısı)
    builder.Services.AddDbContext<VendorDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // 3. Konfigürasyon Bağlamaları (Options Pattern)
    builder.Services.Configure<RiskWeightsOptions>(builder.Configuration.GetSection(RiskWeightsOptions.SectionName));
    builder.Services.Configure<RiskThresholdOptions>(builder.Configuration.GetSection(RiskThresholdOptions.SectionName));
    builder.Services.Configure<FilePathsOptions>(builder.Configuration.GetSection(FilePathsOptions.SectionName));

    // 4. Dependency Injection (DI) Kayıtları

    // Infrastructure (Altyapı)
    builder.Services.AddSingleton<IRiskMatrixProvider, JsonRiskMatrixProvider>();
    
    // YENİ: EfVendorRepository eklendi ve DbContext'e uyumlu olması için Scoped yapıldı.
    builder.Services.AddScoped<IVendorRepository, EfVendorRepository>();

    // Application (Servisler ve Factory)
    builder.Services.AddSingleton<IRiskAssessmentFactory, RiskAssessmentFactory>();
    builder.Services.AddScoped<IRiskEngineService, RiskEngineService>();

    // Strategy Pattern (Kurallar)
    builder.Services.AddScoped<IRiskRule, FinancialRiskRule>();
    builder.Services.AddScoped<IRiskRule, OperationalRiskRule>();
    builder.Services.AddScoped<IRiskRule, SecurityComplianceRiskRule>();

    // Standart API Servisleri
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // 5. Veritabanını Otomatik Oluşturma ve Seed (Veri Doldurma) İşlemi
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<VendorDbContext>();
            
            // Eğer veritabanı yoksa Docker üzerinde oluşturur ve tabloları kurar
            context.Database.EnsureCreated(); 
            
            // appsettings.json içerisindeki dosya yolunu alıp Seed işlemini başlatır
            var filePaths = services.GetRequiredService<IOptions<FilePathsOptions>>().Value;
            var vendorDataPath = Path.Combine(AppContext.BaseDirectory, filePaths.VendorData);
            
            DatabaseSeeder.SeedVendors(context, vendorDataPath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while migrating or seeding the database.");
        }
    }

    // 6. Middleware Pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Gelen istekleri Serilog ile loglamak için
    app.UseSerilogRequestLogging();

    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}