namespace PromoComply.Models;

public class ComplianceIssue
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ComplianceCategory Category { get; set; }

    public IssueSeverity Severity { get; set; }

    public string Location { get; set; } = string.Empty;

    public string Recommendation { get; set; } = string.Empty;

    public bool IsResolved { get; set; }
}
