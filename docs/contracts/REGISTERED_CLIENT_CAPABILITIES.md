# Registered Client Capabilities

- **Contract:** registered client capabilities
- **Version:** 1
- **Status:** Frozen (Stage 0)
- **Rule prefix:** `SC-CAP-`

> **Status note:** nothing implements this contract yet. It fixes required behavior before implementation, per the Stage 0 exit gate.

## 1. What this contract governs

This contract governs the client-to-bridge and bridge-to-service boundaries described in [Architecture](../ARCHITECTURE.md#security-and-trust-boundaries): who may connect to the named pipe, what a registered client is thereby permitted to ask for, and — the point this contract exists to make unambiguous — what registration and same-user pipe access do **not** establish.

Registration is an authorization decision. It answers "may this caller make this bounded request?" It never answers "did a human ask for this?" That second question belongs entirely to [Trusted Human Confirmation](TRUSTED_HUMAN_CONFIRMATION.md), and no amount of client registration substitutes for it.

## 2. What freezing does and does not fix

Frozen here: what registration establishes, what a capability set bounds, how an out-of-range or unauthorized request is treated, how capability expansion may occur, and what happens when the authorization check itself cannot run.

Not frozen here: the named-pipe envelope's wire format, its field names, its schema, the protocol version integers, the concrete capability names, the numeric message-size and result-count limits, and any timeout value. Those are wire and storage surfaces whose integers [Compatibility](../COMPATIBILITY.md) §6 leaves unfrozen until Stage 2, and choosing a limit now would freeze a number picked without measurement.

Also not fixed here: the mechanism by which a client proves its registered identity. This contract fixes that an unregistered caller is refused, not how identity is demonstrated.

## 3. Rules

| ID | Rule | Fails closed by | Source |
|---|---|---|---|
| `SC-CAP-001` | The service must refuse a connection whose client is not registered. Refusal must occur before the request payload is read, and the refusal must be recorded as a security event. An unregistered caller must not receive a partial result, a degraded result, or a default capability set. | Refusing the connection before reading a payload. | [Architecture §Security and trust boundaries](../ARCHITECTURE.md#security-and-trust-boundaries), [Architecture §Expected failure modes](../ARCHITECTURE.md#expected-failure-modes) |
| `SC-CAP-002` | Registration must bind a client to a named capability set. A request that falls outside that set must be refused with an error. It must not be silently narrowed, downgraded, partially served, or answered from a reduced scope, because a silently narrowed request returns a result the caller will treat as complete. | Refusing the out-of-range request rather than serving a narrowed one. | [Architecture §Security and trust boundaries](../ARCHITECTURE.md#security-and-trust-boundaries), [Data and privacy §6](../DATA_AND_PRIVACY.md#6-authority-conflicts-and-change-history) |
| `SC-CAP-003` | Same-user named-pipe access and successful client registration authorize bounded requests only. Neither may be treated as evidence of human intent, human presence, or human approval, and neither may satisfy any requirement that a protected action carry a trusted human confirmation under `SC-CONF-002`. | Treating the absence of a confirmation as absence of approval. | [Architecture §Security and trust boundaries](../ARCHITECTURE.md#security-and-trust-boundaries), [Data and privacy §6](../DATA_AND_PRIVACY.md#6-authority-conflicts-and-change-history) |
| `SC-CAP-003` | No request may widen the capability set of its own client, directly or as a side effect. Expansion of capabilities, permissions, or authority must be an out-of-band change that is separately approved; a request asking for such expansion must be refused, not queued for implicit grant. Approving a capability or permission expansion is a protected action under `SC-CONF-001`. | Refusing the self-expanding request. | [Data and privacy §6](../DATA_AND_PRIVACY.md#6-authority-conflicts-and-change-history), [Data and privacy §11](../DATA_AND_PRIVACY.md#11-updates-repair-and-failure-behavior), [Architecture §MCP interface contract](../ARCHITECTURE.md#mcp-interface-contract) |
| `SC-CAP-005` | Registration, authorization refusal, and attempted capability escalation must each append an audit record identifying the client, the capability requested, and the outcome. The audit record must not copy the evidence content, the returned private text, or any detected secret value; it may carry record identifiers and categories only. | Refusing to proceed when the audit record cannot be written (`SC-CAP-008`). | [Architecture §Security and trust boundaries](../ARCHITECTURE.md#security-and-trust-boundaries), [Data and privacy §4](../DATA_AND_PRIVACY.md#4-secret-protection), [Data and privacy §7](../DATA_AND_PRIVACY.md#7-retrieval-privacy) |
| `SC-CAP-006` | A request carrying an unsupported protocol or envelope version must be refused with a version-mismatch error that names the supported range. A refused version must never be handled by best-effort parsing, field guessing, or tolerant decoding. | Refusing the message rather than parsing it. | [Architecture §Security and trust boundaries](../ARCHITECTURE.md#security-and-trust-boundaries), [Compatibility §3](../COMPATIBILITY.md#3-support-windows) |
| `SC-CAP-007` | Message size and result count must be bounded, and both bounds must be enforced before the request is dispatched to the service. A request exceeding a bound must be refused; a result set exceeding a bound must be truncated only in a way the response declares, never silently. | Refusing before dispatch. | [Architecture §Security and trust boundaries](../ARCHITECTURE.md#security-and-trust-boundaries), [Architecture §MCP interface contract](../ARCHITECTURE.md#mcp-interface-contract) |
| `SC-CAP-008` | When the capability lookup, the authorization check, the schema validation, or the audit write cannot run, the request must be refused. An unavailable check must never be treated as a passing check, and the request must not be retried under a relaxed policy. | Refusing on an unavailable check. | [Architecture §Security and trust boundaries](../ARCHITECTURE.md#security-and-trust-boundaries) |

## 4. Failure behavior

Every way this contract's checks can be unavailable resolves to refusal, per `SC-CAP-008`:

- **Client registry unreadable or not yet loaded.** The connection is refused under `SC-CAP-001`; no default or provisional registration exists.
- **Capability set unresolvable for a registered client.** The request is refused under `SC-CAP-002`; an unresolved set is not an empty set that permits reads.
- **Schema validation unavailable.** The request is refused under `SC-CAP-006` rather than parsed on a best-effort basis.
- **Bound values unavailable.** The request is refused under `SC-CAP-007`; there is no unbounded fallback.
- **Audit store unwritable.** The request is refused under `SC-CAP-005` and `SC-CAP-008`, because an unauditable authorization decision is not a permitted one.
- **Trusted confirmation channel unavailable.** Out of scope here; that case is governed by `SC-CONF-008`. Registration never fills the gap (`SC-CAP-003`).

## 5. Verification owed

This contract owes evidence for these verification classes in [Data and privacy §12](../DATA_AND_PRIVACY.md#12-required-security-verification):

- named-pipe client authorization and local impersonation attempts;
- trusted-human confirmation tests against model-generated requests, replay, expired confirmations, capability escalation, and same-user spoofing — specifically the capability-escalation and same-user-spoofing parts, which `SC-CAP-003` and `SC-CAP-004` bound;
- project isolation and cross-project leakage tests, to the extent isolation depends on the capability and authorization checks this contract governs.

The threat model records which `THR-NNN` entries these rules govern. No test for any of these classes has been written.

## 6. Change procedure

This contract is frozen. A rule changes only through a new entry in the [decision register](../DECISION_REGISTER.md) plus a migration-impact note stating what already-written code, documentation, or test the change invalidates. A rule is never changed by an edit made in passing.

The **Version** integer above increments when a rule's meaning changes — including when a rule is withdrawn or its scope is narrowed or widened. Editorial clarification that does not change what is required or refused does not increment it. A withdrawn rule keeps its identifier and is marked withdrawn; identifiers are never reused or renumbered, because other documents cite them.

If Stage 1 finds a rule here unimplementable as written, that is handled through this procedure — visibly — rather than by quietly implementing something else.
