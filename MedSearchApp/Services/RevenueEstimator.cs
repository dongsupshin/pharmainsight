using MedSearchApp.Models;

namespace MedSearchApp.Services;

/// <summary>
/// 요양기관 추정 매출/규모 등급 산정 (B2B 영업 리드 스코어링)
///
/// ※ 개별 병원 실매출은 공공데이터로 공개되지 않음.
///   아래 계수는 건강보험공단/HIRA의 연도별 종별 요양급여비 합계 통계를
///   기관 수·의사 수로 역산한 "대략적 평균"이며, 실제와 다를 수 있음.
///   → 영업 우선순위 책정용 프록시로만 사용.
///
/// 종별 1인 의사당 연 요양급여비 평균 (근사값, 단위: 억원):
///   상급종합병원: 약 4.5
///   종합병원    : 약 2.5
///   병원        : 약 1.7
///   요양병원    : 약 1.0
///   의원        : 약 1.2
///   치과병원    : 약 1.0
///   치과의원    : 약 0.6
///   한방병원    : 약 0.9
///   한의원      : 약 0.4
///   약국 (약사1인) : 약 2.5 (처방조제료 기준)
/// </summary>
public static class RevenueEstimator
{
    /// <summary>등급 및 점수 산정 후 item에 직접 할당</summary>
    public static void Apply(MedicalInstitution item)
    {
        var perDoctorRevenue = GetPerDoctorAnnualRevenueBillion(item.InstitutionType);
        var perPharmacistRevenue = GetPerPharmacistAnnualRevenueBillion(item.InstitutionType);

        double estimated;
        if (IsPharmacy(item.InstitutionType))
        {
            // 약국은 의사 수 없음 → 평균 약사 1~2인 가정
            estimated = perPharmacistRevenue * 1.5;
        }
        else if (item.DoctorTotalCount > 0)
        {
            estimated = perDoctorRevenue * item.DoctorTotalCount;
        }
        else
        {
            item.RevenueGrade = "-";
            item.EstimatedAnnualRevenueBillionKrw = 0;
            item.RevenueGradeReason = "데이터 부족";
            item.SizeScore = 0;
            return;
        }

        item.EstimatedAnnualRevenueBillionKrw = Math.Round(estimated, 1);
        item.RevenueGrade = GradeFromRevenue(estimated, item.InstitutionType);
        item.SizeScore = ComputeSizeScore(item);
        item.RevenueGradeReason = BuildReason(item, perDoctorRevenue, perPharmacistRevenue);
    }

    // ──────────────────────────── 계수 테이블 ────────────────────────────

    private static double GetPerDoctorAnnualRevenueBillion(string type)
    {
        // type은 clCdNm 값 ("상급종합병원" 등)
        if (string.IsNullOrWhiteSpace(type)) return 1.5;

        if (type.Contains("상급종합")) return 4.5;
        if (type.Contains("종합병원")) return 2.5;
        if (type.Contains("요양병원")) return 1.0;
        if (type.Contains("정신병원")) return 1.2;
        if (type.Contains("치과병원")) return 1.0;
        if (type.Contains("치과의원")) return 0.6;
        if (type.Contains("한방병원")) return 0.9;
        if (type.Contains("한의원"))   return 0.4;
        if (type.Contains("조산원"))   return 0.3;
        if (type.Contains("병원"))     return 1.7;  // 일반 병원
        if (type.Contains("의원"))     return 1.2;  // 일반 의원
        return 1.5;
    }

    private static double GetPerPharmacistAnnualRevenueBillion(string type) => 2.5;

    private static bool IsPharmacy(string type)
        => type?.Contains("약국") == true;

    // ──────────────────────────── 등급 산정 ────────────────────────────

    /// <summary>
    /// 등급 기준 (추정 연 요양급여, 억원):
    ///   S : 1,000 이상 (상급종합 대형)
    ///   A : 100 ~ 1,000 (종합병원급)
    ///   B : 10 ~ 100 (중소 병원, 약국 상위)
    ///   C : 10 미만 (의원, 일반 약국)
    /// </summary>
    private static string GradeFromRevenue(double billionKrw, string type)
    {
        if (billionKrw >= 1000) return "S";
        if (billionKrw >= 100)  return "A";
        if (billionKrw >= 10)   return "B";
        if (billionKrw > 0)     return "C";
        return "-";
    }

    // ──────────────────────────── 규모 점수 (0~100) ────────────────────────────

    private static int ComputeSizeScore(MedicalInstitution item)
    {
        if (IsPharmacy(item.InstitutionType))
            return 20; // 약국은 일괄 기본값

        // 의사수 가중치 (0~70)
        int docScore = item.DoctorTotalCount switch
        {
            >= 300 => 70,
            >= 100 => 60,
            >= 50  => 50,
            >= 20  => 40,
            >= 10  => 30,
            >= 5   => 20,
            >= 1   => 10,
            _      => 0
        };

        // 간호사수 가중치 (0~30)
        int nurseScore = item.NurseCount switch
        {
            >= 500 => 30,
            >= 200 => 25,
            >= 100 => 20,
            >= 50  => 15,
            >= 20  => 10,
            >= 5   => 5,
            _      => 0
        };

        return Math.Min(100, docScore + nurseScore);
    }

    private static string BuildReason(
        MedicalInstitution item,
        double perDoctorRevenue,
        double perPharmacistRevenue)
    {
        if (IsPharmacy(item.InstitutionType))
            return $"약국 평균 (약사 1~2인 × {perPharmacistRevenue}억원/인)";

        var parts = new List<string>
        {
            $"종별={item.InstitutionType}",
            $"의사={item.DoctorTotalCount}명"
        };
        if (item.SpecialistCount > 0) parts.Add($"전문의={item.SpecialistCount}명");
        if (item.NurseCount > 0)      parts.Add($"간호사={item.NurseCount}명");
        parts.Add($"계수={perDoctorRevenue}억원/의사");
        return string.Join(" · ", parts);
    }
}
