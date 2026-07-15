using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using VendorRiskScoring.API.Controllers;
using VendorRiskScoring.Application.Interfaces;
using VendorRiskScoring.Application.Models;
using VendorRiskScoring.Domain.Entities;

namespace VendorRiskScoring.Tests.Controllers;

public class VendorControllerTests
{
    private readonly Mock<IRiskEngineService> _mockRiskEngineService;
    private readonly Mock<IVendorRepository> _mockVendorRepository;
    private readonly Mock<ILogger<VendorController>> _mockLogger;
    private readonly VendorController _controller;

    public VendorControllerTests()
    {
        _mockRiskEngineService = new Mock<IRiskEngineService>();
        _mockVendorRepository = new Mock<IVendorRepository>();
        _mockLogger = new Mock<ILogger<VendorController>>();

        _controller = new VendorController(
            _mockRiskEngineService.Object,
            _mockVendorRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllVendors_ReturnsOkResult_WithListOfVendors()
    {
        // Arrange 
        var mockVendors = new List<Vendor> 
        { 
            new Vendor { Id = 1, Name = "Test Vendor 1" },
            new Vendor { Id = 2, Name = "Test Vendor 2" }
        };
        _mockVendorRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(mockVendors);

        // Act 
        var result = await _controller.GetAllVendors();

        // Assert 
        var okResult = Assert.IsType<OkObjectResult>(result); // 200 OK dönmeli
        var returnVendors = Assert.IsAssignableFrom<IEnumerable<Vendor>>(okResult.Value);
        Assert.Equal(2, returnVendors.Count()); // İçinde 2 kayıt olmalı
    }

    [Fact]
    public async Task CreateVendor_WhenVendorIsNull_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.CreateVendor(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result); 
        Assert.Equal("Vendor data is null.", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateVendor_WhenVendorIsValid_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var newVendor = new Vendor { Id = 1, Name = "New Tech Company" };
        _mockVendorRepository.Setup(repo => repo.AddAsync(newVendor)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateVendor(newVendor);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result); 
        Assert.Equal(nameof(_controller.GetAllVendors), createdResult.ActionName); 
        Assert.Equal(newVendor, createdResult.Value); 
    }

    [Fact]
    public async Task GetVendorRisk_WhenVendorExists_ReturnsOkResult_WithCalculatedRisk()
    {
        var riskAssessment = new RiskAssessment 
        { 
            VendorId = 1, 
            RiskScore = 0.85, 
            RiskLevel = "High", 
            Reason = "Poor financial health." 
        };
        _mockRiskEngineService.Setup(service => service.EvaluateVendorAsync(1)).ReturnsAsync(riskAssessment);

        // Act
        var result = await _controller.GetVendorRisk(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result); 
        Assert.NotNull(okResult.Value); 
    }

    [Fact]
    public async Task GetVendorRisk_WhenVendorNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        _mockRiskEngineService.Setup(service => service.EvaluateVendorAsync(99))
            .ThrowsAsync(new KeyNotFoundException("Vendor with ID 99 not found."));

        // Act
        var result = await _controller.GetVendorRisk(99);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetVendorRisk_WhenUnexpectedErrorOccurs_ReturnsStatusCode500()
    {
        // Arrange
        _mockRiskEngineService.Setup(service => service.EvaluateVendorAsync(1))
            .ThrowsAsync(new Exception("Database connection lost!"));

        // Act
        var result = await _controller.GetVendorRisk(1);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetLeaderboard_ReturnsOkResult_WithSortedRiskAssessments()
    {
        // Arrange
        var mockLeaderboard = new List<RiskAssessment>
        {
            new RiskAssessment { VendorId = 2, RiskScore = 0.95, RiskLevel = "Critical", Reason = "Critical issues" },
            new RiskAssessment { VendorId = 1, RiskScore = 0.45, RiskLevel = "Medium", Reason = "Medium issues" }
        };

        _mockRiskEngineService.Setup(service => service.GetLeaderboardAsync()).ReturnsAsync(mockLeaderboard);

        // Act
        var result = await _controller.GetLeaderboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnList = Assert.IsAssignableFrom<IEnumerable<RiskAssessment>>(okResult.Value);
        Assert.Equal(2, returnList.Count());
        Assert.Equal(0.95, returnList.First().RiskScore);
    }
}