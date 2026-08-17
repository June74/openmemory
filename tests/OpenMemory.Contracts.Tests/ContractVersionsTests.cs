// Copyright 2026 OpenMemory contributors
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace OpenMemory.Contracts.Tests;

/// <summary>
/// Guards agreement between the contract integers declared in code and the
/// values docs/COMPATIBILITY.md records. A change to either that is not
/// mirrored in the other fails the build.
/// </summary>
public class ContractVersionsTests
{
    private static readonly Dictionary<string, int> Declared = new()
    {
        ["MCP protocol"] = ContractVersions.McpProtocol,
        ["Named-pipe envelope"] = ContractVersions.PipeEnvelope,
        ["Database schema"] = ContractVersions.DatabaseSchema,
        ["Normalized event envelope"] = ContractVersions.EventEnvelope,
        ["Markdown projection protocol"] = ContractVersions.ProjectionProtocol,
        ["Portable export format"] = ContractVersions.PortableExportFormat,
    };

    [Fact]
    public void DeclaredVersionsMatchTheCompatibilityDocument()
    {
        var documented = ReadDocumentedIntegerSurfaces();

        Assert.Equal(
            Declared.Keys.OrderBy(k => k, StringComparer.Ordinal),
            documented.Keys.OrderBy(k => k, StringComparer.Ordinal));

        foreach (var (surface, value) in Declared)
        {
            Assert.Equal(documented[surface], value);
        }
    }

    private static Dictionary<string, int> ReadDocumentedIntegerSurfaces()
    {
        var path = Path.Combine(FindRepositoryRoot(), "docs", "COMPATIBILITY.md");
        var found = new Dictionary<string, int>();

        // Rows look like: | Surface | Integer | 1 | notes |
        var row = new Regex(@"^\|\s*(?<surface>[^|]+?)\s*\|\s*Integer\s*\|\s*`?(?<value>\d+)`?\s*\|");

        foreach (var line in File.ReadLines(path))
        {
            var match = row.Match(line);
            if (match.Success)
            {
                found[match.Groups["surface"].Value.Trim()] = int.Parse(match.Groups["value"].Value);
            }
        }

        Assert.NotEmpty(found);
        return found;
    }

    private static string FindRepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, ".git")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return dir!.FullName;
    }
}
