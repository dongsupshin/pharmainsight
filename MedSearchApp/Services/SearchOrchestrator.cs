using System.IO;
using MedSearchApp.Models;

namespace MedSearchApp.Services;

/// <summary>
/// 검색 우선순위 조율 서비스
/// 1순위: 로컬 데이터 — ZIP/XLSX/CSV (전국 병의원 및 약국 현황 / LocalData)
/// 2순위: HIRA API (건강보험심사평가원 실시간 검색)
/// </summary>
public class SearchOrchestrator
{
    private readonly IEnumerable<ISearchService> _services;
    private static readonly string LogPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "search_debug.log");

    public SearchOrchestrator(IEnumerable<ISearchService> services)
    {
        _services = services;
    }

    public async Task<SearchResult> SearchAsync(
        string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new SearchResult([], "검색어를 입력해주세요.");

        Log($"=== 검색 시작: '{name}' ({DateTime.Now:HH:mm:ss}) ===");

        foreach (var service in _services)
        {
            Log($"[{service.ServiceName}] IsAvailable={service.IsAvailable}");

            if (!service.IsAvailable)
            {
                Log($"  → API 키 없음, 스킵");
                continue;
            }

            try
            {
                Log($"  → API 호출 중...");
                var items = await service.SearchByNameAsync(name, ct);
                Log($"  → 결과 {items.Count}건");

                if (items.Count > 0)
                {
                    Log($"  → 반환: {items.Count}건");
                    return new SearchResult(items, $"[{service.ServiceName}] {items.Count}건 검색됨");
                }
            }
            catch (OperationCanceledException)
            {
                Log($"  → 취소됨");
                throw;
            }
            catch (Exception ex)
            {
                Log($"  → 오류: {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                    Log($"     InnerException: {ex.InnerException.Message}");
            }
        }

        Log($"=== 결과 없음 ===");
        return new SearchResult([], "검색 결과가 없습니다. (API 키를 확인해주세요)");
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

public record SearchResult(List<MedicalInstitution> Items, string Message);
