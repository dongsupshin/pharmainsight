# PromoComply Quick Start Guide

## For End Users

### Installation

1. Download PromoComply from Microsoft Store or from the installer file
2. Run the installer or click "Install" in Microsoft Store
3. Launch PromoComply from Windows Start menu

### First Run

1. Click **"Documents"** in the sidebar
2. Click **"Import Document"**
3. Select a PDF, DOCX, or PPTX file
4. Click **"Analyze"** to review the document
5. View results in the **"Review"** tab

### Key Workflows

**Importing a Document**
```
Documents → Import Document → Select File → OK
```

**Analyzing Compliance**
```
Documents → Select Document → Click "Analyze" → Wait for completion
```

**Reviewing Results**
```
Review → Select Document → Review Claims and Issues → Add Comments
```

**Exporting Report**
```
Documents → Select Document → Export Report → Choose Format → Save
```

**Viewing Dashboard**
```
Dashboard → View Summary Metrics and Activity
```

---

## For Developers

### Environment Setup

#### Prerequisites
```powershell
# Verify Windows version
Get-WmiObject -Class Win32_OperatingSystem | Select-Object Caption, Version

# Check for .NET SDK
dotnet --version

# Should show .NET 8.0 or later
```

#### Installation

1. Install Visual Studio 2022 (Community or higher)
2. Include "Desktop development with C++" workload
3. Install .NET 8 SDK (if not included with VS)

### Project Structure

```
PromoComply/
├── PromoComply.sln                 # Solution file
├── src/
│   └── PromoComply/
│       ├── PromoComply.csproj      # Project file
│       ├── App.xaml, App.xaml.cs   # Application entry point
│       ├── MainWindow.xaml/cs      # Main window shell
│       ├── Models/                 # Data models
│       ├── Services/               # Business logic
│       ├── ViewModels/             # MVVM ViewModels
│       ├── Views/                  # XAML views
│       ├── Converters/             # Value converters
│       ├── Themes/                 # UI styling
│       └── Assets/                 # Application assets
├── README.md                        # User documentation
├── FEATURES.md                      # Feature documentation
├── BUILD_AND_DEPLOY.md             # Build instructions
└── QUICK_START.md                  # This file
```

### Building

**From Visual Studio**
```
File → Open Solution → PromoComply.sln
Build → Rebuild Solution
```

**From Command Line**
```powershell
cd PromoComply
dotnet build
dotnet run --project src/PromoComply/PromoComply.csproj
```

### Running for Development

```powershell
# Debug mode (default)
dotnet run --project src/PromoComply/PromoComply.csproj

# With specific configuration
dotnet run --project src/PromoComply/PromoComply.csproj --configuration Debug
```

### Adding New Features

#### Adding a New Service

1. Create interface in `Services/IMyService.cs`:
```csharp
namespace PromoComply.Services;

public interface IMyService
{
    void DoSomething();
}
```

2. Create implementation in `Services/MyService.cs`:
```csharp
namespace PromoComply.Services;

public class MyService : IMyService
{
    public void DoSomething()
    {
        // Implementation
    }
}
```

3. Register in `App.xaml.cs`:
```csharp
services.AddSingleton<IMyService, MyService>();
```

4. Inject into ViewModels:
```csharp
public class MyViewModel
{
    private readonly IMyService _service;

    public MyViewModel(IMyService service)
    {
        _service = service;
    }
}
```

#### Adding a New View

1. Create XAML UserControl in `Views/MyView.xaml`
2. Create code-behind in `Views/MyView.xaml.cs`
3. Create ViewModel in `ViewModels/MyViewModel.cs`
4. Register in `App.xaml.cs`
5. Add navigation in `MainWindow.xaml.cs`

#### Adding a Data Model

1. Create class in `Models/MyModel.cs`:
```csharp
namespace PromoComply.Models;

public class MyModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}
```

2. Use in ViewModels with ObservableCollection for UI binding

### Code Style Guidelines

#### Naming Conventions
- **Classes**: PascalCase (e.g., `DocumentParser`)
- **Methods**: PascalCase (e.g., `ParseDocument`)
- **Properties**: PascalCase (e.g., `FileName`)
- **Fields**: camelCase with underscore prefix (e.g., `_parser`)
- **Constants**: UPPER_SNAKE_CASE (e.g., `MAX_FILE_SIZE`)
- **Enums**: PascalCase values (e.g., `DocumentStatus.Pending`)

#### File Organization
- One public class per file
- Match namespace to folder structure
- Group related classes in folders

#### XAML Standards
- Use `x:Key` for resources
- Bind to ViewModel properties
- Use converters for value transformations
- Styles defined in AppTheme.xaml

### Testing

#### Unit Test Structure (Future)
```csharp
[TestClass]
public class ClaimsDetectorTests
{
    private IClaimsDetector _detector;

    [TestInitialize]
    public void Setup()
    {
        _detector = new ClaimsDetectorService();
    }

    [TestMethod]
    public void DetectClaims_WithEfficacyClaim_ReturnsEfficacyClaim()
    {
        // Arrange
        var text = "This treatment is proven effective.";

        // Act
        var claims = _detector.DetectClaims(text);

        // Assert
        Assert.IsTrue(claims.Any(c => c.ClaimType == ClaimType.Efficacy));
    }
}
```

### Debugging

#### Using Visual Studio Debugger

1. Set breakpoint by clicking line number
2. Press F5 or **Debug → Start Debugging**
3. App pauses at breakpoint
4. Use **Debug → Step Into/Over/Out** to navigate
5. Use **Watch** window to inspect variables

#### Common Debugging Scenarios

**View not updating**
- Check if ViewModel property has `[ObservableProperty]` attribute
- Verify binding path matches property name
- Check IValueConverter implementation

**Data not persisting**
- Verify `JsonProjectRepository` is registered
- Check AppData folder path: `%AppData%\PromoComply\Reviews\`
- Ensure write permissions to directory

**Claims not detecting**
- Check regex patterns in `ClaimsDetectorService`
- Verify text extraction in `DocumentParserService`
- Use breakpoints to inspect `DetectClaims` method

### NuGet Package Updates

Check for updates:
```powershell
dotnet outdated
```

Update packages:
```powershell
dotnet package update
```

### Performance Optimization

#### Memory Profiling
1. Build Release configuration
2. Run in Visual Studio Profiler (**Debug → Performance Profiler**)
3. Analyze memory allocations

#### Common Optimizations
- Avoid large string allocations in loops
- Use `StringBuilder` for string concatenation
- Clear collections before reusing
- Dispose resources in finally blocks

### Git Workflow

```powershell
# Clone repository
git clone https://github.com/pharmainsight/promocomply.git

# Create feature branch
git checkout -b feature/new-feature

# Make changes
# ...

# Commit changes
git add .
git commit -m "Add new feature"

# Push branch
git push origin feature/new-feature

# Create Pull Request on GitHub
```

### Common Issues and Solutions

**Issue**: Project won't build
```
Solution:
1. Delete bin/ and obj/ folders
2. Run: dotnet clean
3. Run: dotnet restore
4. Rebuild solution
```

**Issue**: NuGet restore fails
```
Solution:
1. Check internet connection
2. Clear cache: dotnet nuget locals all --clear
3. Update nuget.org source if needed
4. Try restore again
```

**Issue**: WPF Designer crashes
```
Solution:
1. Close XAML file
2. Rebuild solution
3. Reopen XAML file
4. If persists, restart Visual Studio
```

**Issue**: Claims not detecting correctly
```
Solution:
1. Check regex patterns in ClaimsDetectorService
2. Verify text extraction from document
3. Add debug breakpoints in DetectClaims
4. Inspect text content with quotes
```

### Resources

- **WPF Documentation**: https://learn.microsoft.com/en-us/dotnet/desktop/wpf/
- **.NET 8 Documentation**: https://learn.microsoft.com/en-us/dotnet/
- **MVVM Toolkit**: https://github.com/CommunityToolkit/dotnet
- **PdfPig**: https://github.com/UglyToad/PdfPig
- **OpenXml**: https://learn.microsoft.com/en-us/office/open-xml/open-xml-overview

### Useful Commands

```powershell
# Build for release
dotnet build --configuration Release

# Publish as self-contained
dotnet publish -c Release --self-contained -r win-x64

# Run tests (when added)
dotnet test

# Format code
dotnet format

# Check for vulnerabilities
dotnet list package --vulnerable
```

### Next Steps

1. Explore the codebase starting with `MainViewModel.cs`
2. Review `ClaimsDetectorService.cs` for claim detection logic
3. Check `ComplianceCheckerService.cs` for validation rules
4. Study `MainWindow.xaml` for UI structure
5. Review bound ViewModels to understand data flow

### Support

For questions or issues:
1. Check existing documentation files
2. Review code comments and structure
3. Refer to class XML documentation
4. Contact development team

---

**Last Updated**: 2024
**Version**: 1.0.0
