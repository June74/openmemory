# External Processing Consent and Revocation

- **Contract:** external processing consent and revocation
- **Version:** 1
- **Status:** Frozen (Stage 0)
- **Rule prefix:** `SC-CONSENT-`

> **Status note:** nothing implements this contract yet. It fixes required behavior before implementation, per the Stage 0 exit gate.

## 1. What this contract governs

This contract governs the model-processing boundary described in [Architecture](../ARCHITECTURE.md#security-and-trust-boundaries): the only boundary across which captured evidence may leave the local OpenMemory process. It fixes what must be true before recurring external processing runs at all, what the user must be told, what must happen to evidence before any of it leaves, what revocation does, and what revocation must never do.

Two positions it exists to hold fixed. First, consent is an opt-in that starts off, not a default that can be assumed from installation. Second, revocation pauses model-dependent work and nothing else — a user who revokes consent does not thereby lose local capture, search, or their retained evidence.

## 2. What freezing does and does not fix

Frozen here: the off-by-default position, the disclosure the opt-in must make, the inspectability and immediate effect of revocation, the scope revocation must not touch, the ordered redaction sequence that precedes any outbound evidence, the prohibition on provider substitution, and the fail-closed behavior when secret detection cannot run.

Not frozen here: the job schema, the request encoding, the placeholder syntax used for redacted values, the audit record's field names, the concrete secret-detection patterns and entropy thresholds, the setup screen's wording, and any timeout or retry interval. Detection patterns in particular must be able to improve without a contract change; what is frozen is that detection runs and that nothing is sent when it cannot.

Also not fixed: the local embedding model's identity. `D-014` in the [decision register](../DECISION_REGISTER.md) records that the local model is a small offline embedding model and explicitly not a local generative model, which is why `SC-CONSENT-007` prohibits falling back to one.

## 3. Rules

| ID | Rule | Fails closed by | Source |
|---|---|---|---|
| `SC-CONSENT-001` | Recurring external processing must be off until an explicit setup opt-in records the user's choice. Absence of a recorded choice is not consent, and installation, client registration, or first use must not be treated as consent. No evidence may leave the local process while consent is absent. | Treating an unrecorded choice as refusal. | [Data and privacy §2.3](../DATA_AND_PRIVACY.md#23-local-and-external-computation), [`D-024` in the decision register](../DECISION_REGISTER.md) |
| `SC-CONSENT-002` | The opt-in must disclose what leaves the local process (selected redacted context), through whose account it leaves (the user's own Codex account), and for what purpose (extraction, summarization, reflection, and quality analysis). A choice recorded without that disclosure is not a valid opt-in. | Not enabling processing on an undisclosed opt-in. | [Data and privacy §2.3](../DATA_AND_PRIVACY.md#23-local-and-external-computation), [`D-024` in the decision register](../DECISION_REGISTER.md) |
| `SC-CONSENT-003` | The recorded consent state must be inspectable by the user at any time, and revocable at any time, without requiring the external provider to be reachable. | Reporting consent state from local records only. | [Data and privacy §2.3](../DATA_AND_PRIVACY.md#23-local-and-external-computation) |
| `SC-CONSENT-004` | Revocation must take effect immediately. Model-dependent jobs must be paused — in-flight work is not permitted to complete on the strength of consent that has been withdrawn — and their captured evidence must be retained, not discarded. A paused job resumes only after a new opt-in under `SC-CONSENT-001`. | Pausing rather than completing in-flight work. | [Data and privacy §2.3](../DATA_AND_PRIVACY.md#23-local-and-external-computation), [Architecture §Processing and memory authority](../ARCHITECTURE.md#processing-and-memory-authority), [`D-024` in the decision register](../DECISION_REGISTER.md) |
| `SC-CONSENT-005` | Revocation must not stop automatic capture, secret scanning and redaction, local embedding, indexing, search, retrieval, or evidence retention. Those are local operations and do not depend on consent. Degrading any of them on revocation is a defect, not a safety measure. | Keeping local operation independent of consent state. | [Data and privacy §2.3](../DATA_AND_PRIVACY.md#23-local-and-external-computation), [Architecture §Processing and memory authority](../ARCHITECTURE.md#processing-and-memory-authority) |
| `SC-CONSENT-006` | Before any evidence leaves the process, the service must, in order: select the smallest evidence slice sufficient for the job; run secret detection and quarantine uncertain material; replace every detected value with a typed placeholder; and record in the audit which categories and record identifiers were sent. The audit must never copy a secret value, and no step may be skipped or reordered. | Sending nothing until every step has completed. | [Data and privacy §2.3](../DATA_AND_PRIVACY.md#23-local-and-external-computation), [Data and privacy §4](../DATA_AND_PRIVACY.md#4-secret-protection) |
| `SC-CONSENT-007` | Authentication failure or exhaustion of the user's subscription allowance must pause the affected job. Substituting a different external provider, a different account, or a local generative model is prohibited; the user's disclosed choice under `SC-CONSENT-002` is a choice of that provider through that account. Local capture and retrieval continue while jobs are paused. | Pausing rather than substituting. | [Data and privacy §2.3](../DATA_AND_PRIVACY.md#23-local-and-external-computation), [`D-024` in the decision register](../DECISION_REGISTER.md), [`D-014` in the decision register](../DECISION_REGISTER.md), [Architecture §Expected failure modes](../ARCHITECTURE.md#expected-failure-modes) |
| `SC-CONSENT-008` | When secret detection cannot run, is degraded, or returns an uncertain result, nothing may be sent. The affected material stays quarantined or queued. An unavailable scan must never be treated as a clean scan. | Sending nothing when detection is unavailable. | [Data and privacy §4](../DATA_AND_PRIVACY.md#4-secret-protection), [Architecture §Security and trust boundaries](../ARCHITECTURE.md#security-and-trust-boundaries) |
| `SC-CONSENT-009` | No telemetry may be collected or transmitted. The only permitted unsolicited outbound network use is the update check, and it must disclose only what querying a release endpoint technically requires; it must not carry usage data, evidence, memory content, or identifiers beyond that technical necessity. | Sending no outbound request that is not consented processing or an update check. | [Data and privacy §2.3](../DATA_AND_PRIVACY.md#23-local-and-external-computation), [Architecture §Security and trust boundaries](../ARCHITECTURE.md#security-and-trust-boundaries) |

## 4. Failure behavior

Every way this contract's checks can be unavailable resolves to sending nothing:

- **Consent record unreadable.** Treated as no consent; processing does not run (`SC-CONSENT-001`).
- **Disclosure cannot be shown at setup.** No valid opt-in can be recorded, so processing stays off (`SC-CONSENT-002`).
- **Consent state cannot be displayed.** Inspection reports the failure; it must not report "enabled" or "disabled" by guess (`SC-CONSENT-003`).
- **Revocation cannot be propagated to an in-flight job.** The job is paused and its result discarded rather than accepted (`SC-CONSENT-004`).
- **Secret detection unavailable, degraded, or uncertain.** Nothing is sent; the material is quarantined (`SC-CONSENT-006`, `SC-CONSENT-008`).
- **Audit store unwritable.** The send does not occur, because `SC-CONSENT-006` requires the record of what was sent.
- **Provider unauthenticated or allowance exhausted.** The job pauses; no substitute provider or local generative model is used (`SC-CONSENT-007`). Local capture, indexing, and retrieval continue (`SC-CONSENT-005`).

## 5. Verification owed

This contract owes evidence for these verification classes in [Data and privacy §12](../DATA_AND_PRIVACY.md#12-required-security-verification):

- external-processing setup consent, disclosure, revocation, and local-only continuity tests;
- secret-corpus boundary tests through every ingest and output route, for the outbound route `SC-CONSENT-006` governs;
- evidence-based prompt-injection and authority-escalation tests, to the extent evidence selected for a job carries injected content;
- project isolation and cross-project leakage tests, to the extent the selected evidence slice must respect project sensitivity.

The threat model records which `THR-NNN` entries these rules govern. No test for any of these classes has been written.

## 6. Change procedure

This contract is frozen. A rule changes only through a new entry in the [decision register](../DECISION_REGISTER.md) plus a migration-impact note stating what already-written code, documentation, or test the change invalidates. A rule is never changed by an edit made in passing.

The **Version** integer above increments when a rule's meaning changes — including any change to what may leave the local process or to what revocation stops. Improving a secret-detection pattern, rewording the setup disclosure without changing what it discloses, or changing a placeholder's syntax does not change a rule's meaning and does not increment it. A withdrawn rule keeps its identifier and is marked withdrawn; identifiers are never reused or renumbered.
