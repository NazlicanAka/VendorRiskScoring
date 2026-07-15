using VendorRiskScoring.Domain.Entities;

namespace VendorRiskScoring.Application.Factories;

public interface IRiskAssessmentFactory
{
    RiskAssessment Create(int vendorId, double finalScore, List<string> explanations);
}