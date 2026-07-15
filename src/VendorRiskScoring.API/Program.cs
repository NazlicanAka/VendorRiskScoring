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

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog'u appsettings
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration));

    // DbContext 
    builder.Services.AddDbContext<VendorDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Configuration Options
    builder.Services.Configure<RiskWeightsOptions>(builder.Configuration.GetSection(RiskWeightsOptions.SectionName));
    builder.Services.Configure<RiskThresholdOptions>(builder.Configuration.GetSection(RiskThresholdOptions.SectionName));
    builder.Services.Configure<FilePathsOptions>(builder.Configuration.GetSection(FilePathsOptions.SectionName));

    // Dependency Injection

    // Infrastructure
    builder.Services.AddSingleton<IRiskMatrixProvider, JsonRiskMatrixProvider>();

    // EfVendorRepository 
    builder.Services.AddScoped<IVendorRepository, EfVendorRepository>();

    // Application (Servisler ve Factory)
    builder.Services.AddSingleton<IRiskAssessmentFactory, RiskAssessmentFactory>();
    builder.Services.AddScoped<IRiskEngineService, RiskEngineService>();

    // Strategy Pattern - rules
    builder.Services.AddScoped<IRiskRule, FinancialRiskRule>();
    builder.Services.AddScoped<IRiskRule, OperationalRiskRule>();
    builder.Services.AddScoped<IRiskRule, SecurityComplianceRiskRule>();

    // Redis Distributed Cache Konfigürasyonu
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
        options.InstanceName = "VendorRisk_"; 
    });

    // API Services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

    var app = builder.Build();

    // Seed operation
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<VendorDbContext>();

            context.Database.EnsureCreated();

            // get the file paths from configuration (app.settings.json)
            var filePaths = services.GetRequiredService<IOptions<FilePathsOptions>>().Value;
            var vendorDataPath = Path.Combine(AppContext.BaseDirectory, filePaths.VendorData);

            DatabaseSeeder.SeedVendors(context, vendorDataPath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while migrating or seeding the database.");
        }
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();

    app.UseCors("AllowFrontend");
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