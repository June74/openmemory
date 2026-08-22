# Stage 0 Wave D — Security contracts, threat model, fixtures, and launch checklist Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Close the three remaining Stage 0 exit-gate bullets — the repository-grounded threat model, the four frozen security contracts, and the deterministic test fixtures plus launch checklist — and record the Stage 0 exit evidence, without adding any product capability.

**Architecture:** Documentation, plus test-only code that keeps the documentation from drifting. Four contract documents under `docs/contracts/`, one threat model that cites their rule identifiers, a fixture convention with a checksum manifest, a launch checklist, and a Stage 0 exit record. Three new tests parse those documents and fail the build when they disagree. No `src/` file gains logic.

**Tech Stack:** Markdown, C# / xUnit (test projects only), Bash, SHA-256.

**Spec:** [docs/superpowers/specs/2026-08-21-stage0-wave-d-design.md](../specs/2026-08-21-stage0-wave-d-design.md)

## Global Constraints

Every task's requirements implicitly include this section.

- **No product behavior.** No `src/**` file may be created or modified. The only new code lives under `tests/`. A task that appears to require a `src/` change has hit a plan error — stop and report it rather than working around it.
- **Repository:** `June74/openmemory`. **Branch:** `claude/openmemory-continuation-zu0f7j`. Never commit to `main`.
- **DCO required.** Every commit uses `git commit -s`.
- **Never write a secret value or secret-shaped content** into any file, including examples, placeholders, fixtures, and test data. This is not a style preference: Wave C's `secret-scan` job runs gitleaks over the tree and will fail the build.
- **The local `grep` for secret-shaped content is a tripwire, not evidence.** gitleaks in CI is the detector with maintained rules; the local grep exists only to save a CI cycle. A hit means *look at this*; silence does not mean the tree is clean. The pattern originally used here also matched the ordinary word "task-snapshot" — a hand-rolled regex is fine as a tripwire and dangerous as a guarantee. Secret-detection corpora are generated inside the test at run time (spec D-4), never committed.
- **Apache-2.0 SPDX header on every new `.cs` file**, exactly:
  ```
  // Copyright 2026 OpenMemory contributors
  // SPDX-License-Identifier: Apache-2.0
  ```
- **`TreatWarningsAsErrors` is on.** A warning fails the build. Do not suppress one — report it.
- **`dotnet format --verify-no-changes` must pass.** `.cs` files are CRLF per `.gitattributes`; everything else is LF. `SET-20260816-004` and the Wave C line-ending failure both came from ignoring this.
- **Distinguish planned from implemented behavior** in all prose. Nothing in this wave may describe an unimplemented capability in the present tense as though it exists. Every contract document says plainly that nothing implements it yet.
- **Cite, do not paraphrase.** A statement drawn from `ARCHITECTURE.md`, `DATA_AND_PRIVACY.md`, `COMPATIBILITY.md`, or the decision register links to its source. Paraphrases drift; that is the failure this wave exists to prevent.
- **Scripts are Bash, never PowerShell pipelines** (`SET-20260816-001`).
- **Do not edit `COMPATIBILITY.md`** (spec D-2), and do not renumber any existing `D-*`, `X-*`, `F-*`, `REQ-*`, or `SET-*` identifier.
- **Do not edit a file another task owns.** The ownership table below is exhaustive; if a change seems to require an unowned file, report it instead.

## Frozen allocations

This section is the shared contract. It is frozen before the parallel lanes begin, exactly as `AGENTS.md` requires, so that the threat model can cite contract rules that another worker is writing at the same moment. **A worker may not invent, renumber, or reallocate an identifier here.** A needed identifier that is missing is a plan error: report it to the root integrator rather than allocating one.

### File ownership

| Task | Owner | Owned paths |
|---|---|---|
| 1 | Root integrator | `docs/IDENTIFIERS.md`, `docs/DECISION_REGISTER.md`, `tests/OpenMemory.Contracts.Tests/RepositoryPaths.cs` |
| 2 | Worker A | `docs/contracts/**` |
| 3 | Worker B | `docs/THREAT_MODEL.md` |
| 4 | Worker C | `docs/TEST_FIXTURES.md`, `tests/fixtures/**`, `tests/OpenMemory.Contracts.Tests/FixtureManifestTests.cs`, `.gitattributes` |
| 5 | Worker A | `docs/LAUNCH_CHECKLIST.md` |
| 6 | Worker B | `tests/OpenMemory.Contracts.Tests/SecurityContractDocumentTests.cs`, `tests/OpenMemory.Contracts.Tests/ThreatModelCoverageTests.cs` |
| 7 | Root integrator | `AGENTS.md`, `docs/DATA_AND_PRIVACY.md`, `docs/DECISION_REGISTER.md`, `docs/operations/STAGE0_EXIT.md`, `.github/workflows/ci.yml` |
| 8 | Root integrator | none (verification only) |

Tasks 2, 3, and 4 run concurrently after Task 1. Tasks 5 and 6 run concurrently after their dependencies land. `tests/OpenMemory.Contracts.Tests/ContractVersionsTests.cs` is owned by nobody and is not edited: its private `FindRepositoryRoot` stays as Wave B wrote it, because refactoring a passing test that no requirement touches is churn, not improvement.

### Security contract rule identifiers

Four documents, one per contract. Every rule is normative, fails closed, and carries the identifier below.

**`SC-CAP-*` — registered client capabilities** (`docs/contracts/REGISTERED_CLIENT_CAPABILITIES.md`)

| ID | Rule |
|---|---|
| `SC-CAP-001` | A connection is refused until its client is registered. |
| `SC-CAP-002` | Registration binds a client to a named capability set; a request outside that set is refused, never silently narrowed. |
| `SC-CAP-003` | Same-user named-pipe access authorizes bounded requests and never establishes human intent. |
| `SC-CAP-004` | No request may widen its own client's capability set; escalation is always an out-of-band, approved change. |
| `SC-CAP-005` | Registration, refusal, and escalation attempts are audited without recording evidence content. |
| `SC-CAP-006` | An unsupported protocol or envelope version is refused with an error naming the supported range, never handled by best-effort parsing. |
| `SC-CAP-007` | Message size and result count are bounded before dispatch. |
| `SC-CAP-008` | When the capability or authorization check cannot run, the request is refused. |

**`SC-CONF-*` — trusted human confirmation** (`docs/contracts/TRUSTED_HUMAN_CONFIRMATION.md`)

| ID | Rule |
|---|---|
| `SC-CONF-001` | The protected-action list: conflict resolution, first global promotion, material deletion, project-sensitivity reduction, capability or permission expansion, portable export, unsafe repair, and irreversible update. |
| `SC-CONF-002` | A protected action is finalized only by a confirmation issued through the trusted local interface — terminal or Obsidian. |
| `SC-CONF-003` | A confirmation is bound to the exact displayed action and its before/after hash; any mismatch voids it. |
| `SC-CONF-004` | A confirmation expires quickly, and an expired confirmation is refused rather than renewed. |
| `SC-CONF-005` | A confirmation is single-use and consumed atomically, so a replay finds it already spent. |
| `SC-CONF-006` | No MCP call, model output, or replayed request can mint a confirmation. |
| `SC-CONF-007` | Issuing a confirmation requires an interactive act at the trusted interface that a same-user background process cannot produce on the user's behalf. The exact mechanism is deferred to `F-011`; this rule fixes the requirement it must satisfy. |
| `SC-CONF-008` | When the trusted interface is unavailable, the protected action does not proceed. |
| `SC-CONF-009` | Issuance, consumption, expiry, and refusal are audited. |

**`SC-CONSENT-*` — external processing consent and revocation** (`docs/contracts/EXTERNAL_PROCESSING_CONSENT.md`)

| ID | Rule |
|---|---|
| `SC-CONSENT-001` | Recurring external processing is off until an explicit setup opt-in records the choice. |
| `SC-CONSENT-002` | The opt-in discloses what leaves the local process, through whose account, and for what purpose. |
| `SC-CONSENT-003` | Consent state is inspectable at any time. |
| `SC-CONSENT-004` | Revocation takes effect immediately and pauses model-dependent jobs without discarding their captured evidence. |
| `SC-CONSENT-005` | Revocation never stops local capture, redaction, embedding, indexing, search, retrieval, or evidence retention. |
| `SC-CONSENT-006` | Before evidence leaves the process: select the smallest sufficient slice; run secret detection; replace detected values with typed placeholders; audit categories and record identifiers without copying values. |
| `SC-CONSENT-007` | Authentication failure or allowance exhaustion pauses the job. Substituting another provider or a local model is prohibited. |
| `SC-CONSENT-008` | When secret detection cannot run, nothing is sent. |
| `SC-CONSENT-009` | No telemetry. An update check discloses only what querying a release endpoint technically requires. |

**`SC-PUB-*` — publisher authentication** (`docs/contracts/PUBLISHER_AUTHENTICATION.md`)

| ID | Rule |
|---|---|
| `SC-PUB-001` | A checksum is integrity evidence only and never authenticates a publisher. |
| `SC-PUB-002` | Automatic installation requires a signature or signed attestation verifying against the pinned trusted project identity. |
| `SC-PUB-003` | The pinned identity changes only through an approved, recorded rotation. |
| `SC-PUB-004` | Automatic installation additionally requires all of: a product MINOR or PATCH release; no contract integer increase; no permission or authority expansion; no key-handling change; no irreversible migration; a successful pre-update backup; validated migrations; passing health checks; and an available rollback. |
| `SC-PUB-005` | Failing any condition in `SC-PUB-002` or `SC-PUB-004` requires explicit approval, which is a protected action under `SC-CONF-001`. |
| `SC-PUB-006` | A failed verification installs nothing, retains the artifact as evidence, and warns the user. |
| `SC-PUB-007` | Rollback remains available until post-update health checks pass. |
| `SC-PUB-008` | The update check is the only unsolicited outbound network use. |

### Threat identifiers

Grouped by trust boundary. Each is `live` (reachable in the repository as it exists today) or `planned` (reachable once the named subsystem exists) per spec D-1.

| ID | Threat | Label |
|---|---|---|
| `THR-001` | Database file read directly from disk, bypassing the service. | planned |
| `THR-002` | Backup archive read or copied from its destination. | planned |
| `THR-003` | Key material recovered from another Windows account on the same machine. | planned |
| `THR-004` | Recovery key brute-forced or replayed. | planned |
| `THR-005` | Unregistered same-user process connects to the named pipe. | planned |
| `THR-006` | A registered client requests beyond its capability set. | planned |
| `THR-007` | Model output requests a protected action directly. | planned |
| `THR-008` | A confirmation is replayed after use. | planned |
| `THR-009` | An expired confirmation is presented. | planned |
| `THR-010` | A confirmation issued for one action is applied to another. | planned |
| `THR-011` | A same-user background process simulates user presence at the trusted interface. | planned |
| `THR-012` | A secret value crosses the persistence boundary into database, log, embedding, or index. | planned |
| `THR-013` | A secret value crosses an output boundary into Markdown, export, warning, report, or model request. | planned |
| `THR-014` | Injected instructions inside captured evidence are followed as commands. | planned |
| `THR-015` | Evidence content claims higher authority than the policy engine assigned it. | planned |
| `THR-016` | Restricted or isolated project content surfaces in another project's retrieval. | planned |
| `THR-017` | A tampered update artifact is installed. | planned |
| `THR-018` | A failed migration leaves the database partially migrated. | planned |
| `THR-019` | Rollback to an older binary encounters a newer schema. | planned |
| `THR-020` | Evidence is sent for external processing without active consent. | planned |
| `THR-021` | Revocation silently stops local capture or retrieval. | planned |
| `THR-022` | Interrupted capture loses or duplicates events on replay. | planned |
| `THR-023` | A corrupted database is opened and treated as authoritative. | planned |
| `THR-024` | Portable export is written into a synchronized folder unnoticed. | planned |
| `THR-025` | A synchronized vault produces conflicting concurrent Markdown edits. | planned |
| `THR-026` | An old vault is deleted after an unverified import. | planned |
| `THR-027` | A compromised or retagged third-party GitHub Action executes in CI. | **live** |
| `THR-028` | A substituted or drifted dependency enters the build. | **live** |
| `THR-029` | An unsigned development artifact is mistaken for a released one. | **live** |

Three threats are live. Twenty-six are planned. The threat model must state that ratio plainly rather than presenting all twenty-nine as equally present.

### New deferred and approved decisions

| ID | Content |
|---|---|
| `D-092` | The four Stage 0 security contracts are frozen behaviorally and versioned in their own documents, not added to `COMPATIBILITY.md` §1, because that table's integers remain unfrozen until Stage 2. |
| `D-093` | Security contract rules carry stable `SC-<AREA>-NNN` identifiers so later stages cite rules rather than paraphrase them. |
| `D-094` | Secret-detection fixtures are generated at test run time from documented synthetic patterns and never committed. |
| `D-095` | Committed test fixtures are listed in a checksum manifest that a test verifies, making a silent fixture edit impossible. |
| `F-011` | Exact user-presence mechanism satisfying `SC-CONF-007`, and its resistance to same-user process spoofing. Decide during Stage 1 Worker B's client-connection proof, against real Windows behavior. |

---

### Task 1: Freeze the shared identifiers and the test helper

Root integrator only. Tasks 2–4 cannot start until this lands, because they cite what it registers.

**Files:**
- Modify: `docs/IDENTIFIERS.md`, `docs/DECISION_REGISTER.md`
- Create: `tests/OpenMemory.Contracts.Tests/RepositoryPaths.cs`

**Interfaces:**
- Consumes: nothing.
- Produces: the `THR-NNN` and `SC-<AREA>-NNN` schemes in the identifier registry; decisions `D-092`–`D-095` and `F-011`; and `RepositoryPaths.Root`, the helper the three new test classes use to locate repository files.

- [ ] **Step 1: Confirm the identifiers are unregistered**

```bash
cd /home/user/openmemory && grep -c "THR-NNN\|SC-<AREA>-NNN" docs/IDENTIFIERS.md; grep -c "D-092\|F-011" docs/DECISION_REGISTER.md
```

Expected: `0` and `0`.

- [ ] **Step 2: Register the two schemes**

Add two rows to `docs/IDENTIFIERS.md` §1's table, after `REQ-<AREA>-NNN`:

| Pattern | Meaning | Status | Defined in |
|---|---|---|---|
| `THR-NNN` | Threat model entry | In use | `THREAT_MODEL.md` |
| `SC-<AREA>-NNN` | Frozen security contract rule | In use | `docs/contracts/` |

Then add a paragraph after the existing `REQ-*` paragraph explaining that `SC-*` is namespaced by contract area (`CAP`, `CONF`, `CONSENT`, `PUB`) because the four contracts are owned and versioned separately, and that both schemes follow the existing monotonic, never-reused allocation rule.

- [ ] **Step 3: Add the decisions**

Add `D-092`–`D-095` to the appropriate register sections (technology and authority/privacy), and `F-011` to §9. Each entry states the decision and its rationale in the register's existing two-column style. Do not yet mark `F-010` discharged — Task 7 does that, once the document it must link to exists.

- [ ] **Step 4: Create the test helper**

`tests/OpenMemory.Contracts.Tests/RepositoryPaths.cs`, CRLF, with the SPDX header. It exposes `RepositoryPaths.Root` — the directory walk upward from `AppContext.BaseDirectory` to the folder containing `.git` — and a `Read(params string[] segments)` convenience that reads a repository-relative file. It throws a clear exception naming what it looked for when the root cannot be found, because a silent null here would surface later as an unexplained empty-parse failure in three different tests.

- [ ] **Step 5: Verify**

```bash
cd /home/user/openmemory && dotnet build OpenMemory.sln && dotnet format --verify-no-changes && bash tools/check-links.sh
```

Expected: build succeeds with 0 warnings, format reports no changes, links exit `0`.

- [ ] **Step 6: Commit**

```bash
cd /home/user/openmemory && git add -A && git commit -s -m "Register threat and security-contract identifiers, add test path helper"
```

---

### Task 2: The four frozen security contracts — Worker A

**Files:**
- Create: `docs/contracts/README.md`, `docs/contracts/REGISTERED_CLIENT_CAPABILITIES.md`, `docs/contracts/TRUSTED_HUMAN_CONFIRMATION.md`, `docs/contracts/EXTERNAL_PROCESSING_CONSENT.md`, `docs/contracts/PUBLISHER_AUTHENTICATION.md`

**Interfaces:**
- Consumes: the `SC-*` allocation above; `ARCHITECTURE.md`, `DATA_AND_PRIVACY.md`, `COMPATIBILITY.md`, `DECISION_REGISTER.md` as read-only sources.
- Produces: the rule identifiers Task 3 cites and Task 6 parses.

- [ ] **Step 1: Read the sources before writing a word**

Read `ARCHITECTURE.md` §Security and trust boundaries, §MCP interface contract, §Processing and memory authority; `DATA_AND_PRIVACY.md` §2.3, §4, §5, §6, §11, §12; `COMPATIBILITY.md` §5; and register entries `D-014`, `D-018`, `D-024`, `D-071`, `D-090`. Every rule you write must be traceable to one of these. A rule you cannot trace is a rule you invented, and this wave does not have authority to invent security requirements — only to freeze approved ones.

- [ ] **Step 2: Write `docs/contracts/README.md`**

A short index: what a frozen security contract is, why these four freeze at Stage 0 while `COMPATIBILITY.md`'s integers do not (cite `D-092`), the `SC-<AREA>-NNN` scheme (`D-093`), and a table linking the four documents. State plainly that nothing implements any of these contracts yet.

- [ ] **Step 3: Write the four contract documents**

Each uses this exact structure, because Task 6's test parses it:

```markdown
# <Title>

- **Contract:** <area name>
- **Version:** 1
- **Status:** Frozen (Stage 0)
- **Rule prefix:** `SC-<AREA>-`

> **Status note:** nothing implements this contract yet. It fixes required behavior before implementation, per the Stage 0 exit gate.

## 1. What this contract governs
## 2. What freezing does and does not fix
## 3. Rules

| ID | Rule | Fails closed by | Source |
|---|---|---|---|
| `SC-<AREA>-001` | … | … | [link] |

## 4. Failure behavior
## 5. Verification owed
## 6. Change procedure
```

Requirements for the content:

- The **Rules** table reproduces every rule allocated above for that area, in order, with no additions and no omissions. Expand each into precise normative language — the allocation table is a one-line summary, not the finished rule.
- **§2** must say what the freeze does *not* fix: wire formats, field names, schemas, and timeouts are Stage 2's freeze. `SC-CONF-004`'s "quickly" is deliberately not a number here; naming one now would freeze a value chosen without measurement.
- **§4** states the fail-closed behavior for every way the contract's checks can be unavailable.
- **§5** lists the `DATA_AND_PRIVACY.md` §12 verification classes this contract owes evidence for.
- **§6** states that a frozen contract changes only through a decision-register entry plus a migration-impact note, and that the version integer increments when a rule's meaning changes.
- `TRUSTED_HUMAN_CONFIRMATION.md` §2 must explicitly record that `SC-CONF-007`'s mechanism is deferred to `F-011`, and must not describe a mechanism as though it were chosen.
- `PUBLISHER_AUTHENTICATION.md` must state that `F-007` still defers the signing provider, and that `SC-PUB-002` fixes the requirement rather than the provider.

- [ ] **Step 4: Verify**

```bash
cd /home/user/openmemory && bash tools/check-links.sh
grep -oh 'SC-[A-Z]*-[0-9][0-9][0-9]' docs/contracts/*.md | sort -u | wc -l
grep -rniE '(\bsk-[A-Za-z0-9]{16,}|\bghp_[A-Za-z0-9]{8,}|-----BEGIN [A-Z ]*PRIVATE KEY-----|\bAKIA[0-9A-Z]{16})' docs/contracts/ && echo "LOOK AT THIS" || echo "no hits"
```

Expected: links exit `0`; the identifier count is exactly `34` (8 + 9 + 9 + 8); no secret-shaped content.

> **Plan correction, 2026-08-21.** Two flaws in these commands, both found by Worker A running them as written rather than around them.
> 1. The count command originally omitted `grep -h`. Across multiple files `grep -o` prefixes each match with its filename, so `sort -u` counts unique *file:identifier* pairs and returns `46` — the 34 rules plus 12 legitimate cross-document references, one of which this plan itself mandates (`SC-PUB-005` cites `SC-CONF-001`). The expectation was right; the command measured something else.
> 2. `tools/check-links.sh` iterates `git ls-files`, so it cannot see untracked files. Its exit `0` says nothing about documents that have not been staged yet. Stage the new files (`git add`) before treating a link check as evidence about them — a green check over files it never opened is the most expensive kind of false confidence.

- [ ] **Step 5: Commit**

```bash
cd /home/user/openmemory && git add docs/contracts && git commit -s -m "Freeze the four Stage 0 security contracts"
```

---

### Task 3: The repository-grounded threat model — Worker B

**Files:**
- Create: `docs/THREAT_MODEL.md`

**Interfaces:**
- Consumes: the `THR-*` and `SC-*` allocations above. Worker B cites `SC-*` identifiers **from the allocation table**, not from Worker A's files, which may not exist yet — that is precisely why the allocation is frozen first.
- Produces: the `THR-NNN` entries and the §12 coverage table Task 6 parses.

- [ ] **Step 1: Ground the model in the repository as it exists**

```bash
cd /home/user/openmemory && git log --oneline -1 && ls src tests && grep -n "uses:" .github/workflows/ci.yml
```

Record the commit hash, the project list, and every third-party action with its pin. The three live threats are about *these*, not about an imagined system.

- [ ] **Step 2: Write the document**

Structure, per spec §4.1:

1. **Scope and grounding** — the commit hash, what exists, what does not, and the plain statement that 3 of 29 threats are live today and 26 describe boundaries that are not yet implemented.
2. **Assets**, ranked, each with what its loss costs.
3. **Adversaries** — same-user local process; malicious repository, attachment, or imported history; compromised third-party action or package; network position between updater and release endpoint; the connected model itself, treated as untrusted.
4. **Threats by boundary** — one section per `ARCHITECTURE.md` boundary. Every `THR-NNN` from the allocation appears exactly once, in a table with columns: ID, threat, adversary, effect, label, governing rule, mitigating stage, verification.
5. **The live supply chain** — pinned actions, SBOM, `--frozen-lockfile`, dependency review, and the residual risk Wave C explicitly deferred here.
6. **Accepted residual risks** — stated as accepted, not mitigated. Include that losing both the Windows-protected key material and the recovery key is unrecoverable by design, and that a maintainer with administrator access can disable branch protection, which is visible in the audit log rather than prevented.
7. **`DATA_AND_PRIVACY.md` §12 coverage** — a table with exactly two columns, `Verification class` and `Threats`. The first column reproduces each §12 bullet **verbatim, minus its trailing semicolon or period**; the second lists the covering `THR-NNN` identifiers, comma-separated. Task 6's test compares this column against the source document character for character after normalization, so do not reword.

- [ ] **Step 3: Verify coverage by hand before the test exists**

```bash
cd /home/user/openmemory
grep -o 'THR-0[0-9][0-9]' docs/THREAT_MODEL.md | sort -u | wc -l
sed -n '/## 12. Required security verification/,/^The repository-grounded/p' docs/DATA_AND_PRIVACY.md | grep -c '^- '
bash tools/check-links.sh
```

Expected: `29` distinct threats; `12` verification bullets, every one present in the coverage table; links exit `0`.

> **Plan correction, 2026-08-21.** The bullet-count command originally ended its `sed` range at `/^$/`, which is the blank line immediately *after* the heading — so it counted `0` bullets regardless of the document's content, and would have reported success-shaped output for a threat model covering nothing. Worker B ran it as written, got `0`, and reported it rather than working around it. The range now ends at the section's closing sentence. A verification command that cannot fail correctly is worse than no command, because it is mistaken for evidence.

- [ ] **Step 4: Commit**

```bash
cd /home/user/openmemory && git add docs/THREAT_MODEL.md && git commit -s -m "Add the repository-grounded threat model"
```

---

### Task 4: Deterministic test fixtures — Worker C

**Files:**
- Create: `docs/TEST_FIXTURES.md`, `tests/fixtures/MANIFEST.md`, `tests/fixtures/events/neutral-event-envelope.sample.json`, `tests/fixtures/transcripts/conversation-turn.sample.json`, `tests/fixtures/repositories/synthetic-repository-tree.sample.json`, `tests/OpenMemory.Contracts.Tests/FixtureManifestTests.cs`
- Modify: `.gitattributes`

**Interfaces:**
- Consumes: `RepositoryPaths` from Task 1.
- Produces: the fixture convention and the guard that makes "deterministic" checkable.

- [ ] **Step 1: Write the failing test first**

Write `FixtureManifestTests.cs` before any fixture exists. It must assert three distinct things, each with its own `[Fact]`:

1. every manifest row names a file that exists;
2. every file under `tests/fixtures/` except `MANIFEST.md` appears in the manifest;
3. every file's SHA-256, computed over its bytes, equals the manifest value, lowercase hex.

Run `dotnet test` and observe it fail. A manifest guard that has never been observed failing is not evidence — this is the same reasoning behind Wave C's discovered-test-count assertion.

- [ ] **Step 2: Write the three fixtures**

Each is JSON, LF line endings, two-space indent, keys in a fixed order, and contains **no** timestamp, random value, machine name, path, or anything else that varies between machines — that is what "deterministic" means here and why the checksum can be asserted at all.

- `events/neutral-event-envelope.sample.json` — one provider-neutral event envelope with the fields `ARCHITECTURE.md` §Capture and normalized events names. Identifiers are fixed literal UUIDv7-shaped values, not generated.
- `transcripts/conversation-turn.sample.json` — one complete user/assistant/tool turn, the unit `/store` targets.
- `repositories/synthetic-repository-tree.sample.json` — a small file/symbol/language tree for indexing, describing a repository rather than containing one.

Every fixture is fictional content authored for this purpose. Do not copy real conversation data, real repository content, or anything resembling a credential.

- [ ] **Step 3: Write `tests/fixtures/MANIFEST.md`**

A table: `Fixture`, `SHA-256`, `Purpose`, `Consumed by`. Generate the hashes rather than typing them:

```bash
cd /home/user/openmemory/tests/fixtures && find . -type f -name '*.json' | sort | while read -r f; do echo "$f  $(sha256sum "$f" | cut -d' ' -f1)"; done
```

Above the table, state that a fixture change is a deliberate act requiring the manifest to be regenerated in the same commit, and that secret-detection corpora are deliberately absent per `D-094` — with the reason, so a later contributor does not "fix" the omission by committing one.

- [ ] **Step 4: Write `docs/TEST_FIXTURES.md`**

The convention: directory layout, naming (`<subject>.sample.json`), the determinism rules from Step 2, how to add a fixture (add file, regenerate manifest, both in one commit), the LF requirement and why (`SET-20260816-004`), and the generated-not-committed rule for secret corpora with the concrete generation approach — synthetic patterns assembled at run time from documented components, never a literal in the tree.

- [ ] **Step 5: Add the explicit line-ending rule**

Append `tests/fixtures/** text eol=lf` to `.gitattributes`. `* text=auto eol=lf` already covers it; the explicit rule states the dependency the checksums have on line endings where a future editor will see it.

- [ ] **Step 6: Verify**

```bash
cd /home/user/openmemory && dotnet test && bash tools/check-links.sh
sed -i 's/"schemaVersion": 1/"schemaVersion": 2/' tests/fixtures/events/neutral-event-envelope.sample.json && dotnet test; echo "expected: FAILED above"
git checkout tests/fixtures/events/neutral-event-envelope.sample.json && dotnet test
```

Expected: pass, then a checksum failure naming the file, then pass again. The middle step is the acceptance evidence for this task; a manifest test that cannot be shown failing has proved nothing.

- [ ] **Step 7: Commit**

```bash
cd /home/user/openmemory && git add -A && git commit -s -m "Add deterministic test fixtures with a verified checksum manifest"
```

---

### Task 5: The launch checklist — Worker A

**Files:**
- Create: `docs/LAUNCH_CHECKLIST.md`

**Interfaces:**
- Consumes: `IMPLEMENTATION_PLAN.md` Stage 8, `COMPATIBILITY.md`, `docs/contracts/**` (Task 2), `DATA_AND_PRIVACY.md` §12.

- [ ] **Step 1: Enumerate the obligations from the Stage 8 exit gate**

Read `IMPLEMENTATION_PLAN.md` §Stage 8 and list every obligation it names. Do not add obligations the plan does not state, and do not drop one because its evidence does not exist yet.

- [ ] **Step 2: Write the checklist**

One table: `Obligation`, `Evidence`, `Produced by`, `Status`. `Produced by` names the command, job, or artifact — `dotnet publish`, the CI `artifact` job, the SBOM, the exit record, a specific contract's §5. An obligation whose evidence is not yet decidable is listed with the deferred decision that will supply it (`F-004`, `F-006`, `F-007`, `F-009`, `F-011`), never omitted (spec D-6).

Add a short preamble: this checklist is evidence-based, an item is ticked only when its named artifact exists and is current on the release commit, and recollection is not evidence.

- [ ] **Step 3: Verify**

```bash
cd /home/user/openmemory && bash tools/check-links.sh && grep -c '^| ' docs/LAUNCH_CHECKLIST.md
```

Expected: links exit `0`; every row has a non-empty `Produced by` cell.

- [ ] **Step 4: Commit**

```bash
cd /home/user/openmemory && git add docs/LAUNCH_CHECKLIST.md && git commit -s -m "Add the evidence-based launch checklist"
```

---

### Task 6: The two document-agreement tests — Worker B

Requires Tasks 2 and 3 to have landed.

**Files:**
- Create: `tests/OpenMemory.Contracts.Tests/SecurityContractDocumentTests.cs`, `tests/OpenMemory.Contracts.Tests/ThreatModelCoverageTests.cs`

**Interfaces:**
- Consumes: `RepositoryPaths` (Task 1), `docs/contracts/**` (Task 2), `docs/THREAT_MODEL.md` (Task 3), `docs/DATA_AND_PRIVACY.md` §12.

- [ ] **Step 1: `SecurityContractDocumentTests`**

Facts:

1. all four contract documents exist and each declares `**Version:**` and `**Status:** Frozen`;
2. every `SC-*` identifier in the repository is unique and matches its document's declared rule prefix;
3. the four documents together declare exactly the 34 allocated identifiers, with no gaps in each area's numbering — a gap means a rule was dropped silently, which is the failure this test exists to catch.

- [ ] **Step 2: `ThreatModelCoverageTests`**

Facts:

1. every `DATA_AND_PRIVACY.md` §12 bullet appears as a row in the threat model's coverage table, compared after trimming whitespace and any trailing `;` or `.`;
2. every `THR-NNN` named in the coverage table is defined in the threat model;
3. every `SC-*` identifier the threat model cites exists in `docs/contracts/`.

Fact 3 is the one that keeps the two documents bound as later stages edit them: renaming a contract rule without updating the threat model fails the build.

- [ ] **Step 3: Observe each fact fail before trusting it**

For each of the six facts, make the smallest edit that should break it, run `dotnet test`, confirm the failure names the document and the expected shape, then revert. Record the six observed failure messages in the commit body. A test whose failure message sends the next reader on an archaeology expedition is a test that will be deleted rather than fixed.

- [ ] **Step 4: Commit**

```bash
cd /home/user/openmemory && dotnet format && git add -A && git commit -s -m "Bind the threat model and security contracts with document-agreement tests"
```

---

### Task 7: Records, exit evidence, and the CI floor — root integrator

**Files:**
- Create: `docs/operations/STAGE0_EXIT.md`
- Modify: `AGENTS.md`, `docs/DATA_AND_PRIVACY.md`, `docs/DECISION_REGISTER.md`, `.github/workflows/ci.yml`

- [ ] **Step 1: Discharge `F-010`**

In `DECISION_REGISTER.md` §9, mark `F-010` discharged with a link to `THREAT_MODEL.md` and the date. Do not delete the entry — a discharged deferral is a record, and `IDENTIFIERS.md` forbids reuse.

- [ ] **Step 2: Update `DATA_AND_PRIVACY.md` §12's closing sentence**

Replace "The repository-grounded threat model will be written during Stage 0, after the actual code structure and trust boundaries exist." with a sentence in the past tense linking to the document. This is the only edit to that file.

- [ ] **Step 3: Correct `AGENTS.md`'s status paragraph**

Replace the "documentation-only planning baseline / implementation has not begun" paragraph with an accurate one: Stage 0 is complete, what it delivered, that `src/` holds boundary-only projects with no product behavior, and that Stage 1 work still requires an approved spec and plan before any product code. The constraint is restated more narrowly, not dropped (spec D-7).

- [ ] **Step 4: Write `docs/operations/STAGE0_EXIT.md`**

One row per clause of the Stage 0 exit gate, with `Clause`, `Evidence`, `Verification command`, `Discharged`. Any clause not fully discharged is recorded as such with what remains. Include the wave-by-wave summary and the commit each wave merged at.

- [ ] **Step 5: Raise the CI discovered-test floor**

```bash
cd /home/user/openmemory && dotnet test --list-tests 2>/dev/null | grep -cE '^\s+\S+\.\S+\.\S+$'
```

Set `$expected` in `.github/workflows/ci.yml` to that exact count. A floor left at `4` would let every test added by this wave be deleted without CI noticing, which is the precise failure Wave C's C-5 exists to prevent.

- [ ] **Step 6: Verify and commit**

```bash
cd /home/user/openmemory && dotnet test && bash tools/check-links.sh
git add -A && git commit -s -m "Record Stage 0 exit evidence, discharge F-010, and raise the CI test floor"
```

---

### Task 8: Integration verification and independent review — root integrator

- [ ] **Step 1: Confirm no product behavior was added**

```bash
cd /home/user/openmemory && git diff --name-only main...HEAD
git diff main...HEAD -- 'src/*' | head -20
```

Expected: no `src/` path appears at all. If one does, stop — a global constraint was violated.

- [ ] **Step 2: DCO and secret scan**

```bash
cd /home/user/openmemory
for sha in $(git log main..HEAD --format=%H); do
  git log -1 --format=%B "$sha" | grep -qE '^Signed-off-by:' || echo "MISSING SIGN-OFF: $sha"
done; echo "sign-off check complete"
git diff main...HEAD | grep -inE '(sk-[A-Za-z0-9]{8,}|ghp_[A-Za-z0-9]{8,}|-----BEGIN [A-Z ]*PRIVATE KEY-----)' && echo "FAIL" || echo "PASS: no secret-shaped content"
```

- [ ] **Step 3: Full local suite**

```bash
cd /home/user/openmemory && dotnet format --verify-no-changes && dotnet build OpenMemory.sln && dotnet test && bash tools/check-links.sh; echo "exit=$?"
```

Expected: format clean, build with 0 warnings, all tests pass, links `exit=0`.

- [ ] **Step 4: Confirm CI is green on the branch tip**

Check the Actions run for this branch and compare its `headSha` to local `HEAD`. A green run against an older commit is not evidence about the current tree.

- [ ] **Step 5: Independent specification and security review**

Review the branch diff against `AGENTS.md`, `docs/IMPLEMENTATION_PLAN.md`, and the Wave D spec, verifying: the spec is implemented with no gap and no scope creep; no product behavior was added; every allocated `SC-*` and `THR-*` identifier is present exactly once; no rule was invented without a traceable source; the three new tests cannot pass vacuously; and the §12 coverage mapping is complete. Verify each finding technically per `receiving-code-review` rather than accepting it performatively.

- [ ] **Step 6: Record any unexpected failure**

If any step failed unexpectedly, create a setback record in `docs/operations/setbacks/` following the existing five, add it to `INDEX.md`, and commit.

---

## Wave D completion criteria

1. Tasks 1–8 complete.
2. All four contract documents exist, frozen, declaring exactly the 34 allocated rules with no gaps.
3. The threat model defines all 29 allocated threats, labels 3 live and 26 planned, and covers all 12 `DATA_AND_PRIVACY.md` §12 verification classes.
4. Three fixtures exist with a manifest whose checksum test has been **observed failing** on a deliberate one-byte change and passing after revert.
5. The launch checklist names evidence for every Stage 8 obligation, with deferred items attributed to an `F-*`.
6. `docs/operations/STAGE0_EXIT.md` records every exit-gate clause with evidence and verification command.
7. `F-010` discharged; `D-092`–`D-095` and `F-011` recorded; `AGENTS.md` status accurate.
8. The CI discovered-test floor equals the real count.
9. All six CI jobs green on a run whose `headSha` matches the branch tip.
10. Every commit DCO-signed; no secret-shaped content anywhere in the diff.
11. No `src/**` file created or modified.
12. Independent review findings resolved with evidence.
