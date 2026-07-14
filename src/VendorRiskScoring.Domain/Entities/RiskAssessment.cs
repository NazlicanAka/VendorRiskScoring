namespace VendorRiskScoring.Domain.Entities;

public class RiskAssessment
{
    public int VendorId { get; set; }
    public double RiskScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}