namespace VendorRiskScoring.Application.Configuration;

public class FilePathsOptions
{
    public const string SectionName = "FilePaths";
    public string VendorData { get; set; } = string.Empty;
    public string MatrixData { get; set; } = string.Empty;
}