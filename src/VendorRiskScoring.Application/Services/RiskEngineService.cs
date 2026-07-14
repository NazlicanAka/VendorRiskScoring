using Microsoft.Extensions.Options;
using VendorRiskScoring.Application.Configuration;
using VendorRiskScoring.Application.Factories;
using VendorRiskScoring.Application.Interfaces;
using VendorRiskScoring.Domain.Entities;

namespace VendorRiskScoring.Application.Services;

public class RiskEngineService : IRiskEngineService
{
    private readonly IEnumerable<IRiskRule> _rules;
    private readonly IVendorRepository _vendorRepository;
    private readonly IRiskMatrixProvider _matrixProvider;
    private readonly RiskWeightsOptions _weights;
    private readonly IRiskAssessmentFactory _factory;

    public RiskEngineService(
        IEnumerable<IRiskRule> rules,
        IVendorRepository vendorRepository,
        IRiskMatrixProvider matrixProvider,
        IOptions<RiskWeightsOptions> weights,
        IRiskAssessmentFactory factory)
    {
        _rules = rules;
        _vendorRepository = vendorRepository;
        _matrixProvider = matrixProvider;
        _weights = weights.Value;
        _factory = factory;
    }

    public async Task<RiskAssessment> EvaluateVendorAsync(int vendorId)
    {
        var vendor = await _vendorRepository.GetByIdAsync(vendorId);
        if (vendor == null)
        {
            throw new KeyNotFoundException($"Vendor with ID {vendorId} not found.");
        }

        // Matrisi Provider'dan çekiyoruz ve 'matrix' adlı yerel değişkene atıyoruz
        var matrix = _matrixProvider.GetMatrix();

        double finalScore = 0;
        var allExplanations = new List<string>();

        foreach (var rule in _rules)
        {
            // BURASI DÜZELDİ: _matrix yerine matrix kullanıyoruz
            var result = rule.CalculateRisk(vendor, matrix); 
            
            if (result.Explanations.Any())
            {
                allExplanations.AddRange(result.Explanations);
            }

            double weight = _weights.Weights.TryGetValue(rule.Category, out var w) ? w : 0.0;
            finalScore += (result.Score * weight);
        }

        return _factory.Create(vendor.Id, finalScore, allExplanations);
    }
}