using System.Text.Json;
using Xunit;
using VendorRiskScoring.Application.Rules;
using VendorRiskScoring.Domain.Entities;
using VendorRiskScoring.Domain.Models;

namespace VendorRiskScoring.Tests.Rules;

public class FinancialRiskRuleTests
{
    private readonly FinancialRiskRule _rule;
    private readonly RiskFactorMatrix _matrix;

    public FinancialRiskRuleTests()
    {
        _rule = new FinancialRiskRule();
        
        // Alt sınıflarla (HealthScoreThresholds vs) uğraşmamak için matrisi JSON'dan taklit ediyoruz
        var json = @"{
            ""Financial"": {
                ""HealthScoreThresholds"": {
                    ""LowRiskMin"": 80,
                    ""MediumRiskMin"": 50
                }
            }
        }";
        
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        _matrix = JsonSerializer.Deserialize<RiskFactorMatrix>(json, options) ?? new RiskFactorMatrix();
    }

    [Fact]
    public void CalculateRisk_WhenFinancialHealthIsHigh_ReturnsZeroScore()
    {
        // Arrange
        var vendor = new Vendor { FinancialHealth = 85 };

        // Act
        var result = _rule.CalculateRisk(vendor, _matrix);

        // Assert
        Assert.Equal(0.0, result.Score);
        Assert.Contains("Financial Health > 80.", result.Explanations[0]);
    }

    [Fact]
    public void CalculateRisk_WhenFinancialHealthIsLow_ReturnsScoreOfOne()
    {
        // Arrange
        var vendor = new Vendor { FinancialHealth = 40 };

        // Act
        var result = _rule.CalculateRisk(vendor, _matrix);

        // Assert
        Assert.Equal(1.0, result.Score);
        Assert.Contains("Financial Health < 50", result.Explanations[0]);
    }
}