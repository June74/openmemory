// Copyright 2026 OpenMemory contributors
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace OpenMemory.Contracts.Tests;

/// <summary>
/// Binds docs/THREAT_MODEL.md to the two documents it answers to: the required
/// verification classes in DATA_AND_PRIVACY.md section 12, and the frozen rules
/// under docs/contracts. A verification class that no threat covers, a coverage
/// row citing a threat that no table defines, and a threat citing a contract rule
/// that no contract declares each fail here rather than surviving as prose that
/// reads correct and is not.
/// </summary>
public class ThreatModelCoverageTests
{
    private const string ThreatModelDocument = "docs/THREAT_MODEL.md";

    private const string PrivacyDocument = "docs/DATA_AND_PRIVACY.md";

    private const string ContractDirectory = "docs/contracts";

    private const string RulesHeadingPrefix = "## 3.";

    private const string VerificationHeadingPrefix = "## 12.";

    // The threat model's coverage section heading is
    // '## 7. `DATA_AND_PRIVACY.md` [section] 12 coverage'. It is located by this
    // marker rather than by its number, so renumbering the section does not
    // silently reduce this class to a set of vacuous passes.
    private const string CoverageHeadingMarker = "12 coverage";

    // DATA_AND_PRIVACY.md section 12 lists twelve verification classes, and the
    // plan docs/superpowers/plans/2026-08-21-stage0-wave-d.md, section 'Frozen
    // allocations', allocates THR-001 through THR-029.
    private const int RequiredVerificationClasses = 12;

    private const int AllocatedThreats = 29;

    private static readonly string[] ContractFileNames =
    {
        "EXTERNAL_PROCESSING_CONSENT.md",
        "PUBLISHER_AUTHENTICATION.md",
        "REGISTERED_CLIENT_CAPABILITIES.md",
        "TRUSTED_HUMAN_CONFIRMATION.md",
    };

    // A coverage row: | verification class | `THR-001`, `THR-002` |
    private static readonly Regex CoverageRow = new(
        @"^\|(?<class>[^|]+)\|(?<threats>[^|]+)\|\s*$",
        RegexOptions.Compiled);

    private static readonly Regex ThreatCitation = new(
        @"`(?<id>THR-[0-9]{3})`",
        RegexOptions.Compiled);

    // A threat definition row, whose first cell is the backticked identifier.
    private static readonly Regex ThreatDefinitionRow = new(
        @"^\|\s*`(?<id>THR-[0-9]{3})`\s*\|",
        RegexOptions.Compiled);

    private static readonly Regex ContractRuleRow = new(
        @"^\|\s*`(?<id>SC-[A-Z]+-[0-9]{3})`\s*\|",
        RegexOptions.Compiled);

    private static readonly Regex ContractRuleCitation = new(
        @"SC-[A-Z]+-[0-9]{3}",
        RegexOptions.Compiled);

    [Fact]
    public void PrivacyDocumentListsTheExpectedNumberOfRequiredVerificationClasses()
    {
        var classes = ReadRequiredVerificationClasses();
        var listsExpectedCount = classes.Count == RequiredVerificationClasses;

        Assert.True(
            listsExpectedCount,
            $"Section 12 of {PrivacyDocument} lists {classes.Count} verification-class bullets; "
            + $"{RequiredVerificationClasses} are expected. If a class was added or removed there, "
            + $"the coverage table in {ThreatModelDocument} and this expected count both change in "
            + "the same commit, so that the two documents cannot drift apart unnoticed.");
    }

    [Fact]
    public void EveryRequiredVerificationClassAppearsInTheThreatModelCoverageTable()
    {
        var coverage = ReadCoverageTable();

        foreach (var verificationClass in ReadRequiredVerificationClasses())
        {
            var covered = coverage.ContainsKey(verificationClass);

            Assert.True(
                covered,
                $"Section 12 of {PrivacyDocument} requires the verification class "
                + $"'{verificationClass}', but no row of the coverage table in {ThreatModelDocument} "
                + "names it. The two are compared after trimming whitespace and a single trailing "
                + "';' or '.', and must otherwise match exactly, so that rewording one document "
                + "without the other fails here.");
        }
    }

    [Fact]
    public void TheThreatModelDefinesTheAllocatedNumberOfThreats()
    {
        var defined = ReadDefinedThreats();
        var definesExpectedCount = defined.Count == AllocatedThreats;

        Assert.True(
            definesExpectedCount,
            $"{ThreatModelDocument} defines {defined.Count} threats in its boundary tables; "
            + $"{AllocatedThreats} are allocated. Identifiers are allocated monotonically and never "
            + "reused, so a threat is withdrawn in place rather than deleted.");
    }

    [Fact]
    public void EveryThreatCitedByTheCoverageTableIsDefinedInTheThreatModel()
    {
        var defined = ReadDefinedThreats();

        foreach (var row in ReadCoverageTable())
        {
            foreach (var threat in row.Value)
            {
                var isDefined = defined.Contains(threat);

                Assert.True(
                    isDefined,
                    $"The coverage table in {ThreatModelDocument} cites '{threat}' for the "
                    + $"verification class '{row.Key}', but no boundary table in that document "
                    + "defines it. A coverage row that cites an undefined threat claims coverage "
                    + "that does not exist.");
            }
        }
    }

    [Fact]
    public void EveryContractRuleTheThreatModelCitesExistsInTheContracts()
    {
        var declared = ReadContractRuleIdentifiers();
        var text = RepositoryPaths.Read("docs", "THREAT_MODEL.md");
        var cited = ContractRuleCitation
            .Matches(text)
            .Select(match => match.Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();

        var citesAnyRule = cited.Count > 0;

        Assert.True(
            citesAnyRule,
            $"{ThreatModelDocument} cites no 'SC-AREA-NNN' contract rule at all. Every threat names "
            + "the frozen rules that govern it; a threat model citing none is not bound to the "
            + "contracts, and this class would then check nothing.");

        foreach (var rule in cited)
        {
            var exists = declared.Contains(rule);

            Assert.True(
                exists,
                $"{ThreatModelDocument} cites contract rule '{rule}', which no document under "
                + $"{ContractDirectory} declares in its '{RulesHeadingPrefix} Rules' table. Renaming "
                + "or withdrawing a rule without updating the threat model must fail here rather "
                + "than leaving a citation that points at nothing. The reverse does not hold: a rule "
                + "that no threat cites is allowed, and three rules are deliberately uncited.");
        }
    }

    /// <summary>
    /// The verification classes required by section 12 of DATA_AND_PRIVACY.md,
    /// normalized for comparison against the threat model's coverage table.
    /// </summary>
    private static List<string> ReadRequiredVerificationClasses()
    {
        var section = SectionBody(
            RepositoryPaths.ReadLines("docs", "DATA_AND_PRIVACY.md"),
            heading => heading.StartsWith(VerificationHeadingPrefix, StringComparison.Ordinal));

        var classes = section
            .Where(line => line.StartsWith("- ", StringComparison.Ordinal))
            .Select(line => Normalize(line[2..]))
            .ToList();

        var parsedAnyBullet = classes.Count > 0;

        Assert.True(
            parsedAnyBullet,
            $"No verification-class bullets were parsed from section 12 of {PrivacyDocument}. The "
            + $"section is found by its '{VerificationHeadingPrefix}' heading and read to the next "
            + "'## ' heading; a blank line follows that heading immediately, so a range that stopped "
            + "at the first blank line would match nothing and let every assertion here pass "
            + "vacuously.");

        return classes;
    }

    /// <summary>
    /// The threat model's coverage table, as a map from normalized verification
    /// class to the threats that row cites.
    /// </summary>
    private static SortedDictionary<string, List<string>> ReadCoverageTable()
    {
        var section = SectionBody(
            RepositoryPaths.ReadLines("docs", "THREAT_MODEL.md"),
            heading => heading.Contains(CoverageHeadingMarker, StringComparison.Ordinal));

        var rows = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var line in section)
        {
            var match = CoverageRow.Match(line);
            if (!match.Success)
            {
                continue;
            }

            var threats = ThreatCitation
                .Matches(match.Groups["threats"].Value)
                .Select(citation => citation.Groups["id"].Value)
                .ToList();

            // The table's header and separator rows carry no threat citation, which
            // is what distinguishes them from a coverage row.
            if (threats.Count == 0)
            {
                continue;
            }

            var verificationClass = Normalize(match.Groups["class"].Value);
            var duplicate = rows.ContainsKey(verificationClass);

            Assert.False(
                duplicate,
                $"The coverage table in {ThreatModelDocument} names the verification class "
                + $"'{verificationClass}' in more than one row. Two rows for one class let a reader "
                + "believe the more complete one.");

            rows.Add(verificationClass, threats);
        }

        var parsedAnyRow = rows.Count > 0;

        Assert.True(
            parsedAnyRow,
            $"No coverage rows were parsed from the '{CoverageHeadingMarker}' section of "
            + $"{ThreatModelDocument}. A coverage row is '| verification class | `THR-001` |'; an "
            + "unparsed table would let every other assertion in this class pass without checking "
            + "anything.");

        return rows;
    }

    /// <summary>The threats defined by the boundary tables of the threat model.</summary>
    private static SortedSet<string> ReadDefinedThreats()
    {
        var defined = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var line in RepositoryPaths.ReadLines("docs", "THREAT_MODEL.md"))
        {
            var match = ThreatDefinitionRow.Match(line);
            if (!match.Success)
            {
                continue;
            }

            var id = match.Groups["id"].Value;
            var firstDefinition = defined.Add(id);

            Assert.True(
                firstDefinition,
                $"{ThreatModelDocument} defines '{id}' in more than one row. Every threat appears in "
                + "exactly one boundary table, so that its label, governing rules, mitigating stage, "
                + "and owed verification have one home.");
        }

        var parsedAnyRow = defined.Count > 0;

        Assert.True(
            parsedAnyRow,
            $"No threat definitions were parsed from {ThreatModelDocument}. A definition row starts "
            + "'| `THR-NNN` |'; an unparsed set would let the citation checks in this class pass "
            + "without checking anything.");

        return defined;
    }

    /// <summary>
    /// Every rule identifier declared by the rules table of a document under
    /// docs/contracts. Only those tables are read: a whole-file scan would also
    /// collect the cross-references in the prose, and a citation of a rule that no
    /// longer exists would then vouch for itself.
    /// </summary>
    private static SortedSet<string> ReadContractRuleIdentifiers()
    {
        var declared = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var fileName in ContractFileNames)
        {
            var lines = RepositoryPaths.ReadLines("docs", "contracts", fileName);
            var section = SectionBody(
                lines,
                heading => heading.StartsWith(RulesHeadingPrefix, StringComparison.Ordinal));

            foreach (var line in section)
            {
                var match = ContractRuleRow.Match(line);
                if (match.Success)
                {
                    declared.Add(match.Groups["id"].Value);
                }
            }
        }

        var parsedAnyRule = declared.Count > 0;

        Assert.True(
            parsedAnyRule,
            $"No rule identifiers were parsed from the '{RulesHeadingPrefix} Rules' tables under "
            + $"{ContractDirectory}. Without them every contract-rule citation in "
            + $"{ThreatModelDocument} would report as unknown, or, worse, the check would be skipped "
            + "entirely.");

        return declared;
    }

    /// <summary>
    /// The body lines of the first section whose '## ' heading satisfies the given
    /// predicate, up to the next '## ' heading.
    /// </summary>
    private static List<string> SectionBody(IEnumerable<string> lines, Func<string, bool> isWantedHeading)
    {
        var body = new List<string>();
        var insideSection = false;

        foreach (var line in lines)
        {
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                if (insideSection)
                {
                    break;
                }

                insideSection = isWantedHeading(line);
                continue;
            }

            if (insideSection)
            {
                body.Add(line);
            }
        }

        return body;
    }

    /// <summary>Trims whitespace and a single trailing ';' or '.' for comparison.</summary>
    private static string Normalize(string text)
    {
        var trimmed = text.Trim();

        if (trimmed.Length > 0)
        {
            var last = trimmed[^1];
            if (last == ';' || last == '.')
            {
                trimmed = trimmed[..^1].TrimEnd();
            }
        }

        return trimmed;
    }
}
