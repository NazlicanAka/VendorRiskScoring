using Microsoft.Extensions.Options;
using Xunit;
using VendorRiskScoring.Application.Configuration;
using VendorRiskScoring.Application.Factories;
using System.Collections.Generic;

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
    [InlineData(0.95, "Critical")] 
    [InlineData(0.90, "Critical")] 
    [InlineData(0.85, "High")]     
    [InlineData(0.70, "High")]     
    [InlineData(0.50, "Medium")]   
    [InlineData(0.40, "Medium")]   
    [InlineData(0.20, "Low")]      
    [InlineData(0.00, "Low")]      
    public void Create_ReturnsCorrectRiskLevel_BasedOnScore(double score, string expectedLevel)
    {
        // Act
        var result = _factory.Create(1, score, new Dictionary<string, List<string>>());

        // Assert
        Assert.Equal(expectedLevel, result.RiskLevel);
    }

    [Fact]
    public void Create_RoundsFinalScore_ToTwoDecimalPlaces()
    {
        // Arrange
        double rawScore = 0.75689;

        // Act
        var result = _factory.Create(1, rawScore, new Dictionary<string, List<string>>());

        // Assert
        Assert.Equal(0.76, result.RiskScore); 
    }

    [Fact]
    public void Create_MapsVendorIdAndGeneratesMeaningfulReasonCorrectly()
    {
        // Arrange
        int vendorId = 42;
        var categorizedExplanations = new Dictionary<string, List<string>>
        {
            { "Operational", new List<string> { "an SLA below 95%" } },
            { "SecurityCompliance", new List<string> { "missing ISO27001 certification" } }
        };

        // Act
        var result = _factory.Create(vendorId, 0.75, categorizedExplanations);

        // Assert
        Assert.Equal(vendorId, result.VendorId);
        
        string expectedReason = "An SLA below 95% and missing ISO27001 certification significantly impact the operational and security compliance risk levels, resulting in a High overall risk score.";
        Assert.Equal(expectedReason, result.Reason);
    }

    [Fact]
    public void Create_WhenNoExplanations_ReturnsOptimalPerformanceReason()
    {
        // Arrange
        var emptyExplanations = new Dictionary<string, List<string>>();

        // Act
        var result = _factory.Create(1, 0.10, emptyExplanations);

        // Assert
        Assert.Equal("The vendor has a Low risk profile with optimal performance across all evaluated metrics.", result.Reason);
    }
}