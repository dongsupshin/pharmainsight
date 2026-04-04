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

### 배포 현황
- ✅ Microsoft 개발자 계정 등록 완료
- ✅ 앱 이름 "Pharma Insight KR" 예약 완료 (partner.microsoft.com)
- ✅ MSIX 패키지 빌드 및 Windows 앱 인증 키트(WACK) 통과
- ✅ Partner Center 제출 완료 → **현재 In Certification 심사 중**
- **앱 이름**: Pharma Insight KR
- **가격**: 무료
- **배포 지역**: 한국
- **카테고리**: Business
- **패키지**: PharmaInsight_1.0.17.0_x64_bundle.msixupload

---

### MSIX 패키징 시행착오 및 해결 방법

#### 1. 앱 이름 예약 오류 (Visual Studio에서 "예기치 않은 오류")
- **원인**: Visual Studio에서 직접 예약 시 간헐적 오류 발생
- **해결**: partner.microsoft.com에서 직접 앱 이름 예약

#### 2. `project.assets.json` 누락 오류 (NETSDK1047)
- **원인**: `MedSearchApp.csproj`에 `RuntimeIdentifiers` 미설정 → wappublish 경로에 win-x64 assets 미생성
- **해결**: `MedSearchApp.csproj`의 `PropertyGroup`에 아래 추가
```xml
<RuntimeIdentifiers>win-x64;win-x86;win-arm64</RuntimeIdentifiers>
```
이후 MSBuild로 restore (VS 설치 경로가 `18`인 점 주의):
```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" `
  "PharmaInsight\PharmaInsight.wapproj" `
  /t:Restore /p:RuntimeIdentifier=win-x64 /p:Configuration=Release /p:Platform=x64
```

#### 3. `AppCert.exe` 도구를 찾지 못하는 오류
- **원인**: Windows SDK 실제 설치 경로(`C:\Program Files (x86)\Windows Kits\10\`)와 레지스트리 `KitsRoot10` 값(`C:\Program Files\Windows Kits\10\`) 불일치
- **해결**: Visual Studio를 관리자 권한으로 실행 후 패키지 관리자 콘솔에서:
```powershell
Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows Kits\Installed Roots" `
  -Name "KitsRoot10" -Value "C:\Program Files (x86)\Windows Kits\10\"
```

#### 4. WACK 브랜딩 오류 (기본 이미지 사용)
- **원인**: 템플릿 기본 이미지(StoreLogo, Square44x44 등) 그대로 사용
- **해결**: `PharmaInsight/Images/` 폴더의 모든 이미지를 앱 전용 이미지로 교체
  - StoreLogo.png (50×50)
  - Square44x44Logo.scale-200.png (88×88)
  - Square44x44Logo.targetsize-24_altform-unplated.png (24×24)
  - Square150x150Logo.scale-200.png (300×300)
  - Wide310x150Logo.scale-200.png (620×300)
  - SplashScreen.scale-200.png (1240×600)
  - LockScreenLogo.scale-200.png (48×48)

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
