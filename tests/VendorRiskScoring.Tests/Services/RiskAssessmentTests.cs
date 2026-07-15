using Microsoft.Extensions.Options;
using Xunit;
using VendorRiskScoring.Application.Configuration;
using VendorRiskScoring.Application.Factories;

namespace VendorRiskScoring.Tests.Factories;

public class RiskAssessmentFactoryTests
{
    private readonly RiskAssessmentFactory _factory;

    public RiskAssessmentFactoryTests()
    {
        var options = Options.Create(new RiskThresholdOptions
        {
            Critical = 0.90,
            High = 0.70,
            Medium = 0.40
        });

        _factory = new RiskAssessmentFactory(options);
    }


    [Theory]
    [InlineData(0.95, "Critical")] // Kritik eşiğin üstü
    [InlineData(0.90, "Critical")] // Tam kritik eşik
    [InlineData(0.85, "High")]     // High ile Critical arası
    [InlineData(0.70, "High")]     // Tam High eşiği
    [InlineData(0.50, "Medium")]   // Medium ile High arası
    [InlineData(0.40, "Medium")]   // Tam Medium eşiği
    [InlineData(0.20, "Low")]      // Medium eşiğinin altı
    [InlineData(0.00, "Low")]      // Sıfır skoru
    public void Create_ReturnsCorrectRiskLevel_BasedOnScore(double score, string expectedLevel)
    {
        // Act
        var result = _factory.Create(1, score, new List<string> { "Test reason." });

        // Assert
        Assert.Equal(expectedLevel, result.RiskLevel);
    }
    [Fact]
    public void Create_RoundsFinalScore_ToTwoDecimalPlaces()
    {
        // Arrange
        double rawScore = 0.75689;

        // Act
        var result = _factory.Create(1, rawScore, new List<string> { "Test" });

        // Assert
        Assert.Equal(0.76, result.RiskScore); 
    }


    [Fact]
    public void Create_MapsVendorIdAndJoinsExplanationsCorrectly()
    {
        // Arrange
        int vendorId = 42;
        var explanations = new List<string> 
        { 
            "Financial health is poor.", 
            "SLA uptime is acceptable." 
        };

        // Act
        var result = _factory.Create(vendorId, 0.50, explanations);

        // Assert
        Assert.Equal(vendorId, result.VendorId);
        Assert.Equal("Financial health is poor. SLA uptime is acceptable.", result.Reason);
    }
}