namespace MedSearchApp.Models;

/// <summary>
/// 병원·약국 조회 결과 모델
/// </summary>
public class MedicalInstitution
{
    /// <summary>요양기관명 (병원명/약국명)</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>요양기관코드 (HIRA: 암호화 ykiho / LocalData: 관리번호)</summary>
    public string InstitutionCode { get; set; } = string.Empty;

    /// <summary>사업자등록번호 (사용자가 직접 입력·저장)</summary>
    public string BusinessNumber { get; set; } = string.Empty;

    /// <summary>도로명 주소</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>우편번호</summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>시도명</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>시군구명</summary>
    public string District { get; set; } = string.Empty;

    /// <summary>종별 (상급종합병원, 종합병원, 병원, 의원, 약국 등)</summary>
    public string InstitutionType { get; set; } = string.Empty;

    /// <summary>전화번호</summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>데이터 출처</summary>
    public string DataSource { get; set; } = string.Empty;
}
