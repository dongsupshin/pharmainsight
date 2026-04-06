using PromoComply.Models;

namespace PromoComply.Services;

public interface IReportGenerator
{
    string GenerateTextReport(PromoDocument document);
    byte[] GenerateExcelReport(PromoDocument document);
}
