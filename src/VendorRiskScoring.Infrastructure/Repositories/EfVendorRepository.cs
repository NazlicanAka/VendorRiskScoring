using Microsoft.EntityFrameworkCore;
using VendorRiskScoring.Application.Interfaces;
using VendorRiskScoring.Domain.Entities;
using VendorRiskScoring.Infrastructure.Data;

namespace VendorRiskScoring.Infrastructure.Repositories;

public class EfVendorRepository : IVendorRepository
{
    private readonly VendorDbContext _context;

    public EfVendorRepository(VendorDbContext context)
    {
        _context = context;
    }

    public async Task<Vendor?> GetByIdAsync(int id)
    {
        return await _context.Vendors.FindAsync(id);
    }

    public async Task<IEnumerable<Vendor>> GetAllAsync()
    {
        return await _context.Vendors.ToListAsync();
    }
    public async Task AddAsync(Vendor vendor)
    {
        await _context.Vendors.AddAsync(vendor);
        await _context.SaveChangesAsync();
    }
}