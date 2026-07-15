using VendorRiskScoring.Domain.Entities;

namespace VendorRiskScoring.Infrastructure.Data;
public class VendorDataWrapper
{
    public List<VendorDto> Vendors { get; set; } = new();
}