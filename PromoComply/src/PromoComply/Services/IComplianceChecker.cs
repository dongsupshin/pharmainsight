using PromoComply.Models;

namespace PromoComply.Services;

public interface IComplianceChecker
{
    List<ComplianceIssue> CheckCompliance(PromoDocument document);
    int CalculateComplianceScore(PromoDocument document);
}
