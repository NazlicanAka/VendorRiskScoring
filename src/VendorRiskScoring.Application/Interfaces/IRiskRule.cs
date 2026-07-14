using VendorRiskScoring.Domain.Entities;
using VendorRiskScoring.Domain.Models;
using VendorRiskScoring.Application.Models;

namespace VendorRiskScoring.Application.Interfaces;

public interface IRiskRule
{
    string Category { get; }
    RiskResult CalculateRisk(Vendor vendor, RiskFactorMatrix matrix);
}