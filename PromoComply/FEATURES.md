# PromoComply Features Documentation

## Overview

PromoComply is a comprehensive pharmaceutical promotional material compliance pre-review tool designed to help commercial teams identify potential regulatory issues before formal MLR submission.

## Core Features

### 1. Multi-Format Document Support

**Supported Formats**
- **PDF**: Full text extraction via PdfPig library
- **DOCX**: Word document parsing with OpenXml
- **PPTX**: PowerPoint slide extraction with OpenXml

**Capabilities**
- Automatic text extraction from all supported formats
- Preservation of document structure and hierarchy
- Support for embedded content and annotations
- Handling of complex layouts and multi-page documents

### 2. Intelligent Claim Detection

**Claim Types Detected**

#### Efficacy Claims
- Effectiveness statements: "proven", "demonstrated", "significant improvement"
- Performance metrics: "reduces symptoms", "accelerates healing"
- Statistical language: "statistically significant", "p < 0.05"
- Comparative effectiveness: "works better", "more effective"

Risk Assignment:
- Without reference: High/Critical
- With supporting data: Medium/Low

#### Safety Claims
- Tolerability: "well-tolerated", "minimal side effects", "low risk"
- Absence statements: "no serious adverse events", "free from complications"
- Safety assertions: "safe", "gentle", "does not cause"

Risk Assignment:
- Blanket safety claims: Medium risk
- Well-qualified statements: Low risk

#### Superiority Claims
- Category leadership: "first-in-class", "best-in-class", "gold standard"
- Uniqueness: "only drug", "breakthrough", "revolutionary"
- Excellence: "superior", "unmatched", "unprecedented"

Risk Assignment:
- Usually High/Critical risk
- Require strongest supporting evidence

#### Comparative Claims
- Direct comparisons: "better than", "superior to", "more effective than"
- Competitive positioning: "unlike other treatments", "advantage over"
- Head-to-head language: "versus", "compared to"

Risk Assignment:
- High risk (requires substantiation)
- Head-to-head study data strongly recommended

#### Economic Claims
- Cost-effectiveness: "cost-effective", "saves money"
- Financial benefits: "reduces healthcare costs", "affordable"
- Value propositions: "return on investment", "economical"

Risk Assignment:
- Medium risk (financial claims heavily scrutinized)

#### General Claims
- Product information and facts without comparative language
- Descriptive statements
- Low risk when accurate and substantiated

**Detection Mechanism**
- Pattern-based regex analysis of document text
- Sentence-level processing with context extraction
- Deduplication and filtering of redundant claims
- Location tracking for referenced text

**Accuracy Factors**
- Context surrounding claim (30 characters before/after)
- Presence of qualifiers and hedging language
- Reference indicators (citations, footnotes, study names)
- Risk level assignment based on claim strength

### 3. Comprehensive Compliance Checking

#### Fair Balance Assessment
- Analyzes benefit/risk statement ratio
- Checks for proportionate risk presentation
- Flags when risks section is <1/3 of benefits size
- Requires dedicated safety section

Issues Identified:
- Missing risk information
- Imbalanced benefit-risk presentation
- Inadequate risk discussion

#### Important Safety Information (ISI) Validation
- Searches for required ISI section
- Verifies presence of:
  - Contraindications
  - Warnings and Precautions
  - Adverse Reactions/Events
  - Dosage information

Issues Identified:
- Missing ISI entirely
- Incomplete ISI components
- Outdated safety information reference

#### Black Box Warning Detection
- Identifies drug categories requiring boxed warnings:
  - Antipsychotics
  - Opioids
  - Stimulants
  - Hazardous drug categories

- Verifies boxed warning presence in material
- Flags materials missing required warnings

Issues Identified:
- Missing Black Box Warning
- Inadequate warning visibility

#### Reference Substantiation
- Analyzes all detected claims for citation presence
- Identifies unsubstantiated medium/high/critical claims
- Checks for reference indicators:
  - Superscript numbers: "[1]", "1"
  - Author citations: "et al.", "Author et al"
  - Publication names: "JAMA", "NEJM", "Lancet"
  - Year references: "2023", "2024"
  - URLs and DOIs

Issues Identified:
- Unsubstantiated Claim (per claim)
- Missing Reference sections
- Broken citation chains

#### Off-Label Promotion Detection
- Identifies language suggesting unapproved uses
- Flags comparative claims without approved indication context
- Detects usage patterns outside FDA approval

Flagged Language:
- "May help", "might benefit", "could treat" (without approval context)
- "Also used for", "suitable for" (unapproved indications)
- "Off-label", "unlabeled use", "investigational"

Issues Identified:
- Potential Off-Label Promotion
- Unapproved indication language
- Expanded use claims

#### Adverse Event Completeness
- Verifies adverse events section presence
- Assesses event listing completeness
- Checks for serious adverse event inclusion
- Validates frequency/incidence information

Common Adverse Events Checked:
- Nausea, headache, dizziness, fatigue, pain
- Infection, hypertension, rash, diarrhea
- Vomiting, abdominal pain, insomnia
- Depression, anxiety

Issues Identified:
- Missing Adverse Events section
- Incomplete event listing
- Minimized serious adverse event disclosure

#### Misleading Presentation Assessment
- Identifies unqualified superlatives
- Flags cherry-picked data patterns
- Detects minimized risk presentation
- Checks qualifier adequacy

Flagged Elements:
- Superlatives without context: "best", "greatest", "superior"
- Missing qualifiers: "in clinical trials", "compared to"
- Data presentation issues: "selected studies", "available data"
- Risk minimization: "minimal risk", "low incidence"

Issues Identified:
- Unqualified Superlatives
- Misleading Data Presentation
- High-Risk Claims Need Support

### 4. Compliance Scoring

**Score Calculation Algorithm**

```
Base Score = 100

Critical Issues: -15 points each
Major Issues: -8 points each
Warning Issues: -3 points each

Claim Approval: (Approved Claims / Total Claims) × 5 points
Unresolved Issues: -2 points each

Final Score = Max(0, Min(100, Calculated Score))
```

**Score Interpretation**

| Range | Status | Assessment | Action |
|-------|--------|-----------|--------|
| 90-100 | Excellent | Ready for MLR | Submit as-is |
| 75-89 | Good | Minor revisions | Address identified issues |
| 60-74 | Fair | Moderate revisions | Substantial edits needed |
| Below 60 | Poor | Major revision | Comprehensive review required |

### 5. Interactive Review Interface

#### Claim Review
**Actions Available**
- **Approve**: Mark claim as compliant with evidence
- **Flag**: Mark claim as requiring revision
- **Review**: Mark for further assessment

**Review Notes**
- Add reviewer comments
- Track revision status
- Document evidence sources
- Maintain audit trail

#### Issue Resolution
**Issue Management**
- View detailed issue descriptions
- Read recommendations for remediation
- Toggle resolution status
- Track correction progress

**Categories Available**
- Fair Balance Issues
- ISI-related Issues
- Reference Issues
- Black Box Warnings
- Off-Label Concerns
- Misleading Presentation
- Adverse Event Issues

### 6. Report Generation

#### Text Reports
**Contents**
- Executive summary with key metrics
- Document information
- Claims summary by type and risk
- Detailed compliance issues with recommendations
- Overall assessment
- Actionable recommendations

**Format**: Plain text (TXT), suitable for printing and archival

#### CSV Reports
**Contents**
- Document metadata
- Claims detail (text, type, risk, location, approval status)
- Compliance issues (category, severity, recommendations)
- Summary statistics
- Metrics for analysis

**Format**: Comma-separated values (CSV), importable to Excel/Google Sheets

**Export Options**
- Select export format (TXT or CSV)
- Automatic filename generation
- Custom save location
- Timestamped reports

### 7. Dashboard Analytics

#### Summary Metrics
- **Total Documents**: Count of all imported documents
- **Reviewed Count**: Documents with completed analysis
- **Flagged Count**: Documents with compliance issues
- **Average Score**: Mean compliance score across all documents

#### Issue Tracking
- **Critical Issues**: Total critical-severity findings
- **Major Issues**: Total major-severity findings
- **Warning Items**: Total warning-severity findings

#### Recent Activity Log
- Document import timestamps
- File type indicators
- Status transitions
- Activity chronology

#### Score Distribution Chart
- Visual breakdown by score range
- Count of documents in each range
- Color-coded severity indicators
- Range categories: 90-100, 75-89, 60-74, <60

### 8. Data Management

#### Document Storage
- Documents loaded from local file system
- No cloud upload or synchronization
- Local processing for privacy and security
- File paths tracked for reference

#### Session Persistence
- Review sessions saved to local JSON files
- Storage location: `%AppData%\PromoComply\Reviews\`
- One JSON file per review session
- Sessions can be loaded for continued review

#### Data Privacy
- All data processed locally
- No external API calls
- No telemetry collection
- HIPAA-compliant data handling

### 9. User Interface Features

#### Navigation
- **Sidebar Menu**: Quick access to main sections
  - Dashboard: Overview and metrics
  - Documents: Import and management
  - Review: Detailed analysis
  - Reports: Report generation

#### Professional Design
- Dark theme suitable for long work sessions
- Pharmaceutical industry-appropriate colors
- High-contrast text for readability
- Responsive layout for various screen sizes

#### Accessibility
- Keyboard navigation support
- Color-coded severity indicators with labels
- Readable fonts and sizing
- Standard Windows UI patterns

### 10. Advanced Features

#### Batch Processing
- Import multiple documents sequentially
- Analyze each document independently
- Compare compliance across documents
- Aggregate metrics dashboard

#### Claim Categorization
- Automatic detection of claim type
- Risk level assignment based on content
- Reference indicator identification
- Location tracking within document

#### Severity Escalation
- Critical issues: Blocking MLR submission
- Major issues: Require revision before submission
- Warning issues: Should be addressed
- Info items: Nice-to-know observations

## Integration Capabilities

### Future Enhancement Areas

- **MLR System Integration**: Direct submission to MLR workflow tools
- **Compliance Database**: Link to standard regulatory requirements
- **Historical Comparison**: Track compliance trends over time
- **Template Management**: Save and reuse compliance templates
- **Collaboration Tools**: Multi-user review workflows
- **API Interface**: Integration with commercial systems
- **Mobile App**: Remote document review capability

## Performance Characteristics

### Processing Times (Typical)

| Document Size | File Format | Parse Time | Analysis Time | Total |
|---|---|---|---|---|
| < 1 MB | PDF | 0.5s | 1.0s | 1.5s |
| 1-5 MB | DOCX | 1.0s | 2.0s | 3.0s |
| 5-10 MB | PPTX | 2.0s | 3.0s | 5.0s |
| > 10 MB | Large PDF | 3.0s | 5.0s | 8.0s |

### System Requirements

- **Processor**: Dual-core 2 GHz minimum
- **Memory**: 4 GB RAM recommended (2 GB minimum)
- **Storage**: 100 MB installation + document space
- **Display**: 1280×720 resolution minimum
- **Network**: Not required (local processing)

## Compliance Standards

PromoComply checks for compliance with:

- FDA Promotional Guidance
- PhRMA Code on Interactions with Healthcare Professionals
- ACCME Standards for Commercial Support
- International Marketing Standards (where applicable)

## Future Roadmap

### Version 1.1
- Advanced filter and search capabilities
- Custom compliance rule creation
- Enhanced reporting with graphs and charts

### Version 2.0
- Multi-language support
- MLR system integration
- Collaborative review workflows
- Cloud synchronization option

### Version 3.0
- AI-powered claim analysis
- Predictive compliance scoring
- Automated remediation suggestions
- Enterprise deployment features

---

**Last Updated**: 2024
**Current Version**: 1.0.0
