// Copyright 2026 OpenMemory contributors
// SPDX-License-Identifier: Apache-2.0

namespace OpenMemory.Contracts.Tests;

/// <summary>
/// Locates repository files for tests that assert agreement between code and
/// the documents that specify it. Tests run from a build output directory
/// several levels below the repository root, so the root is found by walking
/// upward to the directory containing <c>.git</c>.
/// </summary>
public static class RepositoryPaths
{
    private static readonly Lazy<string> LazyRoot = new(FindRoot);

    /// <summary>The repository root directory.</summary>
    public static string Root => LazyRoot.Value;

    /// <summary>Combines repository-relative segments into an absolute path.</summary>
    public static string Path(params string[] segments)
    {
        var parts = new string[segments.Length + 1];
        parts[0] = Root;
        segments.CopyTo(parts, 1);
        return System.IO.Path.Combine(parts);
    }

    /// <summary>Reads a repository-relative file in full.</summary>
    public static string Read(params string[] segments) => File.ReadAllText(Path(segments));

    /// <summary>Reads a repository-relative file as lines.</summary>
    public static string[] ReadLines(params string[] segments) => File.ReadAllLines(Path(segments));

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !Directory.Exists(System.IO.Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }

        // A null return here would surface later as an unexplained empty parse
        // in whichever test happened to run first, so fail where the cause is.
        return directory?.FullName
            ?? throw new DirectoryNotFoundException(
                $"No repository root containing a .git directory was found above '{AppContext.BaseDirectory}'.");
    }
}
