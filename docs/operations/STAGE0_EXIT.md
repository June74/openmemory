# Stage 0 exit record

- **Stage:** 0 — Program foundation
- **Recorded:** 2026-08-21
- **Owner:** Root integrator
- **Recorded at commit:** see the wave table below; this record is finalized on the Wave D integration commit

Stage 0's exit gate reads:

> A clean Windows build produces a documentation/dev artifact; CI and security checks pass; contracts and repository ownership are approved; no product capability is claimed.

This document records the evidence for each clause of that gate and for each Stage 0 bullet in the [implementation plan](../IMPLEMENTATION_PLAN.md). A clause that is not fully discharged is recorded as not discharged, with what remains. An exit record that cannot record a failure is not evidence, so the `Discharged` column is written from the verification output rather than from intent.

## 1. Waves

| Wave | Content | Merged at |
|---|---|---|
| A | Governance files, records (`COMPATIBILITY.md`, `IDENTIFIERS.md`), link-check tool | `fdd2162` (PR #1) |
| B | Toolchain pins, solution boundaries and dependency graph, contract version integers | `23d4b20` (PR #3) |
| C | CI with six jobs, SBOM, checksummed development artifact, Dependabot, branch protection | `aa67d61` (PR #4), `8669442` (PR #5) |
| D | Threat model, four frozen security contracts, deterministic fixtures, launch checklist, this record | this branch |

## 2. Exit-gate clauses

| Clause | Evidence | Verification command | Discharged |
|---|---|---|---|
| A clean Windows build | The CI `build-and-test` job on `windows-latest` restores, builds with `TreatWarningsAsErrors`, and tests | CI run on the integration commit | **Pending** until §6 records the run |
| …produces a documentation/dev artifact | The CI `artifact` job publishes `Service`, `Cli`, and `McpBridge`, zips them, writes `openmemory-dev.zip.sha256`, and uploads the archive with the SBOM | CI `artifact` job; the digest was downloaded and recomputed during Wave C | Yes, exercised in Wave C; re-run recorded in §6 |
| CI checks pass | Six required checks - `build-and-test`, `plugin`, `secret-scan`, `dependency-review`, `docs`, `artifact` | CI run whose `headSha` matches the branch tip | **Pending** until §6 records the run |
| Security checks pass | gitleaks over the tree in `secret-scan`; `dependency-review` on pull requests; native GitHub secret scanning and push protection enabled | CI run; repository security settings | **Pending** for this branch's run; settings verified in Wave C, with one limitation in §4 |
| Contracts approved | Four frozen security contracts under [`docs/contracts/`](../contracts/README.md), 34 rules, each traced to approved text or a decision; contract version integers in `ContractVersions` | `SecurityContractDocumentTests`; `ContractVersionsTests` | Documents complete; the guarding tests are **pending** their first CI execution |
| Repository ownership approved | `CODEOWNERS`, `CONTRIBUTING.md` with DCO, `SECURITY.md`, issue templates, PR template, branch protection on `main` with `strict: true` and `enforce_admins: true` | [`.github/branch-protection.md`](../../.github/branch-protection.md); branch protection API | Yes, enabled in Wave C |
| No product capability claimed | No `src/**` file created or modified in Wave D; `src/` holds project files, a dependency graph, and stub entry points only | `git diff --name-only main...HEAD -- src/` returns nothing | Yes |

## 3. Stage 0 bullets

| Bullet | Evidence | Discharged |
|---|---|---|
| Verify repository, workspace, identity, `origin`, `main`, clean worktree | Baseline commits `a4e038a`–`0939e71`; setbacks `SET-20260816-004`, `SET-20260816-005` | Yes |
| Install and pin the .NET and Node/TypeScript toolchains | `global.json` (SDK 10.0.400), `Directory.Build.props`, `Directory.Packages.props`, `package.json` + `pnpm-lock.yaml` | Yes |
| Create solution boundaries for every subsystem | `OpenMemory.sln`, 12 projects under `src/`, 3 under `tests/`, two deliberately absent reference edges | Yes |
| Licensing, DCO, security policy, ownership, issue templates, PR gates | `LICENSE`, `NOTICE`, `CONTRIBUTING.md`, `SECURITY.md`, `CODE_OF_CONDUCT.md`, `.github/` | Yes |
| Record requirements, decisions, data classes, identifiers, protocol versions, compatibility | `PRODUCT_REQUIREMENTS.md`, `DECISION_REGISTER.md`, `DATA_AND_PRIVACY.md`, `IDENTIFIERS.md`, `COMPATIBILITY.md` | Yes |
| Create a repository-grounded threat model | [`THREAT_MODEL.md`](../THREAT_MODEL.md) — 29 threats, 3 live and 26 planned, covering all 12 `DATA_AND_PRIVACY.md` §12 verification classes; discharges `F-010` | Yes |
| Freeze registered-client capabilities, trusted-human confirmation, consent/revocation, and publisher-authentication contracts | [`docs/contracts/`](../contracts/README.md) — 34 rules across four documents, each frozen at version 1 | Yes |
| Define deterministic test fixtures and the launch checklist | [`TEST_FIXTURES.md`](../TEST_FIXTURES.md), `tests/fixtures/` with a verified SHA-256 manifest, [`LAUNCH_CHECKLIST.md`](../LAUNCH_CHECKLIST.md) | Yes |
| Establish CI for formatting, build, unit tests, dependency review, secret scanning, SBOM, checksummed artifacts | `.github/workflows/ci.yml`, six jobs | Yes |

## 4. Limitations carried into Stage 1

These are recorded as limitations, not as discharged items. Stage 1 begins with them true.

- **Enhanced secret-scanning detection is not available on this repository.** Non-provider patterns and validity checks are GitHub Secret Protection features that are not provisioned here; the REST API accepts a `PATCH` and silently changes nothing. The detection gap is covered by the CI `secret-scan` job rather than by a native setting. Recorded in the Wave C design.
- **No test exists for any `DATA_AND_PRIVACY.md` §12 verification class.** Stage 0 froze what must be true and modelled what can go wrong; it wrote no security test, because there is no implementation to test. Each contract's §5 names the classes it owes.
- **`F-007` (signing provider) and `F-011` (user-presence mechanism) remain deferred.** `SC-PUB-002` and `SC-CONF-007` fix the requirements; the implementations are chosen in Stage 1 or later against observed behavior.
- **The .NET toolchain is unavailable in the remote session environment** (`SET-20260821-006`), so C# verification for Wave D ran on CI rather than locally. The Windows CI run is the stronger evidence, but a contributor working in that environment cannot run `dotnet test` before pushing.
- **Three verification commands in this wave's plan were wrong when written** and were corrected after workers ran them and reported the discrepancy: a `sed` range that returned zero for any input, a `grep -o` that counted file-and-identifier pairs, and a link checker blind to untracked files (`SET-20260821-007`). The third was a defect in a Wave A tool that had been reporting success over files it never opened.

## 5. What Stage 0 deliberately did not do

Recorded so that Stage 1 does not mistake absence for oversight:

- No product behavior, in any subsystem.
- No implementation of any frozen contract.
- No wire format, field name, or schema — those freeze in Stage 2.
- No `REQ-<AREA>-NNN` applied to existing requirement text; that remains a separate later task.
- No code signing or attestation; `F-007`, Stage 8, explicit approval required before any paid service.

## 6. Verification run

This section is written from the actual continuous-integration run on the Wave D integration commit and is the evidence the **Pending** rows in §2 refer to. Until it names a run and a commit, Stage 0 is not closed - a stage does not exit on the strength of a plan to verify it.

| Item | Value |
|---|---|
| Integration commit | *to be recorded* |
| Workflow run | *to be recorded* |
| `headSha` matches branch tip | *to be recorded* |
| Six jobs green | *to be recorded* |
| Deliberate-failure demonstration | *to be recorded* - the three document-agreement guards are proved able to fail before being trusted |
