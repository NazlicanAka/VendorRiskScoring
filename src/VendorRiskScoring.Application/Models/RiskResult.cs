namespace VendorRiskScoring.Application.Models;

public class RiskResult
{
    // risk level is calculated from the Score.
    public double Score { get; set; }
    public List<string> Explanations { get; set; } = new();
}