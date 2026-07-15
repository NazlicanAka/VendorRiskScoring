using VendorRiskScoring.Domain.Entities;

namespace VendorRiskScoring.Application.Factories;

public interface IRiskAssessmentFactory
{
    RiskAssessment Create(int vendorId, double finalScore, Dictionary<string, List<string>> categorizedExplanations);
}