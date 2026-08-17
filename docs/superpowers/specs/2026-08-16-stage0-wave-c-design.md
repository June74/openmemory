# Stage 0 Wave C — Continuous integration and branch protection (design)

- **Date:** 2026-08-16
- **Stage:** 0 (Program foundation)
- **Wave:** C of four
- **Owner:** Root integrator
- **Branch:** `codex/stage0-wave-c`
- **Status:** Awaiting user review

## 1. Why this wave exists

Wave A established governance and records. Wave B created the toolchain pins and the solution's dependency graph. Wave C makes both enforceable, covering one Stage 0 bullet:

> "establish CI for formatting, build, unit tests, dependency review, secret scanning, SBOM generation, and checksummed development artifacts"

It also discharges two debts Wave A and Wave B deliberately left:

- [`.github/branch-protection.md`](../../../.github/branch-protection.md) records a ruleset and states that its "Required checks" section is "to be filled in by Wave C with the exact job names from the CI workflow. Wave C is not complete until this section names real, passing checks."
- `D-091` records that branch protection is documented in Wave A but enabled in Wave C.

Wave D (security contracts and the repository-grounded threat model) remains last, because `F-010` requires the threat model to follow the executable trust boundaries.

**Scope discipline.** This wave adds automation, not product behavior. No `.cs` or `.ts` file gains logic. The Stage 0 exit gate's "no product capability is claimed" still binds.

## 2. Current state

Verified on 2026-08-16 against `main` at `23d4b20`, with Wave B merged.

**Buildable:** `dotnet build OpenMemory.sln` succeeds with 0 warnings and 0 errors; `dotnet test` passes 4 tests across 3 projects; `pnpm run typecheck` is clean.

**Repository security settings, read from the API rather than assumed:**

| Setting | State |
|---|---|
| Secret scanning | Enabled (automatic for public repositories) |
| Push protection | Enabled |
| Non-provider patterns | **Disabled** |
| Validity checks | **Disabled** |
| Dependabot security updates | Disabled |
| Open secret-scanning alerts | 0 |

**Missing:** `.github/workflows/` does not exist. `main` has no branch protection. No SBOM or artifact has ever been produced.

## 3. Decisions taken during brainstorming

| Ref | Decision | Rationale |
|---|---|---|
| C-1 | CI produces a checksummed development artifact: publish `Service`, `Cli`, and `McpBridge`, zip, emit SHA-256, upload with the SBOM. | Stage 8's release depends on this path. Exercising it now, while the payload is three empty `Main` methods, is far cheaper than debugging it when the payload is a real signed installer. Nothing can leak, because nothing is implemented. |
| C-2 | Add a CI secret-scanning job **in addition to** the already-enabled native scanning, and enable non-provider patterns. | GitHub's native secret scanning is not a status check — branch protection cannot require it. Stage 0 asks for secret scanning *in CI*, and `D-091` needs a real check to gate on. Non-provider patterns matter because this project's threat model covers private keys and connection strings in captured transcripts, not only vendor-format tokens. |
| C-3 | Enable validity checks. | A secret leaked into a public repository is already exposed, so testing whether it is still live adds negligible risk and real remediation value: it distinguishes "rotate now" from "already dead". |
| C-4 | .NET jobs run on `windows-latest`; jobs that do not touch the build run on `ubuntu-latest`. | `D-003` fixes Windows 11 x64 as the only supported platform and the Stage 0 gate requires a "clean Windows build". The repository is public, so Actions minutes are free and runner cost is not a factor. |
| C-5 | The test job asserts a minimum discovered-test **count**, not merely a zero exit code. | `dotnet test` exits 0 both when a project has no tests and when test discovery is broken. Exit code alone cannot distinguish them. This was observed directly during Wave B and is the failure mode that would let a Stage 2 misconfiguration report green while nothing ran. |
| C-6 | Formatting is enforced for C# only; no TypeScript formatter is added. | `dotnet format` needs no new dependency. Adding Prettier would introduce one for a single 7-line stub file. TypeScript formatting belongs to Stage 6, when the plugin is real. |
| C-7 | One SBOM tool covering both ecosystems rather than one per ecosystem. | The repository has a .NET and an npm dependency tree. A single generator produces one reconciled document instead of two that must be merged. |

## 4. Deliverables

### 4.1 `.github/workflows/ci.yml`

One workflow, triggered on pull requests targeting `main` and on pushes to `main`. Six jobs, each independently required-able.

| Job | Runner | Content |
|---|---|---|
| `build-and-test` | `windows-latest` | `dotnet format --verify-no-changes`, `dotnet restore`, `dotnet build`, `dotnet test`, plus the discovered-test-count assertion (C-5) |
| `plugin` | `ubuntu-latest` | `pnpm install --frozen-lockfile`, `pnpm run typecheck` |
| `secret-scan` | `ubuntu-latest` | gitleaks over the repository |
| `dependency-review` | `ubuntu-latest` | `actions/dependency-review-action`; pull-request events only |
| `docs` | `ubuntu-latest` | `bash tools/check-links.sh` |
| `artifact` | `windows-latest` | `dotnet publish` of the three executables, zip, SHA-256, SBOM, upload |

**On `--frozen-lockfile`:** this is why Wave B committed `pnpm-lock.yaml`. The flag makes the install fail rather than silently resolving different versions when the lockfile and `package.json` disagree, which is what makes the plugin build reproducible.

**On the test-count assertion:** the job uses `dotnet test --list-tests` and counts discovered tests, failing below an expected floor. Counting discovered tests rather than parsing pass/fail summary text distinguishes the two cases C-5 names: zero discovered tests fails, whereas a passing run of a smaller-than-expected set also fails.

**On `dotnet format`:** `Directory.Build.props` deliberately does **not** set `EnforceCodeStyleInBuild`, so style is not enforced during compilation. Running `dotnet format --verify-no-changes` as a separate CI step keeps style failures distinguishable from correctness failures, which was the reason that property was removed in Wave B.

### 4.2 Checksummed development artifact

The `artifact` job publishes `OpenMemory.Service`, `OpenMemory.Cli`, and `OpenMemory.McpBridge`, bundles them into one archive, writes a SHA-256 file beside it, and uploads both together with the SBOM.

The archive is **integrity evidence only**. `D-018` states that automatic installation requires "a signature or signed attestation anchored to a pinned trusted project identity; a checksum is integrity evidence only", and `D-071` requires publisher authentication before any automatic update. A checksum published on the same channel as the file it describes proves the bytes were not corrupted; it proves nothing about who produced them. The artifact is therefore explicitly a development artifact, unsigned and manual-install only, exactly as `D-018` describes for this stage.

### 4.3 SBOM

A software bill of materials — the inventory of every dependency the build pulls in. One generator covers both the .NET and npm trees (C-7). It is uploaded with the artifact so the two travel together, which is the arrangement Stage 8's release evidence requires.

### 4.4 Repository settings

Three changes outside the repository tree, each requiring explicit approval before being applied:

1. Enable **non-provider patterns** for secret scanning (C-2).
2. Enable **validity checks** (C-3).
3. Enable **branch protection** on `main` with the ruleset already recorded in `.github/branch-protection.md`.

These are API calls, not file edits. They are listed here so the wave's full footprint is visible in one place rather than discovered during implementation.

### 4.5 `.github/branch-protection.md`

The "Required checks" section is replaced with the actual job names from §4.1, and the status blockquote changes from "not yet enabled" to enabled, with the date. Nothing else in the document changes — the ruleset table was settled in Wave A.

## 5. Verification

Each check runs before its change to observe it fail first, where that is possible.

| Check | How | Fails before because |
|---|---|---|
| Workflow is valid YAML and GitHub accepts it | Push the branch; `gh run list` shows a run | No workflow exists |
| Every job passes | `gh run watch` / `gh run view` on the branch's run | — |
| The test-count assertion can fail | Temporarily lower the discovered set or raise the floor, observe failure, revert | — |
| Artifact and SBOM exist and are downloadable | `gh run download` the completed run | No artifact has ever been produced |
| SHA-256 matches the archive | Recompute locally against the downloaded file and compare | — |
| Branch protection is active with the right checks | `gh api repos/June74/openmemory/branches/main/protection` returns the ruleset | Currently returns 404 "Branch not protected" |
| Links | `bash tools/check-links.sh` | New documents are linked before they exist |
| Independent review | `codex exec` over the branch diff | — |

**A constraint specific to this wave:** a workflow cannot be verified locally. Its first real execution happens on the pushed branch, so unlike Waves A and B, this wave's acceptance evidence requires the push. The wave is not complete on local checks alone.

## 6. Out of scope

- Any product behavior. No `.cs` or `.ts` file gains logic.
- A TypeScript formatter (C-6, Stage 6).
- Dependabot version-update configuration. `dependency-review-action` covers pull-request-time review, which is what Stage 0 asks for; scheduled dependency bumps are a separate operational decision.
- Code signing and attestation. `D-018` and `F-007` place these at Stage 8, with explicit approval required before any paid service.
- The threat model and the four frozen security contracts — Wave D, `F-010`.
- Release publishing. This wave uploads workflow artifacts; it does not create GitHub Releases or tags.

## 7. Risks

| Risk | Mitigation |
|---|---|
| Enabling branch protection with administrator enforcement means a broken CI blocks merging the fix for that CI. | This is the intended tradeoff — a rule the maintainer can silently bypass is not a control. Recorded here so the consequence is accepted deliberately rather than discovered under pressure. Recovery is to temporarily disable enforcement, which is visible in the audit log. |
| Required checks are named as strings; renaming a job later silently un-enforces it, because a required check that never reports simply never blocks. | `.github/branch-protection.md` records the exact names, and §4.5 makes updating it part of any job rename. |
| A third-party action is a supply-chain dependency in the security-critical path. | Actions are pinned and the set is kept minimal. This is a genuine residual risk, not one this wave eliminates, and belongs in Wave D's threat model. |
| `dotnet format --verify-no-changes` may fail on Wave B's existing files, which were never format-checked. | Expected. If it fails, the correct response is one formatting commit, not relaxing the check. |
| The artifact is downloadable from a public repository. | The binaries are inert stubs with empty entry points. This is the last stage at which that is true, which is part of why the path is being exercised now. |
