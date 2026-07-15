using Xunit;
using VendorRiskScoring.Application.Rules;
using VendorRiskScoring.Domain.Entities;
using VendorRiskScoring.Domain.Models;

namespace VendorRiskScoring.Tests.Rules;

public class FinancialRiskRuleTests
{
    private readonly FinancialRiskRule _rule;

    public FinancialRiskRuleTests()
    {
        _rule = new FinancialRiskRule();
    }

    [Fact]
    public void Category_ShouldReturnFinancial()
    {
        Assert.Equal("Financial", _rule.Category);
    }

    [Theory]
    [InlineData(85, "Financial Health > 80.", 0.0)]
    [InlineData(81, "Financial Health > 80.", 0.0)]
    public void CalculateRisk_HighHealth_ReturnsZeroScore(int health, string expectedExplanation, double expectedScore)
    {
        var vendor = new Vendor { FinancialHealth = health };
        var matrix = new RiskFactorMatrix 
        { 
            FinancialRisk = new Dictionary<string, Dictionary<string, double>>() 
        };

        var result = _rule.CalculateRisk(vendor, matrix);

        Assert.Equal(expectedScore, result.Score);
        Assert.Contains(expectedExplanation, result.Explanations);
    }

    [Fact]
    public void CalculateRisk_LowHealth_WithMatrixValues_ReturnsMatrixAverage()
    {
        var vendor = new Vendor { FinancialHealth = 40 };
        var matrix = new RiskFactorMatrix
        {
            FinancialRisk = new Dictionary<string, Dictionary<string, double>>
            {
                { "lowCashFlow", new Dictionary<string, double> { { "risk1", 0.8 }, { "risk2", 0.6 } } }
            }
        };

        var result = _rule.CalculateRisk(vendor, matrix);

        Assert.Equal(0.7, result.Score);
        Assert.Contains("Financial Health < 50.", result.Explanations);
    }

    [Fact]
    public void CalculateRisk_LowHealth_WithoutMatrixValues_ReturnsDefaultScore()
    {
        var vendor = new Vendor { FinancialHealth = 40 };
        var matrix = new RiskFactorMatrix
        {
            FinancialRisk = new Dictionary<string, Dictionary<string, double>>()
        };

        var result = _rule.CalculateRisk(vendor, matrix);

        Assert.Equal(1.0, result.Score);
        Assert.Contains("Financial Health < 50.", result.Explanations);
    }

    [Fact]
    public void CalculateRisk_MediumHealth_CalculatesProportionalScore()
    {
        var vendor = new Vendor { FinancialHealth = 65 };
        var matrix = new RiskFactorMatrix
        {
            FinancialRisk = new Dictionary<string, Dictionary<string, double>>
            {
                { "lowCashFlow", new Dictionary<string, double> { { "risk1", 0.8 } } }
            }
        };

        var result = _rule.CalculateRisk(vendor, matrix);

        Assert.Equal(0.4, result.Score);
        Assert.Contains("Financial Health between 50 and 80.", result.Explanations);
    }

    [Fact]
    public void CalculateRisk_CalculatedScoreExceedsOne_ClampsToOne()
    {
        var vendor = new Vendor { FinancialHealth = 40 };
        var matrix = new RiskFactorMatrix
        {
            FinancialRisk = new Dictionary<string, Dictionary<string, double>>
            {
                { "lowCashFlow", new Dictionary<string, double> { { "risk1", 2.5 } } }
            }
        };

        var result = _rule.CalculateRisk(vendor, matrix);

        Assert.Equal(1.0, result.Score);
    }
}