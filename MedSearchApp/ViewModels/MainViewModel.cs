using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedSearchApp.Models;
using MedSearchApp.Services;

namespace MedSearchApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly SearchOrchestrator _orchestrator;
    private readonly BiznoApiService? _biznoService;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "병원명 또는 약국명을 입력하고 검색하세요.";

    [ObservableProperty]
    private bool _isLoading = false;

    /// <summary>bizno API 한도 초과 시 true — MainWindow에서 안내 배너 표시</summary>
    [ObservableProperty]
    private bool _isBiznoQuotaExceeded = false;

    [ObservableProperty]
    private MedicalInstitution? _selectedItem;

    /// <summary>선택된 항목의 사업자번호 입력 필드</summary>
    [ObservableProperty]
    private string _selectedBizNo = string.Empty;

    public ObservableCollection<MedicalInstitution> SearchResults { get; } = new();

    /// <summary>설정 창 열기 액션 (App.xaml.cs에서 주입)</summary>
    public Action? OpenSettingsAction { get; set; }

    /// <summary>사용자가 직접 입력한 사업자번호 저장소 (key: 기관명+주소 → value: 사업자번호)</summary>
    private Dictionary<string, string> _bizNoStore = new();
    private static readonly string BizNoStorePath = Path.Combine(
        AppSettings.DefaultDataDir, "user_bizno.json");

    public MainViewModel(SearchOrchestrator orchestrator, BiznoApiService? biznoService = null)
    {
        _orchestrator = orchestrator;
        _biznoService = biznoService;
        LoadBizNoStore();
    }

    partial void OnSelectedItemChanged(MedicalInstitution? value)
    {
        if (value != null)
        {
            // 기존 저장된 사업자번호가 있으면 표시
            var key = MakeBizNoKey(value);
            if (_bizNoStore.TryGetValue(key, out var savedBizNo))
            {
                SelectedBizNo = savedBizNo;
                value.BusinessNumber = savedBizNo;
            }
            else
            {
                SelectedBizNo = value.BusinessNumber;
            }
        }
        else
        {
            SelectedBizNo = string.Empty;
        }
    }

    [RelayCommand]
    private void OpenSettings() => OpenSettingsAction?.Invoke();

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task SearchAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            StatusMessage = "검색어를 입력해주세요.";
            return;
        }

        IsLoading = true;
        StatusMessage = $"'{SearchText}' 검색 중...";
        SearchResults.Clear();
        SelectedItem = null;

        try
        {
            var searchTerm = SearchText.Trim();
            var result = await _orchestrator.SearchAsync(searchTerm, ct);

            foreach (var item in result.Items)
            {
                // 저장된 사업자번호가 있으면 자동 적용
                var key = MakeBizNoKey(item);
                if (_bizNoStore.TryGetValue(key, out var savedBizNo))
                    item.BusinessNumber = savedBizNo;

                SearchResults.Add(item);
            }

            StatusMessage = result.Message;

            // bizno API로 사업자번호 자동 매칭 (비동기)
            if (_biznoService?.IsAvailable == true && result.Items.Count > 0)
            {
                _ = EnrichWithBiznoAsync(result.Items, searchTerm, ct);
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "검색이 취소되었습니다.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"오류 발생: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>bizno API로 사업자번호 자동 매칭 (검색 후 백그라운드 실행)</summary>
    private async Task EnrichWithBiznoAsync(
        List<MedicalInstitution> items, string searchName, CancellationToken ct)
    {
        var baseMsg = StatusMessage;
        try
        {
            StatusMessage = baseMsg + " | 사업자번호 조회 중...";

            var matched = await _biznoService!.EnrichResultsAsync(items, searchName, ct);

            if (matched > 0)
            {
                // UI 갱신: ObservableCollection 항목이 변경되었으므로 DataGrid 리프레시
                var selected = SelectedItem;
                var temp = SearchResults.ToList();
                SearchResults.Clear();
                foreach (var item in temp)
                    SearchResults.Add(item);

                // 선택 복원
                if (selected != null)
                    SelectedItem = SearchResults.FirstOrDefault(x =>
                        x.Name == selected.Name && x.Address == selected.Address);

                StatusMessage = $"{baseMsg} | 사업자번호 {matched}건 자동 매칭됨";
            }
            else
            {
                StatusMessage = $"{baseMsg} | 사업자번호 매칭 0건 (로그 확인)";
            }
        }
        catch (OperationCanceledException) { StatusMessage = baseMsg; }
        catch (BiznoQuotaExceededException)
        {
            // 일일 한도 초과 → 배너 표시, 상태바 안내
            IsBiznoQuotaExceeded = true;
            StatusMessage = $"{baseMsg} | ⚠ 사업자번호 API 한도 초과";
        }
        catch (Exception ex)
        {
            StatusMessage = $"{baseMsg} | bizno 오류: {ex.Message}";
        }
    }

    /// <summary>한도 초과 배너의 '설정 열기' 버튼 커맨드</summary>
    [RelayCommand]
    private void OpenSettingsForBizno()
    {
        IsBiznoQuotaExceeded = false;   // 배너 닫기
        OpenSettingsAction?.Invoke();   // 설정 창 열기
    }

    /// <summary>한도 초과 배너 닫기 커맨드</summary>
    [RelayCommand]
    private void DismissBiznoQuotaBanner() => IsBiznoQuotaExceeded = false;

    /// <summary>사업자번호 저장</summary>
    [RelayCommand]
    private void SaveBizNo()
    {
        if (SelectedItem is null || string.IsNullOrWhiteSpace(SelectedBizNo))
        {
            StatusMessage = "사업자번호를 입력해주세요.";
            return;
        }

        var formatted = FormatBizNo(SelectedBizNo.Trim());
        SelectedItem.BusinessNumber = formatted;
        SelectedBizNo = formatted;

        var key = MakeBizNoKey(SelectedItem);
        _bizNoStore[key] = formatted;
        SaveBizNoStore();

        StatusMessage = $"'{SelectedItem.Name}' 사업자번호 저장됨: {formatted}";
    }

    /// <summary>선택된 항목의 요양기관코드를 클립보드에 복사</summary>
    [RelayCommand]
    private void CopyInstitutionCode()
    {
        if (SelectedItem is null || string.IsNullOrWhiteSpace(SelectedItem.InstitutionCode)) return;
        System.Windows.Clipboard.SetText(SelectedItem.InstitutionCode);
        StatusMessage = $"요양기관코드 '{SelectedItem.InstitutionCode}' 복사됨";
    }

    /// <summary>선택된 항목의 사업자번호를 클립보드에 복사</summary>
    [RelayCommand]
    private void CopyBusinessNumber()
    {
        if (SelectedItem is null || string.IsNullOrWhiteSpace(SelectedItem.BusinessNumber)) return;
        System.Windows.Clipboard.SetText(SelectedItem.BusinessNumber);
        StatusMessage = $"사업자번호 '{SelectedItem.BusinessNumber}' 복사됨";
    }

    /// <summary>선택된 항목의 주소를 클립보드에 복사</summary>
    [RelayCommand]
    private void CopyAddress()
    {
        if (SelectedItem is null) return;
        System.Windows.Clipboard.SetText(SelectedItem.Address);
        StatusMessage = $"주소 '{SelectedItem.Address}' 복사됨";
    }

    /// <summary>검색 결과 전체를 CSV로 저장</summary>
    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        if (SearchResults.Count == 0) return;

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title    = "CSV 파일로 저장",
            Filter   = "CSV 파일 (*.csv)|*.csv",
            FileName = $"검색결과_{SearchText}_{DateTime.Now:yyyyMMdd_HHmm}.csv"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            await using var writer = new StreamWriter(dialog.FileName, false,
                System.Text.Encoding.UTF8);

            // BOM 포함 UTF-8 (Excel에서 한글 깨짐 방지)
            await writer.WriteLineAsync(
                "기관명,요양기관코드,사업자번호,주소,우편번호,시도,시군구,종별,전화번호,출처");

            foreach (var item in SearchResults)
            {
                await writer.WriteLineAsync(
                    $"\"{item.Name}\"," +
                    $"\"{item.InstitutionCode}\"," +
                    $"\"{item.BusinessNumber}\"," +
                    $"\"{item.Address}\"," +
                    $"\"{item.PostalCode}\"," +
                    $"\"{item.City}\"," +
                    $"\"{item.District}\"," +
                    $"\"{item.InstitutionType}\"," +
                    $"\"{item.PhoneNumber}\"," +
                    $"\"{item.DataSource}\"");
            }

            StatusMessage = $"CSV 저장 완료: {dialog.FileName}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"저장 실패: {ex.Message}";
        }
    }

    // ──────────────────── 사업자번호 저장소 관리 ────────────────────

    private static string MakeBizNoKey(MedicalInstitution inst)
        => $"{inst.Name}|{inst.Address}".Trim();

    private static string FormatBizNo(string raw)
    {
        var digits = new string(raw.Where(char.IsDigit).ToArray());
        return digits.Length == 10
            ? $"{digits[..3]}-{digits[3..5]}-{digits[5..]}"
            : raw;
    }

    private void LoadBizNoStore()
    {
        try
        {
            if (File.Exists(BizNoStorePath))
            {
                var json = File.ReadAllText(BizNoStorePath);
                _bizNoStore = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                              ?? new Dictionary<string, string>();
            }
        }
        catch
        {
            _bizNoStore = new Dictionary<string, string>();
        }
    }

    private void SaveBizNoStore()
    {
        try
        {
            var dir = Path.GetDirectoryName(BizNoStorePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(_bizNoStore,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(BizNoStorePath, json);
        }
        catch { }
    }
}
