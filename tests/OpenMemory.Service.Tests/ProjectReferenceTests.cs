// Copyright 2026 OpenMemory contributors
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace OpenMemory.Service.Tests;

/// <summary>
/// Architecture-fitness guard. ARCHITECTURE.md requires the MCP bridge to contain no
/// memory logic and never become a second source of truth: it must have no compile-time
/// path to OpenMemory.Storage. This test asserts that structural boundary directly from
/// the .csproj files, so a future contributor cannot add the forbidden reference without
/// breaking the build.
/// </summary>
public class ProjectReferenceTests
{
    [Fact]
    public void ServiceCliAndMcpBridgeDeclareExactlyTheirIntendedReferences()
    {
        var repositoryRoot = FindRepositoryRoot();

        var serviceReferences = ReadProjectReferences(
            Path.Combine(repositoryRoot, "src", "OpenMemory.Service", "OpenMemory.Service.csproj"));
        var cliReferences = ReadProjectReferences(
            Path.Combine(repositoryRoot, "src", "OpenMemory.Cli", "OpenMemory.Cli.csproj"));
        var mcpBridgeReferences = ReadProjectReferences(
            Path.Combine(repositoryRoot, "src", "OpenMemory.McpBridge", "OpenMemory.McpBridge.csproj"));

        // The service is the only component allowed a compile-time path to Storage,
        // Indexing, and the adapter abstraction (never a concrete adapter).
        Assert.Equal(
            new[]
            {
                "OpenMemory.Adapters.Abstractions",
                "OpenMemory.Contracts",
                "OpenMemory.Indexing",
                "OpenMemory.Storage",
            },
            serviceReferences);

        // The CLI talks to the service over its own boundary, not by linking Storage.
        Assert.Equal(
            new[] { "OpenMemory.Contracts" },
            cliReferences);

        // The MCP bridge must contain no memory logic and never become a second
        // source of truth (ARCHITECTURE.md): it must have no compile-time path to
        // Storage.
        Assert.Equal(
            new[] { "OpenMemory.Contracts" },
            mcpBridgeReferences);
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
