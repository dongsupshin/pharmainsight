using PromoComply.Models;

namespace PromoComply.Services;

public interface IProjectRepository
{
    Task SaveReviewSessionAsync(ReviewSession session);
    Task<ReviewSession?> LoadReviewSessionAsync(Guid sessionId);
    Task<List<ReviewSession>> LoadAllReviewSessionsAsync();
    Task DeleteReviewSessionAsync(Guid sessionId);
}
