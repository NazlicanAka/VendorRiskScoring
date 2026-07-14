namespace VendorRiskScoring.Application.Configuration;

public class RiskWeightsOptions
{
    public const string SectionName = "RiskWeights";

    public Dictionary<string, double> Weights { get; set; } = new();
}