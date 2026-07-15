using System.Text.Json;
using VendorRiskScoring.Domain.Entities;

namespace VendorRiskScoring.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static void SeedVendors(VendorDbContext context, string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath)) return;

        var json = File.ReadAllText(jsonFilePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var wrapper = JsonSerializer.Deserialize<VendorDataWrapper>(json, options);

        if (wrapper == null || !wrapper.Vendors.Any()) return;

        var existingVendors = context.Vendors.ToList();

        foreach (var dto in wrapper.Vendors)
        {
            // Check if the vendor already exists in the database by name, add if any update happens
            var existingVendor = existingVendors.FirstOrDefault(v => v.Name == dto.Name);

            if (existingVendor != null)
            {
                existingVendor.FinancialHealth = dto.FinancialHealth;
                existingVendor.SlaUptime = dto.SlaUptime;
                existingVendor.MajorIncidents = dto.MajorIncidents;
                existingVendor.SecurityCerts = dto.SecurityCerts;
                existingVendor.Documents = dto.Documents;
            }
            else
            {
                context.Vendors.Add(new Vendor
                {
                    Name = dto.Name,
                    FinancialHealth = dto.FinancialHealth,
                    SlaUptime = dto.SlaUptime,
                    MajorIncidents = dto.MajorIncidents,
                    SecurityCerts = dto.SecurityCerts,
                    Documents = dto.Documents
                });
            }
        }

        context.SaveChanges();
    }
}