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
    private readonly AppSettings _settings;

    /// <summary>무료 티어 결과 제한 (Pro는 무제한)</summary>
    public const int FreeTierResultLimit = 5;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Enter a hospital or pharmacy name and press Search.";

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

    // ──────────────── Pro 티어 게이팅 ────────────────

    /// <summary>Pro 라이선스 활성화 여부 (설정에서 로드)</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProBadgeText), nameof(IsFreeTier),
        nameof(ProUpgradePromptVisible))]
    private bool _isPro;

    public bool IsFreeTier => !IsPro;
    public string ProBadgeText => IsPro ? "★ PRO" : "FREE";
    public bool ProUpgradePromptVisible => !IsPro;

    // ──────────────── 필터 (Pro 전용) ────────────────

    /// <summary>추정매출 등급 필터 (전체/S/A/B/C). 빈값=전체</summary>
    [ObservableProperty]
    private string _selectedGradeFilter = string.Empty;

    /// <summary>종별 필터 (전체/상급종합병원/종합병원/병원/의원/약국). 빈값=전체</summary>
    [ObservableProperty]
    private string _selectedTypeFilter = string.Empty;

    /// <summary>최소 의사수 필터 (0=미적용)</summary>
    [ObservableProperty]
    private int _minDoctorCount;

    /// <summary>전체 검색 결과 (필터 전, 내부 저장소)</summary>
    private readonly List<MedicalInstitution> _allResults = new();

    // ── 언어 설정 (English default) ─────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(WindowTitle),    nameof(LblTitle),      nameof(LblSubtitle),
        nameof(LblSettings),   nameof(LblGuide),       nameof(LblLangToggle),
        nameof(LblSearch),      nameof(LblCancel),      nameof(LblExport),
        nameof(SearchPlaceholder), nameof(LblSearching), nameof(LblResultsTitle),
        nameof(LblBizNoLabel), nameof(LblSave),        nameof(LblCopyCode),
        nameof(LblCopyAddr),   nameof(ColName),        nameof(ColCode),
        nameof(ColBizNo),       nameof(ColAddress),     nameof(ColPostal),
        nameof(ColType),        nameof(ColPhone),       nameof(ColSource),
        nameof(ColDoctors),     nameof(ColNurses),      nameof(ColGrade),
        nameof(LblFilterTitle), nameof(LblFilterGrade), nameof(LblFilterType),
        nameof(LblFilterMinDoc), nameof(LblProUpgrade), nameof(ProUpgradeCta),
        nameof(LblApplyFilter), nameof(LblClearFilter),
        nameof(BiznoQuotaBannerTitle), nameof(BiznoQuotaBannerDesc),
        nameof(LblSettingsForBizno))]
    private bool _isEnglish = true;

    // ── Computed label properties ────────────────────────────────────────
    public string WindowTitle    => IsEnglish ? "Hospital & Pharmacy Search (PharmaInsight)"  : "병원·약국 검색 (PharmaInsight)";
    public string LblTitle       => IsEnglish ? "Hospital & Pharmacy Search"                  : "병원·약국 검색";
    public string LblSubtitle    => IsEnglish ? "HIRA / Local Data · Institution Code / Business No. / Address / Postal Code"
                                              : "건강보험심사평가원 / 로컬 데이터 기반 · 요양기관코드 / 사업자번호 자동 조회 / 주소 / 우편번호";
    public string LblSettings    => IsEnglish ? "⚙ Settings"       : "⚙ API 키 설정";
    public string LblGuide       => IsEnglish ? "? Guide"           : "? 가이드";
    public string LblLangToggle  => IsEnglish ? "🌐 한국어"         : "🌐 English";
    public string LblSearch      => IsEnglish ? "🔍 Search"         : "🔍 검색";
    public string LblCancel      => IsEnglish ? "⏹ Cancel"          : "⏹ 취소";
    public string LblExport      => IsEnglish ? "📥 Export CSV"      : "📥 CSV 저장";
    public string SearchPlaceholder => IsEnglish
        ? "Enter hospital or pharmacy name (e.g. Seoul, Samsung, Pharmacy)"
        : "병원명 또는 약국명 입력 (예: 서울대학교병원, 온누리약국)";
    public string LblSearching   => IsEnglish ? "Searching"         : "검색 중";
    public string LblResultsTitle => IsEnglish ? "Search Results"   : "검색 결과";
    public string LblBizNoLabel  => IsEnglish ? "Business No.:"     : "사업자번호:";
    public string LblSave        => IsEnglish ? "Save"              : "저장";
    public string LblCopyCode    => IsEnglish ? "Copy Code"         : "코드 복사";
    public string LblCopyAddr    => IsEnglish ? "Copy Address"      : "주소 복사";
    public string ColName        => IsEnglish ? "Institution"       : "기관명";
    public string ColCode        => IsEnglish ? "Institution Code"  : "요양기관코드";
    public string ColBizNo       => IsEnglish ? "Business No."      : "사업자번호";
    public string ColAddress     => IsEnglish ? "Address"           : "주소";
    public string ColPostal      => IsEnglish ? "Postal"            : "우편번호";
    public string ColType        => IsEnglish ? "Type"              : "종별";
    public string ColPhone       => IsEnglish ? "Phone"             : "전화번호";
    public string ColSource      => IsEnglish ? "Source"            : "출처";
    public string ColDoctors     => IsEnglish ? "Doctors"           : "의사수";
    public string ColNurses      => IsEnglish ? "Nurses"            : "간호사수";
    public string ColGrade       => IsEnglish ? "Revenue Grade"     : "추정매출등급";
    public string LblFilterTitle => IsEnglish ? "🎯 Target Filter (PRO)" : "🎯 타겟 필터 (PRO)";
    public string LblFilterGrade => IsEnglish ? "Grade:"            : "등급:";
    public string LblFilterType  => IsEnglish ? "Type:"             : "종별:";
    public string LblFilterMinDoc => IsEnglish ? "Min. Doctors:"    : "최소 의사수:";
    public string LblApplyFilter => IsEnglish ? "Apply"             : "적용";
    public string LblClearFilter => IsEnglish ? "Clear"             : "초기화";
    public string LblProUpgrade  => IsEnglish
        ? "🔓 Upgrade to PRO — unlimited results, doctor counts, revenue grade, filters, bulk Excel export."
        : "🔓 PRO 업그레이드 — 무제한 결과, 의사수/추정매출등급, 필터, 일괄 Excel 추출 이용 가능.";
    public string ProUpgradeCta  => IsEnglish ? "Activate PRO"      : "PRO 활성화";
    public string BiznoQuotaBannerTitle => IsEnglish
        ? "Business Number API daily limit (200) exceeded."
        : "사업자번호 API 일일 한도(200건)를 초과했습니다.";
    public string BiznoQuotaBannerDesc => IsEnglish
        ? "Register a new API key in Settings to resume automatic lookup. (Free signup at bizno.net)"
        : "설정에서 새 API 키를 등록하면 다시 자동 조회됩니다. (bizno.net 무료 가입 후 발급)";
    public string LblSettingsForBizno => IsEnglish ? "⚙ Settings" : "⚙ API 키 설정";

    // ────────────────────────────────────────────────────────────────────

    public ObservableCollection<MedicalInstitution> SearchResults { get; } = new();

    /// <summary>설정 창 열기 액션 (App.xaml.cs에서 주입)</summary>
    public Action? OpenSettingsAction { get; set; }

    /// <summary>가이드 창 열기 액션 (App.xaml.cs에서 주입)</summary>
    public Action? OpenGuideAction { get; set; }

    /// <summary>사용자가 직접 입력한 사업자번호 저장소 (key: 기관명+주소 → value: 사업자번호)</summary>
    private Dictionary<string, string> _bizNoStore = new();
    private static readonly string BizNoStorePath = Path.Combine(
        AppSettings.DefaultDataDir, "user_bizno.json");

    public MainViewModel(
        SearchOrchestrator orchestrator,
        AppSettings settings,
        BiznoApiService? biznoService = null)
    {
        _orchestrator = orchestrator;
        _settings = settings;
        _biznoService = biznoService;
        _isPro = settings.IsPro;
        LoadBizNoStore();
    }

    /// <summary>Settings 창에서 라이선스 변경 후 호출</summary>
    public void RefreshProStatus()
    {
        IsPro = _settings.IsPro;
        // 결과 재-게이팅
        ApplyGateAndFilter();
    }

    partial void OnSelectedItemChanged(MedicalInstitution? value)
    {
        if (value != null)
        {
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

    [RelayCommand]
    private void OpenGuide() => OpenGuideAction?.Invoke();

    [RelayCommand]
    private void ToggleLanguage() => IsEnglish = !IsEnglish;

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task SearchAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            StatusMessage = IsEnglish ? "Please enter a search term." : "검색어를 입력해주세요.";
            return;
        }

        IsLoading = true;
        StatusMessage = IsEnglish
            ? $"Searching for '{SearchText}'..."
            : $"'{SearchText}' 검색 중...";
        SearchResults.Clear();
        SelectedItem = null;

        try
        {
            var searchTerm = SearchText.Trim();
            var result = await _orchestrator.SearchAsync(searchTerm, ct);

            // 저장된 사업자번호 주입
            foreach (var item in result.Items)
            {
                var key = MakeBizNoKey(item);
                if (_bizNoStore.TryGetValue(key, out var savedBizNo))
                    item.BusinessNumber = savedBizNo;
            }

            // 전체 결과는 _allResults에 보관, 필터+게이팅 후 SearchResults에 표시
            _allResults.Clear();
            _allResults.AddRange(result.Items);

            // 추정매출등급 내림차순 정렬 (S → A → B → C → -) + SizeScore 내림차순
            _allResults.Sort((a, b) =>
            {
                var g = GradeRank(b.RevenueGrade).CompareTo(GradeRank(a.RevenueGrade));
                return g != 0 ? g : b.SizeScore.CompareTo(a.SizeScore);
            });

            _lastBaseStatusMessage = result.Message;
            ApplyGateAndFilter();  // 내부에서 StatusMessage 갱신

            if (_biznoService?.IsAvailable == true && result.Items.Count > 0)
            {
                _ = EnrichWithBiznoAsync(result.Items, searchTerm, ct);
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = IsEnglish ? "Search cancelled." : "검색이 취소되었습니다.";
        }
        catch (Exception ex)
        {
            StatusMessage = IsEnglish ? $"Error: {ex.Message}" : $"오류 발생: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>데모 검색 — GuideWindow의 'Run Demo Search' 버튼에서 호출</summary>
    public void TriggerDemoSearch()
    {
        SearchText = "서울";
        SearchCommand.Execute(null);
    }

    /// <summary>bizno API로 사업자번호 자동 매칭 (검색 후 백그라운드 실행)</summary>
    private async Task EnrichWithBiznoAsync(
        List<MedicalInstitution> items, string searchName, CancellationToken ct)
    {
        var baseMsg = StatusMessage;
        try
        {
            StatusMessage = baseMsg + (IsEnglish ? " | Looking up business numbers..." : " | 사업자번호 조회 중...");

            var matched = await _biznoService!.EnrichResultsAsync(items, searchName, ct);

            if (matched > 0)
            {
                var selected = SelectedItem;
                var temp = SearchResults.ToList();
                SearchResults.Clear();
                foreach (var item in temp)
                    SearchResults.Add(item);

                if (selected != null)
                    SelectedItem = SearchResults.FirstOrDefault(x =>
                        x.Name == selected.Name && x.Address == selected.Address);

                StatusMessage = IsEnglish
                    ? $"{baseMsg} | {matched} business number(s) matched"
                    : $"{baseMsg} | 사업자번호 {matched}건 자동 매칭됨";
            }
            else
            {
                StatusMessage = IsEnglish
                    ? $"{baseMsg} | No business numbers matched"
                    : $"{baseMsg} | 사업자번호 매칭 0건 (로그 확인)";
            }
        }
        catch (OperationCanceledException) { StatusMessage = baseMsg; }
        catch (BiznoQuotaExceededException)
        {
            IsBiznoQuotaExceeded = true;
            StatusMessage = IsEnglish
                ? $"{baseMsg} | ⚠ Business Number API limit exceeded"
                : $"{baseMsg} | ⚠ 사업자번호 API 한도 초과";
        }
        catch (Exception ex)
        {
            StatusMessage = $"{baseMsg} | bizno error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenSettingsForBizno()
    {
        IsBiznoQuotaExceeded = false;
        OpenSettingsAction?.Invoke();
    }

    [RelayCommand]
    private void ApplyFilter() => ApplyGateAndFilter();

    [RelayCommand]
    private void ClearFilter()
    {
        SelectedGradeFilter = string.Empty;
        SelectedTypeFilter = string.Empty;
        MinDoctorCount = 0;
        ApplyGateAndFilter();
    }

    /// <summary>업그레이드 CTA 클릭 시 설정창 (라이선스 입력) 열기</summary>
    [RelayCommand]
    private void UpgradeToPro() => OpenSettingsAction?.Invoke();

    // ──────────────────────────── Pro 게이팅 + 필터 ────────────────────────────

    /// <summary>마지막 검색의 원본 상태 메시지 (필터·게이팅 갱신 시 재사용)</summary>
    private string _lastBaseStatusMessage = string.Empty;

    /// <summary>
    /// _allResults에서 필터 적용 → SearchResults에 반영.
    /// 무료 티어: 상위 N건만 + 의사수/등급 마스킹 (복사본에만 적용, 원본 _allResults 보존).
    /// </summary>
    private void ApplyGateAndFilter()
    {
        SearchResults.Clear();

        IEnumerable<MedicalInstitution> query = _allResults;

        // Pro 필터는 Pro 티어에서만 적용
        if (IsPro)
        {
            if (!string.IsNullOrWhiteSpace(SelectedGradeFilter))
                query = query.Where(x => x.RevenueGrade == SelectedGradeFilter);

            if (!string.IsNullOrWhiteSpace(SelectedTypeFilter))
                query = query.Where(x =>
                    x.InstitutionType?.Contains(SelectedTypeFilter,
                        StringComparison.OrdinalIgnoreCase) == true);

            if (MinDoctorCount > 0)
                query = query.Where(x => x.DoctorTotalCount >= MinDoctorCount);
        }

        var filtered = query.ToList();

        if (!IsPro)
        {
            // 무료 티어: 상위 N건 + 복사본에 마스킹 적용 (원본 보호)
            filtered = filtered.Take(FreeTierResultLimit)
                               .Select(x => MaskedCopy(x))
                               .ToList();
        }

        foreach (var item in filtered)
            SearchResults.Add(item);

        // 상태바 재계산 — 필터/Pro 전환 시에도 최신 정보로 갱신
        if (!string.IsNullOrEmpty(_lastBaseStatusMessage))
            StatusMessage = BuildStatusMessage(_lastBaseStatusMessage);
    }

    /// <summary>원본을 건드리지 않고 민감 필드를 마스킹한 복사본 반환</summary>
    private static MedicalInstitution MaskedCopy(MedicalInstitution item)
    {
        var copy = item.Clone();
        if (copy.DoctorTotalCount > 0) copy.DoctorTotalCount = -1; // -1 = 🔒
        if (copy.NurseCount > 0)       copy.NurseCount       = -1;
        if (!string.IsNullOrEmpty(copy.RevenueGrade) && copy.RevenueGrade != "-")
            copy.RevenueGrade = "🔒";
        return copy;
    }

    private static int GradeRank(string grade) => grade switch
    {
        "S" => 4,
        "A" => 3,
        "B" => 2,
        "C" => 1,
        _   => 0
    };

    /// <summary>
    /// 상태바 메시지 조립:
    /// - 무료 티어 + 결과 잘림 → 업그레이드 안내
    /// - Pro + 필터 0건 → 필터 조정 안내 (LocalData는 의사수 없음 주의)
    /// </summary>
    private string BuildStatusMessage(string baseMsg)
    {
        var total = _allResults.Count;
        var shown = SearchResults.Count;

        // Pro + 필터 적용 중 + 결과 0건
        if (IsPro && total > 0 && shown == 0 && HasActiveFilter())
        {
            var hasHira = _allResults.Any(x => x.DataSource?.StartsWith("HIRA") == true);
            var hint = hasHira
                ? (IsEnglish
                    ? "Try relaxing filters (e.g. lower Min. Doctors or clear Grade)."
                    : "필터를 완화해보세요 (예: 최소 의사수 낮추기 / 등급 필터 해제).")
                : (IsEnglish
                    ? "⚠ Local data lacks doctor counts. Register a HIRA API key in Settings for Pro filters (Grade/Min. Doctors)."
                    : "⚠ 로컬 데이터는 의사수 정보가 없습니다. Settings에서 HIRA API 키 등록 후 검색해야 등급/의사수 필터가 동작합니다.");
            return IsEnglish
                ? $"{baseMsg} — 0/{total} match current filters. {hint}"
                : $"{baseMsg} — 현재 필터와 맞는 결과 0/{total}건. {hint}";
        }

        if (!IsPro && total > shown)
        {
            return IsEnglish
                ? $"{baseMsg} — Free tier shows top {shown}/{total}. Upgrade to PRO for all results."
                : $"{baseMsg} — 무료 티어는 상위 {shown}/{total}건만 표시. PRO 업그레이드로 전체 확인.";
        }
        return baseMsg;
    }

    private bool HasActiveFilter() =>
        !string.IsNullOrWhiteSpace(SelectedGradeFilter) ||
        !string.IsNullOrWhiteSpace(SelectedTypeFilter)  ||
        MinDoctorCount > 0;

    [RelayCommand]
    private void DismissBiznoQuotaBanner() => IsBiznoQuotaExceeded = false;

    [RelayCommand]
    private void SaveBizNo()
    {
        if (SelectedItem is null || string.IsNullOrWhiteSpace(SelectedBizNo))
        {
            StatusMessage = IsEnglish ? "Please enter a business number." : "사업자번호를 입력해주세요.";
            return;
        }

        var formatted = FormatBizNo(SelectedBizNo.Trim());
        SelectedItem.BusinessNumber = formatted;
        SelectedBizNo = formatted;

        var key = MakeBizNoKey(SelectedItem);
        _bizNoStore[key] = formatted;
        SaveBizNoStore();

        StatusMessage = IsEnglish
            ? $"Business number saved for '{SelectedItem.Name}': {formatted}"
            : $"'{SelectedItem.Name}' 사업자번호 저장됨: {formatted}";
    }

    [RelayCommand]
    private void CopyInstitutionCode()
    {
        if (SelectedItem is null || string.IsNullOrWhiteSpace(SelectedItem.InstitutionCode)) return;
        System.Windows.Clipboard.SetText(SelectedItem.InstitutionCode);
        StatusMessage = IsEnglish
            ? $"Institution code copied: {SelectedItem.InstitutionCode}"
            : $"요양기관코드 '{SelectedItem.InstitutionCode}' 복사됨";
    }

    [RelayCommand]
    private void CopyBusinessNumber()
    {
        if (SelectedItem is null || string.IsNullOrWhiteSpace(SelectedItem.BusinessNumber)) return;
        System.Windows.Clipboard.SetText(SelectedItem.BusinessNumber);
        StatusMessage = IsEnglish
            ? $"Business number copied: {SelectedItem.BusinessNumber}"
            : $"사업자번호 '{SelectedItem.BusinessNumber}' 복사됨";
    }

    [RelayCommand]
    private void CopyAddress()
    {
        if (SelectedItem is null) return;
        System.Windows.Clipboard.SetText(SelectedItem.Address);
        StatusMessage = IsEnglish
            ? $"Address copied: {SelectedItem.Address}"
            : $"주소 '{SelectedItem.Address}' 복사됨";
    }

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        if (SearchResults.Count == 0) return;

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title    = IsEnglish ? "Save as CSV" : "CSV 파일로 저장",
            Filter   = "CSV files (*.csv)|*.csv",
            FileName = $"search_{SearchText}_{DateTime.Now:yyyyMMdd_HHmm}.csv"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            await using var writer = new StreamWriter(dialog.FileName, false,
                System.Text.Encoding.UTF8);

            await writer.WriteLineAsync(
                "Institution,Institution Code,Business No.,Address,Postal Code,City,District,Type,Phone,Source");

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

            StatusMessage = IsEnglish
                ? $"CSV saved: {dialog.FileName}"
                : $"CSV 저장 완료: {dialog.FileName}";
        }
        catch (Exception ex)
        {
            StatusMessage = IsEnglish ? $"Save failed: {ex.Message}" : $"저장 실패: {ex.Message}";
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
