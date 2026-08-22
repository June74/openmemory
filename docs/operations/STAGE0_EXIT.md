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
| A clean Windows build | The CI `build-and-test` job on `windows-latest` verified formatting, restored, built with `TreatWarningsAsErrors`, and ran 17 tests | Run `32592883259`, §6 | Yes |
| …produces a documentation/dev artifact | The CI `artifact` job published `Service`, `Cli`, and `McpBridge`, zipped them, wrote `openmemory-dev.zip.sha256`, and uploaded the archive with the SBOM | Run `32592883259`; the digest was additionally downloaded and recomputed in Wave C | Yes |
| CI checks pass | Five jobs succeeded; `dependency-review` is pull-request-only and correctly skipped, which GitHub counts as passing | Run `32592883259`, §6 | Yes |
| Security checks pass | `secret-scan` (gitleaks over repository history) succeeded; native secret scanning and push protection enabled in Wave C | Run `32592883259`, §6 | Yes, with the §4 limitation |
| Contracts approved | Four frozen security contracts under [`docs/contracts/`](../contracts/README.md), 34 rules, each traced to approved text or a decision; contract version integers in `ContractVersions` | `SecurityContractDocumentTests` and `ContractVersionsTests`, both passing in run `32592883259` | Yes |
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

Written from the actual continuous-integration runs, not from an intention to run them.

| Item | Value |
|---|---|
| Integration commit | `bfe4f62` |
| Green run | [`32592883259`](https://github.com/June74/openmemory/actions/runs/32592883259), `headSha` `bfe4f62`, matching the branch tip |
| Jobs | `build-and-test`, `plugin`, `secret-scan`, `docs`, `artifact` succeeded; `dependency-review` skipped, being pull-request-only |
| Discovered tests | 17, against a floor of 17 |
| Product behavior added | None. `git diff --name-only main...HEAD -- src/` returns nothing |
| DCO | All 12 commits carry `Signed-off-by` |
| Links | 44 files, 275 internal links, 0 broken |
| Record commit | `1462c35`, which added this section, verified green by run [`32593102125`](https://github.com/June74/openmemory/actions/runs/32593102125) |

A record of verification cannot verify the commit that writes it: each entry above names the run covering the tree it describes, and the row recording *that* run necessarily lands one commit later. The regress stops here deliberately. Run `32593102125` covers `1462c35`, the commit carrying every deliverable of this wave; the only change after it is this paragraph and the row above it, which alter no code, no contract, and no fixture. A reviewer wanting the tip verified end to end can dispatch the workflow against it, which is one click and needs no pull request.

### The deliberate-failure demonstration

The three document-agreement guards were proved able to fail before being trusted. No .NET SDK exists in the authoring environment (`SET-20260821-006`), so this ran on CI: commit `ce68605` broke three things at once — one byte of a fixture, a duplicated rule identifier, and a removed verification-class row — and was reverted in `bfe4f62`.

Run [`32592671150`](https://github.com/June74/openmemory/actions/runs/32592671150) failed with five failures across the three guard classes, each naming the offending file:

| Test | Caught |
|---|---|
| `FixtureManifestTests.EveryFixtureFileMatchesItsManifestChecksum` | The changed byte, reporting computed and recorded digests and the CRLF possibility |
| `SecurityContractDocumentTests.NoRuleIdentifierIsDeclaredByMoreThanOneContract` | The duplicated identifier |
| `ThreatModelCoverageTests.EveryRequiredVerificationClassAppearsInTheThreatModelCoverageTable` | The removed verification class, by name |
| `ThreatModelCoverageTests.EveryContractRuleTheThreatModelCitesExistsInTheContracts` | **Unplanned.** Renaming `SC-CAP-004` in the contract left the threat model citing a rule no contract declares — the binding assertion catching a consequence of the mutation in a different document from the one that was edited, which is the drift it exists to prevent |

That run also reported `Failed: 5, Passed: 9, Total: 14` for `Contracts.Tests`, which with the 3 tests in the other two projects confirms the floor of 17 counts what it claims to count.

Each guard has now been observed failing on a known-bad input and passing on a known-good one. That is the difference between a guard and a decoration, and it is the standard every later stage's checks are expected to meet.
