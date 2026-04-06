using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PromoComply.Models;
using PromoComply.Services;

namespace PromoComply.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDocumentParser _documentParser;
    private readonly IClaimsDetector _claimsDetector;
    private readonly IComplianceChecker _complianceChecker;
    private readonly IReportGenerator _reportGenerator;

    [ObservableProperty]
    private ObservableCollection<PromoDocument> documents = [];

    [ObservableProperty]
    private PromoDocument? selectedDocument;

    [ObservableProperty]
    private bool isAnalyzing;

    [ObservableProperty]
    private string analyzeProgressText = string.Empty;

    public IAsyncRelayCommand ImportDocumentCommand { get; }
    public IAsyncRelayCommand AnalyzeDocumentCommand { get; }
    public IAsyncRelayCommand ExportReportCommand { get; }
    public IRelayCommand RemoveDocumentCommand { get; }

    public MainViewModel(
        IDocumentParser documentParser,
        IClaimsDetector claimsDetector,
        IComplianceChecker complianceChecker,
        IReportGenerator reportGenerator)
    {
        _documentParser = documentParser;
        _claimsDetector = claimsDetector;
        _complianceChecker = complianceChecker;
        _reportGenerator = reportGenerator;

        ImportDocumentCommand = new AsyncRelayCommand(ImportDocumentAsync);
        AnalyzeDocumentCommand = new AsyncRelayCommand(AnalyzeDocumentAsync, CanAnalyzeDocument);
        ExportReportCommand = new AsyncRelayCommand(ExportReportAsync, CanExportReport);
        RemoveDocumentCommand = new RelayCommand(RemoveDocument, CanRemoveDocument);
    }

    private async Task ImportDocumentAsync()
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Supported Files|*.pdf;*.docx;*.pptx|PDF Files|*.pdf|Word Documents|*.docx|PowerPoint Presentations|*.pptx|All Files|*.*"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                IsAnalyzing = true;
                AnalyzeProgressText = "Importing document...";

                var document = new PromoDocument
                {
                    FileName = System.IO.Path.GetFileName(openFileDialog.FileName),
                    FilePath = openFileDialog.FileName,
                    ImportedDate = DateTime.Now,
                    FileType = GetFileType(openFileDialog.FileName)
                };

                Documents.Add(document);
                SelectedDocument = document;

                AnalyzeProgressText = "Document imported successfully";
                IsAnalyzing = false;
            }
            catch (Exception ex)
            {
                AnalyzeProgressText = $"Error importing document: {ex.Message}";
                IsAnalyzing = false;
            }
        }
    }

    private async Task AnalyzeDocumentAsync()
    {
        if (SelectedDocument == null)
            return;

        try
        {
            IsAnalyzing = true;
            AnalyzeProgressText = "Parsing document...";

            var text = await _documentParser.ParseDocumentAsync(SelectedDocument.FilePath);
            SelectedDocument.ExtractedText = text;

            AnalyzeProgressText = "Detecting claims...";
            var claims = _claimsDetector.DetectClaims(text);
            SelectedDocument.Claims = claims;

            AnalyzeProgressText = "Checking compliance...";
            var issues = _complianceChecker.CheckCompliance(SelectedDocument);
            SelectedDocument.ComplianceIssues = issues;

            var score = _complianceChecker.CalculateComplianceScore(SelectedDocument);
            SelectedDocument.OverallScore = score;

            SelectedDocument.Status = score >= 80 ? DocumentStatus.Approved :
                                      score >= 60 ? DocumentStatus.Reviewed :
                                      DocumentStatus.Flagged;

            AnalyzeProgressText = "Analysis complete";
            IsAnalyzing = false;

            ((AsyncRelayCommand)AnalyzeDocumentCommand).NotifyCanExecuteChanged();
            ((AsyncRelayCommand)ExportReportCommand).NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            AnalyzeProgressText = $"Error analyzing document: {ex.Message}";
            IsAnalyzing = false;
        }
    }

    private async Task ExportReportAsync()
    {
        if (SelectedDocument == null)
            return;

        var saveFileDialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Text Reports|*.txt|CSV Reports|*.csv",
            FileName = $"{System.IO.Path.GetFileNameWithoutExtension(SelectedDocument.FileName)}_Report"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                if (saveFileDialog.FileName.EndsWith(".csv"))
                {
                    var csvData = _reportGenerator.GenerateExcelReport(SelectedDocument);
                    await System.IO.File.WriteAllBytesAsync(saveFileDialog.FileName, csvData);
                }
                else
                {
                    var textReport = _reportGenerator.GenerateTextReport(SelectedDocument);
                    await System.IO.File.WriteAllTextAsync(saveFileDialog.FileName, textReport);
                }

                AnalyzeProgressText = $"Report exported to {System.IO.Path.GetFileName(saveFileDialog.FileName)}";
            }
            catch (Exception ex)
            {
                AnalyzeProgressText = $"Error exporting report: {ex.Message}";
            }
        }
    }

    private void RemoveDocument()
    {
        if (SelectedDocument != null && Documents.Contains(SelectedDocument))
        {
            Documents.Remove(SelectedDocument);
            SelectedDocument = Documents.FirstOrDefault();
            ((RelayCommand)RemoveDocumentCommand).NotifyCanExecuteChanged();
        }
    }

    private bool CanAnalyzeDocument()
    {
        return SelectedDocument != null && !IsAnalyzing && !string.IsNullOrEmpty(SelectedDocument.FilePath);
    }

    private bool CanExportReport()
    {
        return SelectedDocument != null && !IsAnalyzing && SelectedDocument.OverallScore > 0;
    }

    private bool CanRemoveDocument()
    {
        return SelectedDocument != null && Documents.Count > 0;
    }

    private static FileType GetFileType(string filePath)
    {
        var extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => FileType.PDF,
            ".docx" => FileType.DOCX,
            ".pptx" => FileType.PPTX,
            _ => throw new NotSupportedException($"File type {extension} is not supported")
        };
    }
}
