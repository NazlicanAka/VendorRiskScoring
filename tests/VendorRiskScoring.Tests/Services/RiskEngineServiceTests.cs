using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using VendorRiskScoring.Application.Configuration;
using VendorRiskScoring.Application.Factories;
using VendorRiskScoring.Application.Interfaces;
using VendorRiskScoring.Application.Services;
using VendorRiskScoring.Domain.Entities;
using VendorRiskScoring.Domain.Models;
using VendorRiskScoring.Application.Rules;
using VendorRiskScoring.Application.Models;

namespace VendorRiskScoring.Tests.Services;

public class RiskEngineServiceTests
{
    private readonly Mock<IVendorRepository> _mockRepo;
    private readonly Mock<IRiskMatrixProvider> _mockMatrixProvider;
    private readonly RiskEngineService _service;

    public RiskEngineServiceTests()
    {
        _mockRepo = new Mock<IVendorRepository>();
        _mockMatrixProvider = new Mock<IRiskMatrixProvider>();
        
        var mockRule = new Mock<IRiskRule>();
        mockRule.Setup(r => r.Category).Returns("Financial");
        
        mockRule.Setup(r => r.CalculateRisk(It.IsAny<Vendor>(), It.IsAny<RiskFactorMatrix>()))
                .Returns(new RiskResult { Score = 1.0, Explanations = new List<string> { "high debt ratio" } });

        var rules = new List<IRiskRule> { mockRule.Object };

        var weightsOptions = Options.Create(new RiskWeightsOptions
        {
            Weights = new Dictionary<string, double> { { "Financial", 0.5 } }
        });
        
        var thresholdsOptions = Options.Create(new RiskThresholdOptions
        {
            Critical = 0.90, High = 0.70, Medium = 0.40
        });

        var factory = new RiskAssessmentFactory(thresholdsOptions);

        _service = new RiskEngineService(
            rules, 
            _mockRepo.Object, 
            _mockMatrixProvider.Object, 
            weightsOptions, 
            factory);
    }

    [Fact]
    public async Task EvaluateVendorAsync_WhenVendorExists_ReturnsCorrectAssessment()
    {
        _mockRepo.Setup(repo => repo.GetByIdAsync(1))
                 .ReturnsAsync(new Vendor { Id = 1, Name = "Test Vendor" });
                 
        _mockMatrixProvider.Setup(m => m.GetMatrix())
                           .Returns(new RiskFactorMatrix());

        // Act
        var result = await _service.EvaluateVendorAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.VendorId);
        Assert.Equal(0.5, result.RiskScore);
        Assert.Equal("Medium", result.RiskLevel); 

        Assert.Contains("High debt ratio", result.Reason);
        Assert.Contains("financial", result.Reason);
    }

    [Fact]
    public async Task EvaluateVendorAsync_WhenVendorDoesNotExist_ThrowsKeyNotFoundException()
    {
        _mockRepo.Setup(repo => repo.GetByIdAsync(99))
                 .ReturnsAsync((Vendor?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.EvaluateVendorAsync(99));
    }
}