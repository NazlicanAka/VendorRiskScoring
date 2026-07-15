using Microsoft.Extensions.Options;
using VendorRiskScoring.Application.Configuration;
using VendorRiskScoring.Domain.Entities;

namespace VendorRiskScoring.Application.Factories;

public class RiskAssessmentFactory : IRiskAssessmentFactory
{
    private readonly RiskThresholdOptions _thresholds;

    public RiskAssessmentFactory(IOptions<RiskThresholdOptions> options)
    {
        _thresholds = options.Value;
    }

    public RiskAssessment Create(int vendorId, double finalScore, List<string> explanations)
    {
        return new RiskAssessment
        {
            VendorId = vendorId,
            RiskScore = Math.Round(finalScore, 2),
            RiskLevel = DetermineRiskLevel(finalScore),
            Reason = string.Join(" ", explanations)
        };
    }

    private string DetermineRiskLevel(double score)
    {
        if (score >= _thresholds.Critical) return "Critical";
        if (score >= _thresholds.High) return "High";
        if (score >= _thresholds.Medium) return "Medium";
        return "Low";
    }
}