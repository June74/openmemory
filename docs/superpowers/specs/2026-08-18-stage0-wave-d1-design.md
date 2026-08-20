# Stage 0 Wave D1 — Repository-grounded threat model (design)

- **Date:** 2026-08-18
- **Stage:** 0 (Program foundation)
- **Wave:** D1 of two (Wave D splits into D1 threat model, D2 contracts and fixtures)
- **Owner:** Root integrator
- **Branch:** `codex/stage0-wave-d1`
- **Status:** Awaiting user review

## 1. Why this wave exists

Waves A, B, and C established governance, the solution structure, and enforced CI. Three Stage 0 bullets remain:

> - "create a repository-grounded threat model before security-sensitive implementation"
> - "freeze registered-client capabilities, trusted-human confirmation, consent/revocation, and publisher-authentication contracts"
> - "define deterministic test fixtures and the launch checklist"

Wave D is split. **D1 delivers the threat model alone.** D2 delivers the four contracts, the fixtures, and the launch checklist.

The split exists because of ordering, not size. The threat model enumerates what the contracts must resist; designing the contracts first would mean designing against threats nobody has listed. `D-049` makes this concrete — it requires Stage 0 to "prove resistance to replay and local spoofing", which presumes those threats have been named. [DATA_AND_PRIVACY.md](../../DATA_AND_PRIVACY.md) §12 already gestures at the list — replay, expired confirmations, capability escalation, same-user spoofing — and the threat model is where it becomes rigorous.

## 2. Current state

Verified 2026-08-18 against `main` at `8669442`.

**What is built and executing.** A CI workflow with six jobs, running six third-party actions with the default `GITHUB_TOKEN`; gitleaks `v8.30.1` and Syft `v1.51.0` downloaded at run time from external hosts; an artifact path that publishes three executables, archives, checksums, and uploads them; Dependabot watching the `github-actions` ecosystem; branch protection enforced on `main` with six required contexts, strict mode, and administrator enforcement.

**What is not built.** Everything the product does. Wave B produced boundaries rather than behavior: three empty `Main` methods and one constants file — 75 tracked lines of production C# against 200 lines of tests. There is no named pipe, no database, no MCP handler, no adapter, and no secret scanner inside the product itself.

**The consequence for this wave.** `F-010` defers the threat model until "the repository structure and executable trust boundaries exist". That is now half true. The **CI** trust boundaries exist and execute. The **product** trust boundaries described in [ARCHITECTURE.md](../../ARCHITECTURE.md) exist only as approved design.

## 3. Decisions taken during brainstorming

| Ref | Decision | Rationale |
|---|---|---|
| D1-1 | Wave D splits: D1 is the threat model alone; D2 is contracts, fixtures, and the launch checklist. | The threat model enumerates what the contracts must resist. Reversing the order means designing security contracts against assumed rather than enumerated threats. |
| D1-2 | The threat model covers **both** the built CI/supply-chain surface and the designed product architecture, in two explicitly separated parts. | Modelling only what is built would omit everything Stage 2's security-sensitive implementation needs, which is the reason Stage 0 asks for a threat model at all. Modelling only the design would ignore the one surface actually running today. |
| D1-3 | Every claim is marked as evidenced by **code** or by **approved design**, and Part B is labelled *designed, not implemented* throughout. | The `security-threat-model` skill forbids claiming components, flows, or controls without evidence. Design documents evidence intent, not behavior, and conflating the two is the exact defect independent review has already caught twice in this project. |
| D1-4 | Attacker **non-capabilities** are stated explicitly alongside capabilities. | The skill requires this to prevent inflated severity. `D-015` specifies no listening network port and `D-002` specifies single-user, so a remote network attacker and cross-tenant access are out of scope by design rather than by oversight. |
| D1-5 | Two carried debts are absorbed: SHA-pinning the GitHub Actions, and Wave B's `ContractsReferencesNothing` transitive-reference guard. | Both are supply-chain and integrity controls that Part A would recommend anyway. Implementing them inside the wave that identifies them is cheaper than filing them for later. |
| D1-6 | The threat model is written to `docs/security/openmemory-threat-model.md`. | The skill prescribes the basename `<repo-name>-threat-model.md`. The `docs/` placement follows this repository's existing convention rather than dropping a file at the root. |

## 4. Deliverables

### 4.1 `docs/security/openmemory-threat-model.md`

Follows the `security-threat-model` skill's output contract.

**System model and scope.** What is in scope, what is excluded, and the runtime-versus-tooling separation the skill requires. The built/designed distinction is established once here so every later section can lean on it.

**Part A — built and executing.** Every claim cites a file and line:

- CI executing six third-party actions with repository token access;
- gitleaks and Syft binaries fetched at run time from external hosts;
- the publish, archive, checksum, and upload path, and what a compromised runner could substitute into it;
- branch protection as an access control, including what it does and does not prevent;
- the project reference graph as an integrity constraint;
- Dependabot as a change vector into the workflow files.

**Part B — approved design, not implemented.** Every claim cites `ARCHITECTURE.md` or `DATA_AND_PRIVACY.md` by line and is marked *designed, not implemented*. Covers the six trust boundaries `ARCHITECTURE.md` enumerates: client to bridge, bridge to service, input to persistence, model processing, database to vault, and local system to network.

**Assets.** State whose compromise matters: the SQLCipher database and its key material, the recovery key, raw evidence, private provenance, the audit log, build artifacts, and the repository itself.

**Attacker capabilities and non-capabilities.** In scope: a compromised action or dependency; malicious content inside captured transcripts, since `D-027` treats all captured material as untrusted evidence; another process running as the same Windows user, which is `D-049`'s stated concern; someone with repository write access. Explicitly **not** in scope: a remote network attacker, since `D-015` specifies no listening port; and cross-user or cross-tenant access, since `D-002` specifies single-user.

**Threats as abuse paths**, each tied to an asset and a boundary, with qualitative likelihood and impact and a short justification for each rating.

**Mitigations**, separating those that already exist with evidence from those that are recommended, each tied to a concrete component or boundary.

**Assumptions and open questions**, stated explicitly.

### 4.2 SHA-pinning the GitHub Actions

Every `uses:` in `ci.yml` moves from a mutable major tag to a full commit SHA, with the human-readable version retained in a trailing comment. A tag can be moved by its author to point at different code; a commit SHA cannot. This is the hardening the Wave C spec explicitly deferred to the threat model.

**An interaction that must be preserved:** Dependabot understands SHA pins carrying version comments and continues to open update pull requests against them. Pinning must not break the mechanism that keeps the pins current, or it trades a staleness problem for an opacity problem.

### 4.3 `ContractsReferencesNothing` transitive guard

Wave B's architecture tests assert each project's **direct** references. If `OpenMemory.Contracts` ever gained a reference to a future project that itself reached `Storage`, then `McpBridge` would acquire a transitive path to storage and no test would fail. An assertion that `Contracts` declares zero references forecloses that.

Wave B's own re-review identified this gap and deferred it.

## 5. Verification

A threat model is prose, so verification is about grounding rather than execution.

| Check | How |
|---|---|
| Every Part A claim cites a real file and line | Extract cited paths, confirm each exists and that the line supports the claim |
| Every Part B claim cites an approved document and is marked designed-not-implemented | Extract citations; confirm no Part B claim is stated in the present tense as built |
| Every trust boundary in `ARCHITECTURE.md` appears in the model | Six are enumerated there; confirm six are covered |
| Every §12 verification item maps to a threat | `DATA_AND_PRIVACY.md` §12 lists twelve; confirm coverage or state why an item is out of scope |
| SHA pins are real commit SHAs and CI still passes | Workflow run green after pinning |
| Dependabot still recognises the pinned actions | Configuration inspected and behavior confirmed |
| The new architecture test can actually fail | Temporarily add a reference to `Contracts`, observe failure, revert |
| Links | `bash tools/check-links.sh` |
| Independent review | `codex exec` over the branch diff |

**A checkpoint this wave carries that earlier waves did not.** The skill requires: "Summarize key assumptions that materially affect threat ranking or scope, then ask the user to confirm or correct them. Pause and wait for user feedback before producing the final report." Implementation therefore stops mid-wave for the repository owner's input on assumptions. This is a designed interruption, not a failure.

## 6. Out of scope

- The four frozen security contracts — Wave D2, designed against this model's findings.
- Deterministic test fixtures and the launch checklist — Wave D2.
- Any product behavior. The Stage 0 exit gate's "no product capability is claimed" still binds.
- Penetration testing or dynamic analysis. There is nothing running to test.
- Remediating every threat this model identifies. The model enumerates and prioritises; which mitigations are adopted, deferred, or accepted is a separate decision for the repository owner.

## 7. Risks

| Risk | Mitigation |
|---|---|
| Part B models a system that does not exist, so its threats may not survive contact with real code. | Every Part B claim is marked designed-not-implemented, and §5 checks that none is stated as built. The model is explicitly a Stage 2 input, not a completion certificate. |
| A threat model can become a checklist nobody reads. | The skill's guidance is followed: few high-quality threats tied to real assets and boundaries, rather than exhaustive generic coverage. |
| SHA-pinning could break Dependabot's ability to keep actions current, trading staleness for opacity. | §5 verifies Dependabot still recognises the pinned form. If it does not, the pin is reconsidered rather than the update mechanism abandoned. |
| Absorbing two debts widens a wave whose main deliverable is a document. | Both are small, both are controls this model would recommend anyway, and both carry verification steps that prove they work rather than merely exist. |
