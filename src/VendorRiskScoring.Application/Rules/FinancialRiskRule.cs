using VendorRiskScoring.Application.Interfaces;
using VendorRiskScoring.Application.Models;
using VendorRiskScoring.Domain.Entities;
using VendorRiskScoring.Domain.Models;

namespace VendorRiskScoring.Application.Rules;

public class FinancialRiskRule : IRiskRule
{
    public string Category => "Financial";
    public RiskResult CalculateRisk(Vendor vendor, RiskFactorMatrix matrix)
    {
        
        var result = new RiskResult();
        var triggeredScores = new List<double>();
        
        double matrixAverage = 1.0; 
        if (matrix.FinancialRisk.TryGetValue("lowCashFlow", out var similarRisks) && similarRisks.Any())
        {
            matrixAverage = similarRisks.Values.Average();
        }

        if (vendor.FinancialHealth > 80)
        {
            result.Explanations.Add($"Financial Health > 80.");
        }
        else if (vendor.FinancialHealth < 50)
        {
            triggeredScores.Add(matrixAverage);
            result.Explanations.Add($"Financial Health < 50.");
        }
        else
        {
            double riskFactor = (80.0 - vendor.FinancialHealth) / 30.0;
            double calculatedScore = riskFactor * matrixAverage;
            
            triggeredScores.Add(calculatedScore);
            result.Explanations.Add($"Financial Health between 50 and 80.");
        }

        if (triggeredScores.Any())
        {
            result.Score = triggeredScores.Average();
        }
        else
        {
            result.Score = 0.0;
        }

        result.Score = Math.Clamp(result.Score, 0.0, 1.0);

        return result;
    }
}