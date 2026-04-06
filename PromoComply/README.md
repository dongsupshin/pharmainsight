# PromoComply

**Pharmaceutical Promotional Material Compliance Pre-Review Tool**

PromoComply is a professional C# WPF application built with .NET 8 that helps pharmaceutical commercial teams pre-screen promotional materials before formal Medical Legal Regulatory (MLR) review. The application automatically detects claims, identifies compliance issues, and generates detailed reports.

## Features

- **Document Parsing**: Supports PDF, DOCX, and PPTX file formats with automatic text extraction
- **Claim Detection**: Intelligent identification of efficacy, safety, superiority, comparative, and economic claims
- **Compliance Checking**: Validates promotional materials against regulatory requirements including:
  - Fair balance between benefits and risks
  - Important Safety Information (ISI) presence
  - Black Box Warning requirements
  - Reference substantiation
  - Off-label promotion detection
  - Misleading presentation assessment
  - Adverse event completeness

- **Scoring System**: Automatic compliance score calculation (0-100)
- **Interactive Review**: Mark claims as approved/flagged with detailed review notes
- **Report Generation**: Export comprehensive compliance reports in TXT and CSV formats
- **Dashboard Analytics**: Track compliance metrics across all documents

## System Requirements

- Windows 10/11 (Build 17763 or later)
- .NET 8 Runtime or SDK
- Minimum 4GB RAM
- 100MB free disk space

## Installation

### From Source

1. Clone the repository or download the source code
2. Open the solution in Visual Studio 2022 or later
3. Restore NuGet packages:
   ```
   dotnet restore
   ```
4. Build the solution:
   ```
   dotnet build
   ```
5. Run the application:
   ```
   dotnet run --project src/PromoComply/PromoComply.csproj
   ```

### From Microsoft Store

PromoComply is available for distribution through the Microsoft Store. To install from the Store:

1. Open Microsoft Store
2. Search for "PromoComply"
3. Click Install

## Usage

### Importing Documents

1. Click "Documents" in the sidebar
2. Click "Import Document"
3. Select a PDF, DOCX, or PPTX file
4. Click "Analyze" to begin compliance review

### Analyzing a Document

1. Select the document from the list
2. Click "Analyze"
3. Wait for the analysis to complete
4. View detected claims and compliance issues in the "Review" section

### Reviewing Claims

1. Navigate to the "Review" section
2. Review each detected claim
3. Mark claims as "Approve" (compliant), "Flag" (needs revision), or "Review" (requires further assessment)
4. Add detailed review notes as needed

### Checking Compliance Issues

1. In the "Review" section, review the Compliance Issues panel
2. Understand each issue category and severity
3. Follow recommendations to address issues
4. Mark issues as "Resolved" once corrected

### Generating Reports

1. Select a document
2. Click "Export Report"
3. Choose format (TXT for detailed report, CSV for spreadsheet)
4. Save to desired location

### Viewing Dashboard

1. Click "Dashboard" in the sidebar
2. View overall compliance metrics:
   - Total documents reviewed
   - Average compliance score
   - Critical issue count
   - Recent activity log
   - Score distribution chart

## Document Status Indicators

- **Pending**: Document imported but not yet analyzed
- **Reviewing**: Document is currently being analyzed
- **Reviewed**: Analysis complete, ready for review
- **Approved**: Document meets compliance standards (score 80+)
- **Flagged**: Document has compliance issues requiring attention (score <60)

## Compliance Score Interpretation

- **90-100**: Excellent - Ready for MLR submission
- **75-89**: Good - Minor revisions recommended
- **60-74**: Fair - Moderate revisions required
- **Below 60**: Poor - Substantial revisions needed

## Claim Risk Levels

- **Low**: General informational statements, well-supported
- **Medium**: Comparative or safety claims requiring careful review
- **High**: Efficacy claims, superiority statements requiring robust evidence
- **Critical**: High-impact claims requiring strongest evidence and references

## Compliance Issue Categories

- **Missing Fair Balance**: Risks section too brief compared to benefits
- **Missing ISI**: Important Safety Information section absent or incomplete
- **Unsubstantiated Claim**: Claims without supporting references
- **Missing Reference**: High-risk claims lack clinical citations
- **Off-Label Promotion**: Language suggesting unapproved uses
- **Minimized Risk**: Risk information downplayed or minimized
- **Missing Black Box Warning**: Required warning not included
- **Incomplete Adverse Events**: Adverse event list insufficient
- **Misleading Presentation**: Superlatives without qualifiers

## Architecture

### Services

- **DocumentParserService**: Extracts text from PDF, DOCX, PPTX files
- **ClaimsDetectorService**: Identifies promotional claims using regex patterns
- **ComplianceCheckerService**: Validates documents against pharma regulations
- **ReportGeneratorService**: Creates compliance reports
- **JsonProjectRepository**: Persists review sessions

### ViewModels

- **MainViewModel**: Orchestrates document import and analysis
- **DashboardViewModel**: Manages analytics and metrics
- **DocumentReviewViewModel**: Handles claim and issue review

### Views

- **MainWindow**: Application shell with sidebar navigation
- **DashboardView**: Summary statistics and activity
- **DocumentListView**: Document import and list management
- **DocumentReviewView**: Interactive claim and issue review
- **ReportView**: Compliance report preview and export

## NuGet Dependencies

- **DocumentFormat.OpenXml 2.20.0**: DOCX/PPTX parsing
- **PdfPig 0.1.8**: PDF text extraction
- **CommunityToolkit.Mvvm 8.2.2**: MVVM implementation
- **Microsoft.Extensions.DependencyInjection 8.0.0**: Dependency injection

## Configuration

User data and review sessions are stored in:
```
%AppData%\PromoComply\Reviews\
```

All data is stored locally; no cloud synchronization occurs.

## License

PromoComply is proprietary software owned by PharmaInsight. All rights reserved.

## Support

For support, documentation, or bug reports, please contact PharmaInsight support.

## Version

PromoComply v1.0.0
Built with .NET 8.0 and WPF
