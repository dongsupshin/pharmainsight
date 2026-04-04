using System.Windows;
using MedSearchApp.ViewModels;

namespace MedSearchApp.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
