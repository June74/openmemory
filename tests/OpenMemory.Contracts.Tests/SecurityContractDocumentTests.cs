// Copyright 2026 OpenMemory contributors
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.RegularExpressions;

namespace OpenMemory.Contracts.Tests;

/// <summary>
/// Guards the four frozen security contracts under docs/contracts against silent
/// drift: a missing document, a dropped freeze declaration, a rule identifier that
/// does not belong to the document declaring it, and the failure a count can never
/// catch, which is a rule quietly dropped from the middle of an area's numbering.
/// Rule identifiers are read from each document's rules table only. A whole-file
/// scan would be wrong here, because the prose deliberately cites rules from other
/// contracts and every such cross-reference would read as a prefix violation.
/// </summary>
public class SecurityContractDocumentTests
{
    private const string ContractDirectory = "docs/contracts";

    private const string RulesHeadingPrefix = "## 3.";

    private const int TotalAllocatedRules = 34;

    private const string AllocationHint =
        "The allocation is frozen in docs/superpowers/plans/2026-08-21-stage0-wave-d.md, section "
        + "'Frozen allocations'. Identifiers are never renumbered or reused; a withdrawn rule keeps "
        + "its number and is marked withdrawn.";

    // The frozen allocation, from docs/superpowers/plans/2026-08-21-stage0-wave-d.md,
    // 'Frozen allocations' > 'Security contract rule identifiers': SC-CAP 8 rules,
    // SC-CONF 9, SC-CONSENT 9, SC-PUB 8, which is TotalAllocatedRules in total.
    private static readonly ContractDocument[] Contracts =
    {
        new("REGISTERED_CLIENT_CAPABILITIES.md", "SC-CAP-", 8),
        new("TRUSTED_HUMAN_CONFIRMATION.md", "SC-CONF-", 9),
        new("EXTERNAL_PROCESSING_CONSENT.md", "SC-CONSENT-", 9),
        new("PUBLISHER_AUTHENTICATION.md", "SC-PUB-", 8),
    };

    // A rules-table row: | `SC-CAP-001` | rule | fails closed by | source |
    // The header and separator rows cannot match, because a rule row's first cell
    // is backticked.
    private static readonly Regex RuleRow = new(
        @"^\|\s*`(?<id>[^`|]+)`\s*\|",
        RegexOptions.Compiled);

    private static readonly Regex VersionLine = new(
        @"^- \*\*Version:\*\*\s*(?<value>.*?)\s*$",
        RegexOptions.Compiled);

    private static readonly Regex StatusLine = new(
        @"^- \*\*Status:\*\*\s*(?<value>.*?)\s*$",
        RegexOptions.Compiled);

    private static readonly Regex RulePrefixLine = new(
        @"^- \*\*Rule prefix:\*\*\s*`(?<value>[^`]+)`\s*$",
        RegexOptions.Compiled);

    private static readonly Regex DigitsOnly = new(@"^[0-9]+$", RegexOptions.Compiled);

    [Fact]
    public void EveryContractDocumentDeclaresItsFrozenHeader()
    {
        foreach (var contract in Contracts)
        {
            var path = DocumentPath(contract);
            var exists = File.Exists(path);

            Assert.True(
                exists,
                $"The security contract document '{ContractDirectory}/{contract.FileName}' does not "
                + $"exist at '{path}'. All four contracts are required by the Stage 0 exit gate. "
                + AllocationHint);

            var lines = File.ReadAllLines(path);

            var version = FindHeaderValue(lines, VersionLine);
            var declaresIntegerVersion = version is not null && DigitsOnly.IsMatch(version);

            Assert.True(
                declaresIntegerVersion,
                $"'{ContractDirectory}/{contract.FileName}' does not declare an integer version. Its "
                + "header must carry a line of the form '- **Version:** 1'; found "
                + $"{Describe(version)}.");

            var status = FindHeaderValue(lines, StatusLine);
            var declaresFrozen = status is not null && status.StartsWith("Frozen", StringComparison.Ordinal);

            Assert.True(
                declaresFrozen,
                $"'{ContractDirectory}/{contract.FileName}' does not declare itself frozen. Its header "
                + "must carry a line of the form '- **Status:** Frozen (Stage 0)'; found "
                + $"{Describe(status)}.");

            var rulePrefix = FindHeaderValue(lines, RulePrefixLine);
            var declaresExpectedPrefix =
                string.Equals(rulePrefix, contract.RulePrefix, StringComparison.Ordinal);

            Assert.True(
                declaresExpectedPrefix,
                $"'{ContractDirectory}/{contract.FileName}' must carry the header line "
                + $"'- **Rule prefix:** `{contract.RulePrefix}`'; found {Describe(rulePrefix)}. "
                + AllocationHint);
        }
    }

    [Fact]
    public void EveryRuleIdentifierMatchesItsDocumentsDeclaredRulePrefix()
    {
        foreach (var contract in Contracts)
        {
            var declaredPrefix = DeclaredRulePrefix(contract);

            foreach (var id in ReadRuleIdentifiers(contract))
            {
                var matchesPrefix = id.StartsWith(declaredPrefix, StringComparison.Ordinal);

                Assert.True(
                    matchesPrefix,
                    $"The rules table of '{ContractDirectory}/{contract.FileName}' declares rule "
                    + $"'{id}', but that document's declared rule prefix is '{declaredPrefix}'. A rule "
                    + "belongs to exactly one contract; a reference to another contract's rule belongs "
                    + $"in the rule text or the source column, never in the ID column. {AllocationHint}");
            }
        }
    }

    [Fact]
    public void EachContractNumbersItsRulesContiguouslyFromOneWithNoGaps()
    {
        foreach (var contract in Contracts)
        {
            var declared = new SortedSet<int>();

            foreach (var number in ReadRuleNumbers(contract))
            {
                var firstOccurrence = declared.Add(number);

                Assert.True(
                    firstOccurrence,
                    $"'{ContractDirectory}/{contract.FileName}' declares "
                    + $"'{contract.RulePrefix}{number:D3}' more than once. Two rows for one identifier "
                    + $"let a reader satisfy the wrong one. {AllocationHint}");
            }

            var highest = declared.Max;

            for (var expected = 1; expected <= declared.Count; expected++)
            {
                var present = declared.Contains(expected);

                Assert.True(
                    present,
                    $"'{ContractDirectory}/{contract.FileName}' has a gap in its rule numbering: "
                    + $"'{contract.RulePrefix}{expected:D3}' is missing while "
                    + $"'{contract.RulePrefix}{highest:D3}' is present. Numbering runs contiguously "
                    + "from 001, so a gap means a rule was dropped instead of being withdrawn in "
                    + $"place. {AllocationHint}");
            }
        }
    }

    [Fact]
    public void TheFourContractsDeclareExactlyTheAllocatedRuleCounts()
    {
        var total = 0;

        foreach (var contract in Contracts)
        {
            var identifiers = ReadRuleIdentifiers(contract);
            var matchesAllocation = identifiers.Count == contract.ExpectedRuleCount;

            Assert.True(
                matchesAllocation,
                $"'{ContractDirectory}/{contract.FileName}' declares {identifiers.Count} rules in its "
                + $"rules table, but the frozen allocation for '{contract.RulePrefix}' is "
                + $"{contract.ExpectedRuleCount}. {AllocationHint}");

            total += identifiers.Count;
        }

        var matchesTotal = total == TotalAllocatedRules;

        Assert.True(
            matchesTotal,
            $"The four contract documents declare {total} rules in total; the frozen allocation is "
            + $"{TotalAllocatedRules}. {AllocationHint}");
    }

    [Fact]
    public void NoRuleIdentifierIsDeclaredByMoreThanOneContract()
    {
        var owners = new SortedDictionary<string, string>(StringComparer.Ordinal);

        foreach (var contract in Contracts)
        {
            foreach (var id in ReadRuleIdentifiers(contract))
            {
                var alreadyDeclared = owners.TryGetValue(id, out var owner);

                Assert.False(
                    alreadyDeclared,
                    $"Rule '{id}' is declared both by '{ContractDirectory}/{owner}' and by "
                    + $"'{ContractDirectory}/{contract.FileName}'. Every rule identifier names exactly "
                    + $"one rule, in exactly one contract. {AllocationHint}");

                owners[id] = contract.FileName;
            }
        }
    }

    /// <summary>
    /// The first-column identifiers of a contract's '## 3. Rules' table, in document
    /// order. Only that section is read: the surrounding prose cites rules from the
    /// other three contracts, and treating those citations as declarations would
    /// report a prefix violation for a legitimate cross-reference.
    /// </summary>
    private static List<string> ReadRuleIdentifiers(ContractDocument contract)
    {
        var path = DocumentPath(contract);
        var identifiers = new List<string>();
        var insideRulesSection = false;

        foreach (var line in File.ReadLines(path))
        {
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                if (insideRulesSection)
                {
                    break;
                }

                insideRulesSection = line.StartsWith(RulesHeadingPrefix, StringComparison.Ordinal);
                continue;
            }

            if (!insideRulesSection)
            {
                continue;
            }

            var match = RuleRow.Match(line);
            if (match.Success)
            {
                identifiers.Add(match.Groups["id"].Value.Trim());
            }
        }

        var parsedAnyRow = identifiers.Count > 0;

        Assert.True(
            parsedAnyRow,
            $"No rule rows were parsed from the '{RulesHeadingPrefix} Rules' section of "
            + $"'{ContractDirectory}/{contract.FileName}'. A rule row is "
            + "'| `SC-AREA-NNN` | rule | fails closed by | source |' inside that section; an unparsed "
            + "table would let every other assertion in this class pass without checking anything.");

        return identifiers;
    }

    /// <summary>The numeric suffixes of a contract's rule identifiers, in document order.</summary>
    private static List<int> ReadRuleNumbers(ContractDocument contract)
    {
        var declaredPrefix = DeclaredRulePrefix(contract);
        var numbers = new List<int>();

        foreach (var id in ReadRuleIdentifiers(contract))
        {
            var hasExpectedShape = id.Length == declaredPrefix.Length + 3
                && id.StartsWith(declaredPrefix, StringComparison.Ordinal);
            var suffix = hasExpectedShape ? id[declaredPrefix.Length..] : string.Empty;
            var wellFormed = hasExpectedShape && DigitsOnly.IsMatch(suffix);

            Assert.True(
                wellFormed,
                $"Rule identifier '{id}' in '{ContractDirectory}/{contract.FileName}' is not of the "
                + $"form '{declaredPrefix}NNN' with exactly three digits, so its place in that "
                + $"contract's numbering cannot be checked. {AllocationHint}");

            numbers.Add(int.Parse(suffix, CultureInfo.InvariantCulture));
        }

        return numbers;
    }

    private static string DeclaredRulePrefix(ContractDocument contract)
    {
        var value = FindHeaderValue(File.ReadAllLines(DocumentPath(contract)), RulePrefixLine);
        var declared = value is not null;

        Assert.True(
            declared,
            $"'{ContractDirectory}/{contract.FileName}' declares no rule prefix, so the rules in its "
            + "table cannot be checked against the contract they belong to. Its header must carry "
            + $"'- **Rule prefix:** `{contract.RulePrefix}`'.");

        return value!;
    }

    private static string? FindHeaderValue(IEnumerable<string> lines, Regex pattern)
    {
        foreach (var line in lines)
        {
            var match = pattern.Match(line);
            if (match.Success)
            {
                return match.Groups["value"].Value;
            }
        }

        return null;
    }

    private static string Describe(string? value) => value is null ? "no such line" : $"'{value}'";

    private static string DocumentPath(ContractDocument contract) =>
        RepositoryPaths.Path("docs", "contracts", contract.FileName);

    /// <summary>One frozen contract document and the identifiers allocated to it.</summary>
    private sealed record ContractDocument(string FileName, string RulePrefix, int ExpectedRuleCount);
}
