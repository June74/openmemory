// Copyright 2026 OpenMemory contributors
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace OpenMemory.Storage.Tests;

/// <summary>
/// Architecture-fitness guard: OpenMemory.Storage must depend on OpenMemory.Contracts
/// only. It must never reference OpenMemory.Indexing or any adapter, so the storage
/// layer cannot become entangled with indexing or a concrete provider.
/// </summary>
public class ProjectReferenceTests
{
    [Fact]
    public void StorageReferencesOnlyContracts()
    {
        var projectReferences = ReadProjectReferences(
            Path.Combine(FindRepositoryRoot(), "src", "OpenMemory.Storage", "OpenMemory.Storage.csproj"));

        Assert.Equal(
            new[] { "OpenMemory.Contracts" },
            projectReferences);
    }

    private static List<string> ReadProjectReferences(string csprojPath)
    {
        var text = File.ReadAllText(csprojPath);
        var matches = Regex.Matches(text, @"<ProjectReference\s+Include=""(?<path>[^""]+)""");

        return matches
            .Select(m => m.Groups["path"].Value)
            .Select(p => p.Replace('\\', '/'))
            .Select(p => Path.GetFileNameWithoutExtension(p))
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();
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
