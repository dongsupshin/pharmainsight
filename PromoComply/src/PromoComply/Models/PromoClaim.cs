namespace PromoComply.Models;

public class PromoClaim
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Text { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public ClaimType ClaimType { get; set; }

    public bool HasReference { get; set; }

    public string ReferenceText { get; set; } = string.Empty;

    public RiskLevel RiskLevel { get; set; }

    public bool? IsApproved { get; set; }

    public string ReviewNotes { get; set; } = string.Empty;
}
