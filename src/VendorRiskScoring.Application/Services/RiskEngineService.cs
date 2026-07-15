using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VendorRiskScoring.Application.Configuration;
using VendorRiskScoring.Application.Factories;
using VendorRiskScoring.Application.Interfaces;
using VendorRiskScoring.Domain.Entities;
using VendorRiskScoring.Domain.Models;

namespace VendorRiskScoring.Application.Services;

public class RiskEngineService : IRiskEngineService
{
    private readonly IEnumerable<IRiskRule> _rules;
    private readonly IVendorRepository _vendorRepository;
    private readonly IRiskMatrixProvider _matrixProvider;
    private readonly RiskWeightsOptions _weights;
    private readonly IRiskAssessmentFactory _factory;
    private readonly IDistributedCache _cache; // Redis için eklendi

    public RiskEngineService(
        IEnumerable<IRiskRule> rules,
        IVendorRepository vendorRepository,
        IRiskMatrixProvider matrixProvider,
        IOptions<RiskWeightsOptions> weights,
        IRiskAssessmentFactory factory,
        IDistributedCache cache) // Constructor'a eklendi
    {
        _rules = rules;
        _vendorRepository = vendorRepository;
        _matrixProvider = matrixProvider;
        _weights = weights.Value;
        _factory = factory;
        _cache = cache;
    }

    public async Task<RiskAssessment> EvaluateVendorAsync(int vendorId)
    {
        // check if the risk assessment is already cached in Redis
        string cacheKey = $"risk_score_{vendorId}";
        var cachedRiskJson = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedRiskJson))
        {
            // if cached, deserialize and return the cached value
            return JsonSerializer.Deserialize<RiskAssessment>(cachedRiskJson)!;
        }

        var vendor = await _vendorRepository.GetByIdAsync(vendorId);
        if (vendor == null)
        {
            throw new KeyNotFoundException($"Vendor with ID {vendorId} not found.");
        }

        var matrix = _matrixProvider.GetMatrix();
        var assessment = EvaluateVendorInternal(vendor, matrix);

        // save the risk assessment to Redis cache with an expiration time of 15 minutes
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
        };
        
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(assessment), cacheOptions);

        return assessment;
    }

    public async Task<IEnumerable<RiskAssessment>> GetLeaderboardAsync()
    {
        var vendors = await _vendorRepository.GetAllAsync();
        var matrix = _matrixProvider.GetMatrix();

        var leaderboard = vendors
            .Select(vendor => EvaluateVendorInternal(vendor, matrix))
            .OrderByDescending(assessment => assessment.RiskScore)
            .ToList();

        return leaderboard;
    }

    private RiskAssessment EvaluateVendorInternal(Vendor vendor, RiskFactorMatrix matrix)
    {
        double finalScore = 0;
        var categorizedExplanations = new Dictionary<string, List<string>>();

        foreach (var rule in _rules)
        {
            var result = rule.CalculateRisk(vendor, matrix);

            if (result.Explanations.Any())
            {
                categorizedExplanations[rule.Category] = result.Explanations;
            }

            double weight = _weights.Weights.TryGetValue(rule.Category, out var w) ? w : 0.0;
            finalScore += (result.Score * weight);
        }

        return _factory.Create(vendor.Id, finalScore, categorizedExplanations);
    }
}