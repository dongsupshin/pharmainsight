# PromoComply Build and Deployment Guide

## Prerequisites

1. **Windows 10/11** (Build 17763 or later)
2. **Visual Studio 2022** (Community, Professional, or Enterprise)
   - Include "Desktop development with C++" workload for WPF development
3. **.NET 8 SDK** (installed via Visual Studio or separately from dotnet.microsoft.com)
4. **Git** (for version control)

## Building the Application

### Option 1: Building with Visual Studio 2022

1. Open `PromoComply.sln` in Visual Studio 2022
2. Wait for NuGet package restoration to complete
3. From the menu: **Build** → **Rebuild Solution**
4. Check the **Output** window for build status
5. Once successful, the executable will be at:
   ```
   src/PromoComply/bin/Release/net8.0-windows/PromoComply.exe
   ```

### Option 2: Building from Command Line

```powershell
# Navigate to solution directory
cd path\to\PromoComply

# Restore NuGet packages
dotnet restore

# Build the solution (Debug)
dotnet build

# Or build for Release
dotnet build --configuration Release

# Run the application
dotnet run --project src/PromoComply/PromoComply.csproj
```

## Publishing for Distribution

### Option 1: Create Self-Contained Executable

```powershell
dotnet publish -c Release --self-contained -r win-x64 `
  --output ./bin/publish/PromoComply-v1.0.0
```

This creates a standalone executable that doesn't require .NET 8 runtime installation.

### Option 2: Create Windows Installer (MSI)

Prerequisites:
- Install **WiX Toolset v3.14** or later
- Install WiX Visual Studio extension

Steps:
1. Create a WiX installer project in the solution
2. Configure product information and file references
3. Build the WiX project
4. Output will be an `.msi` file ready for distribution

### Option 3: Package for Microsoft Store (MSIX)

The project includes `Package.appxmanifest` configured for MSIX packaging.

#### Prerequisites
1. Developer account or publisher certificate
2. Windows 10/11 (Build 17763+)
3. Windows App Packager SDK tools

#### Steps

1. **Create Self-Signed Certificate** (for testing):
   ```powershell
   # Run as Administrator
   New-SelfSignedCertificate -Type Custom -Subject "CN=PharmaInsight" `
     -KeyUsage DigitalSignature -FriendlyName "PromoComply Dev Cert" `
     -CertStoreLocation "Cert:\CurrentUser\My" -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}Subject Type:End Entity")
   ```

2. **Sign the Manifest**:
   ```powershell
   MakeAppx.exe sign /f "path\to\certificate.pfx" /fd SHA256 `
     /p "certificate_password" /d ".\app_package"
   ```

3. **Create App Package**:
   ```powershell
   MakeAppx.exe pack /d "bin\Release\net8.0-windows\win-x64\publish" `
     /p "PromoComply_1.0.0.0_x64.msix"
   ```

#### Publishing to Microsoft Store

1. Create account at **Microsoft Partner Center** (partner.microsoft.com/dashboard)
2. Create new app and configure details
3. Upload `.msix` package
4. Configure pricing and availability
5. Submit for certification (typically 24-48 hours)
6. Once approved, it's available in Microsoft Store

## Installation Instructions

### For End Users

#### From Microsoft Store
1. Open Microsoft Store app
2. Search for "PromoComply"
3. Click "Get" or "Install"
4. Launch from Windows Start menu

#### From Self-Contained Executable
1. Download `PromoComply-v1.0.0.zip`
2. Extract to desired location
3. Run `PromoComply.exe`
4. No installation required

#### From MSI Installer
1. Download `PromoComply-1.0.0-setup.msi`
2. Double-click to run
3. Follow installation wizard
4. Launch from Windows Start menu or desktop shortcut

### For Developers (Debug)
```powershell
dotnet run --project src/PromoComply/PromoComply.csproj
```

## Version Management

Current version: **1.0.0**

To update version:
1. Edit `PromoComply.csproj`:
   ```xml
   <PropertyGroup>
     <Version>1.1.0</Version>
     <FileVersion>1.1.0</FileVersion>
   </PropertyGroup>
   ```

2. Update `Package.appxmanifest`:
   ```xml
   <Identity Version="1.1.0.0" />
   ```

3. Update `README.md` version reference

4. Create git tag: `git tag -a v1.1.0 -m "Release version 1.1.0"`

## Code Signing

For production releases, sign the executable:

```powershell
signtool.exe sign /f "code_signing_certificate.pfx" `
  /p "certificate_password" /d "PromoComply" `
  /du "https://pharmainsight.com" /tr "http://timestamp.digicert.com" `
  /td SHA256 "bin\Release\net8.0-windows\PromoComply.exe"
```

## Troubleshooting

### Build Fails with "Could not resolve framework"
- Ensure .NET 8 SDK is installed
- Run: `dotnet sdk check`
- If needed, download from: https://dotnet.microsoft.com/download

### WPF Design Surface Not Showing
- Right-click solution → Rebuild
- Close and reopen Visual Studio
- Verify Windows SDK is installed

### NuGet Package Restore Fails
- Check internet connection
- Clear NuGet cache: `dotnet nuget locals all --clear`
- Delete `obj` and `bin` directories
- Run: `dotnet restore`

### MSIX Package Creation Fails
- Ensure `Package.appxmanifest` is properly formatted
- Check all referenced assets exist in `Assets/` directory
- Verify manifest version format matches build version

## Quality Assurance

### Testing Checklist

- [ ] Application launches without errors
- [ ] All UI views render correctly
- [ ] Document import works for PDF, DOCX, PPTX
- [ ] Analysis completes and shows results
- [ ] Claims are detected and categorized
- [ ] Compliance issues are identified
- [ ] Score calculation is accurate
- [ ] Report generation succeeds
- [ ] Data persists correctly
- [ ] No unhandled exceptions
- [ ] Memory usage is reasonable
- [ ] Performance is acceptable (analysis < 30 seconds)

### Performance Testing

- Test with large files (10+ MB)
- Test with many documents (50+ files)
- Monitor memory usage
- Check for memory leaks in long sessions

## Release Checklist

- [ ] All bugs fixed and tested
- [ ] Code reviewed
- [ ] Version number updated
- [ ] README.md updated with new features
- [ ] Build runs successfully
- [ ] All tests pass
- [ ] Security vulnerabilities checked
- [ ] Documentation updated
- [ ] Assets created/updated
- [ ] Manifest updated
- [ ] Build signed with code signing certificate
- [ ] Package created successfully
- [ ] Installation tested
- [ ] Release notes prepared
- [ ] Git tag created
- [ ] Documentation uploaded

## Support and Maintenance

### Bug Reports
- Document steps to reproduce
- Include version number
- Attach sample files if applicable
- Provide system information

### Feature Requests
- Explain use case
- Suggest implementation approach
- Provide examples

### Updates and Patches
- Critical security fixes: released ASAP
- Bug fixes: included in next minor version
- Feature requests: included in next major version

## Contact

**Publisher**: PharmaInsight
**Product**: PromoComply
**Support**: support@pharmainsight.example.com
**Website**: https://pharmainsight.example.com

---

**Last Updated**: 2024
**Documentation Version**: 1.0
