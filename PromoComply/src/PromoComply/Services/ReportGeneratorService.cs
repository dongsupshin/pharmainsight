using System.Text;
using PromoComply.Models;

namespace PromoComply.Services;

public class ReportGeneratorService : IReportGenerator
{
    public string GenerateTextReport(PromoDocument document)
    {
        var sb = new StringBuilder();

        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine("PHARMAINSIGHT PROMOCOMPLY - COMPLIANCE PRE-REVIEW REPORT");
        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine();

        sb.AppendLine("DOCUMENT INFORMATION");
        sb.AppendLine("-".PadRight(80, '-'));
        sb.AppendLine($"Document Name:     {document.FileName}");
        sb.AppendLine($"File Type:          {document.FileType}");
        sb.AppendLine($"Imported Date:      {document.ImportedDate:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Document Status:    {document.Status}");
        sb.AppendLine($"Compliance Score:   {document.OverallScore}/100");
        sb.AppendLine();

        sb.AppendLine("EXECUTIVE SUMMARY");
        sb.AppendLine("-".PadRight(80, '-'));
        sb.AppendLine($"Total Claims Detected:       {document.Claims.Count}");
        sb.AppendLine($"Critical Claims:            {document.Claims.Count(c => c.RiskLevel == RiskLevel.Critical)}");
        sb.AppendLine($"High Risk Claims:           {document.Claims.Count(c => c.RiskLevel == RiskLevel.High)}");
        sb.AppendLine($"Medium Risk Claims:         {document.Claims.Count(c => c.RiskLevel == RiskLevel.Medium)}");
        sb.AppendLine($"Low Risk Claims:            {document.Claims.Count(c => c.RiskLevel == RiskLevel.Low)}");
        sb.AppendLine();

        sb.AppendLine($"Total Compliance Issues:    {document.ComplianceIssues.Count}");
        sb.AppendLine($"Critical Issues:            {document.ComplianceIssues.Count(i => i.Severity == IssueSeverity.Critical)}");
        sb.AppendLine($"Major Issues:               {document.ComplianceIssues.Count(i => i.Severity == IssueSeverity.Major)}");
        sb.AppendLine($"Warnings:                   {document.ComplianceIssues.Count(i => i.Severity == IssueSeverity.Warning)}");
        sb.AppendLine($"Info Items:                 {document.ComplianceIssues.Count(i => i.Severity == IssueSeverity.Info)}");
        sb.AppendLine();

        if (document.OverallScore >= 80)
        {
            sb.AppendLine("ASSESSMENT: Document shows good compliance. Minor issues may require attention.");
        }
        else if (document.OverallScore >= 60)
        {
            sb.AppendLine("ASSESSMENT: Document requires review and modifications before MLR submission.");
        }
        else if (document.OverallScore >= 40)
        {
            sb.AppendLine("ASSESSMENT: Document has significant compliance issues requiring substantial revisions.");
        }
        else
        {
            sb.AppendLine("ASSESSMENT: Document requires major revision before MLR process.");
        }
        sb.AppendLine();

        if (document.ComplianceIssues.Any())
        {
            sb.AppendLine("DETAILED COMPLIANCE ISSUES");
            sb.AppendLine("-".PadRight(80, '-'));

            var criticalIssues = document.ComplianceIssues.Where(i => i.Severity == IssueSeverity.Critical).ToList();
            if (criticalIssues.Any())
            {
                sb.AppendLine("CRITICAL ISSUES:");
                foreach (var issue in criticalIssues)
                {
                    sb.AppendLine($"  • {issue.Title}");
                    sb.AppendLine($"    Category: {issue.Category}");
                    sb.AppendLine($"    Description: {issue.Description}");
                    sb.AppendLine($"    Location: {issue.Location}");
                    sb.AppendLine($"    Recommendation: {issue.Recommendation}");
                    sb.AppendLine($"    Status: {(issue.IsResolved ? "Resolved" : "Unresolved")}");
                    sb.AppendLine();
                }
            }

            var majorIssues = document.ComplianceIssues.Where(i => i.Severity == IssueSeverity.Major).ToList();
            if (majorIssues.Any())
            {
                sb.AppendLine("MAJOR ISSUES:");
                foreach (var issue in majorIssues)
                {
                    sb.AppendLine($"  • {issue.Title}");
                    sb.AppendLine($"    Category: {issue.Category}");
                    sb.AppendLine($"    Recommendation: {issue.Recommendation}");
                    sb.AppendLine();
                }
            }

            var warningIssues = document.ComplianceIssues.Where(i => i.Severity == IssueSeverity.Warning).ToList();
            if (warningIssues.Any())
            {
                sb.AppendLine("WARNINGS:");
                foreach (var issue in warningIssues)
                {
                    sb.AppendLine($"  • {issue.Title}");
                    sb.AppendLine();
                }
            }
        }

        if (document.Claims.Any())
        {
            sb.AppendLine();
            sb.AppendLine("DETECTED CLAIMS SUMMARY");
            sb.AppendLine("-".PadRight(80, '-'));

            var claimsByType = document.Claims.GroupBy(c => c.ClaimType);
            foreach (var group in claimsByType)
            {
                sb.AppendLine($"{group.Key} Claims ({group.Count()}):");
                foreach (var claim in group.Take(3))
                {
                    var riskIndicator = claim.RiskLevel switch
                    {
                        RiskLevel.Critical => "[!!!]",
                        RiskLevel.High => "[!!]",
                        RiskLevel.Medium => "[!]",
                        _ => "[-]"
                    };
                    sb.AppendLine($"  {riskIndicator} {claim.Text}");
                    sb.AppendLine($"      Location: {claim.Location}, Reference: {(claim.HasReference ? "Yes" : "No")}");
                }
                if (group.Count() > 3)
                {
                    sb.AppendLine($"  ... and {group.Count() - 3} more");
                }
                sb.AppendLine();
            }
        }

        sb.AppendLine("RECOMMENDATIONS FOR SUBMISSION");
        sb.AppendLine("-".PadRight(80, '-'));
        var recommendations = GenerateRecommendations(document);
        foreach (var rec in recommendations)
        {
            sb.AppendLine($"  • {rec}");
        }
        sb.AppendLine();

        sb.AppendLine("REPORT GENERATED: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        sb.AppendLine("=".PadRight(80, '='));

        return sb.ToString();
    }

    public byte[] GenerateExcelReport(PromoDocument document)
    {
        var csvContent = new StringBuilder();

        csvContent.AppendLine("PromoComply Compliance Report");
        csvContent.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        csvContent.AppendLine();

        csvContent.AppendLine("Document Information");
        csvContent.AppendLine($"File Name,{document.FileName}");
        csvContent.AppendLine($"File Type,{document.FileType}");
        csvContent.AppendLine($"Imported Date,{document.ImportedDate:yyyy-MM-dd HH:mm:ss}");
        csvContent.AppendLine($"Status,{document.Status}");
        csvContent.AppendLine($"Compliance Score,{document.OverallScore}");
        csvContent.AppendLine();

        csvContent.AppendLine("Claims Summary");
        csvContent.AppendLine("Claim Type,Count,Critical,High,Medium,Low");
        var claimsByType = document.Claims.GroupBy(c => c.ClaimType);
        foreach (var group in claimsByType)
        {
            var critical = group.Count(c => c.RiskLevel == RiskLevel.Critical);
            var high = group.Count(c => c.RiskLevel == RiskLevel.High);
            var medium = group.Count(c => c.RiskLevel == RiskLevel.Medium);
            var low = group.Count(c => c.RiskLevel == RiskLevel.Low);
            csvContent.AppendLine($"{group.Key},{group.Count()},{critical},{high},{medium},{low}");
        }
        csvContent.AppendLine();

        csvContent.AppendLine("Compliance Issues");
        csvContent.AppendLine("Issue Title,Category,Severity,Location,Resolved");
        foreach (var issue in document.ComplianceIssues)
        {
            csvContent.AppendLine($"\"{issue.Title}\",{issue.Category},{issue.Severity},{issue.Location},{issue.IsResolved}");
        }
        csvContent.AppendLine();

        csvContent.AppendLine("Detailed Claims");
        csvContent.AppendLine("Text,Type,Risk Level,Location,Has Reference,Approved");
        foreach (var claim in document.Claims)
        {
            csvContent.AppendLine($"\"{claim.Text}\",{claim.ClaimType},{claim.RiskLevel},{claim.Location},{claim.HasReference},{claim.IsApproved}");
        }

        return Encoding.UTF8.GetBytes(csvContent.ToString());
    }

    private List<string> GenerateRecommendations(PromoDocument document)
    {
        var recommendations = new List<string>();

        var criticalCount = document.ComplianceIssues.Count(i => i.Severity == IssueSeverity.Critical);
        if (criticalCount > 0)
        {
            recommendations.Add($"URGENT: Address all {criticalCount} critical compliance issues before MLR submission.");
        }

        if (!document.ComplianceIssues.Any(i => i.Category == ComplianceCategory.MissingISI))
        {
            recommendations.Add("Ensure Important Safety Information section is complete and current.");
        }

        if (document.Claims.Any(c => c.RiskLevel >= RiskLevel.High && !c.HasReference))
        {
            recommendations.Add("Add clinical citations for all high and critical risk claims.");
        }

        if (document.ComplianceIssues.Any(i => i.Category == ComplianceCategory.MissingFairBalance))
        {
            recommendations.Add("Expand risks/adverse events section to ensure fair balance with benefits.");
        }

        if (document.Claims.Any(c => c.IsApproved != true))
        {
            recommendations.Add("Review and formally approve all detected claims.");
        }

        if (document.OverallScore >= 80)
        {
            recommendations.Add("Document is ready for MLR review process.");
        }

        return recommendations;
    }
}
