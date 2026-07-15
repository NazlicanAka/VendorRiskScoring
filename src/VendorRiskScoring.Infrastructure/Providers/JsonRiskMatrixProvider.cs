using System.Text.Json;
using Microsoft.Extensions.Options;
using VendorRiskScoring.Application.Configuration;
using VendorRiskScoring.Application.Interfaces;
using VendorRiskScoring.Domain.Models;

namespace VendorRiskScoring.Infrastructure.Providers;

public class JsonRiskMatrixProvider : IRiskMatrixProvider
{
    private readonly RiskFactorMatrix _matrix;

    public JsonRiskMatrixProvider(IOptions<FilePathsOptions> filePaths)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, filePaths.Value.MatrixData);
        
        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _matrix = JsonSerializer.Deserialize<RiskFactorMatrix>(json, options) ?? new RiskFactorMatrix();
        }
        else
        {
            _matrix = new RiskFactorMatrix();
        }
    }

    public RiskFactorMatrix GetMatrix()
    {
        return _matrix;
    }
}