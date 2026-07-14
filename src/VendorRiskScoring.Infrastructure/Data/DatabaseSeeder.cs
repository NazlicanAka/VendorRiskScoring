using System.Text.Json;
using VendorRiskScoring.Domain.Entities;

namespace VendorRiskScoring.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static void SeedVendors(VendorDbContext context, string jsonFilePath)
    {
        // Eğer veritabanında zaten veri varsa, tekrar ekleme yapma
        if (context.Vendors.Any()) return;

        if (File.Exists(jsonFilePath))
        {
            var json = File.ReadAllText(jsonFilePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var wrapper = JsonSerializer.Deserialize<VendorDataWrapper>(json, options);

            if (wrapper != null && wrapper.Vendors.Any())
            {
                var vendors = wrapper.Vendors.Select(dto => new Vendor
                {
                    // Id'yi veritabanı otomatik vereceği için dto.Id'yi almıyoruz
                    Name = dto.Name,
                    FinancialHealth = dto.FinancialHealth,
                    SlaUptime = dto.SlaUptime,
                    MajorIncidents = dto.MajorIncidents,
                    SecurityCerts = dto.SecurityCerts,
                    Documents = dto.Documents
                });

                context.Vendors.AddRange(vendors);
                context.SaveChanges();
            }
        }
    }
}