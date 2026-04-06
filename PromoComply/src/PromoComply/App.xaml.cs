using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PromoComply.Services;
using PromoComply.ViewModels;

namespace PromoComply;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        services.AddSingleton<IDocumentParser, DocumentParserService>();
        services.AddSingleton<IClaimsDetector, ClaimsDetectorService>();
        services.AddSingleton<IComplianceChecker, ComplianceCheckerService>();
        services.AddSingleton<IReportGenerator, ReportGeneratorService>();
        services.AddSingleton<IProjectRepository, JsonProjectRepository>();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<DocumentReviewViewModel>();

        services.AddSingleton<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
