using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PromoComply.Models;

namespace PromoComply.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty]
    private int totalDocuments;

    [ObservableProperty]
    private int reviewedCount;

    [ObservableProperty]
    private int flaggedCount;

    [ObservableProperty]
    private double averageScore;

    [ObservableProperty]
    private ObservableCollection<DocumentActivity> recentActivity = [];

    [ObservableProperty]
    private ObservableCollection<ScoreDistribution> scoreDistribution = [];

    [ObservableProperty]
    private int criticalIssuesCount;

    [ObservableProperty]
    private int majorIssuesCount;

    public void UpdateMetrics(ObservableCollection<PromoDocument> documents)
    {
        TotalDocuments = documents.Count;
        ReviewedCount = documents.Count(d => d.Status == DocumentStatus.Reviewed || d.Status == DocumentStatus.Approved);
        FlaggedCount = documents.Count(d => d.Status == DocumentStatus.Flagged);

        if (documents.Count > 0)
        {
            AverageScore = documents.Average(d => d.OverallScore);
        }
        else
        {
            AverageScore = 0;
        }

        CriticalIssuesCount = documents.Sum(d => d.ComplianceIssues.Count(i => i.Severity == IssueSeverity.Critical));
        MajorIssuesCount = documents.Sum(d => d.ComplianceIssues.Count(i => i.Severity == IssueSeverity.Major));

        UpdateRecentActivity(documents);
        UpdateScoreDistribution(documents);
    }

    private void UpdateRecentActivity(ObservableCollection<PromoDocument> documents)
    {
        RecentActivity.Clear();

        var activities = documents
            .OrderByDescending(d => d.ImportedDate)
            .Take(5)
            .Select(d => new DocumentActivity
            {
                DocumentName = d.FileName,
                Action = $"Imported ({d.FileType})",
                Timestamp = d.ImportedDate,
                Status = d.Status.ToString()
            })
            .ToList();

        foreach (var activity in activities)
        {
            RecentActivity.Add(activity);
        }
    }

    private void UpdateScoreDistribution(ObservableCollection<PromoDocument> documents)
    {
        ScoreDistribution.Clear();

        if (documents.Count == 0)
            return;

        var excellent = documents.Count(d => d.OverallScore >= 90);
        var good = documents.Count(d => d.OverallScore >= 75 && d.OverallScore < 90);
        var fair = documents.Count(d => d.OverallScore >= 60 && d.OverallScore < 75);
        var poor = documents.Count(d => d.OverallScore < 60);

        if (excellent > 0)
            ScoreDistribution.Add(new ScoreDistribution { Range = "90-100", Count = excellent, Color = "#2E8B57" });
        if (good > 0)
            ScoreDistribution.Add(new ScoreDistribution { Range = "75-89", Count = good, Color = "#5FA8D3" });
        if (fair > 0)
            ScoreDistribution.Add(new ScoreDistribution { Range = "60-74", Count = fair, Color = "#E8871E" });
        if (poor > 0)
            ScoreDistribution.Add(new ScoreDistribution { Range = "<60", Count = poor, Color = "#C1292E" });
    }
}

public class DocumentActivity
{
    public string DocumentName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ScoreDistribution
{
    public string Range { get; set; } = string.Empty;
    public int Count { get; set; }
    public string Color { get; set; } = string.Empty;
}
