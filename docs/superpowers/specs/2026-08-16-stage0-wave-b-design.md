# Stage 0 Wave B — Toolchain and solution boundaries (design)

- **Date:** 2026-08-16
- **Stage:** 0 (Program foundation)
- **Wave:** B of four
- **Owner:** Root integrator
- **Branch:** `codex/stage0-wave-b`
- **Status:** Awaiting user review

## 1. Why this wave exists

Wave A established governance and records. Wave B builds the structure every later stage compiles inside, covering one Stage 0 bullet:

> "install and pin the .NET 10 SDK and Node/TypeScript toolchain without adding product features; create solution boundaries for the service, contracts, MCP bridge, adapters, storage, indexing, Obsidian plugin, installer, and tests"

Waves C (CI) and D (security contracts and threat model) both depend on this wave, because there is nothing to build, test, or threat-model until the structure exists.

**Scope discipline.** This wave creates *boundaries*, not behavior. No memory logic, no database access, no MCP handling, no adapter implementation. The Stage 0 exit gate requires that "no product capability is claimed," and that constraint still binds.

## 2. Current state

**Toolchain, verified 2026-08-16 rather than assumed:**

| Tool | Version | Evidence |
|---|---|---|
| .NET SDK | 10.0.400 | `dotnet --list-sdks`, plus a scaffolded console project that compiled and ran |
| Node | 24.13.0 | `node --version` |
| pnpm | 11.9.0 | `pnpm --version` |
| GitHub CLI | 2.95.0 | `gh --version` |
| Codex CLI | 0.147.0 | `codex --version` |

The SDK was absent at the start of Stage 0 — `C:\Program Files\dotnet` held the runtime host with no `sdk\` directory — and was installed during this wave's preparation via `winget install Microsoft.DotNet.SDK.10`.

**Repository:** `main` at `fdd2162`, Wave A merged. No source code exists yet.

## 3. Decisions taken during brainstorming

| Ref | Decision | Rationale |
|---|---|---|
| B-1 | `OpenMemory.sln` at the repository root, C# projects under `src/`, test projects under `tests/`, TypeScript plugin as `src/OpenMemory.ObsidianPlugin/`. | Conventional .NET layout. One tree rather than two toolchain silos, keeping `CODEOWNERS` paths and CI globs simple. |
| B-2 | Adapters split into a shared abstractions project plus one project per client. | Stage 3 runs three concurrent adapter workers, and [AGENTS.md](../../../AGENTS.md) forbids two active agents owning overlapping files. Separate projects also enforce the contract's dependency direction at compile time: `Adapters.Abstractions` cannot reference a concrete adapter. |
| B-3 | The `openmemory` CLI is its own project, separate from the service. | A .NET project produces one assembly with at most one entry point; two `Main` methods in one project is compiler error CS0017, verified directly. The service and CLI are two programs with different lifetimes — the service is registered to start at login under `D-017`, while the CLI is invoked interactively. A single multi-command executable would be technically possible; two projects is a deliberate design choice, not a compiler requirement. |
| B-4 | `src/OpenMemory.Installer/` is reserved as a directory with a README, with no project file. | The project file's `Sdk` attribute *is* the choice of MSI tool — WiX uses `.wixproj` with `WixToolset.Sdk`, not `.csproj`. `F-001` defers that tool pending Stage 1 proof, and the register states deferred items must not be silently chosen in unrelated work. Creating any installer project now would close `F-001` by scaffolding. |
| B-5 | Test projects are created per major subsystem, not one per production project, with `tests/<ProjectName>.Tests/` fixed as the naming convention. | The disjoint-ownership argument that justifies B-2 does not transfer to tests: SDK-style projects glob source files, so concurrent workers adding separate test files to one project touch no shared file. Test projects are also leaves that nothing references, so they gain none of B-2's compile-time contract enforcement. |
| B-6 | xUnit as the test framework. | Dominant in .NET open source with the strongest tooling support. No project constraint favours an alternative. |

## 4. Deliverables

### 4.1 Solution and project layout

```
OpenMemory.sln
global.json
Directory.Build.props
Directory.Packages.props
src/
  OpenMemory.Contracts/              Library
  OpenMemory.Storage/                Library
  OpenMemory.Indexing/               Library
  OpenMemory.Adapters.Abstractions/  Library
  OpenMemory.Adapters.Codex/         Library
  OpenMemory.Adapters.ClaudeCode/    Library
  OpenMemory.Adapters.Antigravity/   Library
  OpenMemory.Service/                Exe
  OpenMemory.Cli/                    Exe
  OpenMemory.McpBridge/              Exe
  OpenMemory.ObsidianPlugin/         TypeScript, own package.json, not in the .sln
  OpenMemory.Installer/              reserved directory + README, no project (B-4)
tests/
  OpenMemory.Contracts.Tests/
  OpenMemory.Storage.Tests/
  OpenMemory.Service.Tests/
```

**A constraint that shapes the work:** a library project with no source files compiles successfully, but an executable project with no entry point fails with CS5001. The three `Exe` projects therefore need a minimal `Program.cs` each. These stubs must do nothing but exist — no service host, no command parsing, no MCP handling. Writing anything more would begin Stage 2 and later work inside a Stage 0 wave.

**Project references** establish the dependency direction, which is the real architectural content of this wave:

- `Adapters.Codex`, `.ClaudeCode`, `.Antigravity` → `Adapters.Abstractions` → `Contracts`
- `Storage` → `Contracts`; `Indexing` → `Contracts`
- `Service` → `Contracts`, `Storage`, `Indexing`, `Adapters.Abstractions`
- `McpBridge` → `Contracts` **only** — [ARCHITECTURE.md](../../ARCHITECTURE.md) requires the bridge to hold no memory logic and never become a second source of truth. Denying it a reference to `Storage` enforces that with the compiler rather than with review discipline.
- `Cli` → `Contracts` only, for the same reason: the CLI talks to the service, it does not open the database.

### 4.2 Toolchain pinning

**`global.json`** pins the SDK:

```json
{ "sdk": { "version": "10.0.400", "rollForward": "latestPatch" } }
```

`latestPatch` accepts 10.0.4xx patch updates but refuses to build on .NET 11, so a contributor with a newer SDK cannot silently produce a different build.

**`Directory.Build.props`** — settings inherited by every project, so twelve project files do not each restate them: `net10.0`, nullable reference types enabled, warnings as errors, deterministic builds, and the shared assembly metadata.

**`Directory.Packages.props`** — central package management. NuGet versions live in one file rather than scattered across projects, which matters for the dependency review Wave C adds.

**Node and pnpm** are pinned in the plugin's `package.json` via `engines` and the `packageManager` field, matching Node 24 and pnpm 11.

### 4.3 Apache-2.0 file headers

Wave A defined this policy but deferred application, correctly, because no source files existed. This wave adds the first ones. Every `.cs` and `.ts` file carries the standard Apache-2.0 header. `LICENSE` and `NOTICE` remain authoritative; the header points at them rather than restating terms.

### 4.4 Tests

Three test projects, each with at least one test that asserts something real. A test project containing no tests would let `dotnet test` report success while proving nothing about the harness.

`OpenMemory.Contracts.Tests` asserts that the contract version constants in `Contracts` equal the values `COMPATIBILITY.md` documents — all `1` at this stage. This ties code to the approved document, so a later change to either that is not mirrored in the other fails the build.

**A deliberate scope call:** defining those version constants is the only code in this wave with product meaning. It is included because `COMPATIBILITY.md` already fixes the values as approved decisions, so encoding them invents nothing, and because it gives the test harness something genuine to verify rather than a placeholder assertion. Freezing the wider contract surface remains Stage 2's work.

## 5. Verification

The Stage 0 exit gate requires a clean Windows build. Each check is run before its change to observe it fail first.

| Check | Command | Fails before because |
|---|---|---|
| Solution restores | `dotnet restore` | No solution exists |
| Solution builds clean | `dotnet build --nologo -warnaserror` | No projects exist |
| Tests run and pass | `dotnet test --nologo` | No test projects exist |
| SDK pin honoured | `dotnet --version` inside the repo returns `10.0.400` | No `global.json` exists |
| Plugin toolchain resolves | `pnpm install --frozen-lockfile` in the plugin directory | No `package.json` exists |
| Dependency direction holds | A deliberate reference from `McpBridge` to `Storage` must fail review | — |
| Links | `bash tools/check-links.sh` | New READMEs are linked before they exist |
| Independent review | `codex exec` over the branch diff | — |

Build artifacts must not be committed: `bin/` and `obj/` are already in `.gitignore`.

## 6. Out of scope

- Any behavior in any project beyond entry-point stubs and version constants.
- The installer project (B-4, `F-001`).
- CI workflows — Wave C.
- Enabling branch protection — Wave C, `D-091`.
- Security contracts and the threat model — Wave D, `F-010`.
- Database schema, migrations, or vector representation — `F-002`, Stage 2.

## 7. Risks

| Risk | Mitigation |
|---|---|
| Empty projects are scaffolding that later stages must rework. | The dependency graph, not the file count, is the deliverable. Reference direction is the thing Stage 2 onward relies on and the thing that is expensive to change later. |
| Encoding version constants edges toward Stage 2's contract freeze. | Limited strictly to integers already fixed in `COMPATIBILITY.md`. No envelope shape, schema, or message type is defined. |
| The three `Exe` stubs invite premature implementation. | The spec states they must contain nothing but a minimal entry point, and review checks for exactly that. |
| Test projects created now may be split later. | B-5 fixes the naming convention, making a later split mechanical rather than a redesign. |
