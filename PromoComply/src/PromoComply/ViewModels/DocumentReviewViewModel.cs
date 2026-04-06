using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PromoComply.Models;

namespace PromoComply.ViewModels;

public partial class DocumentReviewViewModel : ObservableObject
{
    [ObservableProperty]
    private PromoDocument? document;

    [ObservableProperty]
    private ObservableCollection<PromoClaim> claims = [];

    [ObservableProperty]
    private ObservableCollection<ComplianceIssue> complianceIssues = [];

    [ObservableProperty]
    private int complianceScore;

    [ObservableProperty]
    private PromoClaim? selectedClaim;

    [ObservableProperty]
    private ComplianceIssue? selectedIssue;

    [ObservableProperty]
    private string reviewComment = string.Empty;

    public IRelayCommand ApproveClaim { get; }
    public IRelayCommand FlagClaim { get; }
    public IRelayCommand RejectClaim { get; }
    public IRelayCommand AddComment { get; }
    public IRelayCommand ResolveIssue { get; }

    public DocumentReviewViewModel()
    {
        ApproveClaim = new RelayCommand(OnApproveClaim, CanApproveClaim);
        FlagClaim = new RelayCommand(OnFlagClaim, CanFlagClaim);
        RejectClaim = new RelayCommand(OnRejectClaim, CanRejectClaim);
        AddComment = new RelayCommand(OnAddComment, CanAddComment);
        ResolveIssue = new RelayCommand(OnResolveIssue, CanResolveIssue);
    }

    partial void OnDocumentChanged(PromoDocument? oldValue, PromoDocument? newValue)
    {
        if (newValue != null)
        {
            Claims.Clear();
            foreach (var claim in newValue.Claims)
            {
                Claims.Add(claim);
            }

            ComplianceIssues.Clear();
            foreach (var issue in newValue.ComplianceIssues)
            {
                ComplianceIssues.Add(issue);
            }

            ComplianceScore = newValue.OverallScore;
        }
    }

    private void OnApproveClaim()
    {
        if (SelectedClaim != null)
        {
            SelectedClaim.IsApproved = true;
            OnPropertyChanged(nameof(SelectedClaim));
        }
    }

    private void OnFlagClaim()
    {
        if (SelectedClaim != null)
        {
            SelectedClaim.IsApproved = false;
            OnPropertyChanged(nameof(SelectedClaim));
        }
    }

    private void OnRejectClaim()
    {
        if (SelectedClaim != null)
        {
            SelectedClaim.IsApproved = null;
            OnPropertyChanged(nameof(SelectedClaim));
        }
    }

    private void OnAddComment()
    {
        if (SelectedClaim != null && !string.IsNullOrEmpty(ReviewComment))
        {
            SelectedClaim.ReviewNotes = ReviewComment;
            ReviewComment = string.Empty;
            OnPropertyChanged(nameof(SelectedClaim));
        }
    }

    private void OnResolveIssue()
    {
        if (SelectedIssue != null)
        {
            SelectedIssue.IsResolved = !SelectedIssue.IsResolved;
            OnPropertyChanged(nameof(SelectedIssue));
        }
    }

    private bool CanApproveClaim()
    {
        return SelectedClaim != null;
    }

    private bool CanFlagClaim()
    {
        return SelectedClaim != null;
    }

    private bool CanRejectClaim()
    {
        return SelectedClaim != null;
    }

    private bool CanAddComment()
    {
        return SelectedClaim != null && !string.IsNullOrEmpty(ReviewComment);
    }

    private bool CanResolveIssue()
    {
        return SelectedIssue != null;
    }
}
