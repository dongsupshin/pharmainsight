using System.Text.RegularExpressions;
using PromoComply.Models;

namespace PromoComply.Services;

public class ClaimsDetectorService : IClaimsDetector
{
    private readonly Dictionary<ClaimType, (Regex[] patterns, RiskLevel[] riskLevels)> _claimPatterns;

    public ClaimsDetectorService()
    {
        _claimPatterns = new Dictionary<ClaimType, (Regex[], RiskLevel[])>
        {
            {
                ClaimType.Efficacy,
                (
                    [
                        new Regex(@"\b(effective|proven|demonstrated|clinically proven|significantly improved|superior efficacy|improved outcomes?|enhance|boost|promote\s+recovery|accelerate\s+healing)\b", RegexOptions.IgnoreCase),
                        new Regex(@"\b(reduces?|decreases?|lowers?|diminishes?|alleviates?)\s+(symptoms?|pain|inflammation|adverse|risk|complications?)\b", RegexOptions.IgnoreCase),
                        new Regex(@"\b(increases?|improves?|enhances?)\s+(response|remission|survival|quality\s+of\s+life|efficacy|effectiveness)\b", RegexOptions.IgnoreCase),
                        new Regex(@"\b(works?\s+(better|faster|more\s+effectively|longer))\b", RegexOptions.IgnoreCase),
                        new Regex(@"\b(statistically\s+significant|p\s*<|p\s*=|confidence\s+interval)\b", RegexOptions.IgnoreCase)
                    ],
                    [RiskLevel.Medium, RiskLevel.Medium, RiskLevel.High, RiskLevel.High, RiskLevel.Medium]
                )
            },
            {
                ClaimType.Safety,
                (
                    [
                        new Regex(@"\b(safe|safer|safe\s+for|gentle|well[\s-]tolerated|minimal\s+side\s+effects?|low\s+risk|negligible\s+risk|safe\s+in|tolerable)\b", RegexOptions.IgnoreCase),
                        new Regex(@"\b(no\s+serious|lacks?|without\s+(significant\s+)?adverse|free\s+from)\b", RegexOptions.IgnoreCase),
                        new Regex(@"\b(does\s+not\s+cause|unlikely\s+to\s+cause|minimal\s+risk\s+of)\b", RegexOptions.IgnoreCase)
                    ],
                    [RiskLevel.Low, RiskLevel.Medium, RiskLevel.Medium]
                )
            },
            {
                ClaimType.Superiority,
                (
                    [
                        new Regex(@"\b(best[\s-]in[\s-]class|first[\s-]in[\s-]class|breakthrough|revolutionary|only\s+drug|gold\s+standard|most\s+effective|superior\s+to)\b", RegexOptions.IgnoreCase),
                        new Regex(@"\b(unique|unprecedented|unmatched|fastest[\s-]acting|longest[\s-]lasting)\b", RegexOptions.IgnoreCase)
                    ],
                    [RiskLevel.Critical, RiskLevel.High]
                )
            },
            {
                ClaimType.Comparative,
                (
                    [
                        new Regex(@"\b(compared\s+to|versus|versus|better\s+than|more\s+effective\s+than|superior\s+to|outperforms?|vs\.?)\b", RegexOptions.IgnoreCase),
                        new Regex(@"\b(unlike|in\s+contrast\s+to|advantage\s+over)\b", RegexOptions.IgnoreCase)
                    ],
                    [RiskLevel.High, RiskLevel.High]
                )
            },
            {
                ClaimType.Economic,
                (
                    [
                        new Regex(@"\b(cost[\s-]effective|cost\s+savings?|reduces?\s+costs?|affordable|economical|saves?\s+money|decreases?\s+healthcare\s+costs?|reduces?\s+disease\s+burden|return\s+on\s+investment)\b", RegexOptions.IgnoreCase)
                    ],
                    [RiskLevel.Medium]
                )
            }
        };
    }

    public List<PromoClaim> DetectClaims(string text)
    {
        var claims = new List<PromoClaim>();

        if (string.IsNullOrEmpty(text))
            return claims;

        var sentences = SplitIntoSentences(text);
        var lineNumber = 1;

        foreach (var sentence in sentences)
        {
            foreach (var (claimType, (patterns, riskLevels)) in _claimPatterns)
            {
                for (int i = 0; i < patterns.Length; i++)
                {
                    var matches = patterns[i].Matches(sentence);
                    foreach (Match match in matches)
                    {
                        var claim = ExtractClaimContext(sentence, match.Index, match.Length);
                        if (claim.Length > 10 && !claims.Any(c => c.Text == claim))
                        {
                            claims.Add(new PromoClaim
                            {
                                Text = claim,
                                ClaimType = claimType,
                                RiskLevel = riskLevels[i],
                                Location = $"Line {lineNumber}",
                                HasReference = DetectReference(sentence, match.Index),
                                ReferenceText = ExtractReferenceInfo(sentence, match.Index)
                            });
                        }
                    }
                }
            }

            lineNumber++;
        }

        return claims.Distinct(new ClaimComparer()).ToList();
    }

    private List<string> SplitIntoSentences(string text)
    {
        var sentences = new List<string>();
        var regex = new Regex(@"(?<=[.!?])\s+|(?<=[.!?]$)");
        var parts = regex.Split(text);

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (trimmed.Length > 0)
            {
                sentences.Add(trimmed);
            }
        }

        return sentences.Count > 0 ? sentences : [text];
    }

    private string ExtractClaimContext(string sentence, int matchIndex, int matchLength)
    {
        var startIndex = Math.Max(0, matchIndex - 30);
        var endIndex = Math.Min(sentence.Length, matchIndex + matchLength + 30);
        var context = sentence.Substring(startIndex, endIndex - startIndex).Trim();

        if (context.Length > 200)
        {
            context = context.Substring(0, 200) + "...";
        }

        return context;
    }

    private bool DetectReference(string text, int claimIndex)
    {
        var surroundingText = text.Substring(
            Math.Max(0, claimIndex - 50),
            Math.Min(100, text.Length - Math.Max(0, claimIndex - 50))
        );

        var referencePatterns = new[]
        {
            new Regex(@"[\[\(]\d+[\]\)]"),
            new Regex(@"\b[Ee]t\s+al\.?"),
            new Regex(@"\b(et al|reference|citation|study|trial|data)"),
            new Regex(@"\b\d{4}\b"),
            new Regex(@"\b(JAMA|NEJM|Lancet|Nature|Science|BMJ|FDA|ACC|AHA|ADA)\b")
        };

        return referencePatterns.Any(pattern => pattern.IsMatch(surroundingText));
    }

    private string ExtractReferenceInfo(string text, int claimIndex)
    {
        var startIndex = Math.Max(0, claimIndex - 100);
        var endIndex = Math.Min(text.Length, claimIndex + 150);
        var context = text.Substring(startIndex, endIndex - startIndex);

        var referenceMatch = Regex.Match(context, @"[\[\(]\d+[\]\)]|et\s+al\.?|(\d{4})", RegexOptions.IgnoreCase);
        return referenceMatch.Success ? referenceMatch.Value : string.Empty;
    }

    private class ClaimComparer : IEqualityComparer<PromoClaim>
    {
        public bool Equals(PromoClaim? x, PromoClaim? y)
        {
            return x?.Text == y?.Text && x?.ClaimType == y?.ClaimType;
        }

        public int GetHashCode(PromoClaim obj)
        {
            return HashCode.Combine(obj.Text, obj.ClaimType);
        }
    }
}
