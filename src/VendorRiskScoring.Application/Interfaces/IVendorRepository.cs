using VendorRiskScoring.Domain.Entities;

namespace VendorRiskScoring.Application.Interfaces;

public interface IVendorRepository
{
    Task<Vendor?> GetByIdAsync(int id);
    Task<IEnumerable<Vendor>> GetAllAsync();
}