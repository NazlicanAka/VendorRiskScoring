using VendorRiskScoring.Application.Interfaces;
using VendorRiskScoring.Application.Models;
using VendorRiskScoring.Domain.Entities;
using VendorRiskScoring.Domain.Models;

namespace VendorRiskScoring.Application.Rules;

public class SecurityComplianceRiskRule : IRiskRule
{
    public string Category => "SecurityCompliance";
    public RiskResult CalculateRisk(Vendor vendor, RiskFactorMatrix matrix)
    {
        var result = new RiskResult();
        var triggeredScores = new List<double>();

        // --- SECURITY CONTROLS ---
        if (!vendor.SecurityCerts.Contains("ISO27001"))
        {
            if (matrix.SecurityRisk.TryGetValue("missingISO27001", out var similarRisks) && similarRisks.Any())
            {
                double avg = similarRisks.Values.Average();
                triggeredScores.Add(avg);
                result.Explanations.Add($"Missing ISO27001.");
            }
        }

        if (!vendor.Documents.PentestReportValid)
        {
            if (matrix.SecurityRisk.TryGetValue("failedPenTest", out var similarRisks) && similarRisks.Any())
            {
                double avg = similarRisks.Values.Average();
                triggeredScores.Add(avg);
                result.Explanations.Add($"No Pentest Report.");
            }
        }

        // --- COMPLIANCE CONTROLS ---
        if (!vendor.Documents.PrivacyPolicyValid)
        {
            if (matrix.ComplianceRisk.TryGetValue("expiredPrivacyPolicy", out var similarRisks) && similarRisks.Any())
            {
                double avg = similarRisks.Values.Average();
                triggeredScores.Add(avg);
                result.Explanations.Add($"Expired Privacy Policy.");
            }
        }

        if (!vendor.Documents.ContractValid)
        {
            if (matrix.ComplianceRisk.TryGetValue("expiredContract", out var similarRisks) && similarRisks.Any())
            {
                double avg = similarRisks.Values.Average();
                triggeredScores.Add(avg);
                result.Explanations.Add($"Expired Contract.");
            }
        }

        // Tetiklenen tüm güvenlik ve uyumluluk risklerinin ortak ortalamasını al
        if (triggeredScores.Any())
        {
            result.Score = triggeredScores.Average();
        }
        else
        {
            result.Score = 0.0;
            result.Explanations.Add("All security and compliance checks passed.");
        }

        return result;
    }
}