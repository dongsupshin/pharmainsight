using System.IO;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using PromoComply.Models;
using UglyToad.PdfPig;

namespace PromoComply.Services;

public class DocumentParserService : IDocumentParser
{
    public async Task<string> ParseDocumentAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".pdf" => await ExtractTextFromPdfAsync(filePath),
            ".docx" => await ExtractTextFromDocxAsync(filePath),
            ".pptx" => await ExtractTextFromPptxAsync(filePath),
            _ => throw new NotSupportedException($"File type {extension} is not supported")
        };
    }

    private async Task<string> ExtractTextFromPdfAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();
            using (var document = PdfDocument.Open(filePath))
            {
                foreach (var page in document.GetPages())
                {
                    var text = page.Text;
                    if (!string.IsNullOrEmpty(text))
                    {
                        sb.AppendLine(text);
                    }
                }
            }
            return sb.ToString();
        });
    }

    private async Task<string> ExtractTextFromDocxAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();
            using (var wordDoc = WordprocessingDocument.Open(filePath, false))
            {
                if (wordDoc.MainDocumentPart?.Document?.Body is Body body)
                {
                    foreach (var paragraph in body.Descendants<Paragraph>())
                    {
                        var text = string.Join(string.Empty,
                            paragraph.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>()
                                .Select(t => t.Text));
                        if (!string.IsNullOrEmpty(text))
                        {
                            sb.AppendLine(text);
                        }
                    }
                }
            }
            return sb.ToString();
        });
    }

    private async Task<string> ExtractTextFromPptxAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();
            using (var pptDoc = PresentationDocument.Open(filePath, false))
            {
                if (pptDoc.PresentationPart != null)
                {
                    foreach (var slide in pptDoc.PresentationPart.SlideParts)
                    {
                        if (slide.Slide?.CommonSlideData?.ShapeTree != null)
                        {
                            foreach (var shape in slide.Slide.CommonSlideData.ShapeTree.Descendants<DocumentFormat.OpenXml.Presentation.Shape>())
                            {
                                foreach (var text in shape.Descendants<DocumentFormat.OpenXml.Drawing.Text>())
                                {
                                    if (!string.IsNullOrEmpty(text.Text))
                                    {
                                        sb.AppendLine(text.Text);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return sb.ToString();
        });
    }
}
