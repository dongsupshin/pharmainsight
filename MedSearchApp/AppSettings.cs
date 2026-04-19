using System.IO;
using System.Text.Json;

namespace MedSearchApp;

/// <summary>
/// API 키 설정 파일 관리
/// appsettings.json 파일에 API 키를 저장/불러오기
/// </summary>
public class AppSettings
{
    /// <summary>건강보험심사평가원(HIRA) Open API 인증키 (공공데이터포털에서 발급)</summary>
    public string HiraApiKey { get; set; } = string.Empty;

    /// <summary>bizno.net (머니핀) 사업자 검색 API 키 — 상호명→사업자등록번호 자동 조회</summary>
    public string BiznoApiKey { get; set; } = "Gc9nG3BuJhBVH04nGhBdGvuOH0pn2v9m";

    /// <summary>
    /// Pro 라이선스 키 — 입력 시 Pro 기능 활성화 (전체 결과/규모 점수/추정매출 등급 등).
    /// MS Store IAP 연동 전 임시 키 검증 (ValidatePro() 참조).
    /// </summary>
    public string ProLicenseKey { get; set; } = string.Empty;

    /// <summary>
    /// Pro 활성화 여부 — ProLicenseKey가 유효하면 true.
    /// 무료: 상위 5건 + 등급/의사수 마스킹.
    /// Pro: 무제한 + 전체 필드.
    /// </summary>
    public bool IsPro => ValidatePro(ProLicenseKey);

    /// <summary>
    /// 임시 라이선스 검증 — 추후 MS Store IAP 또는 서버 검증으로 교체.
    /// 현재는 접두사 "PHARMA-PRO-"로 시작하는 16자 이상 키를 유효로 간주.
    /// </summary>
    private static bool ValidatePro(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;
        return key.StartsWith("PHARMA-PRO-") && key.Length >= 16;
    }

    /// <summary>사업자번호 데이터 파일 경로 (CSV, XLSX, ZIP)</summary>
    public string CsvFilePath { get; set; } = string.Empty;

    /// <summary>data.go.kr 페이지 URL (참고용)</summary>
    public string CsvDownloadUrl { get; set; } = "https://www.data.go.kr/data/15051059/fileData.do";

    /// <summary>HIRA 공공데이터 다운로드 페이지 (실제 파일 다운로드)</summary>
    public string HiraDownloadUrl { get; set; } = "https://opendata.hira.or.kr/op/opc/selectOpenData.do?sno=11925";

    /// <summary>데이터 기본 저장 폴더</summary>
    public static string DefaultDataDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     "MedSearchApp");

    /// <summary>데이터 기본 파일 경로 (XLSX 우선)</summary>
    public static string DefaultDataPath =>
        Path.Combine(DefaultDataDir, "bizno_data.xlsx");

    // ── 하위 호환용 (기존 CSV 경로 지원) ──
    public static string DefaultCsvDir => DefaultDataDir;
    public static string DefaultCsvPath => Path.Combine(DefaultDataDir, "bizno_data.csv");

    /// <summary>유효한 데이터 파일 경로 결정 (설정 → 기본 XLSX → 기본 CSV)</summary>
    public string ResolveDataPath()
    {
        // 1. 설정에 저장된 경로
        if (!string.IsNullOrWhiteSpace(CsvFilePath) && File.Exists(CsvFilePath))
            return CsvFilePath;

        // 2. 기본 XLSX
        if (File.Exists(DefaultDataPath))
            return DefaultDataPath;

        // 3. 기본 CSV (하위 호환)
        if (File.Exists(DefaultCsvPath))
            return DefaultCsvPath;

        // 4. 기본 경로 반환 (파일 없음)
        return DefaultDataPath;
    }

    private static readonly string SettingsPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json    = File.ReadAllText(SettingsPath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<AppSettings>(json, options) ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[설정 로드 오류] {ex.Message}");
        }

        var defaults = new AppSettings();
        defaults.Save();
        return defaults;
    }

    public void Save()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, options));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[설정 저장 오류] {ex.Message}");
        }
    }
}
