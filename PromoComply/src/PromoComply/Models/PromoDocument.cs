namespace PromoComply.Models;

public class PromoDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public FileType FileType { get; set; }

    public DateTime ImportedDate { get; set; }

    public string ExtractedText { get; set; } = string.Empty;

    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;

    public List<PromoClaim> Claims { get; set; } = [];

    public List<ComplianceIssue> ComplianceIssues { get; set; } = [];

    public int OverallScore { get; set; }
}
