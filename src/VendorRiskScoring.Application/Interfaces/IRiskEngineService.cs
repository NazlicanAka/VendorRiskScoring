using VendorRiskScoring.Domain.Entities;

namespace VendorRiskScoring.Application.Interfaces;

public interface IRiskEngineService
{
    Task<RiskAssessment> EvaluateVendorAsync(int vendorId);
    Task<IEnumerable<RiskAssessment>> GetLeaderboardAsync();
}