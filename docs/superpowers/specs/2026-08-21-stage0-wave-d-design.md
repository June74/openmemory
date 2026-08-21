# Stage 0 Wave D — Security contracts, threat model, fixtures, and launch checklist (design)

- **Date:** 2026-08-21
- **Stage:** 0 (Program foundation)
- **Wave:** D of four — the last
- **Owner:** Root integrator
- **Branch:** `claude/openmemory-continuation-zu0f7j`
- **Status:** Awaiting user review

## 1. Why this wave exists

Waves A, B, and C established governance and records, the toolchain and solution dependency graph, and continuous integration with branch protection. Three Stage 0 bullets remain unclaimed by any wave:

> - "create a repository-grounded threat model before security-sensitive implementation"
> - "freeze registered-client capabilities, trusted-human confirmation, consent/revocation, and publisher-authentication contracts"
> - "define deterministic test fixtures and the launch checklist"

Wave D covers all three, and then records the Stage 0 exit-gate evidence so the stage closes on written proof rather than on the absence of objections.

This wave is last by design, not by convenience. [`F-010`](../../DECISION_REGISTER.md) defers the threat model until "the repository structure and executable trust boundaries exist," and all three earlier specs restate that dependency. Those boundaries now exist: a solution whose missing project references are compiler-enforced (Wave B), and a CI pipeline that is itself a trust surface with third-party actions in it (Wave C). A threat model written before them would have modelled an imagined system.

**Scope discipline.** No `src/` file gains logic. The only executable code this wave adds lives under `tests/` and exists to keep documents and fixtures from drifting apart. The Stage 0 exit gate's "no product capability is claimed" still binds.

## 2. Current state

Verified on 2026-08-21 against `main` at `8669442`, with Waves A, B, and C merged and no open pull requests.

**Buildable:** the Wave C CI workflow's six jobs are the current definition of green; `main` requires all six.

**What exists that the threat model can be grounded in:**

| Surface | State |
|---|---|
| Trust boundaries in prose | [`ARCHITECTURE.md` §Security and trust boundaries](../../ARCHITECTURE.md) — six named boundaries, none implemented |
| Boundaries enforced by the compiler | Wave B's dependency graph, including two deliberately absent edges |
| Boundaries enforced by CI | Six required checks, gitleaks, dependency review, `--frozen-lockfile`, pinned actions |
| Required security verification | [`DATA_AND_PRIVACY.md` §12](../../DATA_AND_PRIVACY.md) — twelve verification classes, no test written |
| Contract version integers | `ContractVersions` (Wave B), all unfrozen until Stage 2 |

**Missing:** no threat model document. No contract document for any of the four security contracts — they exist only as scattered prose across `ARCHITECTURE.md`, `DATA_AND_PRIVACY.md`, and the decision register, with no stable rule identifiers to cite. No fixture convention and no `tests/fixtures/` directory. No launch checklist. `AGENTS.md` still opens by describing the repository as "a documentation-only planning baseline" in which "implementation has not begun," which Waves B and C made false.

## 3. Decisions taken during brainstorming

| Ref | Decision | Rationale |
|---|---|---|
| D-1 | Every threat entry is labelled **live** (reachable in the repository as it exists today) or **planned** (reachable only once a described subsystem is implemented), and every mitigation names the stage and the verification that discharges it. | A threat model whose entries are all hypothetical cannot be checked, and one that silently mixes today's supply-chain exposure with Stage 4's prompt-injection surface invites treating both as equally remote. Today, exactly one class is live: the build and repository supply chain. Saying so plainly is the difference between a threat model and a wish list. |
| D-2 | The four security contracts are **frozen at Stage 0 as behavioral contracts**, versioned in their own documents, and deliberately **not** added to `COMPATIBILITY.md` §1. | §1 lists wire and storage surfaces whose integers §6 declares unfrozen until Stage 2. Adding Stage-0-frozen behavioral contracts to that table would put two opposite freeze states in one list and contradict §6. The two kinds of contract are versioned separately because they freeze at different times, for different reasons. |
| D-3 | Each contract rule gets a stable identifier — `SC-CAP-NNN`, `SC-CONF-NNN`, `SC-CONSENT-NNN`, `SC-PUB-NNN` — registered in `IDENTIFIERS.md`. | Stage 1 proofs, Stage 3 adapters, and Stage 7's adversarial tests must cite the exact rule they satisfy or attack. Without identifiers each will paraphrase, and paraphrases drift. This is the same reasoning that gave `D-*` and `REQ-*` their schemes. |
| D-4 | Secret-detection fixtures are **generated at test time** from documented synthetic patterns and never committed. Every other fixture is a committed file. | `AGENTS.md` prohibits writing a secret value into any file "including examples, placeholders, and templates," and Wave C's gitleaks job would fail the build on committed secret-shaped content. A secret-corpus fixture that lives in the tree is therefore both a policy violation and a self-inflicted CI failure. Generating the corpus in the test keeps the corpus deterministic without ever placing it under version control. |
| D-5 | Committed fixtures are listed in a manifest with SHA-256 values, and a test recomputes them. | "Deterministic" is a claim that decays silently: a fixture edited to make a failing test pass looks identical in review to a fixture edited to fix a real defect. The manifest makes the first case a deliberate, visible act. The pattern is Wave B's `ContractVersionsTests`, which already proves a document and its code cannot drift apart unnoticed. |
| D-6 | The launch checklist is a checklist of **evidence**, one line per obligation, each naming the command or artifact that proves it — not a list of intentions. | Stage 8's exit gate requires "every preceding gate passes on the release commit." A checklist item that cannot name its evidence is not checkable, and would be ticked on recollection. |
| D-7 | `AGENTS.md`'s status paragraph is corrected in this wave. | It currently tells every future agent that implementation has not begun and that adding project files is prohibited. That instruction is now false and actively misleading; leaving it until a later wave means every agent reads a false constraint first. |
| D-8 | The Stage 0 exit-gate evidence is recorded in `docs/operations/STAGE0_EXIT.md`, one row per gate clause, each with its evidence and its verification command. | The gate says "CI and security checks pass; contracts and repository ownership are approved." Without a written record, Stage 1 would begin on an undocumented assertion that Stage 0 finished. |

## 4. Deliverables

### 4.1 `docs/THREAT_MODEL.md`

The repository-grounded threat model `F-010` defers to this point. Structure:

1. **Scope and grounding** — what exists today versus what is planned, and the honest statement that most boundaries are not yet implemented.
2. **Assets**, ranked: the encryption key and recovery key; the encrypted database; raw evidence and private provenance; secret values transiting the capture boundary; the user's Codex account allowance; the published artifact and its identity.
3. **Adversaries**: a same-user local process; a malicious or compromised repository, attachment, or imported history; a compromised third-party action or package in the build; a network position between the updater and its release endpoint; the user's own connected model, treated as an untrusted component rather than a trusted one.
4. **Boundary-by-boundary threats**, one section per boundary in `ARCHITECTURE.md`, each entry `THR-NNN` with adversary, effect, live-or-planned label (D-1), the contract rule that governs it, the stage that implements the mitigation, and the verification that proves it.
5. **The supply chain as it exists today** — the one live class: pinned actions, the SBOM, `--frozen-lockfile`, dependency review, and the residual risk Wave C recorded and explicitly left to this document.
6. **Accepted residual risks**, stated as accepted rather than mitigated — among them that losing both the Windows-protected key material and the recovery key is unrecoverable by design.

Every `DATA_AND_PRIVACY.md` §12 verification class must map to at least one `THR-NNN`. A verification class with no threat behind it means either the class or the model is incomplete; the mapping is asserted by a test (§4.6).

### 4.2 `docs/contracts/` — four frozen security contracts

One document per contract, each stating its version, its freeze status, what freezing does and does not fix, and its numbered rules.

| Document | Freezes | Anchored in |
|---|---|---|
| `REGISTERED_CLIENT_CAPABILITIES.md` | What registration establishes and — more importantly — what it does not: same-user pipe access is authorization to make bounded requests, never evidence of human intent. Capability grant, refusal, and escalation-refusal behavior. | `ARCHITECTURE.md` §Security and trust boundaries; `DATA_AND_PRIVACY.md` §6 |
| `TRUSTED_HUMAN_CONFIRMATION.md` | The protected-action list; the trusted display path; action-binding to the before/after hash; short expiry; single use; atomic consumption; and — the item `ARCHITECTURE.md` explicitly assigns to the Stage 0 threat model — the user-presence mechanism and its resistance to same-user process spoofing. | `ARCHITECTURE.md` §MCP interface contract; `DATA_AND_PRIVACY.md` §6 |
| `EXTERNAL_PROCESSING_CONSENT.md` | Setup disclosure, the recorded choice, inspection, revocation, the five-step redaction sequence before any evidence leaves the process, and the guarantee that revocation pauses model-dependent jobs without stopping capture, redaction, embedding, indexing, search, or retention. | `DATA_AND_PRIVACY.md` §2.3; `D-014`, `D-024` |
| `PUBLISHER_AUTHENTICATION.md` | That a checksum is integrity evidence only and can never authenticate a publisher; the signature-or-attestation requirement against a pinned trusted identity; the full automatic-installation condition set; and rollback availability. | `D-018`, `D-071`; `COMPATIBILITY.md` §5 |

Each rule is `SC-<AREA>-NNN` (D-3), phrased normatively, and fails closed. Each contract closes with its change procedure: a frozen contract changes only through a decision-register entry plus a migration impact note, never through an edit in passing.

**What "frozen" means here.** These four fix *behavior that security depends on* — what may authorize an action, what must be refused, what must fail closed. They do not fix wire formats, field names, or schemas; those are Stage 2's freeze. Stage 1 may discover that a rule is unimplementable as written, and the change procedure is how that is handled — visibly, not silently.

### 4.3 `docs/TEST_FIXTURES.md` and `tests/fixtures/`

The fixture convention: where fixtures live, how they are named, what determinism requires of them, how one is added, and which are generated rather than committed (D-4).

The initial committed set is deliberately small — a fixture with no consumer is speculative, and Stage 0 has no product code to consume one. It covers only what Stage 1 provably needs on day one: a canonical neutral event-envelope sample, a canonical conversation-turn transcript, and a synthetic repository tree description for indexing. Each is accompanied by a statement of what it is for and which stage consumes it.

`tests/fixtures/MANIFEST.md` lists every committed fixture with its SHA-256 (D-5). Secret-detection corpora are specified in the document as generation rules and are explicitly absent from the manifest, with the reason stated inline so a later contributor does not "fix" the omission by committing one.

### 4.4 `docs/LAUNCH_CHECKLIST.md`

One row per Stage 8 release obligation — reproducible build, SBOM, checksums, signature or attestation, installer and upgrade and rollback and uninstall, migration, backup, recovery, transfer, privacy documentation, evaluation results with limitations stated plainly, clean issue tracker, published compatibility boundaries — each naming the evidence that discharges it and the command or artifact that produces it (D-6). Items that cannot yet name their evidence are listed with the deferred decision (`F-*`) that will supply it, rather than omitted.

### 4.5 `docs/operations/STAGE0_EXIT.md`

One row per clause of the Stage 0 exit gate, with evidence and verification command (D-8). Any clause not fully discharged is recorded as not discharged, with what remains — an exit record that cannot record a failure is not evidence.

### 4.6 Executable guards in `tests/OpenMemory.Contracts.Tests/`

Test-only code, following Wave B's proven doc-agreement pattern:

| Test | Fails when |
|---|---|
| Fixture manifest agreement | A committed fixture's SHA-256 differs from `MANIFEST.md`, a manifest entry names a missing file, or a fixture file is absent from the manifest. |
| Contract rule identifiers | Two rules share an ID, an ID does not match its document's area prefix, or a contract document declares no version or freeze status. |
| Threat-model coverage | A `DATA_AND_PRIVACY.md` §12 verification class maps to no `THR-NNN`, or a threat cites a contract rule ID that does not exist. |

Each is written to fail first against the tree as it stands, then to pass. The third is the one that keeps the wave's three documents mutually honest as later stages edit them.

### 4.7 Record edits

- `IDENTIFIERS.md` §1 gains `THR-NNN` and `SC-<AREA>-NNN`.
- `DECISION_REGISTER.md` gains entries from `D-092` for the decisions in §3 that outlive this wave (D-2, D-3, D-4, D-6), and `F-010` is marked discharged with a link to the threat model.
- `DATA_AND_PRIVACY.md` §12's closing sentence — "will be written during Stage 0" — becomes a link to the written document.
- `AGENTS.md`'s status paragraph is corrected (D-7).
- `COMPATIBILITY.md` is **not** edited (D-2).

## 5. Verification

Each check runs before its change to observe it fail first, where that is possible.

| Check | How | Fails before because |
|---|---|---|
| Fixture manifest agreement | `dotnet test`; then corrupt one fixture byte, observe failure, restore | No manifest and no fixtures exist |
| Rule identifiers unique and well-formed | `dotnet test`; then duplicate one ID, observe failure, revert | No contract documents exist |
| Threat-model coverage of §12 | `dotnet test`; then delete one threat entry, observe failure, revert | No threat model exists |
| Discovered-test count still above Wave C's floor | The `build-and-test` job's C-5 assertion | New tests raise the count; the floor is raised with them |
| Every internal link resolves | `bash tools/check-links.sh` | New documents are cross-linked before they exist |
| No secret-shaped content | Wave C's `secret-scan` job, plus a local `git diff` grep | — |
| Every commit DCO-signed | `git log` sign-off scan over `main..HEAD` | — |
| All six CI jobs green on the branch tip | Actions run whose `headSha` matches local `HEAD` | — |
| Independent specification and security review | Independent review over the branch diff, verified technically per `receiving-code-review` | — |

The fixture and coverage tests are the wave's real acceptance evidence. Prose can be reviewed but not executed; these three tests are what stop the prose from rotting after review.

## 6. Out of scope

- Any product behavior. No `src/` file gains logic.
- Implementing any of the four contracts. Stage 0 freezes what they must do; Stage 1 proves it can be done and Stage 2 onward implements it.
- Wire formats, field names, and schemas for the contracts — Stage 2's freeze (D-2).
- Applying `REQ-<AREA>-NNN` to the existing requirement text, still the separate later task Wave A deferred.
- Code signing and attestation selection — `F-007`, Stage 8, explicit approval required before any paid service. The contract fixes the *requirement*; the provider choice stays deferred.
- Secret-corpus fixture files (D-4). The generation rules are in scope; committed corpora never are.
- Stage 1's plans and specs. Stage 0 closes with this wave; Stage 1 is scoped separately.

## 7. Risks

| Risk | Mitigation |
|---|---|
| A threat model written before any product code exists describes intentions and reads as complete, becoming a compliance artifact nobody consults. | D-1's live-or-planned labelling makes the emptiness visible rather than hidden: today exactly one threat class is live, and the document says so. The coverage test then keeps it bound to `DATA_AND_PRIVACY.md` §12 as that document changes. |
| Freezing four contracts before a single line of the subsystem exists may freeze something unimplementable. | Each contract carries an explicit change procedure, and D-2 confines the freeze to behavior rather than wire shape. A rule that Stage 1 proves unimplementable changes through a recorded decision — the freeze makes that visible, which is the point of freezing. |
| Fixtures with no consumer are speculative work that must be rewritten when a consumer appears. | §4.3 keeps the committed set to three, each named with the stage that consumes it. The durable deliverable is the convention and the manifest guard, not the fixture count. |
| The three new tests parse Markdown, so an innocuous formatting change to a document can fail the build. | Accepted deliberately, and the same tradeoff Wave B already accepted with `ContractVersionsTests`. Parsers key on stable table and heading structure, and each failure message names the document and the expected shape so the fix is obvious rather than archaeological. |
| Correcting `AGENTS.md`'s status paragraph loosens a constraint that has been protecting `main` from premature application code. | The replacement is narrower, not absent: it states what Stage 0 delivered and that Stage 1 work still requires an approved spec and plan. The constraint that matters — no product behavior without an approved plan — is restated, not dropped. |
