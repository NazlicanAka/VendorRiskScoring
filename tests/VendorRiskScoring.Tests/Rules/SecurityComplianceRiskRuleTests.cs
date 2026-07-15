using Xunit;
using VendorRiskScoring.Application.Rules;
using VendorRiskScoring.Domain.Entities;
using VendorRiskScoring.Domain.Models;

namespace VendorRiskScoring.Tests.Rules;

public class SecurityComplianceRiskRuleTests
{
    private readonly SecurityComplianceRiskRule _rule;

    public SecurityComplianceRiskRuleTests()
    {
        _rule = new SecurityComplianceRiskRule();
    }

    [Fact]
    public void Category_ShouldReturnSecurityCompliance()
    {
        Assert.Equal("SecurityCompliance", _rule.Category);
    }

    [Fact]
    public void CalculateRisk_AllChecksPassed_ReturnsZeroScore()
    {
        var vendor = new Vendor 
        { 
            SecurityCerts = new List<string> { "ISO27001" },
            Documents = new VendorDocuments 
            { 
                PentestReportValid = true, 
                PrivacyPolicyValid = true, 
                ContractValid = true 
            }
        };
        var matrix = new RiskFactorMatrix();

        var result = _rule.CalculateRisk(vendor, matrix);

        Assert.Equal(0.0, result.Score);
        Assert.Contains("All security and compliance checks passed.", result.Explanations);
    }

    [Fact]
    public void CalculateRisk_MissingISO27001_ReturnsCalculatedScore()
    {
        var vendor = new Vendor 
        { 
            SecurityCerts = new List<string>(),
            Documents = new VendorDocuments { PentestReportValid = true, PrivacyPolicyValid = true, ContractValid = true }
        };
        var matrix = new RiskFactorMatrix
        {
            SecurityRisk = new Dictionary<string, Dictionary<string, double>>
            {
                { "missingISO27001", new Dictionary<string, double> { { "risk1", 0.8 } } }
            }
        };

        var result = _rule.CalculateRisk(vendor, matrix);

        Assert.Equal(0.8, result.Score);
        Assert.Contains("Missing ISO27001.", result.Explanations);
    }

    [Fact]
    public void CalculateRisk_MissingPentest_ReturnsCalculatedScore()
    {
        var vendor = new Vendor 
        { 
            SecurityCerts = new List<string> { "ISO27001" },
            Documents = new VendorDocuments { PentestReportValid = false, PrivacyPolicyValid = true, ContractValid = true }
        };
        var matrix = new RiskFactorMatrix
        {
            SecurityRisk = new Dictionary<string, Dictionary<string, double>>
            {
                { "failedPenTest", new Dictionary<string, double> { { "risk1", 0.9 } } }
            }
        };

        var result = _rule.CalculateRisk(vendor, matrix);

        Assert.Equal(0.9, result.Score);
        Assert.Contains("No Pentest Report.", result.Explanations);
    }

    [Fact]
    public void CalculateRisk_ExpiredPrivacyPolicy_ReturnsCalculatedScore()
    {
        var vendor = new Vendor 
        { 
            SecurityCerts = new List<string> { "ISO27001" },
            Documents = new VendorDocuments { PentestReportValid = true, PrivacyPolicyValid = false, ContractValid = true }
        };
        var matrix = new RiskFactorMatrix
        {
            ComplianceRisk = new Dictionary<string, Dictionary<string, double>>
            {
                { "expiredPrivacyPolicy", new Dictionary<string, double> { { "risk1", 0.5 } } }
            }
        };

        var result = _rule.CalculateRisk(vendor, matrix);

        Assert.Equal(0.5, result.Score);
        Assert.Contains("Expired Privacy Policy.", result.Explanations);
    }

    [Fact]
    public void CalculateRisk_ExpiredContract_ReturnsCalculatedScore()
    {
        var vendor = new Vendor 
        { 
            SecurityCerts = new List<string> { "ISO27001" },
            Documents = new VendorDocuments { PentestReportValid = true, PrivacyPolicyValid = true, ContractValid = false }
        };
        var matrix = new RiskFactorMatrix
        {
            ComplianceRisk = new Dictionary<string, Dictionary<string, double>>
            {
                { "expiredContract", new Dictionary<string, double> { { "risk1", 1.0 } } }
            }
        };

        var result = _rule.CalculateRisk(vendor, matrix);

        Assert.Equal(1.0, result.Score);
        Assert.Contains("Expired Contract.", result.Explanations);
    }

    [Fact]
    public void CalculateRisk_MultipleFailures_ReturnsAverageScore()
    {
        var vendor = new Vendor 
        { 
            SecurityCerts = new List<string>(),
            Documents = new VendorDocuments { PentestReportValid = false, PrivacyPolicyValid = true, ContractValid = false }
        };
        var matrix = new RiskFactorMatrix
        {
            SecurityRisk = new Dictionary<string, Dictionary<string, double>>
            {
                { "missingISO27001", new Dictionary<string, double> { { "risk1", 0.8 } } },
                { "failedPenTest", new Dictionary<string, double> { { "risk1", 0.6 } } }
            },
            ComplianceRisk = new Dictionary<string, Dictionary<string, double>>
            {
                { "expiredContract", new Dictionary<string, double> { { "risk1", 1.0 } } }
            }
        };

        var result = _rule.CalculateRisk(vendor, matrix);

        Assert.Equal(0.8, result.Score, 5);
        Assert.Contains("Missing ISO27001.", result.Explanations);
        Assert.Contains("No Pentest Report.", result.Explanations);
        Assert.Contains("Expired Contract.", result.Explanations);
    }

    [Fact]
    public void CalculateRisk_MissingMatrixData_ReturnsZeroScoreWithExplanations()
    {
        var vendor = new Vendor 
        { 
            SecurityCerts = new List<string>(),
            Documents = new VendorDocuments { PentestReportValid = false, PrivacyPolicyValid = false, ContractValid = false }
        };
        var matrix = new RiskFactorMatrix
        {
            SecurityRisk = new Dictionary<string, Dictionary<string, double>>(),
            ComplianceRisk = new Dictionary<string, Dictionary<string, double>>()
        };

        var result = _rule.CalculateRisk(vendor, matrix);

        Assert.Equal(0.0, result.Score);
        Assert.Contains("All security and compliance checks passed.", result.Explanations);
    }
}