namespace PromoComply.Services;

public interface IDocumentParser
{
    Task<string> ParseDocumentAsync(string filePath);
}
