using MedSearchApp.Models;

namespace MedSearchApp.Services;

/// <summary>
/// 내장 샘플 데이터 서비스 — API 키나 로컬 파일 없이도 즉시 동작.
/// 전국 주요 병원·약국 데이터를 포함하며, 항상 IsAvailable = true.
/// </summary>
public class SampleDataService : ISearchService
{
    public string ServiceName => "샘플 데이터";
    public bool IsAvailable   => true;

    private static readonly List<MedicalInstitution> _data = new()
    {
        // ── 상급종합병원 ─────────────────────────────────────────────
        new() { Name="서울대학교병원",           InstitutionCode="JDQ4MTg4MSM1MS",  BusinessNumber="110-82-00670", Address="서울특별시 종로구 대학로 101",              PostalCode="03080", City="서울", District="종로구", InstitutionType="상급종합병원", PhoneNumber="02-2072-2114", DataSource="샘플 데이터" },
        new() { Name="삼성서울병원",             InstitutionCode="JDQ4MTg0MSM1MS",  BusinessNumber="211-82-04879", Address="서울특별시 강남구 일원로 81",               PostalCode="06351", City="서울", District="강남구", InstitutionType="상급종합병원", PhoneNumber="02-3410-2114", DataSource="샘플 데이터" },
        new() { Name="연세대학교세브란스병원",    InstitutionCode="JDQ4MTg1MSM1MS",  BusinessNumber="117-82-00333", Address="서울특별시 서대문구 연세로 50-1",           PostalCode="03722", City="서울", District="서대문구", InstitutionType="상급종합병원", PhoneNumber="02-2228-1234", DataSource="샘플 데이터" },
        new() { Name="서울아산병원",             InstitutionCode="JDQ4MTg2MSM1MS",  BusinessNumber="209-82-05307", Address="서울특별시 송파구 올림픽로43길 88",          PostalCode="05505", City="서울", District="송파구", InstitutionType="상급종합병원", PhoneNumber="02-3010-3114", DataSource="샘플 데이터" },
        new() { Name="가톨릭대학교서울성모병원",  InstitutionCode="JDQ4MTg3MSM1MS",  BusinessNumber="202-82-00139", Address="서울특별시 서초구 반포대로 222",             PostalCode="06591", City="서울", District="서초구", InstitutionType="상급종합병원", PhoneNumber="02-2258-5745", DataSource="샘플 데이터" },
        new() { Name="고려대학교안암병원",        InstitutionCode="JDQ4MTg4MSM2MS",  BusinessNumber="209-82-00562", Address="서울특별시 성북구 인촌로 73",               PostalCode="02841", City="서울", District="성북구", InstitutionType="상급종합병원", PhoneNumber="02-920-5114", DataSource="샘플 데이터" },
        new() { Name="한양대학교병원",            InstitutionCode="JDQ4MTg5MSM1MS",  BusinessNumber="204-82-01478", Address="서울특별시 성동구 왕십리로 222-1",          PostalCode="04763", City="서울", District="성동구", InstitutionType="상급종합병원", PhoneNumber="02-2290-8114", DataSource="샘플 데이터" },
        new() { Name="강남세브란스병원",          InstitutionCode="JDQ4MTkwMSM1MS",  BusinessNumber="211-82-01921", Address="서울특별시 강남구 언주로 211",              PostalCode="06273", City="서울", District="강남구", InstitutionType="상급종합병원", PhoneNumber="02-2019-3114", DataSource="샘플 데이터" },
        new() { Name="분당서울대학교병원",        InstitutionCode="JDQ4MTkxMSM1MS",  BusinessNumber="220-82-06565", Address="경기도 성남시 분당구 구미로173번길 82",      PostalCode="13620", City="경기", District="성남시", InstitutionType="상급종합병원", PhoneNumber="031-787-7114", DataSource="샘플 데이터" },
        new() { Name="강북삼성병원",             InstitutionCode="JDQ4MTkyMSM1MS",  BusinessNumber="204-82-01594", Address="서울특별시 종로구 새문안로 29",             PostalCode="03181", City="서울", District="종로구", InstitutionType="종합병원",   PhoneNumber="02-2001-2001", DataSource="샘플 데이터" },

        // ── 지방 주요 병원 ───────────────────────────────────────────
        new() { Name="부산대학교병원",            InstitutionCode="JDQ4MTkzMSM1MS",  BusinessNumber="215-82-03560", Address="부산광역시 서구 구덕로 179",                PostalCode="49241", City="부산", District="서구",   InstitutionType="상급종합병원", PhoneNumber="051-240-7000", DataSource="샘플 데이터" },
        new() { Name="경북대학교병원",            InstitutionCode="JDQ4MTk0MSM1MS",  BusinessNumber="514-82-07834", Address="대구광역시 중구 동덕로 130",                PostalCode="41944", City="대구", District="중구",   InstitutionType="상급종합병원", PhoneNumber="053-200-5114", DataSource="샘플 데이터" },
        new() { Name="전남대학교병원",            InstitutionCode="JDQ4MTk1MSM1MS",  BusinessNumber="409-82-02810", Address="광주광역시 동구 제봉로 42",                 PostalCode="61469", City="광주", District="동구",   InstitutionType="상급종합병원", PhoneNumber="062-220-5114", DataSource="샘플 데이터" },
        new() { Name="충남대학교병원",            InstitutionCode="JDQ4MTk2MSM1MS",  BusinessNumber="314-82-02209", Address="대전광역시 중구 문화로 282",                PostalCode="35015", City="대전", District="중구",   InstitutionType="상급종합병원", PhoneNumber="042-280-7114", DataSource="샘플 데이터" },
        new() { Name="제주대학교병원",            InstitutionCode="JDQ4MTk3MSM1MS",  BusinessNumber="616-82-00062", Address="제주특별자치도 제주시 아란13길 15",         PostalCode="63241", City="제주", District="제주시", InstitutionType="상급종합병원", PhoneNumber="064-717-1114", DataSource="샘플 데이터" },
        new() { Name="인하대학교병원",            InstitutionCode="JDQ4MTk4MSM1MS",  BusinessNumber="130-82-00308", Address="인천광역시 중구 인항로 27",                 PostalCode="22332", City="인천", District="중구",   InstitutionType="상급종합병원", PhoneNumber="032-890-2114", DataSource="샘플 데이터" },
        new() { Name="강원대학교병원",            InstitutionCode="JDQ4MTk5MSM1MS",  BusinessNumber="221-82-08323", Address="강원특별자치도 춘천시 백령로 156",          PostalCode="24289", City="강원", District="춘천시", InstitutionType="종합병원",   PhoneNumber="033-258-2000", DataSource="샘플 데이터" },

        // ── 종합병원 ─────────────────────────────────────────────────
        new() { Name="이대목동병원",             InstitutionCode="JDQ4MjAwMSM1MS",  BusinessNumber="107-82-00785", Address="서울특별시 양천구 안양천로 1071",           PostalCode="07985", City="서울", District="양천구", InstitutionType="종합병원",   PhoneNumber="02-2650-5114", DataSource="샘플 데이터" },
        new() { Name="중앙대학교병원",            InstitutionCode="JDQ4MjAxMSM1MS",  BusinessNumber="110-82-02661", Address="서울특별시 동작구 흑석로 102",              PostalCode="06974", City="서울", District="동작구", InstitutionType="상급종합병원", PhoneNumber="02-6299-1114", DataSource="샘플 데이터" },
        new() { Name="경희대학교병원",            InstitutionCode="JDQ4MjAyMSM1MS",  BusinessNumber="201-82-03282", Address="서울특별시 동대문구 경희대로 23",           PostalCode="02447", City="서울", District="동대문구", InstitutionType="상급종합병원", PhoneNumber="02-958-8114", DataSource="샘플 데이터" },
        new() { Name="노원을지대학교병원",        InstitutionCode="JDQ4MjAzMSM1MS",  BusinessNumber="217-82-02825", Address="서울특별시 노원구 한글비석로 68",           PostalCode="01830", City="서울", District="노원구", InstitutionType="종합병원",   PhoneNumber="02-970-8000", DataSource="샘플 데이터" },
        new() { Name="명지병원",                 InstitutionCode="JDQ4MjA0MSM1MS",  BusinessNumber="128-82-00193", Address="경기도 고양시 덕양구 화수로14번길 55",      PostalCode="10475", City="경기", District="고양시", InstitutionType="종합병원",   PhoneNumber="031-810-5114", DataSource="샘플 데이터" },

        // ── 의원 ─────────────────────────────────────────────────────
        new() { Name="연세의원",                 InstitutionCode="JDQ4MjA1MSM1MS",  BusinessNumber="",             Address="서울특별시 강남구 테헤란로 123",            PostalCode="06234", City="서울", District="강남구", InstitutionType="의원",       PhoneNumber="02-555-1234", DataSource="샘플 데이터" },
        new() { Name="서울내과의원",             InstitutionCode="JDQ4MjA2MSM1MS",  BusinessNumber="",             Address="서울특별시 마포구 마포대로 45",             PostalCode="04175", City="서울", District="마포구", InstitutionType="의원",       PhoneNumber="02-711-2345", DataSource="샘플 데이터" },

        // ── 약국 ─────────────────────────────────────────────────────
        new() { Name="온누리약국",               InstitutionCode="JDQ4MjA3MSM1MS",  BusinessNumber="",             Address="서울특별시 종로구 종로 1",                 PostalCode="03154", City="서울", District="종로구", InstitutionType="약국",       PhoneNumber="02-732-5678", DataSource="샘플 데이터" },
        new() { Name="서울약국",                 InstitutionCode="JDQ4MjA4MSM1MS",  BusinessNumber="",             Address="서울특별시 중구 을지로 10",                PostalCode="04523", City="서울", District="중구",   InstitutionType="약국",       PhoneNumber="02-777-8901", DataSource="샘플 데이터" },
        new() { Name="한마음약국",               InstitutionCode="JDQ4MjA5MSM1MS",  BusinessNumber="",             Address="경기도 수원시 팔달구 효원로 1",             PostalCode="16489", City="경기", District="수원시", InstitutionType="약국",       PhoneNumber="031-222-3456", DataSource="샘플 데이터" },
        new() { Name="부산약국",                 InstitutionCode="JDQ4MjEwMSM1MS",  BusinessNumber="",             Address="부산광역시 부산진구 중앙대로 666",          PostalCode="47259", City="부산", District="부산진구", InstitutionType="약국",      PhoneNumber="051-808-9012", DataSource="샘플 데이터" },
        new() { Name="대구중앙약국",             InstitutionCode="JDQ4MjExMSM1MS",  BusinessNumber="",             Address="대구광역시 중구 동성로 10",                PostalCode="41183", City="대구", District="중구",   InstitutionType="약국",       PhoneNumber="053-424-5678", DataSource="샘플 데이터" },
    };

    public Task<List<MedicalInstitution>> SearchByNameAsync(
        string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Task.FromResult(new List<MedicalInstitution>());

        var q = name.Trim().ToLowerInvariant();

        var results = _data
            .Where(x => x.Name.ToLowerInvariant().Contains(q)
                     || x.City.ToLowerInvariant().Contains(q)
                     || x.InstitutionType.ToLowerInvariant().Contains(q))
            .ToList();

        return Task.FromResult(results);
    }
}
