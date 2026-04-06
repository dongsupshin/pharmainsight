using System.Text.RegularExpressions;
using PromoComply.Models;

namespace PromoComply.Services;

public class ComplianceCheckerService : IComplianceChecker
{
    private const int BenefitsSection = 1;
    private const int RisksSection = 2;

    public List<ComplianceIssue> CheckCompliance(PromoDocument document)
    {
        var issues = new List<ComplianceIssue>();
        var text = document.ExtractedText;

        issues.AddRange(CheckFairBalance(text));
        issues.AddRange(CheckISIPresence(text));
        issues.AddRange(CheckBlackBoxWarning(text));
        issues.AddRange(CheckReferences(document));
        issues.AddRange(CheckOffLabelPromotion(text));
        issues.AddRange(CheckMisleadingPresentation(document));
        issues.AddRange(CheckAdverseEvents(text));

        return issues;
    }

    public int CalculateComplianceScore(PromoDocument document)
    {
        var issues = document.ComplianceIssues;

        int baseScore = 100;

        var criticalCount = issues.Count(i => i.Severity == IssueSeverity.Critical);
        baseScore -= criticalCount * 15;

        var majorCount = issues.Count(i => i.Severity == IssueSeverity.Major);
        baseScore -= majorCount * 8;

        var warningCount = issues.Count(i => i.Severity == IssueSeverity.Warning);
        baseScore -= warningCount * 3;

        int approvedClaimsCount = document.Claims.Count(c => c.IsApproved == true);
        int totalClaimsCount = document.Claims.Count;
        int claimApprovalScore = totalClaimsCount > 0 ? (approvedClaimsCount * 5) / totalClaimsCount : 0;
        baseScore -= (5 - claimApprovalScore);

        var unresolvedIssuesCount = issues.Count(i => !i.IsResolved);
        baseScore -= unresolvedIssuesCount * 2;

        baseScore = Math.Max(0, Math.Min(100, baseScore));

        return baseScore;
    }

    private List<ComplianceIssue> CheckFairBalance(string text)
    {
        var issues = new List<ComplianceIssue>();
        var textLower = text.ToLowerInvariant();

        var benefitsSection = ExtractSection(text, new[] { "benefits", "efficacy", "indication", "indications", "advantages" });
        var risksSection = ExtractSection(text, new[] { "risks", "adverse", "side effects", "contraindication", "warnings", "precautions", "safety" });

        if (string.IsNullOrEmpty(risksSection))
        {
            issues.Add(new ComplianceIssue
            {
                Title = "Missing Risk Information",
                Description = "Document lacks a dedicated risks or adverse events section.",
                Category = ComplianceCategory.MissingFairBalance,
                Severity = IssueSeverity.Critical,
                Location = "Document structure",
                Recommendation = "Add comprehensive risks, contraindications, and adverse events section to ensure fair balance.",
                IsResolved = false
            });
        }
        else if (!string.IsNullOrEmpty(benefitsSection) && benefitsSection.Length > risksSection.Length * 3)
        {
            issues.Add(new ComplianceIssue
            {
                Title = "Imbalanced Benefit-Risk Presentation",
                Description = "Benefits section is significantly larger than risks section, suggesting imbalance in presentation.",
                Category = ComplianceCategory.MissingFairBalance,
                Severity = IssueSeverity.Major,
                Location = "Document body",
                Recommendation = "Expand the risks section to ensure proportionate presentation with benefits.",
                IsResolved = false
            });
        }

        return issues;
    }

    private List<ComplianceIssue> CheckISIPresence(string text)
    {
        var issues = new List<ComplianceIssue>();
        var textLower = text.ToLowerInvariant();

        var isiPatterns = new[]
        {
            @"important\s+safety\s+information",
            @"prescribing\s+information",
            @"contraindications?",
            @"warnings?\s+(and\s+)?precautions",
            @"adverse\s+(drug\s+)?reactions?",
            @"adverse\s+events?"
        };

        var hasISI = isiPatterns.Any(pattern =>
            Regex.IsMatch(textLower, pattern, RegexOptions.IgnoreCase));

        if (!hasISI)
        {
            issues.Add(new ComplianceIssue
            {
                Title = "Missing Important Safety Information (ISI)",
                Description = "Document does not contain required Important Safety Information section with contraindications, warnings, or adverse reactions.",
                Category = ComplianceCategory.MissingISI,
                Severity = IssueSeverity.Critical,
                Location = "Document structure",
                Recommendation = "Include a complete ISI section with contraindications, warnings, precautions, and adverse reactions from the prescribing information.",
                IsResolved = false
            });
        }

        return issues;
    }

    private List<ComplianceIssue> CheckBlackBoxWarning(string text)
    {
        var issues = new List<ComplianceIssue>();
        var textLower = text.ToLowerInvariant();

        var blackBoxPatterns = new[]
        {
            @"black\s+box",
            @"boxed\s+warning",
            @"fda\s+warning"
        };

        var hasBlackBox = blackBoxPatterns.Any(pattern =>
            Regex.IsMatch(textLower, pattern, RegexOptions.IgnoreCase));

        var restrictedDrugClassPatterns = new[]
        {
            @"\b(antipsychotic|opioid|stimulant|abiraterone|clozapine|thalidomide|isotretinoin|finasteride)\b"
        };

        var isRestrictedDrug = restrictedDrugClassPatterns.Any(pattern =>
            Regex.IsMatch(textLower, pattern, RegexOptions.IgnoreCase));

        if (isRestrictedDrug && !hasBlackBox)
        {
            issues.Add(new ComplianceIssue
            {
                Title = "Missing Black Box Warning",
                Description = "Document appears to promote a drug with a Black Box Warning but does not display the required boxed warning.",
                Category = ComplianceCategory.MissingBlackBoxWarning,
                Severity = IssueSeverity.Critical,
                Location = "Document structure",
                Recommendation = "Include the FDA Black Box Warning prominently in the promotional material.",
                IsResolved = false
            });
        }

        return issues;
    }

    private List<ComplianceIssue> CheckReferences(PromoDocument document)
    {
        var issues = new List<ComplianceIssue>();

        var claimsWithoutReferences = document.Claims
            .Where(c => !c.HasReference && c.RiskLevel >= RiskLevel.Medium)
            .ToList();

        foreach (var claim in claimsWithoutReferences)
        {
            issues.Add(new ComplianceIssue
            {
                Title = "Unsubstantiated Claim",
                Description = $"Claim lacks supporting reference: \"{claim.Text}\"",
                Category = ComplianceCategory.MissingReference,
                Severity = claim.RiskLevel == RiskLevel.Critical ? IssueSeverity.Critical : IssueSeverity.Major,
                Location = claim.Location,
                Recommendation = "Add citation to clinical data or study results supporting this claim.",
                IsResolved = false
            });
        }

        return issues;
    }

    private List<ComplianceIssue> CheckOffLabelPromotion(string text)
    {
        var issues = new List<ComplianceIssue>();
        var textLower = text.ToLowerInvariant();

        var offLabelPatterns = new[]
        {
            @"\b(also\s+used|can\s+be\s+used|used\s+for|suitable\s+for)\s+(?!as\s+indicated)",
            @"\b(may\s+help|might\s+benefit|could\s+treat)\b",
            @"\b(off[\s-]?label|unapproved\s+indication|unlabeled\s+use)\b"
        };

        var approvedIndicationPattern = @"(indicated\s+for|indicated|approved\s+for|fda\s+approved)";
        var hasApprovedIndication = Regex.IsMatch(textLower, approvedIndicationPattern, RegexOptions.IgnoreCase);

        var hasOffLabelLanguage = offLabelPatterns.Any(pattern =>
            Regex.IsMatch(textLower, pattern, RegexOptions.IgnoreCase));

        if (hasOffLabelLanguage && !hasApprovedIndication)
        {
            issues.Add(new ComplianceIssue
            {
                Title = "Potential Off-Label Promotion",
                Description = "Document contains language suggesting uses beyond FDA-approved indications.",
                Category = ComplianceCategory.OffLabelPromotion,
                Severity = IssueSeverity.Critical,
                Location = "Document body",
                Recommendation = "Ensure all promotional language is restricted to FDA-approved indications only.",
                IsResolved = false
            });
        }

        return issues;
    }

    private List<ComplianceIssue> CheckMisleadingPresentation(PromoDocument document)
    {
        var issues = new List<ComplianceIssue>();
        var text = document.ExtractedText;
        var textLower = text.ToLowerInvariant();

        var superlatfivePatterns = new[]
        {
            @"\b(best|greatest|most\s+effective|superior|unequaled|unsurpassed|finest|leading)\b"
        };

        var hasSuperlatfives = superlatfivePatterns.Any(pattern =>
            Regex.IsMatch(textLower, pattern, RegexOptions.IgnoreCase));

        var hasQualifiers = Regex.IsMatch(textLower, @"\b(in\s+clinical\s+studies|in\s+our\s+study|in\s+some\s+patients|may\s+be|compared\s+to)\b", RegexOptions.IgnoreCase);

        if (hasSuperlatfives && !hasQualifiers)
        {
            issues.Add(new ComplianceIssue
            {
                Title = "Unqualified Superlatives",
                Description = "Document contains superlative claims without appropriate qualifiers or comparative context.",
                Category = ComplianceCategory.MisleadingPresentation,
                Severity = IssueSeverity.Major,
                Location = "Document body",
                Recommendation = "Qualify superlative claims with specific comparative data, study context, or population specifications.",
                IsResolved = false
            });
        }

        var highRiskClaims = document.Claims.Count(c => c.RiskLevel == RiskLevel.Critical);
        if (highRiskClaims > 0 && document.ComplianceIssues.Count(i => i.Category == ComplianceCategory.MissingReference) > 0)
        {
            issues.Add(new ComplianceIssue
            {
                Title = "High-Risk Claims Need Strong Support",
                Description = $"Document contains {highRiskClaims} critical-level claims that require robust clinical evidence.",
                Category = ComplianceCategory.MisleadingPresentation,
                Severity = IssueSeverity.Major,
                Location = "Throughout document",
                Recommendation = "Ensure all high-impact claims are supported by robust, recent clinical evidence with proper citations.",
                IsResolved = false
            });
        }

        return issues;
    }

    private List<ComplianceIssue> CheckAdverseEvents(string text)
    {
        var issues = new List<ComplianceIssue>();
        var textLower = text.ToLowerInvariant();

        var adverseEventPatterns = new[]
        {
            @"adverse\s+events?",
            @"side\s+effects?",
            @"adverse\s+reactions?",
            @"unwanted\s+effects?",
            @"serious\s+adverse"
        };

        var hasAdverseEventSection = adverseEventPatterns.Any(pattern =>
            Regex.IsMatch(textLower, pattern, RegexOptions.IgnoreCase));

        if (hasAdverseEventSection)
        {
            var adverseSection = ExtractSection(text, new[] { "adverse", "side effects", "reactions" });

            if (!string.IsNullOrEmpty(adverseSection))
            {
                var commonAdverseEvents = new[]
                {
                    "nausea", "headache", "dizziness", "fatigue", "pain", "infection", "hypertension", "rash",
                    "diarrhea", "vomiting", "abdominal", "insomnia", "depression", "anxiety"
                };

                var foundEvents = commonAdverseEvents.Count(ae =>
                    adverseSection.ToLowerInvariant().Contains(ae));

                if (foundEvents < 3)
                {
                    issues.Add(new ComplianceIssue
                    {
                        Title = "Incomplete Adverse Events Listing",
                        Description = "Adverse events section may be incomplete with limited event details provided.",
                        Category = ComplianceCategory.IncompleteAdverseEvents,
                        Severity = IssueSeverity.Warning,
                        Location = "Adverse Events Section",
                        Recommendation = "Provide a comprehensive list of common and serious adverse events from clinical trials.",
                        IsResolved = false
                    });
                }
            }
        }
        else
        {
            issues.Add(new ComplianceIssue
            {
                Title = "Missing Adverse Events Information",
                Description = "Document does not contain a dedicated adverse events or side effects section.",
                Category = ComplianceCategory.IncompleteAdverseEvents,
                Severity = IssueSeverity.Major,
                Location = "Document structure",
                Recommendation = "Include comprehensive adverse event information including incidence rates and serious events.",
                IsResolved = false
            });
        }

        return issues;
    }

    private string ExtractSection(string text, string[] sectionHeaders)
    {
        var lowerText = text.ToLowerInvariant();
        var sections = new List<string>();

        foreach (var header in sectionHeaders)
        {
            var index = lowerText.IndexOf(header.ToLowerInvariant());
            if (index != -1)
            {
                var endIndex = FindNextSectionHeader(lowerText, index + header.Length);
                var sectionLength = endIndex - index;
                if (sectionLength > 0 && sectionLength < text.Length)
                {
                    sections.Add(text.Substring(index, sectionLength));
                }
            }
        }

        return string.Join(" ", sections);
    }

    private int FindNextSectionHeader(string text, int startIndex)
    {
        var commonHeaders = new[] { "indication", "contraindication", "warning", "adverse", "dosage", "reference", "conclusion" };
        var nextIndex = text.Length;

        foreach (var header in commonHeaders)
        {
            var index = text.IndexOf(header, startIndex, StringComparison.OrdinalIgnoreCase);
            if (index != -1 && index < nextIndex)
            {
                nextIndex = index;
            }
        }

        return nextIndex;
    }
}
