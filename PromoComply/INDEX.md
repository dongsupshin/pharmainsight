# PromoComply Project Index

## Quick Navigation

### Getting Started

1. **For End Users**: Read [README.md](README.md) - Installation and usage guide
2. **For Developers**: Read [QUICK_START.md](QUICK_START.md) - Setup and development guide
3. **For Deployment**: Read [BUILD_AND_DEPLOY.md](BUILD_AND_DEPLOY.md) - Build and distribution
4. **For Reference**: Read [PROJECT_SUMMARY.txt](PROJECT_SUMMARY.txt) - Complete overview

### Key Documentation Files

| File | Purpose | Audience | Pages |
|------|---------|----------|-------|
| [README.md](README.md) | User guide and features overview | End Users, Managers | 15 |
| [FEATURES.md](FEATURES.md) | Detailed feature documentation | Users, Developers | 30 |
| [BUILD_AND_DEPLOY.md](BUILD_AND_DEPLOY.md) | Build and deployment instructions | Developers, DevOps | 25 |
| [QUICK_START.md](QUICK_START.md) | Quick reference for setup and development | Developers | 20 |
| [PROJECT_SUMMARY.txt](PROJECT_SUMMARY.txt) | Complete project overview and inventory | Everyone | 10 |

---

## Project Structure

```
PromoComply/
├── README.md                          # Start here for users
├── FEATURES.md                        # Feature documentation
├── QUICK_START.md                     # Developer quick start
├── BUILD_AND_DEPLOY.md               # Build instructions
├── PROJECT_SUMMARY.txt               # Project overview
├── INDEX.md                          # This file
├── .gitignore                        # Git configuration
│
├── PromoComply.sln                   # Solution file
│
└── src/PromoComply/
    ├── PromoComply.csproj           # Project file
    ├── App.xaml & App.xaml.cs       # Application entry
    ├── MainWindow.xaml & .cs        # Main window
    ├── AssemblyInfo.cs              # Assembly metadata
    │
    ├── Models/                      # Data models (12 files)
    │   ├── FileType.cs              # Enum for file types
    │   ├── DocumentStatus.cs        # Enum for document status
    │   ├── ClaimType.cs             # Enum for claim types
    │   ├── RiskLevel.cs             # Enum for risk levels
    │   ├── PromoClaim.cs            # Claim model
    │   ├── PromoDocument.cs         # Document model
    │   ├── ComplianceIssue.cs       # Issue model
    │   ├── ReviewSession.cs         # Review session model
    │   └── ... (6 more enum/class files)
    │
    ├── Services/                    # Business logic (10 files)
    │   ├── DocumentParserService.cs # PDF/DOCX/PPTX parsing
    │   ├── ClaimsDetectorService.cs # Claim detection
    │   ├── ComplianceCheckerService.cs # Compliance validation
    │   ├── ReportGeneratorService.cs # Report generation
    │   ├── JsonProjectRepository.cs # Data persistence
    │   └── ... (5 more interface/service files)
    │
    ├── ViewModels/                  # MVVM ViewModels (3 files)
    │   ├── MainViewModel.cs         # Main orchestrator
    │   ├── DashboardViewModel.cs    # Dashboard metrics
    │   └── DocumentReviewViewModel.cs # Review interface
    │
    ├── Views/                       # XAML Views (10 files)
    │   ├── MainWindow.xaml          # Application shell
    │   ├── DashboardView.xaml       # Dashboard UI
    │   ├── DocumentListView.xaml    # Document management
    │   ├── DocumentReviewView.xaml  # Review interface
    │   ├── ReportView.xaml          # Report display
    │   └── ... (5 more .xaml and .cs files)
    │
    ├── Converters/                  # Value converters (4 files)
    │   ├── StatusToColorConverter.cs
    │   ├── SeverityToColorConverter.cs
    │   ├── ScoreToColorConverter.cs
    │   └── BoolToVisibilityConverter.cs
    │
    ├── Themes/                      # UI Styling (1 file)
    │   └── AppTheme.xaml            # Professional dark theme
    │
    ├── Assets/                      # Application assets
    │   └── README.txt               # Asset guide
    │
    └── Package.appxmanifest         # MSIX package manifest
```

---

## Technology Stack

- **Framework**: .NET 8.0 (net8.0-windows)
- **UI**: Windows Presentation Foundation (WPF)
- **MVVM**: CommunityToolkit.Mvvm 8.2.2
- **DI**: Microsoft.Extensions.DependencyInjection 8.0.0
- **PDF**: PdfPig 0.1.8
- **Office**: DocumentFormat.OpenXml 2.20.0
- **IDE**: Visual Studio 2022

---

## Core Features

### 1. Document Processing
- PDF, DOCX, PPTX file support
- Async text extraction
- Multi-page handling

### 2. Claim Detection
- Efficacy claims
- Safety claims
- Superiority claims
- Comparative claims
- Economic claims
- Risk level assignment
- Reference detection

### 3. Compliance Validation
- Fair balance checking
- ISI presence verification
- Black Box warning detection
- Reference substantiation
- Off-label detection
- Adverse event validation
- Misleading presentation assessment

### 4. Interactive Review
- Claim approval/flagging
- Issue resolution tracking
- Review notes
- Real-time feedback

### 5. Reporting
- Text reports (TXT)
- CSV exports
- Compliance scoring
- Recommendations

### 6. Dashboard Analytics
- Summary metrics
- Activity tracking
- Score distribution
- Trend analysis

---

## How to Use This Project

### For Users
1. Read [README.md](README.md) for installation and basic usage
2. Check [FEATURES.md](FEATURES.md) for detailed feature descriptions
3. Refer to specific sections as needed

### For Developers
1. Start with [QUICK_START.md](QUICK_START.md)
2. Review [PROJECT_SUMMARY.txt](PROJECT_SUMMARY.txt) for architecture
3. Explore code starting with `MainViewModel.cs`
4. Review specific services for functionality details
5. Use [BUILD_AND_DEPLOY.md](BUILD_AND_DEPLOY.md) for building/deploying

### For Deployment
1. Read [BUILD_AND_DEPLOY.md](BUILD_AND_DEPLOY.md) thoroughly
2. Choose distribution method (MSI, MSIX, or self-contained)
3. Follow format-specific instructions
4. Test thoroughly before release

---

## Important Files to Know

| File | Purpose |
|------|---------|
| `PromoComply.csproj` | Project configuration and dependencies |
| `App.xaml.cs` | Dependency injection setup |
| `MainWindow.xaml` | Application shell and navigation |
| `MainViewModel.cs` | Main application logic |
| `ClaimsDetectorService.cs` | Claim detection engine |
| `ComplianceCheckerService.cs` | Compliance validation engine |
| `Themes/AppTheme.xaml` | All styling definitions |

---

## Build Instructions

### Quick Build (Development)
```powershell
dotnet build
dotnet run --project src/PromoComply/PromoComply.csproj
```

### Release Build
```powershell
dotnet build --configuration Release
```

### Publish for Distribution
```powershell
dotnet publish -c Release --self-contained -r win-x64
```

See [BUILD_AND_DEPLOY.md](BUILD_AND_DEPLOY.md) for detailed instructions.

---

## Development Guidelines

- Follow C# naming conventions (PascalCase for classes, camelCase for fields)
- Use MVVM pattern for UI logic
- Register all services in `App.xaml.cs`
- Update documentation when adding features
- Test thoroughly before committing
- Use meaningful commit messages

---

## Support and Contact

- **Product**: PromoComply v1.0.0
- **Publisher**: PharmaInsight
- **Target**: Microsoft Store distribution
- **Status**: Production-ready

---

## Quick Links

- [User Guide](README.md)
- [Features Reference](FEATURES.md)
- [Build Instructions](BUILD_AND_DEPLOY.md)
- [Developer Guide](QUICK_START.md)
- [Project Overview](PROJECT_SUMMARY.txt)

---

## Version History

| Version | Date | Status |
|---------|------|--------|
| 1.0.0 | 2024 | Complete |

---

**Last Updated**: 2024
**Documentation Version**: 1.0
