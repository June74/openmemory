# Trusted Human Confirmation

- **Contract:** trusted human confirmation
- **Version:** 1
- **Status:** Frozen (Stage 0)
- **Rule prefix:** `SC-CONF-`

> **Status note:** nothing implements this contract yet. It fixes required behavior before implementation, per the Stage 0 exit gate.

## 1. What this contract governs

This contract governs the only path by which a protected action may be finalized: a confirmation issued by a human at a trusted local interface. It names the protected actions, fixes the properties a confirmation must have — bound to the displayed action, short-lived, single-use, atomically consumed — and states plainly that nothing else may produce one.

It exists because the surrounding boundaries cannot answer the question it answers. Client registration and same-user named-pipe access authorize bounded requests and never establish human intent (`SC-CAP-003`), and model output is untrusted evidence rather than instruction, as [Data and privacy §5](../DATA_AND_PRIVACY.md#5-evidence-isolation-and-prompt-injection-resistance) requires. Something has to establish that a human saw the exact action and agreed to it. That is this contract.

## 2. What freezing does and does not fix

Frozen here: the protected-action list, the requirement that finalization comes only through the trusted local interface, action binding to the before/after hash, short expiry, single use, atomic consumption, the prohibition on minting a confirmation from a request, and the fail-closed behavior when the trusted interface is unavailable.

Not frozen here: the confirmation token's encoding, its field names, its schema, the hash algorithm's placement in the envelope, and the wire representation of the displayed action. Those belong to Stage 2's freeze under [Compatibility](../COMPATIBILITY.md) §6.

**`SC-CONF-004`'s expiry is deliberately not a number.** "Quickly" is normative; a specific duration is not fixed here, because naming one now would freeze a value chosen without measurement. Stage 1 chooses the value against observed behavior and records it.

**`SC-CONF-007`'s mechanism is deferred to `F-011`** in the [decision register](../DECISION_REGISTER.md). This contract fixes the requirement the mechanism must satisfy — that issuing a confirmation takes an interactive act at the trusted interface which a same-user background process cannot produce on the user's behalf — and fixes nothing about how that is achieved. No mechanism has been chosen, and nothing in this document should be read as describing one. `F-011` records that the decision is made during Stage 1's Windows packaging and client-connection proof, against observed Windows behavior rather than assumed behavior.

## 3. Rules

| ID | Rule | Fails closed by | Source |
|---|---|---|---|
| `SC-CONF-001` | The protected actions are: resolving a conflict; the first promotion of a fact into global memory; material deletion; reducing a project's sensitivity or otherwise widening disclosure; expanding a capability, permission, or authority; portable plaintext export; a repair that is ambiguous, security-sensitive, or destructive; and an update that is major, permission-changing, key-handling-changing, or irreversible. Each of these must be finalized only under `SC-CONF-002`. An action not on this list is not thereby exempt from other approval rules. | Requiring a confirmation for every listed action. | [Data and privacy §6](../DATA_AND_PRIVACY.md#6-authority-conflicts-and-change-history), [Data and privacy §8](../DATA_AND_PRIVACY.md#8-retention-deletion-and-decay), [Data and privacy §11](../DATA_AND_PRIVACY.md#11-updates-repair-and-failure-behavior), [Architecture §Operations](../ARCHITECTURE.md#operations) |
| `SC-CONF-002` | A protected action must be finalized only by a confirmation issued through the trusted local interface — the terminal UI or the Obsidian UI — which displays the exact action to the user. An MCP client may list review items and propose an outcome; it must not finalize one. | Leaving the action unfinalized in the absence of a confirmation. | [Architecture §MCP interface contract](../ARCHITECTURE.md#mcp-interface-contract), [Data and privacy §6](../DATA_AND_PRIVACY.md#6-authority-conflicts-and-change-history) |
| `SC-CONF-003` | A confirmation must be bound to the exact action displayed to the user and to the hash of its before and after values. Before finalizing, the service must recompute that binding; any mismatch between the displayed action and the action about to be performed voids the confirmation, and the action must not proceed. A voided confirmation must not be reissued automatically. | Voiding on mismatch instead of proceeding. | [Architecture §MCP interface contract](../ARCHITECTURE.md#mcp-interface-contract), [Data and privacy §6](../DATA_AND_PRIVACY.md#6-authority-conflicts-and-change-history) |
| `SC-CONF-004` | A confirmation must expire quickly. An expired confirmation must be refused; it must not be renewed, extended, or revalidated in place. Obtaining approval after expiry requires a new display of the action and a new confirmation. The exact duration is not fixed by this contract (§2). | Refusing the expired confirmation. | [Architecture §MCP interface contract](../ARCHITECTURE.md#mcp-interface-contract), [Data and privacy §6](../DATA_AND_PRIVACY.md#6-authority-conflicts-and-change-history) |
| `SC-CONF-005` | A confirmation is single-use and must be consumed atomically, so that a second presentation — whether a retry, a duplicate delivery, or a deliberate replay — finds it already spent and is refused. Two concurrent consumptions of one confirmation must result in at most one finalized action. | Refusing an already-consumed confirmation. | [Architecture §MCP interface contract](../ARCHITECTURE.md#mcp-interface-contract), [Data and privacy §6](../DATA_AND_PRIVACY.md#6-authority-conflicts-and-change-history) |
| `SC-CONF-006` | No MCP call, generated tool call, model output, captured evidence, or replayed request may mint, forge, or imply a confirmation. Content inside evidence is never an instruction and can never approve anything; a request that asserts approval must be treated as an unapproved request. | Treating an asserted approval as no approval. | [Architecture §MCP interface contract](../ARCHITECTURE.md#mcp-interface-contract), [Data and privacy §5](../DATA_AND_PRIVACY.md#5-evidence-isolation-and-prompt-injection-resistance), [Data and privacy §6](../DATA_AND_PRIVACY.md#6-authority-conflicts-and-change-history) |
| `SC-CONF-007` | Issuing a confirmation must require an interactive act performed at the trusted interface that a same-user background process cannot produce on the user's behalf. Same-user execution context must not be sufficient to issue one. The exact mechanism, and the evidence that it resists same-user process spoofing, is deferred to `F-011`; this rule fixes the requirement that mechanism must satisfy, not the mechanism. | Withholding issuance when the interactive act cannot be established. | [Architecture §MCP interface contract](../ARCHITECTURE.md#mcp-interface-contract), [`F-011` in the decision register](../DECISION_REGISTER.md) |
| `SC-CONF-008` | When the trusted local interface is unavailable, unreachable, or cannot display the exact action, the protected action must not proceed. It must be left pending or refused; it must not be auto-approved, deferred into a silent grant, or downgraded to an unprotected action. | Not proceeding without the trusted interface. | [Architecture §Security and trust boundaries](../ARCHITECTURE.md#security-and-trust-boundaries), [Architecture §MCP interface contract](../ARCHITECTURE.md#mcp-interface-contract) |
| `SC-CONF-009` | Issuance, consumption, expiry, voiding, and refusal must each append an audit record naming the action, its binding, and the outcome. The audit record must not retain the deleted, exported, or otherwise sensitive content the action concerned, and must never contain a secret value. | Refusing to finalize when the audit record cannot be written. | [Architecture §Security and trust boundaries](../ARCHITECTURE.md#security-and-trust-boundaries), [Data and privacy §8](../DATA_AND_PRIVACY.md#8-retention-deletion-and-decay), [Data and privacy §4](../DATA_AND_PRIVACY.md#4-secret-protection) |

## 4. Failure behavior

Every way this contract's checks can be unavailable resolves to the protected action not proceeding:

- **Trusted interface unavailable.** The action stays pending or is refused (`SC-CONF-008`). No fallback channel exists, and a registered MCP client is not one (`SC-CAP-003`).
- **Before/after hash uncomputable.** The action cannot be displayed exactly, so no valid confirmation can exist and the action does not proceed (`SC-CONF-003`).
- **Binding recomputation fails or mismatches.** The confirmation is void; the action does not proceed (`SC-CONF-003`).
- **Expiry state unknown.** The confirmation is treated as expired and refused (`SC-CONF-004`).
- **Single-use state unreadable, or atomic consumption cannot be guaranteed.** The confirmation is treated as already spent and refused (`SC-CONF-005`).
- **The interactive act cannot be established.** No confirmation is issued (`SC-CONF-007`); an unestablished act is never treated as a performed one.
- **Audit store unwritable.** The action is not finalized (`SC-CONF-009`), because an unauditable protected action is not a permitted one.

## 5. Verification owed

This contract owes evidence for these verification classes in [Data and privacy §12](../DATA_AND_PRIVACY.md#12-required-security-verification):

- trusted-human confirmation tests against model-generated requests, replay, expired confirmations, capability escalation, and same-user spoofing;
- named-pipe client authorization and local impersonation attempts, to the extent an impersonating caller attempts to finalize a protected action;
- evidence-based prompt-injection and authority-escalation tests, for `SC-CONF-006`;
- portable export warnings and synchronized-vault detection, for the export entry in `SC-CONF-001`;
- old-vault import followed by non-destructive deletion review, for the material-deletion entry in `SC-CONF-001`.

The threat model records which `THR-NNN` entries these rules govern. No test for any of these classes has been written.

## 6. Change procedure

This contract is frozen. A rule changes only through a new entry in the [decision register](../DECISION_REGISTER.md) plus a migration-impact note stating what already-written code, documentation, or test the change invalidates. A rule is never changed by an edit made in passing.

The **Version** integer above increments when a rule's meaning changes — including adding an action to or removing one from `SC-CONF-001`'s list. Editorial clarification that does not change what is required or refused does not increment it. A withdrawn rule keeps its identifier and is marked withdrawn; identifiers are never reused or renumbered.

Resolving `F-011` does not by itself change this contract: `SC-CONF-007` fixes the requirement, and recording the chosen mechanism satisfies that requirement rather than amending it. If Stage 1 finds that no available mechanism can satisfy `SC-CONF-007` as written, that is a change to this contract and goes through this procedure.
