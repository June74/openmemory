# OpenMemory Threat Model

- **Status:** Stage 0, grounded in the repository at commit `a2df138`
- **Scheme:** every entry carries a stable `THR-NNN` identifier registered in [IDENTIFIERS.md](IDENTIFIERS.md)
- **Discharges:** `F-010`, which deferred the repository-grounded threat model to Stage 0

## 1. Scope and grounding

**Three of the twenty-nine threats below are live today. The other twenty-six describe boundaries that are not yet implemented.** This document says so at the top because a threat model that lists twenty-nine threats as though they were equally present would misrepresent the repository: it would claim a defended system where there is currently a documentation baseline plus a build pipeline.

This model was written against the repository as it actually exists, not against the system described in [ARCHITECTURE.md](ARCHITECTURE.md).

**What exists at commit `a2df138`:**

- Twelve projects under `src/`: `OpenMemory.Adapters.Abstractions`, `OpenMemory.Adapters.Antigravity`, `OpenMemory.Adapters.ClaudeCode`, `OpenMemory.Adapters.Codex`, `OpenMemory.Cli`, `OpenMemory.Contracts`, `OpenMemory.Indexing`, `OpenMemory.Installer`, `OpenMemory.McpBridge`, `OpenMemory.ObsidianPlugin`, `OpenMemory.Service`, and `OpenMemory.Storage`. The three executable projects contain entry-point placeholders with empty `Main` methods; `OpenMemory.Contracts` holds contract version constants. No project opens a database, listens on a pipe, parses an MCP message, reads a credential, or performs network I/O.
- Three test projects under `tests/`: `OpenMemory.Contracts.Tests`, `OpenMemory.Service.Tests`, `OpenMemory.Storage.Tests`.
- A CI workflow, [`.github/workflows/ci.yml`](../.github/workflows/ci.yml), with six jobs — `build-and-test`, `plugin`, `docs`, `secret-scan`, `dependency-review`, and `artifact` — all six required by the branch-protection ruleset recorded in [`.github/branch-protection.md`](../.github/branch-protection.md).
- A published, downloadable development artifact (`openmemory-dev.zip`) with a SHA-256 checksum file and a CycloneDX SBOM, produced by the `artifact` job.

**What does not exist:** the service host, the named pipe, the MCP bridge behavior, SQLCipher storage, key protection, secret scanning, capture, extraction, retrieval, the Obsidian projection, the updater, and the consent flow. Every threat that crosses one of those boundaries is therefore `planned` — reachable once the named subsystem exists, not reachable now.

**Labels.** A threat is `live` when an adversary could act on it against this repository today. A threat is `planned` when it becomes reachable only once the subsystem that carries its boundary is implemented. The distinction is normative, not editorial: a `planned` threat has no mitigation in place, and describing one as mitigated would be false.

**Governing rules.** The **governing rule** column cites the frozen Stage 0 security contract rules by their `SC-<AREA>-NNN` identifiers. Those rules fix required behavior; nothing implements them yet. Where a threat's primary defense is architectural rather than contractual — encryption at rest, for example, or idempotent replay — the row cites the contract rule that governs the service-mediated path and the prose names the architectural source.

## 2. Assets

Ranked by what their loss costs, most costly first.

| Rank | Asset | Cost of loss |
|---|---|---|
| 1 | The database encryption key and the recovery key | Loss of confidentiality if disclosed; permanent, by-design loss of all encrypted data if both are lost ([ARCHITECTURE.md](ARCHITECTURE.md), §Durable storage and keys). |
| 2 | The encrypted database and its backups | Contains complete chats, tool evidence, private provenance, embeddings, indexes, and audit records ([DATA_AND_PRIVACY.md](DATA_AND_PRIVACY.md), §2.1). A single readable copy discloses the user's entire working history. |
| 3 | Raw evidence and private provenance | Immutable evidence is what makes every derived claim auditable; provenance links a memory to its source. Corrupting either destroys the ability to explain or repair memory. |
| 4 | Secret values transiting the capture boundary | A credential that reaches storage, a log, an embedding, an export, or a model request is disclosed to a system that was never meant to hold it, and cannot be un-disclosed ([DATA_AND_PRIVACY.md](DATA_AND_PRIVACY.md), §4). |
| 5 | The user's Codex account and its allowance | Spending it without consent converts a private, opt-in dependency into an unbudgeted external cost, and every job it runs moves evidence off the machine. |
| 6 | The published artifact and the project's identity | An artifact users trust because of who published it. If the identity can be impersonated, every other control is downstream of a compromised install. |

## 3. Adversaries

- **A same-user local process.** Runs as the same Windows user, so filesystem and named-pipe access are granted by the operating system. It is the adversary the architecture treats most carefully, because same-user access is *authorization for bounded requests* and never *evidence of human intent* ([ARCHITECTURE.md](ARCHITECTURE.md), §Security and trust boundaries).
- **A malicious or compromised repository, attachment, or imported history.** Supplies text that OpenMemory ingests as evidence. Its goal is to have that text read as an instruction, a higher authority claim, or an approval.
- **A compromised third-party action or package in the build.** Executes inside CI with the workflow's permissions, or inside the build output that ships to users. This is the adversary with live reach today.
- **A network position between the updater and its release endpoint.** Substitutes or modifies an artifact in transit, and can supply a matching checksum, because a checksum shipped beside an artifact authenticates nothing.
- **The connected model itself.** Treated as an untrusted component. Model output is a proposal, never a command and never an approval ([ARCHITECTURE.md](ARCHITECTURE.md), §Processing and memory authority).

## 4. Threats by boundary

One section per boundary in [ARCHITECTURE.md](ARCHITECTURE.md), §Security and trust boundaries, plus the storage, capture-integrity, and build boundaries that section depends on. Every threat appears in exactly one table.

### 4.1 Durable storage and keys

| ID | Threat | Adversary | Effect | Label | Governing rule | Mitigating stage | Verification |
|---|---|---|---|---|---|---|---|
| `THR-001` | Database file read directly from disk, bypassing the service. | Same-user local process | Full disclosure of chats, evidence, provenance, and indexes if the file is readable in plaintext. | planned | `SC-CAP-001`, `SC-CAP-002` (the service path is the only authorized path; SQLCipher encryption at rest is fixed by [ARCHITECTURE.md](ARCHITECTURE.md), §Durable storage and keys) | Stage 1 Worker A, hardened Stage 2 | Database and backup confidentiality at rest; open the file with no key and prove it is unreadable. |
| `THR-002` | Backup archive read or copied from its destination. | Same-user local process | Same disclosure as `THR-001`, from a copy that outlives the original. | planned | `SC-CAP-002`, `SC-CONF-001` | Stage 1 Worker A, operations in Stage 6 Lane C | Backup confidentiality at rest, exercised through a restore drill on a second profile. |
| `THR-003` | Key material recovered from another Windows account on the same machine. | Same-user local process, elevated to a second account | Another account decrypts the database, defeating user-scoped protection. | planned | `SC-CAP-003` (same-user scoping authorizes bounded requests only) | Stage 1 Worker A | Windows key scoping exercises: attempt access from a second Windows user profile and prove refusal. |
| `THR-004` | Recovery key brute-forced or replayed. | Same-user local process | Recovery workflow becomes a second, weaker path to the key. | planned | `SC-CAP-005`, `SC-CAP-008` (attempts audited; refuse when the check cannot run) | Stage 1 Worker A | Recovery-key exercises, including rate limiting and audit of failed attempts. |

### 4.2 Client to bridge, and bridge to service

| ID | Threat | Adversary | Effect | Label | Governing rule | Mitigating stage | Verification |
|---|---|---|---|---|---|---|---|
| `THR-005` | Unregistered same-user process connects to the named pipe. | Same-user local process | An unenrolled program reads or writes memory through a channel the OS already permits it to open. | planned | `SC-CAP-001`, `SC-CAP-003`, `SC-CAP-008` | Stage 1 Worker B, enforced Stage 2 Lane C | Named-pipe client authorization tests; reject before reading a payload and record a security event. |
| `THR-006` | A registered client requests beyond its capability set. | Same-user local process, or the connected model through a client | Silent widening of what a client may do, with no refusal the user could notice. | planned | `SC-CAP-002`, `SC-CAP-004`, `SC-CAP-006`, `SC-CAP-007` | Stage 2 Lane C, exercised Stage 6 Lane B | Local impersonation attempts and out-of-set requests; prove refusal, never silent narrowing. |

### 4.3 Trusted human confirmation

| ID | Threat | Adversary | Effect | Label | Governing rule | Mitigating stage | Verification |
|---|---|---|---|---|---|---|---|
| `THR-007` | Model output requests a protected action directly. | The connected model | A deletion, promotion, export, or permission change is finalized without a human ever seeing it. | planned | `SC-CONF-002`, `SC-CONF-006` | Stage 6 Lane B | Trusted-human confirmation tests against model-generated requests. |
| `THR-008` | A confirmation is replayed after use. | Same-user local process | One human approval authorizes a second, unseen action. | planned | `SC-CONF-005` | Stage 6 Lane B | Replay a consumed confirmation and prove it is already spent. |
| `THR-009` | An expired confirmation is presented. | Same-user local process | An approval given for one moment authorizes an action taken much later, under different state. | planned | `SC-CONF-004` | Stage 6 Lane B | Present an expired confirmation and prove refusal rather than renewal. |
| `THR-010` | A confirmation issued for one action is applied to another. | Same-user local process, or the connected model | The user approves what they were shown and something else happens. | planned | `SC-CONF-003` | Stage 6 Lane B | Mismatch the bound action or its before/after hash and prove the confirmation is void. |
| `THR-011` | A same-user background process simulates user presence at the trusted interface. | Same-user local process | Every confirmation control collapses, because the process mints its own approvals. | planned | `SC-CONF-007` (mechanism deferred to `F-011`; the rule fixes the requirement it must satisfy), `SC-CONF-008` | Stage 1 Worker B decides the mechanism; Stage 6 Lane B enforces it | Same-user spoofing tests against real Windows behavior. |

### 4.4 Input to persistence

| ID | Threat | Adversary | Effect | Label | Governing rule | Mitigating stage | Verification |
|---|---|---|---|---|---|---|---|
| `THR-012` | A secret value crosses the persistence boundary into database, log, embedding, or index. | Malicious or ordinary repository, attachment, or imported history | An irreversible disclosure into a store the user believed held only redacted content ([DATA_AND_PRIVACY.md](DATA_AND_PRIVACY.md), §4). | planned | `SC-CONSENT-006`, `SC-CONSENT-008` | Stage 2 Lane A, hardened Stage 7 Lane B | Secret-corpus boundary tests through every ingest route. |
| `THR-013` | A secret value crosses an output boundary into Markdown, export, warning, report, or model request. | Malicious or ordinary repository, attachment, or imported history | The same disclosure, into destinations that leave the machine or the encrypted store. | planned | `SC-CONSENT-006`, `SC-CAP-005` (audit categories and identifiers, never values) | Stage 2 Lane A, hardened Stage 7 Lane B | Secret-corpus boundary tests through every output route; a release fails if any test secret crosses. |

### 4.5 Model processing and memory authority

| ID | Threat | Adversary | Effect | Label | Governing rule | Mitigating stage | Verification |
|---|---|---|---|---|---|---|---|
| `THR-014` | Injected instructions inside captured evidence are followed as commands. | Malicious repository, attachment, or imported history | Untrusted text steers the service or the connected model, defeating the evidence boundary ([DATA_AND_PRIVACY.md](DATA_AND_PRIVACY.md), §5). | planned | `SC-CONF-006`, `SC-CAP-004` | Stage 4 Lane A, hardened Stage 7 Lane B | Evidence-based prompt-injection tests through every ingest route. |
| `THR-015` | Evidence content claims higher authority than the policy engine assigned it. | Malicious repository, attachment, or imported history | A provisional, model-derived claim is treated as an approved fact. | planned | `SC-CONF-001`, `SC-CONF-006` | Stage 4 Lane A and Lane C | Authority-escalation tests; repeated injected text must not become approved. |
| `THR-020` | Evidence is sent for external processing without active consent. | Same-user local process, or a scheduler defect | Private evidence leaves the machine through the user's Codex account with no recorded choice behind it. | planned | `SC-CONSENT-001`, `SC-CONSENT-002`, `SC-CONSENT-003` | Stage 4 Lane A, exercised Stage 6 Lane C | External-processing setup consent and disclosure tests. |
| `THR-021` | Revocation silently stops local capture or retrieval. | Not adversarial — a design or implementation defect | The user is punished for revoking consent, which pressures them not to revoke it. | planned | `SC-CONSENT-004`, `SC-CONSENT-005`, `SC-CONSENT-007` | Stage 4 Lane A, exercised Stage 6 Lane C | Revocation and local-only continuity tests: capture, redaction, embedding, indexing, search, and retention all continue. |

### 4.6 Retrieval and project isolation

| ID | Threat | Adversary | Effect | Label | Governing rule | Mitigating stage | Verification |
|---|---|---|---|---|---|---|---|
| `THR-016` | Restricted or isolated project content surfaces in another project's retrieval. | Same-user local process, or the connected model through a client | The isolation the user chose per project is silently not honored ([ARCHITECTURE.md](ARCHITECTURE.md), §Processing and memory authority). | planned | `SC-CAP-002`, `SC-CONF-001` (sensitivity reduction is a protected action) | Stage 4 Lane B and Lane C | Project isolation and cross-project leakage tests; zero known leaks is a Stage 7 exit condition. |

### 4.7 Database to vault

| ID | Threat | Adversary | Effect | Label | Governing rule | Mitigating stage | Verification |
|---|---|---|---|---|---|---|---|
| `THR-024` | Portable export is written into a synchronized folder unnoticed. | Not adversarial — a configuration the user did not notice | Plaintext memory leaves the device through OneDrive or a similar sync root ([DATA_AND_PRIVACY.md](DATA_AND_PRIVACY.md), §2.2). | planned | `SC-CONF-001`, `SC-CONF-002`, `SC-CONF-003` | Stage 6 Lane C | Portable export warnings and synchronized-vault detection. |
| `THR-025` | A synchronized vault produces conflicting concurrent Markdown edits. | Not adversarial — concurrent edits through a sync client | Either the user's edit or the projection is silently overwritten, losing an authored change. | planned | `SC-CONF-001` (conflict resolution is a protected action), `SC-CONF-002` | Stage 2 Lane C, surfaced Stage 6 Lane A | Synchronized-vault detection and three-way review; never silently overwrite either version. |
| `THR-026` | An old vault is deleted after an unverified import. | Not adversarial — an operator acting on an import that reported success | Irreversible loss of the only copy of pre-import material. | planned | `SC-CONF-001` (material deletion is a protected action), `SC-CONF-003` | Stage 7 Lane A | Old-vault import followed by a non-destructive deletion review with an exact target list. |

### 4.8 Local system to network — updates

| ID | Threat | Adversary | Effect | Label | Governing rule | Mitigating stage | Verification |
|---|---|---|---|---|---|---|---|
| `THR-017` | A tampered update artifact is installed. | Network position between the updater and its release endpoint | Arbitrary code runs with the service's access to keys and the database. A matching checksum does not prevent this ([DATA_AND_PRIVACY.md](DATA_AND_PRIVACY.md), §11). | planned | `SC-PUB-001`, `SC-PUB-002`, `SC-PUB-005`, `SC-PUB-006` | Stage 6 Lane C, hardened Stage 7 Lane B | Update tampering tests: a failed verification installs nothing and retains the artifact as evidence. |
| `THR-018` | A failed migration leaves the database partially migrated. | Not adversarial — an interrupted or defective migration | The authoritative store is in a state no code expects, and a further write compounds it. | planned | `SC-PUB-004`, `SC-PUB-007` | Stage 2 Lane B, exercised Stage 6 Lane C | Migration failure tests: transactional migration, pre-update backup, and refusal to open a partially applied schema. |
| `THR-019` | Rollback to an older binary encounters a newer schema. | Not adversarial — recovery from a failed update | Rollback, the mitigation for `THR-017` and `THR-018`, itself corrupts data. | planned | `SC-PUB-004`, `SC-PUB-007` | Stage 2 Lane B, exercised Stage 6 Lane C | Rollback tests: refuse unsafe downgrade, restore binaries and schema from the pre-change backup. |

### 4.9 Capture durability and database integrity

| ID | Threat | Adversary | Effect | Label | Governing rule | Mitigating stage | Verification |
|---|---|---|---|---|---|---|---|
| `THR-022` | Interrupted capture loses or duplicates events on replay. | Not adversarial — a crash between capture and acknowledgement | Memory silently gains phantom duplicates or silently loses evidence; both corrupt every derived claim. | planned | `SC-CONSENT-004`, `SC-CONSENT-007` (pause without discarding captured evidence; idempotent replay is fixed by [ARCHITECTURE.md](ARCHITECTURE.md), §Durable storage and keys) | Stage 2 Lane A | Interrupted capture and duplicate replay tests. |
| `THR-023` | A corrupted database is opened and treated as authoritative. | Not adversarial — disk or process failure | Corrupt state is served as memory and written back over good backups. | planned | `SC-CAP-008` (refuse when the check cannot run), `SC-PUB-004` | Stage 2 Lane A, exercised Stage 6 Lane C | Database corruption tests: stop writes, run integrity diagnostics, offer verified restore or repair. |

### 4.10 The build and release supply chain — live today

| ID | Threat | Adversary | Effect | Label | Governing rule | Mitigating stage | Verification |
|---|---|---|---|---|---|---|---|
| `THR-027` | A compromised or retagged third-party GitHub Action executes in CI. | Compromised third-party action | Attacker code runs in every required job, including the one that builds and checksums the published artifact. | **live** | `SC-PUB-002`, `SC-PUB-003` (pinned identity, changed only by recorded rotation) | Partially mitigated today by pinning and a minimal action set; fully addressed Stage 7 Lane B and Stage 8 | Review the pin set in [`.github/workflows/ci.yml`](../.github/workflows/ci.yml) on every workflow change; dependency risk review at Stage 7. |
| `THR-028` | A substituted or drifted dependency enters the build. | Compromised package, or an unpinned resolution | A malicious or vulnerable package ships inside the artifact, or a build stops being reproducible. | **live** | `SC-PUB-001`, `SC-PUB-004` | Partially mitigated today by central version pinning, `--frozen-lockfile`, `dependency-review`, and the SBOM; fully addressed Stage 7 Lane B and Stage 8 | The `dependency-review` and `plugin` jobs on every pull request; SBOM produced by the `artifact` job. |
| `THR-029` | An unsigned development artifact is mistaken for a released one. | Any user who downloads it | A user installs an unauthenticated build believing it was published by the project. | **live** | `SC-PUB-001`, `SC-PUB-002`, `SC-PUB-006` | Mitigated today only by the fact that the binaries are inert placeholders and are not published as a release; addressed at Stage 8 | Stage 8 release evidence: signature or signed attestation anchored to the documented trusted project identity. |

## 5. The live supply chain

This is the one boundary where the repository has real exposure today, so it gets its own section rather than a row.

**What is in place at commit `a2df138`:**

- **Pinned actions, minimal set.** [`.github/workflows/ci.yml`](../.github/workflows/ci.yml) uses seven third-party actions: `actions/checkout@v7` (five times), `actions/setup-dotnet@v6` (twice), `actions/setup-node@v7`, `actions/dependency-review-action@v5`, `anchore/sbom-action@v0`, and `actions/upload-artifact@v7`. The gitleaks binary is fetched at a pinned release version (`v8.30.1`), and `anchore/sbom-action` is given an explicit `syft-version` input rather than being allowed to float.
- **A pinned dependency set.** `Directory.Packages.props` centrally manages package versions with `ManagePackageVersionsCentrally`, and the Obsidian plugin job installs with `pnpm install --frozen-lockfile` against the committed `pnpm-lock.yaml`.
- **Dependency review.** The `dependency-review` job runs on every pull request. It checks vulnerabilities only; licence policy is deliberately not configured, because which licences are acceptable is an undecided product question ([`.github/branch-protection.md`](../.github/branch-protection.md)).
- **An SBOM and a checksum.** The `artifact` job emits a CycloneDX SBOM and a SHA-256 file for `openmemory-dev.zip`.
- **Secret scanning over full history.** The `secret-scan` job runs gitleaks across the whole repository history, not just the diff.

**The residual risk, stated as Wave C left it.** [The Wave C design](superpowers/specs/2026-08-16-stage0-wave-c-design.md), §7, recorded that "a third-party action is a supply-chain dependency in the security-critical path", that actions are pinned and kept minimal, and that this "is a genuine residual risk, not one this wave eliminates" — explicitly deferring it here. That is `THR-027`, and this document does not close it either. Two facts make it concrete:

1. **Major-version tags are mutable.** `actions/checkout@v7` resolves to whatever commit the tag points at. A compromised upstream repository can move that tag, and CI will consume the new commit without any change to this repository. Pinning to a full commit SHA would remove that specific move; it has not been done, because doing so trades tag mutability for a manual update burden that the project has not yet decided how to carry. Recorded as accepted for now, not as solved.
2. **`SC-PUB-*` does not yet apply to CI.** The publisher-authentication contract governs what OpenMemory installs on a user's machine. Nothing in it governs what GitHub Actions executes in this repository's CI. The two are related by analogy — pinned identity, recorded rotation — but the contract has no enforcement surface here, and pretending otherwise would overstate the coverage.

`THR-028` and `THR-029` are similarly bounded: pinning and `dependency-review` reduce the first without eliminating substitution risk in transitive dependencies, and the second is presently mitigated only by the artifact being an inert placeholder — which, as Wave C noted, is true for the last time at this stage.

## 6. Accepted residual risks

Stated as accepted. Each is a consequence the project has chosen deliberately, not a gap awaiting a fix.

| Risk | Why it is accepted |
|---|---|
| Losing both the Windows-protected key material and the recovery key makes encrypted data unrecoverable. | This is the design ([ARCHITECTURE.md](ARCHITECTURE.md), §Durable storage and keys). Any escrow or reconstruction path would be a second way to reach the key, which is exactly what `THR-003` and `THR-004` exist to prevent. OpenMemory must explain the loss and must never create a replacement database over the old one. |
| A maintainer with administrator access can disable branch protection. | Enforcement for administrators is enabled, so bypass requires deliberately turning the ruleset off — which appears in the GitHub audit log. The control is *auditable*, not *unpreventable-by-the-owner*; a repository owner can always change their own repository's settings. Recorded so the consequence is accepted rather than discovered under pressure ([`.github/branch-protection.md`](../.github/branch-protection.md)). |
| Enforcing branch protection means a broken CI blocks the pull request that fixes that CI. | The intended tradeoff, recorded in [the Wave C design](superpowers/specs/2026-08-16-stage0-wave-c-design.md), §7: a rule the maintainer can silently bypass is not a control. Recovery is to disable enforcement temporarily, which is visible in the audit log. |
| Major-version action tags are mutable (`THR-027`). | See §5. Accepted for now in exchange for not carrying a manual SHA-update burden the project has not decided how to staff. |
| Required checks are matched by job name, so a rename blocks every merge. | GitHub fails closed here, which is the safe direction. The exact names are recorded in [`.github/branch-protection.md`](../.github/branch-protection.md), and updating both files together is part of any rename. |
| Same-user filesystem access to the database file cannot be prevented by OpenMemory. | The operating system grants it. The answer is encryption at rest plus user-scoped key protection (`THR-001`, `THR-003`), not access control OpenMemory does not own. |

## 7. `DATA_AND_PRIVACY.md` §12 coverage

Every verification class required by [DATA_AND_PRIVACY.md](DATA_AND_PRIVACY.md), §12, maps to at least one threat above. A class with no threat behind it would mean either the class or this model is incomplete.

| Verification class | Threats |
|---|---|
| database and backup confidentiality at rest | `THR-001`, `THR-002` |
| Windows key scoping and recovery-key exercises | `THR-003`, `THR-004` |
| named-pipe client authorization and local impersonation attempts | `THR-005`, `THR-006` |
| trusted-human confirmation tests against model-generated requests, replay, expired confirmations, capability escalation, and same-user spoofing | `THR-007`, `THR-008`, `THR-009`, `THR-010`, `THR-011` |
| secret-corpus boundary tests through every ingest and output route | `THR-012`, `THR-013` |
| evidence-based prompt-injection and authority-escalation tests | `THR-014`, `THR-015` |
| project isolation and cross-project leakage tests | `THR-016` |
| update tampering, migration failure, and rollback tests | `THR-017`, `THR-018`, `THR-019`, `THR-027`, `THR-028`, `THR-029` |
| external-processing setup consent, disclosure, revocation, and local-only continuity tests | `THR-020`, `THR-021` |
| interrupted capture, database corruption, and duplicate replay tests | `THR-022`, `THR-023` |
| portable export warnings and synchronized-vault detection | `THR-024`, `THR-025` |
| old-vault import followed by non-destructive deletion review | `THR-026` |

## 8. Maintaining this document

A `THR-NNN` identifier is allocated monotonically and never reused, per [IDENTIFIERS.md](IDENTIFIERS.md). A threat's label changes from `planned` to `live` in the same change that makes the subsystem carrying its boundary reachable — that transition is the point at which its mitigating stage owes verification evidence, not a bookkeeping detail. Removing a threat requires a decision-register entry stating why it is no longer reachable; a threat whose mitigation shipped stays here with its verification recorded.
