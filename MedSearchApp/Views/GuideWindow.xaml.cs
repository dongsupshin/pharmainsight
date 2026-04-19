using System.Windows;
using MedSearchApp.ViewModels;

namespace MedSearchApp.Views;

public partial class GuideWindow : Window
{
    private readonly MainViewModel _vm;

    public GuideWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _vm = viewModel;
    }

    private void BtnRunDemo_Click(object sender, RoutedEventArgs e)
    {
        _vm.TriggerDemoSearch();
        Close();
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
