using Microsoft.EntityFrameworkCore;
using Xunit;
using VendorRiskScoring.Domain.Entities;
using VendorRiskScoring.Infrastructure.Data;

namespace VendorRiskScoring.Tests.Infrastructure;

public class DatabaseSeederTests : IDisposable
{
    private readonly VendorDbContext _context;
    private readonly string _tempFilePath;

    public DatabaseSeederTests()
    {
        var options = new DbContextOptionsBuilder<VendorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new VendorDbContext(options);
        _tempFilePath = Path.GetTempFileName();
    }

    [Fact]
    public void SeedVendors_FileNotFound_DoesNothing()
    {
        DatabaseSeeder.SeedVendors(_context, "non_existent_file.json");

        Assert.Empty(_context.Vendors);
    }

    [Fact]
    public void SeedVendors_EmptyOrInvalidJson_DoesNothing()
    {
        File.WriteAllText(_tempFilePath, "{\"Vendors\": []}");

        DatabaseSeeder.SeedVendors(_context, _tempFilePath);

        Assert.Empty(_context.Vendors);
    }

    [Fact]
    public void SeedVendors_WithNewVendors_AddsToDatabase()
    {
        var jsonContent = @"
        {
            ""Vendors"": [
                {
                    ""Name"": ""New Vendor"",
                    ""FinancialHealth"": 80,
                    ""SlaUptime"": 99,
                    ""MajorIncidents"": 0,
                    ""SecurityCerts"": [""ISO27001""],
                    ""Documents"": { ""ContractValid"": true, ""PrivacyPolicyValid"": true, ""PentestReportValid"": true }
                }
            ]
        }";
        File.WriteAllText(_tempFilePath, jsonContent);

        DatabaseSeeder.SeedVendors(_context, _tempFilePath);

        Assert.Single(_context.Vendors);
        var vendor = _context.Vendors.First();
        Assert.Equal("New Vendor", vendor.Name);
        Assert.Equal(80, vendor.FinancialHealth);
        Assert.Equal(99, vendor.SlaUptime);
    }

    [Fact]
    public void SeedVendors_WithExistingVendor_UpdatesExistingRecord()
    {
        var existingVendor = new Vendor
        {
            Name = "Existing Vendor",
            FinancialHealth = 50,
            SlaUptime = 90,
            MajorIncidents = 5,
            SecurityCerts = new List<string>(),
            Documents = new VendorDocuments { ContractValid = false, PrivacyPolicyValid = false, PentestReportValid = false }
        };
        _context.Vendors.Add(existingVendor);
        _context.SaveChanges();

        var jsonContent = @"
        {
            ""Vendors"": [
                {
                    ""Name"": ""Existing Vendor"",
                    ""FinancialHealth"": 90,
                    ""SlaUptime"": 99,
                    ""MajorIncidents"": 1,
                    ""SecurityCerts"": [""SOC2""],
                    ""Documents"": { ""ContractValid"": true, ""PrivacyPolicyValid"": true, ""PentestReportValid"": true }
                }
            ]
        }";
        File.WriteAllText(_tempFilePath, jsonContent);

        DatabaseSeeder.SeedVendors(_context, _tempFilePath);

        Assert.Single(_context.Vendors);
        var updatedVendor = _context.Vendors.First();
        Assert.Equal("Existing Vendor", updatedVendor.Name);
        Assert.Equal(90, updatedVendor.FinancialHealth);
        Assert.Equal(99, updatedVendor.SlaUptime);
        Assert.Equal(1, updatedVendor.MajorIncidents);
        Assert.Contains("SOC2", updatedVendor.SecurityCerts);
        Assert.True(updatedVendor.Documents.ContractValid);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();

        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }
    }
}