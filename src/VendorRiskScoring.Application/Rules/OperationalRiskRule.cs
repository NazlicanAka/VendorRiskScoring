using VendorRiskScoring.Application.Interfaces;
using VendorRiskScoring.Application.Models;
using VendorRiskScoring.Domain.Entities;
using VendorRiskScoring.Domain.Models;

namespace VendorRiskScoring.Application.Rules;

public class OperationalRiskRule : IRiskRule
{
    public string Category => "Operational";
    public RiskResult CalculateRisk(Vendor vendor, RiskFactorMatrix matrix)
    {
        var result = new RiskResult();
        var triggeredScores = new List<double>();

        // 1. SLA Uptime Kontrolü
        if (vendor.SlaUptime < 95)
        {
            if (matrix.OperationalRisk.TryGetValue("slaDrop", out var slaRisks) && slaRisks.Any())
            {
                double slaAvg = slaRisks.Values.Average();
                triggeredScores.Add(slaAvg);
                result.Explanations.Add($"SLA < 95%.");
            }
        }
        else
        {
            result.Explanations.Add($"SLA >= 95%.");
        }

        // 2. Major Incidents Kontrolü
        if (vendor.MajorIncidents > 2)
        {
            if (matrix.OperationalRisk.TryGetValue("majorIncident", out var incidentRisks) && incidentRisks.Any())
            {
                double incidentAvg = incidentRisks.Values.Average();
                triggeredScores.Add(incidentAvg);
                result.Explanations.Add($"Major incidents > 2.");
            }
        }
        else 
        {
            result.Explanations.Add($"Major incidents <= 2.");
        }

        // Eğer riskler tetiklendiyse ortalamalarını alıyoruz, yoksa 0 dönüyoruz
        if (triggeredScores.Any())
        {
            result.Score = triggeredScores.Average();
        }
        else
        {
            result.Score = 0.0;
        }

        return result;
    }
}