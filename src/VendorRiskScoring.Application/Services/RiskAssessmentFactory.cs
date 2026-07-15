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

    public RiskAssessment Create(int vendorId, double finalScore, Dictionary<string, List<string>> categorizedExplanations)
    {
        var riskLevel = DetermineRiskLevel(finalScore);
        
        var reason = GenerateMeaningfulReason(riskLevel, categorizedExplanations);

        return new RiskAssessment
        {
            VendorId = vendorId,
            RiskScore = Math.Round(finalScore, 2),
            RiskLevel = riskLevel,
            Reason = reason
        };
    }

    private string DetermineRiskLevel(double score)
    {
        if (score >= _thresholds.Critical) return "Critical";
        if (score >= _thresholds.High) return "High";
        if (score >= _thresholds.Medium) return "Medium";
        return "Low";
    }

    private string GenerateMeaningfulReason(string riskLevel, Dictionary<string, List<string>> categorizedExplanations)
    {
        if (!categorizedExplanations.Any())
        {
            return $"The vendor has a {riskLevel} risk profile with optimal performance across all evaluated metrics.";
        }
        var allReasons = categorizedExplanations.Values.SelectMany(x => x).ToList();
        
        var categories = categorizedExplanations.Keys
            .Select(k => k.Replace("Compliance", " compliance").ToLower()) 
            .ToList();

        string categoryText = categories.Count > 1 
            ? string.Join(", ", categories.Take(categories.Count - 1)) + " and " + categories.Last()
            : categories.First();

        string reasonsText = allReasons.Count > 1
            ? string.Join(", ", allReasons.Take(allReasons.Count - 1)) + " and " + allReasons.Last()
            : allReasons.First();
            
        reasonsText = char.ToUpper(reasonsText[0]) + reasonsText.Substring(1);

        return $"{reasonsText} significantly impact the {categoryText} risk levels, resulting in a {riskLevel} overall risk score.";
    }
}