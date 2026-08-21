// Copyright 2026 OpenMemory contributors
// SPDX-License-Identifier: Apache-2.0

using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace OpenMemory.Contracts.Tests;

/// <summary>
/// Guards agreement between the committed fixtures under tests/fixtures and the
/// SHA-256 manifest that records them (decision D-095). A fixture edited without
/// regenerating the manifest, a manifest row naming a file that no longer exists,
/// and a fixture added without a manifest row each fail the build here rather
/// than silently weakening whichever test consumes the fixture later.
/// </summary>
public class FixtureManifestTests
{
    private const string ManifestFileName = "MANIFEST.md";

    private const string RegenerateHint =
        "Regenerate the manifest in the same commit; docs/TEST_FIXTURES.md gives the command.";

    // Rows look like: | `events/sample.json` | `<64 lowercase hex>` | purpose | consumer |
    // The header and separator rows cannot match, because a fixture row's second
    // cell must be a backticked 64-character lowercase hex digest.
    private static readonly Regex ManifestRow = new(
        @"^\|\s*`(?<fixture>[^`|]+)`\s*\|\s*`(?<sha256>[0-9a-f]{64})`\s*\|",
        RegexOptions.Compiled);

    private static string FixtureRoot => RepositoryPaths.Path("tests", "fixtures");

    [Fact]
    public void EveryManifestRowNamesAnExistingFile()
    {
        foreach (var (fixture, _) in ReadManifest())
        {
            var path = FixturePath(fixture);
            var exists = File.Exists(path);

            Assert.True(
                exists,
                $"tests/fixtures/{ManifestFileName} lists fixture '{fixture}', but no file exists at "
                + $"'{path}'. Either the fixture was renamed, moved, or deleted without updating the "
                + $"manifest, or the row names the wrong path. {RegenerateHint}");
        }
    }

    [Fact]
    public void EveryFixtureFileAppearsInTheManifest()
    {
        var manifest = ReadManifest();
        var files = EnumerateFixtureFiles();

        Assert.NotEmpty(files);

        foreach (var fixture in files)
        {
            var listed = manifest.ContainsKey(fixture);

            Assert.True(
                listed,
                $"Fixture file 'tests/fixtures/{fixture}' is not listed in tests/fixtures/"
                + $"{ManifestFileName}. Every committed fixture carries a manifest row naming it and "
                + $"its SHA-256, so that an edit to it is a deliberate, visible act. {RegenerateHint}");
        }
    }

    [Fact]
    public void EveryFixtureFileMatchesItsManifestChecksum()
    {
        foreach (var (fixture, expected) in ReadManifest())
        {
            var path = FixturePath(fixture);
            var exists = File.Exists(path);

            Assert.True(
                exists,
                $"tests/fixtures/{ManifestFileName} lists fixture '{fixture}', but no file exists at "
                + $"'{path}', so its checksum cannot be verified. {RegenerateHint}");

            var actual = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
            var matches = string.Equals(expected, actual, StringComparison.Ordinal);

            Assert.True(
                matches,
                $"Fixture 'tests/fixtures/{fixture}' has SHA-256 '{actual}', but tests/fixtures/"
                + $"{ManifestFileName} records '{expected}'. The fixture changed, the manifest is "
                + "stale, or the file was checked out with CRLF line endings instead of the LF that "
                + $"'tests/fixtures/** text eol=lf' requires. {RegenerateHint}");
        }
    }

    /// <summary>
    /// Reads the manifest as a map from fixture path (relative to tests/fixtures,
    /// forward slashes) to its recorded lowercase-hex SHA-256.
    /// </summary>
    private static SortedDictionary<string, string> ReadManifest()
    {
        var manifestPath = Path.Combine(FixtureRoot, ManifestFileName);
        var rows = new SortedDictionary<string, string>(StringComparer.Ordinal);

        foreach (var line in File.ReadLines(manifestPath))
        {
            var match = ManifestRow.Match(line);
            if (!match.Success)
            {
                continue;
            }

            var fixture = match.Groups["fixture"].Value.Trim();
            var duplicate = rows.ContainsKey(fixture);

            Assert.False(
                duplicate,
                $"tests/fixtures/{ManifestFileName} lists fixture '{fixture}' more than once. Two rows "
                + $"for one file let a reader verify the stale one. {RegenerateHint}");

            rows.Add(fixture, match.Groups["sha256"].Value);
        }

        var parsedAnyRow = rows.Count > 0;

        Assert.True(
            parsedAnyRow,
            $"No fixture rows were parsed from '{manifestPath}'. A row is "
            + "'| `<path>` | `<64 lowercase hex>` | purpose | consumer |'; an unparsed table would let "
            + "every other assertion in this class pass without checking anything.");

        return rows;
    }

    /// <summary>
    /// Every file under tests/fixtures except the manifest itself, as paths
    /// relative to that directory with forward slashes.
    /// </summary>
    private static List<string> EnumerateFixtureFiles()
    {
        var root = FixtureRoot;

        return Directory
            .EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Select(file => Path.GetRelativePath(root, file).Replace('\\', '/'))
            .Where(relative => !string.Equals(relative, ManifestFileName, StringComparison.Ordinal))
            .OrderBy(relative => relative, StringComparer.Ordinal)
            .ToList();
    }

    private static string FixturePath(string fixture) =>
        Path.Combine(FixtureRoot, fixture.Replace('/', Path.DirectorySeparatorChar));
}
