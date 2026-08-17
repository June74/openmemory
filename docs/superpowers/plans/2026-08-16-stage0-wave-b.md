# Stage 0 Wave B — Toolchain and Solution Boundaries Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Pin the .NET and Node toolchains and create the solution's project boundaries and dependency graph, without adding product behavior.

**Architecture:** A single `OpenMemory.sln` at the repository root, C# projects under `src/`, test projects under `tests/`, and the TypeScript Obsidian plugin as `src/OpenMemory.ObsidianPlugin/` outside the solution. The deliverable is the **dependency graph**, not the file count: which project may reference which is what later stages rely on and what is expensive to change. Two edges are deliberately absent so the compiler enforces architecture that would otherwise depend on review discipline.

**Tech Stack:** .NET SDK 10.0.400, C# targeting `net10.0`, xUnit, MSBuild with central package management, Node 24 / pnpm 11, TypeScript.

**Spec:** [docs/superpowers/specs/2026-08-16-stage0-wave-b-design.md](../specs/2026-08-16-stage0-wave-b-design.md)

## Global Constraints

Every task's requirements implicitly include this section.

- **No product behavior.** No memory logic, database access, MCP handling, or adapter implementation. Executable projects get a stub entry point and nothing more. The only code with product meaning in this entire wave is the version constants in Task 5.
- **Repository:** `June74/openmemory`. **Branch:** `codex/stage0-wave-b`. Never commit to `main`.
- **DCO required.** Every commit uses `git commit -s`.
- **Never write a secret value** into any file, including examples.
- **Apache-2.0 header on every new `.cs` and `.ts` file**, SPDX short form, exactly:
  ```
  // Copyright 2026 OpenMemory contributors
  // SPDX-License-Identifier: Apache-2.0
  ```
  `LICENSE` and `NOTICE` remain authoritative; the header points at them rather than restating terms.
- **Never commit build output.** `bin/` and `obj/` are already in `.gitignore`. If either appears in `git status`, stop and investigate rather than force-adding.
- **`TreatWarningsAsErrors` is on.** A warning fails the build. Do not suppress one to get green — report it.
- **Do not add NuGet packages** beyond what the xUnit template itself requires.
- **Use Bash** (Git Bash available), never a PowerShell pipeline. `&&` chaining works in Bash; PowerShell 5.1 rejects it.
- **Do not guess package versions.** Take them from what the `dotnet new` template generates, then centralize them.

---

### Task 1: Toolchain pins and empty solution

**Files:**
- Create: `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `OpenMemory.sln`

**Interfaces:**
- Consumes: nothing.
- Produces: the solution file every later task adds projects to, and the inherited MSBuild properties (`net10.0`, nullable, warnings-as-errors) that every project relies on rather than restating.

- [ ] **Step 1: Verify the pin is absent**

Run: `cd "C:/Users/2006i/projects/openmemory" && test -f global.json && echo "exists" || echo "absent"`

Expected: `absent`.

- [ ] **Step 2: Write `global.json`**

```json
{
  "sdk": {
    "version": "10.0.400",
    "rollForward": "latestPatch"
  }
}
```

`latestPatch` accepts 10.0.4xx patches but refuses .NET 11, so a contributor with a newer SDK cannot silently produce a different build.

- [ ] **Step 3: Write `Directory.Build.props`**

MSBuild automatically imports this into every project in the tree, so the twelve project files do not each restate these settings.

```xml
<Project>

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <Deterministic>true</Deterministic>
    <GenerateDocumentationFile>false</GenerateDocumentationFile>
  </PropertyGroup>

  <PropertyGroup>
    <Product>OpenMemory</Product>
    <Company>OpenMemory contributors</Company>
    <Copyright>Copyright 2026 OpenMemory contributors</Copyright>
    <PackageLicenseExpression>Apache-2.0</PackageLicenseExpression>
    <RepositoryUrl>https://github.com/June74/openmemory</RepositoryUrl>
  </PropertyGroup>

</Project>
```

- [ ] **Step 4: Write `Directory.Packages.props`**

Central package management. Package *versions* live here; project files reference packages without versions. The `ItemGroup` is deliberately empty — Task 5 fills it from the xUnit template's own output rather than from a guessed version.

```xml
<Project>

  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
  </ItemGroup>

</Project>
```

- [ ] **Step 5: Create the empty solution**

```bash
cd "C:/Users/2006i/projects/openmemory" && dotnet new sln --name OpenMemory
```

- [ ] **Step 6: Verify the pin is honoured**

```bash
cd "C:/Users/2006i/projects/openmemory" && dotnet --version && dotnet build OpenMemory.sln --nologo 2>&1 | tail -5
```

Expected: `10.0.400`, and a build that succeeds with zero projects.

If `dotnet --version` reports anything other than `10.0.400`, the pin is not working — stop and report rather than proceeding.

- [ ] **Step 7: Confirm no build output is staged**

```bash
cd "C:/Users/2006i/projects/openmemory" && git status --short
```

Expected: only the four new files. No `bin/` or `obj/`.

- [ ] **Step 8: Commit**

```bash
git add global.json Directory.Build.props Directory.Packages.props OpenMemory.sln
git commit -s -m "Pin .NET 10 SDK and add empty solution

Pins 10.0.400 with rollForward latestPatch so a newer SDK cannot
silently change the build. Shared MSBuild properties are inherited
from Directory.Build.props rather than restated per project."
```

---

### Task 2: Core library projects

**Files:**
- Create: `src/OpenMemory.Contracts/OpenMemory.Contracts.csproj`
- Create: `src/OpenMemory.Storage/OpenMemory.Storage.csproj`
- Create: `src/OpenMemory.Indexing/OpenMemory.Indexing.csproj`

**Interfaces:**
- Consumes: `OpenMemory.sln` and the inherited properties from Task 1.
- Produces: `OpenMemory.Contracts`, which every other project in the solution references. `Storage` and `Indexing` each reference `Contracts` and nothing else.

- [ ] **Step 1: Verify the projects are absent**

Run: `ls src/ 2>&1`

Expected: `No such file or directory`.

- [ ] **Step 2: Create the three library projects and add them to the solution**

```bash
cd "C:/Users/2006i/projects/openmemory"
for p in Contracts Storage Indexing; do
  dotnet new classlib -o "src/OpenMemory.$p" --name "OpenMemory.$p"
  rm -f "src/OpenMemory.$p/Class1.cs"
  dotnet sln OpenMemory.sln add "src/OpenMemory.$p/OpenMemory.$p.csproj"
done
```

The template's `Class1.cs` is removed deliberately. A library project with zero source files compiles successfully, and an empty placeholder class would be exactly the kind of scaffolding this wave is meant to avoid.

- [ ] **Step 3: Add the project references**

```bash
cd "C:/Users/2006i/projects/openmemory"
dotnet add src/OpenMemory.Storage/OpenMemory.Storage.csproj reference src/OpenMemory.Contracts/OpenMemory.Contracts.csproj
dotnet add src/OpenMemory.Indexing/OpenMemory.Indexing.csproj reference src/OpenMemory.Contracts/OpenMemory.Contracts.csproj
```

`Contracts` references nothing. It is the root of the dependency graph, and anything it referenced would become a transitive dependency of the entire solution.

- [ ] **Step 4: Verify the build and the reference direction**

```bash
cd "C:/Users/2006i/projects/openmemory" && dotnet build OpenMemory.sln --nologo -v quiet 2>&1 | tail -5
grep -c "ProjectReference" src/OpenMemory.Contracts/OpenMemory.Contracts.csproj || echo "Contracts has no references (correct)"
```

Expected: `Build succeeded`, `0 Warning(s)`, `0 Error(s)`, and `Contracts has no references (correct)`.

- [ ] **Step 5: Commit**

```bash
git add src/ OpenMemory.sln
git commit -s -m "Add contracts, storage, and indexing library projects

Contracts is the root of the dependency graph and references nothing.
Storage and Indexing reference Contracts only."
```

---

### Task 3: Adapter projects

**Files:**
- Create: `src/OpenMemory.Adapters.Abstractions/OpenMemory.Adapters.Abstractions.csproj`
- Create: `src/OpenMemory.Adapters.Codex/OpenMemory.Adapters.Codex.csproj`
- Create: `src/OpenMemory.Adapters.ClaudeCode/OpenMemory.Adapters.ClaudeCode.csproj`
- Create: `src/OpenMemory.Adapters.Antigravity/OpenMemory.Adapters.Antigravity.csproj`

**Interfaces:**
- Consumes: `OpenMemory.Contracts` from Task 2.
- Produces: the boundary Stage 3's three concurrent workers own separately. `Adapters.Abstractions` is the frozen contract, owned by the root integrator; the three client projects each reference it.

This structure exists because of a process constraint, not a technical one: [AGENTS.md](../../../AGENTS.md) forbids two active agents owning overlapping files, and Stage 3 runs three adapter workers concurrently. It also enforces the contract's dependency direction at compile time — `Abstractions` cannot reference a concrete adapter.

- [ ] **Step 1: Create the four projects and add them to the solution**

```bash
cd "C:/Users/2006i/projects/openmemory"
for p in Adapters.Abstractions Adapters.Codex Adapters.ClaudeCode Adapters.Antigravity; do
  dotnet new classlib -o "src/OpenMemory.$p" --name "OpenMemory.$p"
  rm -f "src/OpenMemory.$p/Class1.cs"
  dotnet sln OpenMemory.sln add "src/OpenMemory.$p/OpenMemory.$p.csproj"
done
```

- [ ] **Step 2: Add the references**

```bash
cd "C:/Users/2006i/projects/openmemory"
dotnet add src/OpenMemory.Adapters.Abstractions/OpenMemory.Adapters.Abstractions.csproj reference src/OpenMemory.Contracts/OpenMemory.Contracts.csproj
for p in Codex ClaudeCode Antigravity; do
  dotnet add "src/OpenMemory.Adapters.$p/OpenMemory.Adapters.$p.csproj" reference src/OpenMemory.Adapters.Abstractions/OpenMemory.Adapters.Abstractions.csproj
done
```

Each client adapter reaches `Contracts` transitively through `Abstractions`. Do not add a direct `Contracts` reference to the client adapters — the point is that they depend on the neutral abstraction, not on the contract directly.

- [ ] **Step 3: Verify the direction holds**

```bash
cd "C:/Users/2006i/projects/openmemory"
dotnet build OpenMemory.sln --nologo -v quiet 2>&1 | tail -4
grep -o 'Include="[^"]*"' src/OpenMemory.Adapters.Abstractions/OpenMemory.Adapters.Abstractions.csproj
```

Expected: build succeeds with 0 warnings and 0 errors, and `Abstractions` shows a reference to `Contracts` only — never to `Codex`, `ClaudeCode`, or `Antigravity`.

- [ ] **Step 4: Commit**

```bash
git add src/ OpenMemory.sln
git commit -s -m "Add adapter abstraction and per-client adapter projects

One project per Stage 3 worker so concurrent lanes own disjoint files,
per AGENTS.md. Abstractions references Contracts only, so the contract's
dependency direction is enforced by the compiler."
```

---

### Task 4: Executable projects

**Files:**
- Create: `src/OpenMemory.Service/` (project + `Program.cs`)
- Create: `src/OpenMemory.Cli/` (project + `Program.cs`)
- Create: `src/OpenMemory.McpBridge/` (project + `Program.cs`)

**Interfaces:**
- Consumes: `Contracts`, `Storage`, `Indexing`, `Adapters.Abstractions`.
- Produces: three executables. **Their reference sets are the architectural content of this task** and are deliberately unequal.

Each of these needs a `Program.cs` because an executable project with no entry point fails with CS5001, while a library with no files compiles fine. The stubs must contain nothing but an entry point — no service host, no command parsing, no MCP handling. Anything more begins Stage 2 or later work inside a Stage 0 wave.

- [ ] **Step 1: Create the three projects**

```bash
cd "C:/Users/2006i/projects/openmemory"
for p in Service Cli McpBridge; do
  dotnet new console -o "src/OpenMemory.$p" --name "OpenMemory.$p"
  dotnet sln OpenMemory.sln add "src/OpenMemory.$p/OpenMemory.$p.csproj"
done
```

- [ ] **Step 2: Replace each generated `Program.cs` with a headed stub**

`src/OpenMemory.Service/Program.cs`:

```csharp
// Copyright 2026 OpenMemory contributors
// SPDX-License-Identifier: Apache-2.0

namespace OpenMemory.Service;

/// <summary>
/// Entry point placeholder. The service host is implemented in a later stage;
/// this exists only because an executable project requires an entry point.
/// </summary>
internal static class Program
{
    private static void Main()
    {
    }
}
```

`src/OpenMemory.Cli/Program.cs` — identical but `namespace OpenMemory.Cli;` and the summary reading "The `openmemory` command-line interface is implemented in a later stage".

`src/OpenMemory.McpBridge/Program.cs` — identical but `namespace OpenMemory.McpBridge;` and the summary reading "The MCP stdio bridge is implemented in a later stage".

Write all three out in full; do not abbreviate one as "same as above".

- [ ] **Step 3: Add the references — note the deliberate asymmetry**

```bash
cd "C:/Users/2006i/projects/openmemory"
# Service composes the subsystems.
for r in Contracts Storage Indexing Adapters.Abstractions; do
  dotnet add src/OpenMemory.Service/OpenMemory.Service.csproj reference "src/OpenMemory.$r/OpenMemory.$r.csproj"
done
# Cli and McpBridge get Contracts ONLY.
dotnet add src/OpenMemory.Cli/OpenMemory.Cli.csproj reference src/OpenMemory.Contracts/OpenMemory.Contracts.csproj
dotnet add src/OpenMemory.McpBridge/OpenMemory.McpBridge.csproj reference src/OpenMemory.Contracts/OpenMemory.Contracts.csproj
```

**Do not give `McpBridge` or `Cli` a reference to `Storage`.** [ARCHITECTURE.md](../../ARCHITECTURE.md) requires that the bridge "contains no memory logic and must not become a second source of truth." Denying it a compile-time path to `Storage` makes that structural instead of a rule someone has to remember. The CLI is restricted for the same reason: it talks to the service, it does not open the database.

- [ ] **Step 4: Verify the build and the restriction**

```bash
cd "C:/Users/2006i/projects/openmemory"
dotnet build OpenMemory.sln --nologo -v quiet 2>&1 | tail -4
echo "--- McpBridge references (expect Contracts only) ---"
grep -o 'OpenMemory\.[A-Za-z.]*\.csproj' src/OpenMemory.McpBridge/OpenMemory.McpBridge.csproj
echo "--- Cli references (expect Contracts only) ---"
grep -o 'OpenMemory\.[A-Za-z.]*\.csproj' src/OpenMemory.Cli/OpenMemory.Cli.csproj
```

Expected: build succeeds, 0 warnings, 0 errors. Both listings show `OpenMemory.Contracts.csproj` and nothing else.

- [ ] **Step 5: Commit**

```bash
git add src/ OpenMemory.sln
git commit -s -m "Add service, CLI, and MCP bridge executable projects

Entry-point stubs only; no host, command parsing, or MCP handling.
McpBridge and Cli reference Contracts alone, so neither has a
compile-time path to Storage, per ARCHITECTURE.md."
```

---

### Task 5: Test projects and the contract-version test

**Files:**
- Create: `tests/OpenMemory.Contracts.Tests/` (project + `ContractVersionsTests.cs`)
- Create: `tests/OpenMemory.Storage.Tests/`, `tests/OpenMemory.Service.Tests/`
- Create: `src/OpenMemory.Contracts/ContractVersions.cs`
- Modify: `Directory.Packages.props`

**Interfaces:**
- Consumes: `Contracts` from Task 2.
- Produces: `OpenMemory.Contracts.ContractVersions` — public constants naming each contract integer. Later stages read these rather than restating numbers.

This is the only code in the wave with product meaning. It is included because `COMPATIBILITY.md` already fixes these values as approved decisions, so encoding them invents nothing, and because it gives the test harness something real to verify.

- [ ] **Step 1: Write the failing test first — create the test project**

```bash
cd "C:/Users/2006i/projects/openmemory"
dotnet new xunit -o tests/OpenMemory.Contracts.Tests --name OpenMemory.Contracts.Tests
rm -f tests/OpenMemory.Contracts.Tests/UnitTest1.cs
dotnet sln OpenMemory.sln add tests/OpenMemory.Contracts.Tests/OpenMemory.Contracts.Tests.csproj
dotnet add tests/OpenMemory.Contracts.Tests/OpenMemory.Contracts.Tests.csproj reference src/OpenMemory.Contracts/OpenMemory.Contracts.csproj
```

**Expect a restore failure here, and do not try to fix it in this step.** The template writes `PackageReference` entries carrying `Version` attributes, which central package management rejects — usually NuGet error NU1008 ("Projects that use central package version management should not define the version on the PackageReference items"). That is the exact condition Step 2 resolves. Proceed to Step 2 rather than deleting `Directory.Packages.props` or re-adding versions.

Note also that the .NET 10 template may generate xUnit v3 (`xunit.v3`) rather than the older `xunit` package. Either is fine — record whichever package names and versions the template actually produced and centralize those.

- [ ] **Step 2: Centralize the package versions**

The template wrote `PackageReference` entries **with** versions, which conflicts with central package management and will fail the build. Move them:

1. Read the versions the template generated in `tests/OpenMemory.Contracts.Tests/OpenMemory.Contracts.Tests.csproj`.
2. For each, add `<PackageVersion Include="<name>" Version="<version>" />` to the `ItemGroup` in `Directory.Packages.props`, using the exact versions the template produced. **Do not invent version numbers.**
3. In the test `.csproj`, strip the `Version` attribute from each `PackageReference`, leaving `<PackageReference Include="<name>" />`.

- [ ] **Step 3: Write the test**

`tests/OpenMemory.Contracts.Tests/ContractVersionsTests.cs`:

```csharp
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
```

- [ ] **Step 4: Run the test to verify it fails**

Run: `cd "C:/Users/2006i/projects/openmemory" && dotnet test tests/OpenMemory.Contracts.Tests --nologo 2>&1 | tail -10`

Expected: FAIL — compile error, because `ContractVersions` does not exist yet.

- [ ] **Step 5: Write the minimal implementation**

`src/OpenMemory.Contracts/ContractVersions.cs`:

```csharp
// Copyright 2026 OpenMemory contributors
// SPDX-License-Identifier: Apache-2.0

namespace OpenMemory.Contracts;

/// <summary>
/// Version integers for OpenMemory's wire and storage contracts, as fixed by
/// docs/COMPATIBILITY.md. Each is an independent integer rather than part of
/// the product's SemVer, per decision D-090. All remain unfrozen until Stage 2.
/// </summary>
public static class ContractVersions
{
    /// <summary>MCP protocol version, negotiated per connection.</summary>
    public const int McpProtocol = 1;

    /// <summary>Named-pipe framing and capability envelope version.</summary>
    public const int PipeEnvelope = 1;

    /// <summary>Database schema migration number. Forward-only.</summary>
    public const int DatabaseSchema = 1;

    /// <summary>Normalized, client-neutral event envelope version.</summary>
    public const int EventEnvelope = 1;

    /// <summary>Markdown projection protocol version.</summary>
    public const int ProjectionProtocol = 1;

    /// <summary>Portable export format version. Support is permanent.</summary>
    public const int PortableExportFormat = 1;
}
```

- [ ] **Step 6: Run the test to verify it passes**

Run: `cd "C:/Users/2006i/projects/openmemory" && dotnet test tests/OpenMemory.Contracts.Tests --nologo 2>&1 | tail -6`

Expected: `Passed!` with 1 test passing.

If it fails because the regex matched no rows, read `docs/COMPATIBILITY.md`'s §1 table and report the actual row format rather than loosening the regex until it passes — a test that matches nothing and asserts nothing is worse than a failing one.

- [ ] **Step 7: Create the two remaining test projects**

```bash
cd "C:/Users/2006i/projects/openmemory"
for p in Storage Service; do
  dotnet new xunit -o "tests/OpenMemory.$p.Tests" --name "OpenMemory.$p.Tests"
  rm -f "tests/OpenMemory.$p.Tests/UnitTest1.cs"
  dotnet sln OpenMemory.sln add "tests/OpenMemory.$p.Tests/OpenMemory.$p.Tests.csproj"
  dotnet add "tests/OpenMemory.$p.Tests/OpenMemory.$p.Tests.csproj" reference "src/OpenMemory.$p/OpenMemory.$p.csproj"
done
```

Strip the `Version` attributes from these two `.csproj` files as in Step 2. They need no new `PackageVersion` entries — the versions are already central.

These two projects contain no tests yet. That is intentional: they establish the `tests/<ProjectName>.Tests/` convention and prove multi-project test discovery works, and Stage 2 fills them.

- [ ] **Step 8: Run the whole suite**

```bash
cd "C:/Users/2006i/projects/openmemory" && dotnet test OpenMemory.sln --nologo 2>&1 | tail -8
```

Expected: all three test projects discovered, 1 test total, passing, 0 warnings.

- [ ] **Step 9: Commit**

```bash
git add src/ tests/ OpenMemory.sln Directory.Packages.props
git commit -s -m "Add test projects and contract version constants

ContractVersions encodes the integers COMPATIBILITY.md already fixes.
The test parses that document and asserts the declared constants match
it, so code and document cannot drift apart silently."
```

---

### Task 6: Obsidian plugin scaffold

**Files:**
- Create: `src/OpenMemory.ObsidianPlugin/package.json`, `tsconfig.json`, `manifest.json`, `src/main.ts`, `.gitignore`

**Interfaces:**
- Consumes: nothing from the .NET side.
- Produces: the TypeScript toolchain boundary and its Node/pnpm pins. Deliberately **not** added to `OpenMemory.sln` — it is not an MSBuild project.

- [ ] **Step 1: Verify absence**

Run: `test -d src/OpenMemory.ObsidianPlugin && echo "exists" || echo "absent"`

Expected: `absent`.

- [ ] **Step 2: Write `package.json`**

```json
{
  "name": "openmemory-obsidian-plugin",
  "version": "0.1.0",
  "description": "Obsidian interface for OpenMemory. Not yet implemented.",
  "license": "Apache-2.0",
  "private": true,
  "type": "module",
  "scripts": {
    "typecheck": "tsc --noEmit"
  },
  "engines": {
    "node": ">=24.0.0",
    "pnpm": ">=11.0.0"
  },
  "packageManager": "pnpm@11.9.0",
  "devDependencies": {
    "typescript": "^5.7.0"
  }
}
```

- [ ] **Step 3: Write `manifest.json`**

Obsidian requires this file, and it is why the plugin uses SemVer rather than a contract integer — see `COMPATIBILITY.md` and `D-090`.

```json
{
  "id": "openmemory",
  "name": "OpenMemory",
  "version": "0.1.0",
  "minAppVersion": "1.5.0",
  "description": "Local, private, temporal memory for terminal AI clients. Not yet implemented.",
  "author": "OpenMemory contributors",
  "isDesktopOnly": true
}
```

- [ ] **Step 4: Write `tsconfig.json`**

```json
{
  "compilerOptions": {
    "target": "ES2022",
    "module": "ESNext",
    "moduleResolution": "bundler",
    "strict": true,
    "noEmit": true,
    "skipLibCheck": true,
    "isolatedModules": true,
    "forceConsistentCasingInFileNames": true
  },
  "include": ["src/**/*.ts"]
}
```

- [ ] **Step 5: Write the stub entry file**

`src/OpenMemory.ObsidianPlugin/src/main.ts`:

```typescript
// Copyright 2026 OpenMemory contributors
// SPDX-License-Identifier: Apache-2.0

// Placeholder. The Obsidian plugin is implemented in Stage 6; this file exists
// so the TypeScript toolchain boundary is real and type-checks in CI.

export const PLUGIN_ID = "openmemory";
```

- [ ] **Step 6: Write the plugin `.gitignore`**

```
node_modules/
main.js
*.tsbuildinfo
```

- [ ] **Step 7: Install and typecheck**

```bash
cd "C:/Users/2006i/projects/openmemory/src/OpenMemory.ObsidianPlugin" && pnpm install && pnpm run typecheck
```

Expected: install succeeds, `pnpm-lock.yaml` is created, and `tsc --noEmit` produces no output and exits 0.

- [ ] **Step 8: Confirm `node_modules` is not staged**

```bash
cd "C:/Users/2006i/projects/openmemory" && git status --short | head -20
```

Expected: the new plugin files and `pnpm-lock.yaml`. **No `node_modules/`.** The lockfile IS committed — Wave C's CI runs `pnpm install --frozen-lockfile`, which requires it.

- [ ] **Step 9: Commit**

```bash
git add src/OpenMemory.ObsidianPlugin/
git commit -s -m "Add Obsidian plugin TypeScript scaffold

Pins Node 24 and pnpm 11 via engines and packageManager. Stub module
only; the plugin is implemented in Stage 6. Not part of the .NET
solution, since it is not an MSBuild project."
```

---

### Task 7: Installer reservation and header audit

**Files:**
- Create: `src/OpenMemory.Installer/README.md`

**Interfaces:**
- Consumes: everything from Tasks 1–6.
- Produces: nothing later tasks depend on.

- [ ] **Step 1: Write the installer reservation**

`src/OpenMemory.Installer/README.md`:

```markdown
# OpenMemory installer — reserved

> **Status:** reserved directory. No project file exists here yet, and that is
> deliberate.

## Why there is no project here

A project file's `Sdk` attribute *is* the choice of build tool. A C# project
opens with `<Project Sdk="Microsoft.NET.Sdk">`; a WiX installer project is a
`.wixproj` opening with `<Project Sdk="WixToolset.Sdk">`. The file cannot be
written without first choosing the toolset.

That choice is deferred. `F-001` in the [decision register](../../docs/DECISION_REGISTER.md)
reserves "exact MSI authoring tool, bootstrapper, and upgrade implementation"
pending clean Windows install, update, and uninstall proof in Stage 1. The
register states that deferred items must not be silently chosen in unrelated
work, and scaffolding a project here would choose one.

The approved distribution format is not in question: `D-017` fixes a per-user
MSI installer. Only the authoring tool is open.

## What closes this

Stage 1's Windows packaging proof. When `F-001` is decided, the project is
created here with the chosen toolset and added to the build.
```

- [ ] **Step 2: Audit that every source file carries a licence header**

```bash
cd "C:/Users/2006i/projects/openmemory"
missing=0
for f in $(git ls-files '*.cs' '*.ts'); do
  head -2 "$f" | grep -q 'SPDX-License-Identifier: Apache-2.0' || { echo "MISSING HEADER: $f"; missing=1; }
done
[ $missing -eq 0 ] && echo "PASS: all source files carry the SPDX header"
```

Expected: `PASS`. If any file is listed, add the header to it and re-run.

- [ ] **Step 3: Verify links resolve**

```bash
cd "C:/Users/2006i/projects/openmemory" && git add src/OpenMemory.Installer/ && bash tools/check-links.sh; echo "exit=$?"
```

Expected: `exit=0`. The README links to the decision register with a relative path — if it does not resolve, fix the path rather than removing the link.

- [ ] **Step 4: Commit**

```bash
git add src/OpenMemory.Installer/README.md
git commit -s -m "Reserve the installer directory pending F-001

The project file's Sdk attribute is itself the choice of MSI tool, which
F-001 defers to Stage 1 proof. Scaffolding a project here would decide it."
```

---

### Task 8: Integration verification and independent review

**Files:** none created or modified except a possible setback record.

**Interfaces:**
- Consumes: everything from Tasks 1–7.
- Produces: the evidence recorded in the pull-request body.

- [ ] **Step 1: Clean build from scratch**

```bash
cd "C:/Users/2006i/projects/openmemory"
rm -rf src/*/bin src/*/obj tests/*/bin tests/*/obj
dotnet restore OpenMemory.sln --nologo 2>&1 | tail -3
dotnet build OpenMemory.sln --nologo 2>&1 | tail -5
```

Expected: `Build succeeded`, `0 Warning(s)`, `0 Error(s)`. This is the Stage 0 exit gate's "clean Windows build."

- [ ] **Step 2: Full test suite**

```bash
cd "C:/Users/2006i/projects/openmemory" && dotnet test OpenMemory.sln --nologo 2>&1 | tail -8
```

Expected: 3 test projects, 1 test, passing, no warnings.

- [ ] **Step 3: Verify the dependency restrictions still hold**

```bash
cd "C:/Users/2006i/projects/openmemory"
for p in McpBridge Cli; do
  echo "--- $p ---"
  grep -o 'OpenMemory\.[A-Za-z.]*\.csproj' "src/OpenMemory.$p/OpenMemory.$p.csproj"
done
grep -l "Storage" src/OpenMemory.McpBridge/*.csproj src/OpenMemory.Cli/*.csproj && echo "FAIL: forbidden Storage reference" || echo "PASS: no Storage reference"
```

Expected: each shows `OpenMemory.Contracts.csproj` only, and `PASS: no Storage reference`.

- [ ] **Step 4: Confirm no build output or dependencies were committed**

```bash
cd "C:/Users/2006i/projects/openmemory"
git ls-files | grep -E '(^|/)(bin|obj|node_modules)/' && echo "FAIL: build output committed" || echo "PASS: no build output committed"
git status --short
```

Expected: `PASS`, and a clean working tree.

- [ ] **Step 5: DCO and secret scan**

```bash
cd "C:/Users/2006i/projects/openmemory"
for sha in $(git log main..HEAD --format=%H); do
  git log -1 --format=%B "$sha" | grep -qE '^Signed-off-by:' || echo "MISSING SIGN-OFF: $sha"
done; echo "sign-off check complete"
git diff main...HEAD | grep -inE '(sk-[A-Za-z0-9]{8,}|ghp_[A-Za-z0-9]{8,}|-----BEGIN [A-Z ]*PRIVATE KEY-----)' && echo "FAIL" || echo "PASS: no secret-shaped content"
```

- [ ] **Step 6: Links**

```bash
cd "C:/Users/2006i/projects/openmemory" && bash tools/check-links.sh; echo "exit=$?"
```

Expected: `exit=0`.

- [ ] **Step 7: Independent review by Codex**

```bash
cd "C:/Users/2006i/projects/openmemory"
git diff main...HEAD > "$TMPDIR/wave-b.diff"
codex exec "Review this diff as an independent specification and quality reviewer for the OpenMemory project. Read AGENTS.md, docs/IMPLEMENTATION_PLAN.md, and docs/superpowers/specs/2026-08-16-stage0-wave-b-design.md first. Verify: (1) the diff implements the Wave B spec with no gap and no scope creep; (2) no product behavior was added beyond entry-point stubs and the documented contract version constants; (3) the project dependency graph matches the spec, especially that McpBridge and Cli reference Contracts only and cannot reach Storage; (4) no build output, node_modules, or secret value is committed; (5) the toolchain pins actually pin. Report findings by severity with concrete evidence. Do not modify files." < "$TMPDIR/wave-b.diff"
```

Record findings verbatim. Per `receiving-code-review`, verify each technically rather than accepting it performatively.

- [ ] **Step 8: Record any unexpected failure**

If any step failed unexpectedly, create a setback record in `docs/operations/setbacks/` following the existing five, add it to `INDEX.md`, and commit.

---

## Wave B completion criteria

1. Tasks 1–8 complete.
2. `dotnet build OpenMemory.sln` succeeds from clean with 0 warnings and 0 errors.
3. `dotnet test OpenMemory.sln` passes.
4. `pnpm run typecheck` passes in the plugin directory.
5. `McpBridge` and `Cli` reference `Contracts` only.
6. No `bin/`, `obj/`, or `node_modules/` committed.
7. Every commit DCO-signed; no secret-shaped content.
8. Codex independent review findings resolved with evidence.
9. No product behavior beyond entry-point stubs and `ContractVersions`.
