using VendorRiskScoring.Domain.Models;

namespace VendorRiskScoring.Application.Interfaces;

public interface IRiskMatrixProvider
{
    RiskFactorMatrix GetMatrix();
}