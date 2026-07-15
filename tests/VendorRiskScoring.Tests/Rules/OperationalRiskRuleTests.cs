using Xunit;
using VendorRiskScoring.Application.Rules;
using VendorRiskScoring.Domain.Entities;
using VendorRiskScoring.Domain.Models;

namespace VendorRiskScoring.Tests.Rules;

public class OperationalRiskRuleTests
{
    private readonly OperationalRiskRule _rule;

    public OperationalRiskRuleTests()
    {
        _rule = new OperationalRiskRule();
    }

    [Fact]
    public void Category_ShouldReturnOperational()
    {
        Assert.Equal("Operational", _rule.Category);
    }

    [Fact]
    public void CalculateRisk_HighSla_LowIncidents_ReturnsZeroScore()
    {
        var vendor = new Vendor { SlaUptime = 98, MajorIncidents = 1 };
        var matrix = new RiskFactorMatrix 
        { 
            OperationalRisk = new Dictionary<string, Dictionary<string, double>>() 
        };

        var result = _rule.CalculateRisk(vendor, matrix);

        Assert.Equal(0.0, result.Score);
    }

    [Fact]
    public void CalculateRisk_LowSla_WithMatrixData_ReturnsSlaRiskScore()
    {
        var vendor = new Vendor { SlaUptime = 90, MajorIncidents = 1 };
        var matrix = new RiskFactorMatrix
        {
            OperationalRisk = new Dictionary<string, Dictionary<string, double>>
            {
                { "slaDrop", new Dictionary<string, double> { { "risk1", 0.6 }, { "risk2", 0.8 } } }
            }
        };

        var result = _rule.CalculateRisk(vendor, matrix);

        Assert.Equal(0.7, result.Score);
        Assert.Contains("SLA below 95%", result.Explanations);
    }

    [Fact]
    public void CalculateRisk_HighIncidents_WithMatrixData_ReturnsIncidentRiskScore()
    {
        var vendor = new Vendor { SlaUptime = 99, MajorIncidents = 4 };
        var matrix = new RiskFactorMatrix
        {
            OperationalRisk = new Dictionary<string, Dictionary<string, double>>
            {
                { "majorIncident", new Dictionary<string, double> { { "risk1", 0.9 } } }
            }
        };

        var result = _rule.CalculateRisk(vendor, matrix);

        Assert.Equal(0.9, result.Score);
        Assert.Contains("more than 2 major incidents", result.Explanations);
    }

    [Fact]
    public void CalculateRisk_LowSla_HighIncidents_ReturnsAverageScore()
    {
        var vendor = new Vendor { SlaUptime = 90, MajorIncidents = 5 };
        var matrix = new RiskFactorMatrix
        {
            OperationalRisk = new Dictionary<string, Dictionary<string, double>>
            {
                { "slaDrop", new Dictionary<string, double> { { "risk1", 0.5 } } },
                { "majorIncident", new Dictionary<string, double> { { "risk1", 0.9 } } }
            }
        };

        var result = _rule.CalculateRisk(vendor, matrix);

        Assert.Equal(0.7, result.Score);
        Assert.Contains("SLA below 95%", result.Explanations);
        Assert.Contains("more than 2 major incidents", result.Explanations);
    }

    [Fact]
    public void CalculateRisk_MissingMatrixData_ReturnsZeroScore()
    {
        var vendor = new Vendor { SlaUptime = 80, MajorIncidents = 5 };
        var matrix = new RiskFactorMatrix
        {
            OperationalRisk = new Dictionary<string, Dictionary<string, double>>()
        };

        var result = _rule.CalculateRisk(vendor, matrix);

        Assert.Equal(0.0, result.Score);
        Assert.Empty(result.Explanations);
    }
}