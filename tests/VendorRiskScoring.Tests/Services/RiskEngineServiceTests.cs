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
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace VendorRiskScoring.Tests.Services;

public class RiskEngineServiceTests
{
    private readonly Mock<IVendorRepository> _mockRepo;
    private readonly Mock<IRiskMatrixProvider> _mockMatrixProvider;
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly RiskEngineService _service;

    public RiskEngineServiceTests()
    {
        _mockRepo = new Mock<IVendorRepository>();
        _mockMatrixProvider = new Mock<IRiskMatrixProvider>();
        _mockCache = new Mock<IDistributedCache>();

        _mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync((byte[]?)null);
        
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
            factory,
            _mockCache.Object);
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

    [Fact]
    public async Task GetLeaderboardAsync_WhenVendorsExist_ReturnsSortedLeaderboard()
    {
        // Arrange
        var mockVendors = new List<Vendor>
        {
            new Vendor { Id = 101, Name = "High Risk Vendor" },
            new Vendor { Id = 102, Name = "Low Risk Vendor" }
        };
        
        _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(mockVendors);
        _mockMatrixProvider.Setup(m => m.GetMatrix()).Returns(new RiskFactorMatrix());

        // Act
        var result = await _service.GetLeaderboardAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }
}