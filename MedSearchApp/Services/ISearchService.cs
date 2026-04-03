using MedSearchApp.Models;

namespace MedSearchApp.Services;

public interface ISearchService
{
    /// <summary>기관명으로 검색</summary>
    Task<List<MedicalInstitution>> SearchByNameAsync(string name, CancellationToken ct = default);

    /// <summary>이 서비스를 사용 가능한지 여부 (API 키 필요 여부 등)</summary>
    bool IsAvailable { get; }

    /// <summary>서비스 이름</summary>
    string ServiceName { get; }
}
