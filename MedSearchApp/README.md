# 🏥 병원·약국 사업자번호 조회 앱 (MedSearchApp)

병원명 또는 약국명을 입력하면 사업자번호, 주소, 우편번호를 조회해주는 Windows 데스크톱 앱입니다.

---

## 📁 프로젝트 구조

```
MedSearchApp/
├── Models/
│   └── MedicalInstitution.cs       # 조회 결과 데이터 모델
├── Services/
│   ├── ISearchService.cs           # 검색 서비스 인터페이스
│   ├── HiraApiService.cs           # ① HIRA (건강보험심사평가원) API - 1순위
│   ├── LocalDataService.cs         # ② LocalData (행정안전부) API - 2순위
│   └── SearchOrchestrator.cs       # 우선순위 조율 (1→2 순서로 검색)
├── ViewModels/
│   └── MainViewModel.cs            # MVVM ViewModel (검색 로직)
├── Views/
│   ├── MainWindow.xaml             # 메인 UI
│   └── MainWindow.xaml.cs
├── Converters/
│   └── BoolToVisibilityConverter.cs
├── App.xaml / App.xaml.cs          # 앱 시작점, DI 설정
├── AppSettings.cs                  # API 키 설정 관리
├── appsettings.json                # ← API 키 입력 파일
└── MedSearchApp.csproj
```

---

## 🔧 빌드 방법

### 사전 요구사항
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (무료)
- [Visual Studio 2022 Community](https://visualstudio.microsoft.com/ko/vs/community/) (무료)
  - "Windows 개발" 워크로드 설치 필요

또는 Visual Studio 없이 명령줄로 빌드:
```bash
dotnet build
dotnet run
```

### 실행 파일 생성
```bash
# Windows 단일 실행 파일로 빌드
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

---

## 🔑 API 키 발급 방법

### 1순위: 건강보험심사평가원 (HIRA) API - 가장 정확한 의료기관 데이터

1. [공공데이터포털](https://www.data.go.kr) 접속 → 회원가입
2. 검색창에 **"건강보험심사평가원 병원정보서비스"** 검색
3. **활용신청** 클릭 → 승인 (보통 1~2일 소요)
4. 마이페이지 → 개발계정 → **일반 인증키(Encoding)** 복사
5. `appsettings.json` 파일의 `HiraApiKey`에 붙여넣기

```json
{
  "HiraApiKey": "여기에_발급받은_키_입력",
  "LocalDataApiKey": ""
}
```

> 📌 약국 검색을 위해서는 추가로 **"건강보험심사평가원 약국정보서비스"**도 활용신청 권장

### 2순위: 행정안전부 LocalData API (HIRA에서 못 찾을 경우 자동 전환)

1. [지방행정인허가데이터개방](https://www.localdata.go.kr/devcenter/main.do) 접속
2. 개발자 센터 → API 키 발급 신청
3. `appsettings.json`의 `LocalDataApiKey`에 입력

---

## ✨ 주요 기능

| 기능 | 설명 |
|------|------|
| 이름 검색 | 병원명/약국명으로 검색 |
| 사업자번호 조회 | XXX-XX-XXXXX 형식으로 표시 |
| 주소/우편번호 | 도로명 주소 + 5자리 우편번호 |
| 복사 기능 | 사업자번호/주소 원클릭 클립보드 복사 |
| CSV 내보내기 | 검색 결과 전체를 CSV로 저장 (Excel 호환) |
| 검색 취소 | 검색 중 취소 가능 |

---

## 🏪 Microsoft Store 배포

### MSIX 패키징 (Store 제출용)

Visual Studio에서:
1. 솔루션 탐색기 → 프로젝트 우클릭 → **게시(Publish)**
2. Microsoft Store 선택 → 패키징 마법사 진행

또는 명령줄:
```bash
dotnet publish -c Release -r win-x64 /p:AppxPackage=true
```

### 필요한 추가 작업 (Store 제출 전)
- [ ] 앱 아이콘 (`app.ico`, 각종 해상도 PNG) 추가
- [ ] Package.appxmanifest 작성
- [ ] Microsoft 개발자 계정 등록 ($19 일회성)
- [ ] 개인정보처리방침 URL 준비 (무료 호스팅 가능)

---

## 📦 사용 라이브러리 (모두 무료/MIT)

| 라이브러리 | 용도 | 라이선스 |
|-----------|------|---------|
| CommunityToolkit.Mvvm | MVVM 패턴 구현 | MIT |
| Microsoft.Extensions.Http | HttpClient 관리 | MIT |
| System.Text.Json | JSON 파싱 | MIT |
| CsvHelper | CSV 내보내기 | Apache 2.0 |

---

## 🔄 검색 우선순위 흐름

```
사용자 입력 (병원명/약국명)
    ↓
[1순위] HIRA API (건강보험심사평가원)
    → 결과 있음 → 표시
    → API 키 없음 / 결과 없음 / 오류
    ↓
[2순위] LocalData API (행정안전부)
    → 결과 있음 → 표시
    → 결과 없음
    ↓
"검색 결과가 없습니다" 메시지
```
