using PromoComply.Models;

namespace PromoComply.Services;

public interface IClaimsDetector
{
    List<PromoClaim> DetectClaims(string text);
}
