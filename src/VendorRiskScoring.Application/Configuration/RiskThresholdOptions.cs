namespace VendorRiskScoring.Application.Configuration;

public class RiskThresholdOptions
{
    public const string SectionName = "RiskThresholds";

    public double Critical { get; set; } = 0.90;
    public double High { get; set; } = 0.70;
    public double Medium { get; set; } = 0.40;
}