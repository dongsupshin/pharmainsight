using System.Windows;
using PromoComply.ViewModels;
using PromoComply.Views;

namespace PromoComply;

public partial class MainWindow : Window
{
    private readonly MainViewModel _mainViewModel;
    private readonly DashboardViewModel _dashboardViewModel;
    private readonly DocumentReviewViewModel _reviewViewModel;

    public MainWindow(
        MainViewModel mainViewModel,
        DashboardViewModel dashboardViewModel,
        DocumentReviewViewModel reviewViewModel)
    {
        InitializeComponent();

        _mainViewModel = mainViewModel;
        _dashboardViewModel = dashboardViewModel;
        _reviewViewModel = reviewViewModel;

        DataContext = _mainViewModel;
    }

    private void DashboardBtn_Click(object sender, RoutedEventArgs e)
    {
        _dashboardViewModel.UpdateMetrics(_mainViewModel.Documents);
        var dashboardView = new DashboardView { DataContext = _dashboardViewModel };
        ContentControl.Content = dashboardView;
    }

    private void DocumentsBtn_Click(object sender, RoutedEventArgs e)
    {
        var documentListView = new DocumentListView { DataContext = _mainViewModel };
        ContentControl.Content = documentListView;
    }

    private void ReviewBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_mainViewModel.SelectedDocument != null)
        {
            _reviewViewModel.Document = _mainViewModel.SelectedDocument;
            var reviewView = new DocumentReviewView { DataContext = _reviewViewModel };
            ContentControl.Content = reviewView;
        }
        else
        {
            MessageBox.Show("Please select a document to review.", "No Document Selected", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void ReportsBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_mainViewModel.SelectedDocument != null)
        {
            var reportView = new ReportView { DataContext = _mainViewModel };
            ContentControl.Content = reportView;
        }
        else
        {
            MessageBox.Show("Please select a document to generate a report.", "No Document Selected", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
