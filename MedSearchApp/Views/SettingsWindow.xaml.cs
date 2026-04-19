using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using MedSearchApp.Services;

namespace MedSearchApp.Views;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;

    public SettingsWindow(AppSettings settings)
    {
        InitializeComponent();
        _settings = settings;

        // 기존 키 표시
        HiraKeyBox.Text = settings.HiraApiKey;
        BiznoKeyBox.Text = settings.BiznoApiKey;
        ProKeyBox.Text = settings.ProLicenseKey;

        // 배지 업데이트
        HiraKeyBox.TextChanged += (_, _) => UpdateHiraBadge();
        BiznoKeyBox.TextChanged += (_, _) => UpdateBiznoBadge();
        ProKeyBox.TextChanged += (_, _) => UpdateProBadge();
        UpdateHiraBadge();
        UpdateBiznoBadge();
        UpdateProBadge();

        // 데이터 파일 상태 업데이트
        UpdateDataStatus();
    }

    private void UpdateProBadge()
    {
        var tempSettings = new AppSettings { ProLicenseKey = ProKeyBox.Text.Trim() };
        if (tempSettings.IsPro)
        {
            ProBadge.Background  = Brush("#D1E7DD");
            ProBadge.BorderBrush = Brush("#198754");
            ProBadgeText.Text       = "★ PRO 활성";
            ProBadgeText.Foreground = Brush("#0A3622");
        }
        else
        {
            ProBadge.Background  = Brush("#FFF3CD");
            ProBadge.BorderBrush = Brush("#FFCA2C");
            ProBadgeText.Text       = "FREE";
            ProBadgeText.Foreground = Brush("#856404");
        }
    }

    private void UpdateHiraBadge()
    {
        var hasKey = !string.IsNullOrWhiteSpace(HiraKeyBox.Text);
        if (hasKey)
        {
            HiraBadge.Background  = Brush("#D1E7DD");
            HiraBadge.BorderBrush = Brush("#198754");
            HiraBadgeText.Text       = "키 등록됨";
            HiraBadgeText.Foreground = Brush("#0A3622");
        }
        else
        {
            HiraBadge.Background  = Brush("#FFF3CD");
            HiraBadge.BorderBrush = Brush("#FFCA2C");
            HiraBadgeText.Text       = "키 없음";
            HiraBadgeText.Foreground = Brush("#856404");
        }
    }

    private void UpdateDataStatus()
    {
        var dataPath = _settings.ResolveDataPath();

        if (File.Exists(dataPath))
        {
            var fi = new FileInfo(dataPath);
            var sizeMb = fi.Length / 1024.0 / 1024.0;
            DataStatusText.Text = $"데이터 로드됨  |  {sizeMb:F1} MB  |  {fi.LastWriteTime:yyyy-MM-dd}  |  {fi.Name}";
            DataStatusText.Foreground = Brush("#198754");

            DataBadge.Background     = Brush("#D1E7DD");
            DataBadge.BorderBrush    = Brush("#198754");
            DataBadgeText.Text       = "설치됨";
            DataBadgeText.Foreground = Brush("#0A3622");
        }
        else
        {
            DataStatusText.Text = "데이터 파일이 없습니다. 아래 버튼으로 다운로드 후 파일을 선택해주세요.";
            DataStatusText.Foreground = Brush("#888888");

            DataBadge.Background     = Brush("#FFF3CD");
            DataBadge.BorderBrush    = Brush("#FFCA2C");
            DataBadgeText.Text       = "미설치";
            DataBadgeText.Foreground = Brush("#856404");
        }
    }

    private void UpdateBiznoBadge()
    {
        var hasKey = !string.IsNullOrWhiteSpace(BiznoKeyBox.Text);
        if (hasKey)
        {
            BiznoBadge.Background  = Brush("#D1E7DD");
            BiznoBadge.BorderBrush = Brush("#198754");
            BiznoBadgeText.Text       = "키 등록됨";
            BiznoBadgeText.Foreground = Brush("#0A3622");
        }
        else
        {
            BiznoBadge.Background  = Brush("#FFF3CD");
            BiznoBadge.BorderBrush = Brush("#FFCA2C");
            BiznoBadgeText.Text       = "키 없음";
            BiznoBadgeText.Foreground = Brush("#856404");
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _settings.HiraApiKey = HiraKeyBox.Text.Trim();
        _settings.BiznoApiKey = BiznoKeyBox.Text.Trim();
        _settings.ProLicenseKey = ProKeyBox.Text.Trim();
        _settings.Save();

        var proMsg = _settings.IsPro
            ? "\n★ PRO 기능이 활성화되었습니다."
            : "";
        MessageBox.Show($"설정이 저장되었습니다.{proMsg}",
                        "저장 완료", MessageBoxButton.OK, MessageBoxImage.Information);

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void OpenHiraPage_Click(object sender, RoutedEventArgs e)
        => Process.Start(new ProcessStartInfo(
            "https://www.data.go.kr/data/15001698/openapi.do")
            { UseShellExecute = true });

    private void OpenBiznoPage_Click(object sender, RoutedEventArgs e)
        => Process.Start(new ProcessStartInfo(
            "https://api.bizno.net/")
            { UseShellExecute = true });

    /// <summary>HIRA 공공데이터 다운로드 페이지 열기</summary>
    private void OpenHiraDownloadPage_Click(object sender, RoutedEventArgs e)
        => Process.Start(new ProcessStartInfo(
            _settings.HiraDownloadUrl)
            { UseShellExecute = true });

    /// <summary>데이터 파일 선택 (ZIP/XLSX/CSV)</summary>
    private void PickDataFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title  = "사업자번호 데이터 파일 선택",
            Filter = "ZIP/XLSX/CSV 파일|*.zip;*.xlsx;*.csv|ZIP 파일 (*.zip)|*.zip|XLSX 파일 (*.xlsx)|*.xlsx|CSV 파일 (*.csv)|*.csv|모든 파일 (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            var ext = Path.GetExtension(dialog.FileName).ToLowerInvariant();
            string destPath;

            if (ext == ".zip")
            {
                // ZIP → XLSX/CSV 추출
                var extracted = CsvDataService.ExtractDataFromZip(dialog.FileName);
                if (extracted == null)
                {
                    MessageBox.Show("ZIP 파일 내 XLSX 또는 CSV 파일을 찾을 수 없습니다.",
                        "설치 실패", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                destPath = extracted;
            }
            else
            {
                // XLSX/CSV 직접 복사
                var destDir = AppSettings.DefaultDataDir;
                if (!Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);

                destPath = Path.Combine(destDir, $"bizno_data{ext}");
                File.Copy(dialog.FileName, destPath, overwrite: true);
            }

            _settings.CsvFilePath = destPath;
            _settings.Save();

            UpdateDataStatus();

            var fi = new FileInfo(destPath);
            MessageBox.Show(
                $"데이터 파일이 설치되었습니다!\n\n" +
                $"원본: {dialog.FileName}\n" +
                $"저장: {destPath}\n" +
                $"크기: {fi.Length / 1024.0 / 1024.0:F1} MB\n\n" +
                $"앱을 재시작하면 사업자번호 검색이 가능합니다.",
                "설치 완료", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"파일 설치 실패: {ex.Message}",
                "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static SolidColorBrush Brush(string hex)
        => new((Color)ColorConverter.ConvertFromString(hex));
}
