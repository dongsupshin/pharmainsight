using System.IO;
using System.Net.Http;
using System.Xml.Linq;
using MedSearchApp.Models;

namespace MedSearchApp.Services;

/// <summary>
/// 건강보험심사평가원(HIRA) Open API 서비스
/// getHospBasisList / getParmacyBasisList 로 기관 목록 검색
/// 요양기관코드(ykiho), 주소, 우편번호, 전화번호, 종별 등 반환
/// ※ 사업자등록번호는 HIRA API에서 제공하지 않음 (사용자 직접 입력)
/// </summary>
public class HiraApiService : ISearchService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    private static readonly string LogPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "search_debug.log");

    private const string HospitalListUrl =
        "http://apis.data.go.kr/B551182/hospInfoServicev2/getHospBasisList";
    private const string HospitalDetailUrl =
        "http://apis.data.go.kr/B551182/hospInfoServicev2/getHospBasisList"; // 상세도 동일 엔드포인트, ykiho 파라미터 사용
    private const string PharmacyListUrl =
        "http://apis.data.go.kr/B551182/pharmacyInfoService/getParmacyBasisList";

    public string ServiceName => "건강보험심사평가원 (HIRA)";
    public bool IsAvailable => !string.IsNullOrWhiteSpace(_apiKey);

    private static void Log(string message)
    {
        try
        {
            File.AppendAllText(LogPath, message + Environment.NewLine,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        }
        catch { }
    }

    /// <summary>XML 숫자 필드를 int로 안전 파싱</summary>
    private static int ParseInt(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return 0;
        return int.TryParse(raw.Trim(), out var v) ? v : 0;
    }

    public HiraApiService(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
    }

    public async Task<List<MedicalInstitution>> SearchByNameAsync(
        string name, CancellationToken ct = default)
    {
        if (!IsAvailable)
            throw new InvalidOperationException("HIRA API 키가 설정되지 않았습니다.");

        var results = new List<MedicalInstitution>();

        var hospitals = await SearchListAsync(HospitalListUrl, name, "HIRA-병원", ct);
        results.AddRange(hospitals);

        var pharmacies = await SearchListAsync(PharmacyListUrl, name, "HIRA-약국", ct);
        results.AddRange(pharmacies);

        return results;
    }

    private async Task<List<MedicalInstitution>> SearchListAsync(
        string baseUrl, string name, string source, CancellationToken ct)
    {
        var results = new List<MedicalInstitution>();
        try
        {
            // 공공데이터포털 Encoding 키는 이미 URL 인코딩된 상태(%2F, %2B 등 포함)
            // EscapeDataString 적용 시 이중인코딩(%252F) → 401 오류
            // 키를 그대로 사용해야 함
            var url = $"{baseUrl}?" +
                      $"serviceKey={_apiKey}" +
                      $"&yadmNm={Uri.EscapeDataString(name)}" +
                      $"&numOfRows=20&pageNo=1";

            Log($"  [{source}] URL: {url}");
            // .NET Uri 클래스가 쿼리스트링을 정규화(%2F→/)하는 것을 방지
            var uriOptions = new UriCreationOptions
            {
                DangerousDisablePathAndQueryCanonicalization = true
            };
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url, uriOptions));
            var resp = await _httpClient.SendAsync(request, ct);
            var xml  = await resp.Content.ReadAsStringAsync(ct);
            Log($"  [{source}] 응답 길이: {xml.Length}자");
            Log($"  [{source}] 앞부분: {xml[..Math.Min(300, xml.Length)]}");

            var doc   = XDocument.Parse(xml);
            var items = doc.Descendants("item").ToList();
            Log($"  [{source}] item 수: {items.Count}");

            foreach (var item in items)
            {
                var inst = new MedicalInstitution
                {
                    Name            = item.Element("yadmNm")?.Value   ?? string.Empty,
                    InstitutionCode = item.Element("ykiho")?.Value    ?? string.Empty,
                    Address         = item.Element("addr")?.Value     ?? string.Empty,
                    PostalCode      = item.Element("postNo")?.Value   ?? string.Empty,
                    City            = item.Element("sidoCdNm")?.Value ?? string.Empty,
                    District        = item.Element("sgguCdNm")?.Value ?? string.Empty,
                    InstitutionType = item.Element("clCdNm")?.Value   ?? string.Empty,
                    PhoneNumber     = item.Element("telno")?.Value    ?? string.Empty,
                    DataSource      = source,
                    BusinessNumber  = string.Empty,  // 공공데이터 미제공, 사용자 직접 입력

                    // ── B2B Pro 필드: 의료진/규모 ──
                    DoctorTotalCount = ParseInt(item.Element("drTotCnt")?.Value),
                    NurseCount       = ParseInt(item.Element("pnursCnt")?.Value),
                    EstablishedDate  = item.Element("estbDd")?.Value ?? string.Empty,
                    XPos             = item.Element("XPos")?.Value   ?? string.Empty,
                    YPos             = item.Element("YPos")?.Value   ?? string.Empty,
                };

                // 전문의 = mdeptGdrCnt + detyGdrCnt + cmdcGdrCnt (의/치/한 일반의=전문의 수)
                inst.SpecialistCount =
                    ParseInt(item.Element("mdeptGdrCnt")?.Value) +
                    ParseInt(item.Element("detyGdrCnt")?.Value)  +
                    ParseInt(item.Element("cmdcGdrCnt")?.Value);

                // 인턴 = mdeptIntnCnt + detyIntnCnt + cmdcIntnCnt
                inst.InternCount =
                    ParseInt(item.Element("mdeptIntnCnt")?.Value) +
                    ParseInt(item.Element("detyIntnCnt")?.Value)  +
                    ParseInt(item.Element("cmdcIntnCnt")?.Value);

                // 레지던트 = mdeptResdntCnt + detyResdntCnt + cmdcResdntCnt
                inst.ResidentCount =
                    ParseInt(item.Element("mdeptResdntCnt")?.Value) +
                    ParseInt(item.Element("detyResdntCnt")?.Value)  +
                    ParseInt(item.Element("cmdcResdntCnt")?.Value);

                // drTotCnt가 없는 경우 합산으로 대체
                if (inst.DoctorTotalCount == 0)
                {
                    var sdr =
                        ParseInt(item.Element("mdeptSdrCnt")?.Value) +
                        ParseInt(item.Element("detySdrCnt")?.Value)  +
                        ParseInt(item.Element("cmdcSdrCnt")?.Value);
                    inst.DoctorTotalCount =
                        inst.SpecialistCount + inst.InternCount + inst.ResidentCount + sdr;
                }

                // 추정 매출 등급 산정
                RevenueEstimator.Apply(inst);

                if (!string.IsNullOrWhiteSpace(inst.Name))
                    results.Add(inst);
            }
        }
        catch (Exception ex)
        {
            Log($"  [{source}] 예외: {ex.GetType().Name}: {ex.Message}");
        }
        return results;
    }
}
