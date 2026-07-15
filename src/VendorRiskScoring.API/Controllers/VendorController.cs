using Microsoft.AspNetCore.Mvc;
using VendorRiskScoring.Application.Interfaces;
using VendorRiskScoring.Domain.Entities;

namespace VendorRiskScoring.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VendorController : ControllerBase
{
    private readonly IRiskEngineService _riskEngineService;
    private readonly IVendorRepository _vendorRepository;
    private readonly ILogger<VendorController> _logger;

    // Repository Pattern is used here to abstract the data access layer. This allows us to switch out the underlying data source (e.g., from PostgreSQL to another database) without changing the controller logic.
    public VendorController(
        IRiskEngineService riskEngineService, 
        IVendorRepository vendorRepository,
        ILogger<VendorController> logger)
    {
        _riskEngineService = riskEngineService;
        _vendorRepository = vendorRepository;
        _logger = logger;
    }

    // GET /api/vendor
    [HttpGet]
    public async Task<IActionResult> GetAllVendors()
    {
        var vendors = await _vendorRepository.GetAllAsync();
        return Ok(vendors);
    }

    // POST: api/vendor
    [HttpPost]
    public async Task<IActionResult> CreateVendor([FromBody] Vendor vendor)
    {
        if (vendor == null)
        {
            return BadRequest("Vendor data is null.");
        }
        await _vendorRepository.AddAsync(vendor);

        _logger.LogInformation("New vendor created successfully with ID {VendorId}: {VendorName}", vendor.Id, vendor.Name);

        return CreatedAtAction(nameof(GetAllVendors), new { id = vendor.Id }, vendor);
    }

    // GET /api/vendor/{id}/risk
    [HttpGet("{id}/risk")]
    public async Task<IActionResult> GetVendorRisk(int id)
    {
        try
        {
            _logger.LogInformation("Calculating risk for vendor ID: {VendorId}", id);
            
            var riskAssessment = await _riskEngineService.EvaluateVendorAsync(id);
            
            _logger.LogInformation("Risk calculation successful for vendor ID: {VendorId}. Final Score: {Score}", id, riskAssessment.RiskScore);
            
            return Ok(new
            {
                riskScore = riskAssessment.RiskScore,
                riskLevel = riskAssessment.RiskLevel,
                reason = riskAssessment.Reason
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Vendor not found. ID: {VendorId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while calculating risk for vendor ID: {VendorId}", id);
            return StatusCode(500, new { message = "An internal server error occurred." });
        }
    }

    // GET /api/vendor/leaderboard
    [HttpGet("leaderboard")]
    public async Task<IActionResult> GetLeaderboard()
    {
        try
        {
            _logger.LogInformation("Generating vendor risk leaderboard.");
            
            var leaderboard = await _riskEngineService.GetLeaderboardAsync();
            
            _logger.LogInformation("Leaderboard generated successfully with {Count} vendors.", leaderboard.Count());
            
            return Ok(leaderboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while generating the vendor leaderboard.");
            return StatusCode(500, new { message = "An internal server error occurred." });
        }
    }
}