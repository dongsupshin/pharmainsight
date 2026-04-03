using System.Globalization;
using System.IO;
using System.IO.Compression;
using ClosedXML.Excel;
using MedSearchApp.Models;

namespace MedSearchApp.Services;

/// <summary>
/// 로컬 데이터 검색 서비스 (CSV / XLSX / ZIP 지원)
/// 출처: 건강보험심사평가원_전국 병의원 및 약국 현황 또는 LocalData
///   - HIRA 공공데이터: https://opendata.hira.or.kr/op/opc/selectOpenData.do?sno=11925
///   - LocalData: https://www.localdata.go.kr/devcenter/dataDown.do
///   - data.go.kr: https://www.data.go.kr/data/15051059/fileData.do
///
/// ZIP 파일 내 XLSX, 또는 직접 XLSX/CSV 파일을 로드합니다.
/// </summary>
public class CsvDataService : ISearchService
{
    private readonly string _dataPath;
    private List<DataRecord>? _records;
    private bool _loadAttempted;

    private static readonly string LogPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "search_debug.log");

    public string ServiceName => "로컬 데이터";
    public bool IsAvailable => File.Exists(_dataPath);

    /// <summary>로드된 레코드 수 (UI 표시용)</summary>
    public int RecordCount => _records?.Count ?? 0;

    public CsvDataService(string dataPath)
    {
        _dataPath = dataPath;
    }

    public async Task<List<MedicalInstitution>> SearchByNameAsync(
        string name, CancellationToken ct = default)
    {
        if (!IsAvailable)
            throw new InvalidOperationException("데이터 파일이 없습니다. 설정에서 다운로드해주세요.");

        await EnsureLoadedAsync(ct);

        if (_records == null || _records.Count == 0)
            return new List<MedicalInstitution>();

        // 이름 포함 검색 (대소문자 무시)
        var query = name.Trim();
        var matches = _records
            .Where(r => r.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(50)  // 최대 50건
            .Select(r =>
            {
                var inst = new MedicalInstitution
                {
                    Name            = r.Name,
                    InstitutionCode = r.Code,
                    BusinessNumber  = FormatBizNo(r.BizNo),
                    Address         = r.Address,
                    PostalCode      = r.PostalCode,
                    PhoneNumber     = r.Phone,
                    InstitutionType = r.TypeName,
                    DataSource      = "로컬 데이터"
                };

                var parts = r.Address.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    inst.City     = parts[0];
                    inst.District = parts[1];
                }

                return inst;
            })
            .ToList();

        Log($"[DATA] '{query}' 검색 → {matches.Count}건 (전체 {_records.Count}건 중)");
        return matches;
    }

    /// <summary>데이터 파일을 메모리에 로드 (최초 1회)</summary>
    private async Task EnsureLoadedAsync(CancellationToken ct)
    {
        if (_loadAttempted) return;
        _loadAttempted = true;

        try
        {
            var ext = Path.GetExtension(_dataPath).ToLowerInvariant();
            Log($"[DATA] 파일 로드 시작: {_dataPath} (확장자: {ext})");

            if (ext == ".zip")
                await LoadFromZipAsync(ct);
            else if (ext == ".xlsx" || ext == ".xls")
                LoadFromXlsx(_dataPath);
            else
                await LoadFromCsvAsync(_dataPath, ct);

            Log($"[DATA] 로드 완료: {_records?.Count ?? 0}건");
        }
        catch (Exception ex)
        {
            Log($"[DATA] 로드 실패: {ex.GetType().Name}: {ex.Message}");
        }
    }

    // ──────────────────────────── ZIP 처리 ────────────────────────────

    /// <summary>ZIP 파일에서 XLSX 또는 CSV 추출 후 로드</summary>
    private async Task LoadFromZipAsync(CancellationToken ct)
    {
        using var zip = ZipFile.OpenRead(_dataPath);

        // XLSX 우선 → CSV 차선
        var entry = zip.Entries.FirstOrDefault(e =>
                        e.FullName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    ?? zip.Entries.FirstOrDefault(e =>
                        e.FullName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase));

        if (entry == null)
        {
            Log("[DATA] ZIP 내 XLSX/CSV 파일을 찾을 수 없습니다.");
            return;
        }

        Log($"[DATA] ZIP 내 파일: {entry.FullName} ({entry.Length / 1024.0 / 1024.0:F1} MB)");

        // 임시 파일로 추출
        var tempDir = Path.Combine(Path.GetTempPath(), "MedSearchApp");
        if (!Directory.Exists(tempDir))
            Directory.CreateDirectory(tempDir);

        var tempFile = Path.Combine(tempDir, entry.Name);
        entry.ExtractToFile(tempFile, overwrite: true);

        try
        {
            if (tempFile.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                LoadFromXlsx(tempFile);
            else
                await LoadFromCsvAsync(tempFile, ct);
        }
        finally
        {
            // 임시 파일 정리
            try { File.Delete(tempFile); } catch { }
        }
    }

    // ──────────────────────────── XLSX 처리 ────────────────────────────

    /// <summary>XLSX 파일에서 데이터 로드 (ClosedXML)</summary>
    private void LoadFromXlsx(string path)
    {
        using var workbook = new XLWorkbook(path);
        var worksheet = workbook.Worksheets.First();

        var firstRow = worksheet.FirstRowUsed();
        if (firstRow == null) { Log("[XLSX] 빈 파일"); return; }

        // 헤더 읽기
        var headerRow = firstRow.RowNumber();
        var lastCol = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;
        var headers = new string[lastCol];
        for (int c = 1; c <= lastCol; c++)
            headers[c - 1] = worksheet.Cell(headerRow, c).GetString().Trim();

        Log($"[XLSX] 헤더 ({headers.Length}개): {string.Join(", ", headers.Take(20))}");

        // 필드 인덱스 매핑 (0-based)
        var nameIdx   = FindColumn(headers, "요양기관명칭", "요양기관명", "기관명", "사업장명", "약국명");
        var codeIdx   = FindColumn(headers, "요양기관기호", "암호화요양기호", "ykiho", "관리번호", "개방서비스ID");
        var bizNoIdx  = FindColumn(headers, "사업자등록번호", "사업자번호", "bizrNo", "bizNo");
        var addrIdx   = FindColumn(headers, "주소", "도로명주소", "소재지도로명주소", "소재지주소");
        var postalIdx = FindColumn(headers, "우편번호", "소재지우편번호", "도로명우편번호", "postNo");
        var phoneIdx  = FindColumn(headers, "전화번호", "대표전화번호", "telno");
        var typeIdx   = FindColumn(headers, "종별코드명", "종별", "업태구분명");

        Log($"[XLSX] 매핑: name={nameIdx}, code={codeIdx}, bizNo={bizNoIdx}, addr={addrIdx}, postal={postalIdx}, phone={phoneIdx}, type={typeIdx}");

        if (nameIdx < 0)
        {
            Log("[XLSX] 기관명 컬럼을 찾을 수 없습니다.");
            return;
        }

        // 데이터 행 파싱
        var records = new List<DataRecord>();
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? headerRow;

        for (int r = headerRow + 1; r <= lastRow; r++)
        {
            var record = new DataRecord
            {
                Name       = SafeGetCell(worksheet, r, nameIdx),
                Code       = SafeGetCell(worksheet, r, codeIdx),
                BizNo      = SafeGetCell(worksheet, r, bizNoIdx),
                Address    = SafeGetCell(worksheet, r, addrIdx),
                PostalCode = SafeGetCell(worksheet, r, postalIdx),
                Phone      = SafeGetCell(worksheet, r, phoneIdx),
                TypeName   = SafeGetCell(worksheet, r, typeIdx)
            };

            if (!string.IsNullOrWhiteSpace(record.Name))
                records.Add(record);
        }

        _records = records;
    }

    /// <summary>XLSX 셀 값 안전하게 읽기 (0-based index → 1-based column)</summary>
    private static string SafeGetCell(IXLWorksheet ws, int row, int colIdx0)
    {
        if (colIdx0 < 0) return string.Empty;
        try
        {
            return ws.Cell(row, colIdx0 + 1).GetString().Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    // ──────────────────────────── CSV 처리 ────────────────────────────

    /// <summary>CSV 파일에서 데이터 로드</summary>
    private async Task LoadFromCsvAsync(string path, CancellationToken ct)
    {
        var encoding = DetectEncoding(path);
        Log($"[CSV] 감지된 인코딩: {encoding.EncodingName}");

        using var reader = new StreamReader(path, encoding);
        var headerLine = await reader.ReadLineAsync(ct);
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            Log("[CSV] 빈 파일");
            return;
        }

        var headers = ParseCsvLine(headerLine);
        Log($"[CSV] 헤더 ({headers.Length}개): {string.Join(", ", headers.Take(20))}");

        var nameIdx   = FindColumn(headers, "요양기관명칭", "요양기관명", "기관명", "사업장명", "약국명");
        var codeIdx   = FindColumn(headers, "요양기관기호", "암호화요양기호", "ykiho", "관리번호", "개방서비스ID");
        var bizNoIdx  = FindColumn(headers, "사업자등록번호", "사업자번호", "bizrNo", "bizNo");
        var addrIdx   = FindColumn(headers, "주소", "도로명주소", "소재지도로명주소", "소재지주소");
        var postalIdx = FindColumn(headers, "우편번호", "소재지우편번호", "도로명우편번호", "postNo");
        var phoneIdx  = FindColumn(headers, "전화번호", "대표전화번호", "telno");
        var typeIdx   = FindColumn(headers, "종별코드명", "종별", "업태구분명");

        Log($"[CSV] 매핑: name={nameIdx}, code={codeIdx}, bizNo={bizNoIdx}, addr={addrIdx}, postal={postalIdx}, phone={phoneIdx}, type={typeIdx}");

        if (nameIdx < 0)
        {
            Log("[CSV] 기관명 컬럼을 찾을 수 없습니다.");
            return;
        }

        var records = new List<DataRecord>();
        string? line;
        var lineNo = 1;
        while ((line = await reader.ReadLineAsync(ct)) != null)
        {
            ct.ThrowIfCancellationRequested();
            lineNo++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cols = ParseCsvLine(line);
            var record = new DataRecord
            {
                Name       = SafeGet(cols, nameIdx),
                Code       = SafeGet(cols, codeIdx),
                BizNo      = SafeGet(cols, bizNoIdx),
                Address    = SafeGet(cols, addrIdx),
                PostalCode = SafeGet(cols, postalIdx),
                Phone      = SafeGet(cols, phoneIdx),
                TypeName   = SafeGet(cols, typeIdx)
            };

            if (!string.IsNullOrWhiteSpace(record.Name))
                records.Add(record);
        }

        _records = records;
        Log($"[CSV] 파싱 완료: {records.Count}건 (파일 {lineNo}줄)");
    }

    /// <summary>CSV 행 파싱 (따옴표 처리 포함)</summary>
    private static string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = "";
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current += '"';
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current += c;
                }
            }
            else
            {
                if (c == '"')
                    inQuotes = true;
                else if (c == ',')
                {
                    fields.Add(current.Trim());
                    current = "";
                }
                else
                    current += c;
            }
        }
        fields.Add(current.Trim());
        return fields.ToArray();
    }

    // ──────────────────────────── 공통 유틸리티 ────────────────────────────

    /// <summary>여러 컬럼명 후보 중 첫 번째 매칭 인덱스 반환 (0-based)</summary>
    private static int FindColumn(string[] headers, params string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                if (headers[i].Trim().Equals(candidate, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
        }
        return -1;
    }

    private static string SafeGet(string[] cols, int idx)
        => idx >= 0 && idx < cols.Length ? cols[idx].Trim() : string.Empty;

    /// <summary>EUC-KR/UTF-8 인코딩 자동 감지</summary>
    private static System.Text.Encoding DetectEncoding(string path)
    {
        var bytes = new byte[4096];
        int read;
        using (var fs = File.OpenRead(path))
            read = fs.Read(bytes, 0, bytes.Length);

        if (read >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return System.Text.Encoding.UTF8;

        try
        {
            var utf8 = new System.Text.UTF8Encoding(false, true);
            utf8.GetString(bytes, 0, read);
            return System.Text.Encoding.UTF8;
        }
        catch
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            return System.Text.Encoding.GetEncoding(949);
        }
    }

    private static string FormatBizNo(string raw)
    {
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

    // ──────────────────────────── 정적 유틸리티 ────────────────────────────

    /// <summary>
    /// ZIP 파일에서 XLSX/CSV를 추출하여 앱 데이터 폴더에 저장.
    /// 반환: 추출된 파일 경로 (null이면 실패)
    /// </summary>
    public static string? ExtractDataFromZip(string zipPath)
    {
        try
        {
            using var zip = ZipFile.OpenRead(zipPath);

            var entry = zip.Entries.FirstOrDefault(e =>
                            e.FullName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                        ?? zip.Entries.FirstOrDefault(e =>
                            e.FullName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase));

            if (entry == null) return null;

            var destDir = AppSettings.DefaultDataDir;
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            var ext = Path.GetExtension(entry.Name).ToLowerInvariant();
            var destPath = Path.Combine(destDir, $"bizno_data{ext}");
            entry.ExtractToFile(destPath, overwrite: true);
            return destPath;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>데이터 레코드 내부 모델</summary>
    private class DataRecord
    {
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public string BizNo { get; set; } = "";
        public string Address { get; set; } = "";
        public string PostalCode { get; set; } = "";
        public string Phone { get; set; } = "";
        public string TypeName { get; set; } = "";
    }
}
