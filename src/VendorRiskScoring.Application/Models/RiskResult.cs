namespace VendorRiskScoring.Application.Models;

public class RiskResult
{
    public double Score { get; set; }
    public List<string> Explanations { get; set; } = new();
}