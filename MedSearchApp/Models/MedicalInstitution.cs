namespace MedSearchApp.Models;

/// <summary>
/// 병원·약국 조회 결과 모델
/// Pro 티어 (B2B 영업 타겟팅) 필드: 의사수, 간호사수, 추정매출등급 등
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

    // ──────────────────── B2B Pro 타겟팅 필드 (HIRA getHospBasisList 기반) ────────────────────

    /// <summary>의사 총수 (drTotCnt) = 일반의 + 인턴 + 레지던트 + 전문의 (의과+치과+한방 합산)</summary>
    public int DoctorTotalCount { get; set; }

    /// <summary>전문의 수 (의과 전문의 + 치과 전문의 + 한방 전문의)</summary>
    public int SpecialistCount { get; set; }

    /// <summary>레지던트 수 (mdeptResdntCnt + detyResdntCnt + cmdcResdntCnt)</summary>
    public int ResidentCount { get; set; }

    /// <summary>인턴 수 (mdeptIntnCnt + detyIntnCnt + cmdcIntnCnt)</summary>
    public int InternCount { get; set; }

    /// <summary>간호사 수 (pnursCnt)</summary>
    public int NurseCount { get; set; }

    /// <summary>설립일 (estbDd, yyyyMMdd)</summary>
    public string EstablishedDate { get; set; } = string.Empty;

    /// <summary>좌표 X (경도)</summary>
    public string XPos { get; set; } = string.Empty;

    /// <summary>좌표 Y (위도)</summary>
    public string YPos { get; set; } = string.Empty;

    // ──────────────────── 추정 매출/규모 등급 (B2B 영업 리드 스코어링) ────────────────────

    /// <summary>
    /// 추정 매출 등급: S / A / B / C / -
    /// 종별 가중치 × (의사수, 간호사수) 룰 기반. RevenueEstimator에서 산정.
    /// ※ 실매출 아님. HIRA 공개 통계(종별 1인당 평균 요양급여비) 프록시.
    /// </summary>
    public string RevenueGrade { get; set; } = "-";

    /// <summary>추정 연간 요양급여 (억원, 매우 러프한 추정치 - 영업 우선순위 책정용)</summary>
    public double EstimatedAnnualRevenueBillionKrw { get; set; }

    /// <summary>등급 산정 근거 요약 (툴팁용)</summary>
    public string RevenueGradeReason { get; set; } = string.Empty;

    /// <summary>
    /// 규모 점수 (0~100, 소팅용). 의사수+간호사수 기반 정규화 값.
    /// Pro 티어 전용.
    /// </summary>
    public int SizeScore { get; set; }

    /// <summary>얕은 복사 — 마스킹용 뷰 복사본 생성 (원본 _allResults 보호)</summary>
    public MedicalInstitution Clone() => (MedicalInstitution)MemberwiseClone();
}
