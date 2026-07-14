using Microsoft.EntityFrameworkCore;
using VendorRiskScoring.Domain.Entities;

namespace VendorRiskScoring.Infrastructure.Data;

public class VendorDbContext : DbContext
{
    public VendorDbContext(DbContextOptions<VendorDbContext> options) : base(options)
    {
    }

    public DbSet<Vendor> Vendors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // PostgreSQL, List<string> tipini otomatik olarak text[] (array) tipine çevirir.
            
            // Documents alt nesnesini ayrı bir tablo yapmak yerine aynı tabloya kolon olarak gömüyoruz (Owned Entity Pattern)
            entity.OwnsOne(e => e.Documents, doc =>
            {
                doc.Property(d => d.ContractValid).HasColumnName("Document_ContractValid");
                doc.Property(d => d.PrivacyPolicyValid).HasColumnName("Document_PrivacyPolicyValid");
                doc.Property(d => d.PentestReportValid).HasColumnName("Document_PentestReportValid");
            });
        });
    }
}