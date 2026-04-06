namespace PromoComply.Models;

public class ReviewComment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Author { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public CommentType CommentType { get; set; }
}
