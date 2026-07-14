namespace VendorRiskScoring.Domain.Entities;

public class Vendor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int FinancialHealth { get; set; }
    public int SlaUptime { get; set; }
    public int MajorIncidents { get; set; }
    public List<string> SecurityCerts { get; set; } = new();
    public VendorDocuments Documents { get; set; } = new();
}