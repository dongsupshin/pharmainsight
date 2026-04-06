namespace PromoComply.Models;

public class ReviewSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DocumentId { get; set; }

    public DateTime CreatedDate { get; set; }

    public string ReviewerName { get; set; } = string.Empty;

    public ReviewStage ReviewStage { get; set; }

    public List<ReviewComment> Comments { get; set; } = [];

    public ReviewStatus Status { get; set; }

    public DateTime? CompletedDate { get; set; }
}
