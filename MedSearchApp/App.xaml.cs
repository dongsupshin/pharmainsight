using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using MedSearchApp.Services;
using MedSearchApp.ViewModels;
using MedSearchApp.Views;

namespace MedSearchApp;

public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        var settings = AppSettings.Load();

        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 5
        };
        var httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        httpClient.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

        // 데이터 파일 경로 결정
        var dataPath = settings.ResolveDataPath();

        // ── 데이터 파일 없으면 다운로드 안내 ──
        if (!File.Exists(dataPath))
        {
            dataPath = ShowDataDownloadGuide(settings);
        }

        // 검색 서비스 우선순위:
        //   HIRA 키 있음 → HIRA 우선 (의사수/간호사수/등급 등 B2B Pro 필수 데이터)
        //                    → LocalData 폴백 (사업자번호는 bizno.net으로 enrichment)
        //   HIRA 키 없음 → LocalData 우선 (오프라인 동작)
        //   둘 다 없음 → 샘플 데이터 (설치 즉시 사용)
        var csvService    = new CsvDataService(dataPath);
        var hiraService   = new HiraApiService(httpClient, settings.HiraApiKey);
        var sampleService = new SampleDataService();

        var services = new List<ISearchService>();
        if (!string.IsNullOrWhiteSpace(settings.HiraApiKey))
        {
            // HIRA 우선: Pro 필터용 의사수/등급 데이터 확보
            services.Add(hiraService);
            services.Add(csvService);
        }
        else
        {
            // HIRA 키 없음: 로컬 데이터 먼저
            services.Add(csvService);
            services.Add(hiraService);
        }
        services.Add(sampleService);

        // Bizno API (사업자번호 자동 조회)
        var biznoService  = new BiznoApiService(httpClient, settings.BiznoApiKey);

        var orchestrator = new SearchOrchestrator(services);
        var viewModel    = new MainViewModel(orchestrator, settings, biznoService);
        var window       = new MainWindow(viewModel);

        viewModel.OpenSettingsAction = () =>
        {
            var settingsWindow = new SettingsWindow(AppSettings.Load());
            if (settingsWindow.ShowDialog() == true)
            {
                // 설정 저장 후 bizno API 키 + Pro 라이선스 업데이트 (재시작 없이 적용)
                var updated = AppSettings.Load();
                biznoService.UpdateApiKey(updated.BiznoApiKey);

                // ViewModel이 참조 중인 settings를 업데이트하고 Pro 상태 재확인
                settings.ProLicenseKey = updated.ProLicenseKey;
                settings.HiraApiKey    = updated.HiraApiKey;
                settings.BiznoApiKey   = updated.BiznoApiKey;
                settings.CsvFilePath   = updated.CsvFilePath;
                viewModel.RefreshProStatus();
            }
        };

        viewModel.OpenGuideAction = () =>
        {
            var guideWindow = new GuideWindow(viewModel) { Owner = window };
            guideWindow.ShowDialog();
        };

        window.Show();
    }

    /// <summary>
    /// 사업자번호 데이터 파일이 없을 때 다운로드 안내 다이얼로그 표시.
    /// 반환: 설치된 데이터 파일 경로
    /// </summary>
    private string ShowDataDownloadGuide(AppSettings settings)
    {
        var result = MessageBox.Show(
            "사업자번호 조회를 위한 데이터 파일이 없습니다.\n\n" +
            "「건강보험심사평가원 — 전국 병의원 및 약국 현황」\n" +
            "ZIP 파일을 다운로드한 뒤, 파일을 선택해주세요.\n\n" +
            "  [예]  →  다운로드 페이지 열기 + 파일 선택\n" +
            "  [아니오]  →  나중에 설정에서 설치",
            "데이터 파일 필요",
            MessageBoxButton.YesNo,
            MessageBoxImage.Information);

        if (result != MessageBoxResult.Yes)
            return settings.ResolveDataPath();

        // HIRA 다운로드 페이지 열기
        try
        {
            Process.Start(new ProcessStartInfo(settings.HiraDownloadUrl)
                { UseShellExecute = true });
        }
        catch { }

        MessageBox.Show(
            "브라우저에서 다운로드 페이지가 열렸습니다.\n\n" +
            "1.  가장 최신 날짜의 ZIP 파일을 클릭하여 다운로드\n" +
            "     (예: 전국 병의원 및 약국 현황 2025.12.zip)\n\n" +
            "2.  다운로드 완료 후 [확인]을 누르세요.\n" +
            "     파일 선택 창이 나타납니다.",
            "다운로드 안내",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        // 파일 선택 다이얼로그
        return PickAndInstallDataFile(settings);
    }

    /// <summary>파일 선택 다이얼로그 → 데이터 설치</summary>
    private string PickAndInstallDataFile(AppSettings settings)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title  = "다운로드한 데이터 파일 선택",
            Filter = "ZIP/XLSX/CSV 파일|*.zip;*.xlsx;*.csv|ZIP 파일 (*.zip)|*.zip|XLSX 파일 (*.xlsx)|*.xlsx|CSV 파일 (*.csv)|*.csv|모든 파일 (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                               + "\\Downloads"
        };

        if (dialog.ShowDialog() != true)
            return settings.ResolveDataPath();

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
                    return settings.ResolveDataPath();
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

            settings.CsvFilePath = destPath;
            settings.Save();

            var fi = new FileInfo(destPath);
            MessageBox.Show(
                $"데이터 파일이 설치되었습니다!\n\n" +
                $"파일: {fi.Name}\n" +
                $"크기: {fi.Length / 1024.0 / 1024.0:F1} MB\n" +
                $"경로: {destPath}",
                "설치 완료", MessageBoxButton.OK, MessageBoxImage.Information);

            return destPath;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"파일 설치 실패: {ex.Message}",
                "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            return settings.ResolveDataPath();
        }
    }
}
