using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MedSearchApp.Services;

/// <summary>bizno.net API 일일 무료 한도(200건)를 초과했을 때 발생합니다.</summary>
public class BiznoQuotaExceededException : Exception
{
    public BiznoQuotaExceededException()
        : base("bizno.net API 일일 무료 한도(200건)를 초과했습니다. 설정에서 새 API 키를 입력해주세요.") { }
}

/// <summary>
/// bizno.net 무료 API 서비스
/// 엔드포인트: https://bizno.net/api/fapi
/// 파라미터: key, gb=3(상호명검색), q=검색어, type=json, pagecnt=최대20
/// 무료 한도: 1일 200건
/// </summary>
public class BiznoApiService
{
    private readonly HttpClient _httpClient;
    private string _apiKey;

    private static readonly string LogPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "search_debug.log");

    private const string BaseUrl = "https://bizno.net/api/fapi";

    public bool IsAvailable => !string.IsNullOrWhiteSpace(_apiKey);

    public BiznoApiService(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
    }

    /// <summary>API 키를 런타임에 업데이트 (설정 변경 시)</summary>
    public void UpdateApiKey(string newKey) => _apiKey = newKey;

    /// <summary>상호명으로 사업자 정보를 검색합니다 (gb=3).</summary>
    public async Task<List<BiznoResult>> SearchAsync(
        string name, int pagecnt = 20, CancellationToken ct = default)
    {
        if (!IsAvailable) return new List<BiznoResult>();

        // gb=3: 상호명검색, 최대 pagecnt 20
        var url = $"{BaseUrl}" +
                  $"?key={Uri.EscapeDataString(_apiKey)}" +
                  $"&gb=3" +
                  $"&q={Uri.EscapeDataString(name)}" +
                  $"&type=json" +
                  $"&pagecnt={Math.Clamp(pagecnt, 1, 20)}";

        Log($"[Bizno] 요청: {url[..Math.Min(150, url.Length)]}");

        try
        {
            var resp = await _httpClient.GetAsync(url, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            Log($"[Bizno] 응답: HTTP {(int)resp.StatusCode}, 길이={body.Length}자");
            if (body.Length > 0)
                Log($"[Bizno] 응답 앞부분: {body[..Math.Min(400, body.Length)]}");

            if (!resp.IsSuccessStatusCode)
            {
                Log($"[Bizno] HTTP 오류");
                return new List<BiznoResult>();
            }

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            // 에러코드 체크
            if (root.TryGetProperty("resultCode", out var codeEl))
            {
                var code = codeEl.ValueKind == JsonValueKind.Number
                    ? codeEl.GetInt32()
                    : int.Parse(codeEl.GetString() ?? "0");

                if (code != 0)
                {
                    var msg = root.TryGetProperty("resultMsg", out var m) ? m.GetString() : "";
                    Log($"[Bizno] API 오류 코드 {code}: {msg}");

                    // -3: 1일 200건 초과 → 호출자에게 알림
                    if (code == -3)
                        throw new BiznoQuotaExceededException();

                    return new List<BiznoResult>();
                }
            }

            // items 파싱
            if (!root.TryGetProperty("items", out var items) ||
                items.ValueKind != JsonValueKind.Array)
            {
                Log("[Bizno] items 필드 없음");
                return new List<BiznoResult>();
            }

            var results = new List<BiznoResult>();
            foreach (var item in items.EnumerateArray())
            {
                var bizNo  = GetStr(item, "bno");
                var name2  = GetStr(item, "company");
                var status = GetStr(item, "bstt");

                if (!string.IsNullOrWhiteSpace(bizNo) || !string.IsNullOrWhiteSpace(name2))
                    results.Add(new BiznoResult
                    {
                        BizNo   = bizNo,
                        BizName = name2,
                        Status  = status
                    });
            }

            var total = root.TryGetProperty("totalCount", out var tc) ? tc.ToString() : "?";
            Log($"[Bizno] 파싱 성공: totalCount={total}, 반환={results.Count}건");
            return results;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            Log($"[Bizno] 예외: {ex.GetType().Name}: {ex.Message}");
            return new List<BiznoResult>();
        }
    }

    /// <summary>
    /// 검색 결과 목록에 사업자번호를 자동 매칭합니다.
    /// 상호명 기준으로 매칭하며, 이미 사업자번호가 있는 항목은 건너뜁니다.
    /// </summary>
    public async Task<int> EnrichResultsAsync(
        List<Models.MedicalInstitution> results, string searchName, CancellationToken ct = default)
    {
        if (!IsAvailable || results.Count == 0) return 0;

        var biznoList = await SearchAsync(searchName, 20, ct);
        if (biznoList.Count == 0) return 0;

        int matched = 0;

        foreach (var inst in results)
        {
            if (!string.IsNullOrWhiteSpace(inst.BusinessNumber)) continue;

            // 1순위: 정확히 같은 이름
            var match = biznoList.FirstOrDefault(b =>
                !string.IsNullOrWhiteSpace(b.BizNo) &&
                Normalize(b.BizName) == Normalize(inst.Name));

            // 2순위: 포함 관계
            match ??= biznoList.FirstOrDefault(b =>
                !string.IsNullOrWhiteSpace(b.BizNo) &&
                (Normalize(b.BizName).Contains(Normalize(inst.Name)) ||
                 Normalize(inst.Name).Contains(Normalize(b.BizName))));

            if (match != null)
            {
                inst.BusinessNumber = FormatBizNo(match.BizNo);
                matched++;
            }
        }

        Log($"[Bizno] 매칭 완료: {matched}/{results.Count}건");
        return matched;
    }

    // ── 유틸리티 ──────────────────────────────────────────

    private static string GetStr(JsonElement el, params string[] names)
    {
        foreach (var n in names)
            if (el.TryGetProperty(n, out var v))
                return v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : v.ToString();
        return string.Empty;
    }

    private static string Normalize(string? s)
        => (s ?? "").Replace(" ", "").Replace("(", "").Replace(")", "")
                    .Replace("（", "").Replace("）", "").ToLowerInvariant();

    private static string FormatBizNo(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var digits = new string(raw.Where(char.IsDigit).ToArray());
        return digits.Length == 10
            ? $"{digits[..3]}-{digits[3..5]}-{digits[5..]}"
            : raw;
    }

    private static void Log(string message)
    {
        try
        {
            File.AppendAllText(LogPath, message + Environment.NewLine,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        }
        catch { }
    }
}

public class BiznoResult
{
    public string BizNo   { get; set; } = string.Empty;
    public string BizName { get; set; } = string.Empty;
    public string Status  { get; set; } = string.Empty;
}
