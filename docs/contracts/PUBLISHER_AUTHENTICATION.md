# Publisher Authentication

- **Contract:** publisher authentication
- **Version:** 1
- **Status:** Frozen (Stage 0)
- **Rule prefix:** `SC-PUB-`

> **Status note:** nothing implements this contract yet. It fixes required behavior before implementation, per the Stage 0 exit gate.

## 1. What this contract governs

This contract governs the local-system-to-network boundary for updates described in [Architecture](../ARCHITECTURE.md#security-and-trust-boundaries): what may be installed without asking the user, what evidence authenticates the publisher of an artifact, and what must remain true — backup, validated migrations, health checks, rollback — before an automatic installation is permitted at all.

Its central distinction is between *integrity* and *identity*. A checksum delivered beside an artifact proves the bytes were not corrupted in transit by an adversary who did not also control the checksum. It proves nothing about who published them. Treating a same-channel checksum as authentication is the specific mistake this contract forbids.

The signing provider is **not** chosen. `F-007` in the [decision register](../DECISION_REGISTER.md) defers the exact free or paid signing provider and attestation implementation, pending the threat model, the available open-source signing programs, and explicit approval before any paid service. `SC-PUB-002` fixes the *requirement* — a signature or signed attestation verifying against a pinned trusted project identity — and deliberately does not fix the provider that satisfies it. `D-018` records that the staged signing policy itself remains fixed while `F-007` is open, and that unsigned development builds are manual-install only.

## 2. What freezing does and does not fix

Frozen here: that a checksum never authenticates, that automatic installation requires publisher authentication against a pinned identity, how that pinned identity may change, the full condition set for automatic installation, what a failed verification does, how long rollback must remain available, and that the update check is the only unsolicited outbound network use.

Not frozen here: the signing provider and attestation format (`F-007`), the release-endpoint URL and its request shape, the artifact and manifest layout, the checksum algorithm's placement, the health checks' concrete contents, and any interval or timeout — including how often the update check runs. Those are implementation and Stage 2 concerns, and this contract's rules must hold whatever choices are made there.

Not fixed either: the contract integers this rule set refers to. [Compatibility](../COMPATIBILITY.md) §6 leaves every integer in its §1 unfrozen until Stage 2. `SC-PUB-004`'s condition is that no such integer increases, whatever their frozen values eventually are.

## 3. Rules

| ID | Rule | Fails closed by | Source |
|---|---|---|---|
| `SC-PUB-001` | A checksum is integrity evidence only. It must never be treated as authenticating a publisher, and a checksum delivered through the same channel as the artifact must never satisfy `SC-PUB-002`. A matching checksum on an unauthenticated artifact does not permit automatic installation. | Requiring publisher authentication separately from integrity. | [Data and privacy §11](../DATA_AND_PRIVACY.md#11-updates-repair-and-failure-behavior), [`D-018` in the decision register](../DECISION_REGISTER.md), [Architecture §Operations](../ARCHITECTURE.md#operations) |
| `SC-PUB-002` | Automatic installation requires a cryptographic signature or signed attestation that verifies against the pinned trusted project identity. Verification must succeed before installation begins; an absent, malformed, unverifiable, or unpinned-identity signature must be treated as failed verification under `SC-PUB-006`. The provider and attestation implementation are deferred to `F-007`; this rule fixes the requirement, not the provider. | Treating unverified as failed. | [`D-018` in the decision register](../DECISION_REGISTER.md), [`D-071` in the decision register](../DECISION_REGISTER.md), [Data and privacy §11](../DATA_AND_PRIVACY.md#11-updates-repair-and-failure-behavior) |
| `SC-PUB-003` | The pinned trusted project identity changes only through an approved, recorded rotation. A rotation must not be accepted from the update channel itself, must not be inferred from a newly presented signature, and — because it is a change to key handling — requires explicit approval as a protected action under `SC-CONF-001`. | Refusing an unapproved identity change. | [Data and privacy §11](../DATA_AND_PRIVACY.md#11-updates-repair-and-failure-behavior), [Compatibility §5](../COMPATIBILITY.md#5-relationship-to-automatic-updates), [`D-018` in the decision register](../DECISION_REGISTER.md) |
| `SC-PUB-004` | Beyond `SC-PUB-002`, automatic installation requires **all** of: the release is a product MINOR or PATCH, never a MAJOR; no contract integer in [Compatibility](../COMPATIBILITY.md#1-versioned-surfaces) §1 increases; the update changes no permission and expands no authority; the update changes no key handling; the update performs no irreversible migration; a pre-update backup completed successfully; migrations validate; health checks pass; and rollback is available. The contract-integer test supplements the permission judgment and never replaces it. | Requiring every condition to hold, not any. | [Compatibility §5](../COMPATIBILITY.md#5-relationship-to-automatic-updates), [Data and privacy §11](../DATA_AND_PRIVACY.md#11-updates-repair-and-failure-behavior), [`D-071` in the decision register](../DECISION_REGISTER.md), [`D-090` in the decision register](../DECISION_REGISTER.md) |
| `SC-PUB-005` | Failing any condition in `SC-PUB-002` or `SC-PUB-004` means the update must not install automatically. It may proceed only with explicit user approval, and granting that approval is a protected action under `SC-CONF-001` — so it is subject to `SC-CONF-002` through `SC-CONF-009` in full. A condition that cannot be evaluated counts as failed. | Requiring approval whenever a condition fails or cannot be evaluated. | [Compatibility §5](../COMPATIBILITY.md#5-relationship-to-automatic-updates), [`D-071` in the decision register](../DECISION_REGISTER.md), [Data and privacy §11](../DATA_AND_PRIVACY.md#11-updates-repair-and-failure-behavior) |
| `SC-PUB-006` | A failed verification must install nothing. The artifact must be retained as evidence rather than silently discarded, no partial installation may remain, and the user must be warned. The warning must name the artifact, the release, and the failure category; it must not be suppressed as routine noise. | Installing nothing on failure. | [Architecture §Security and trust boundaries](../ARCHITECTURE.md#security-and-trust-boundaries), [Architecture §Operations](../ARCHITECTURE.md#operations), [Architecture §Expected failure modes](../ARCHITECTURE.md#expected-failure-modes) |
| `SC-PUB-007` | Rollback to the pre-update state must remain available until post-update health checks pass. Rollback must not be discarded, garbage-collected, or invalidated before that point, and the pre-update backup that makes it possible must not be rotated away while it is the only path back. | Retaining rollback until health checks confirm the update. | [Data and privacy §11](../DATA_AND_PRIVACY.md#11-updates-repair-and-failure-behavior), [Architecture §Operations](../ARCHITECTURE.md#operations), [Compatibility §3](../COMPATIBILITY.md#3-support-windows) |
| `SC-PUB-008` | The update check is the only unsolicited outbound network use. Every other outbound request must be either consented external processing under `SC-CONSENT-001` or a direct result of a user action. No telemetry, usage reporting, or background beacon is permitted, consistent with `SC-CONSENT-009`. | Making no outbound request that is not the update check or consented processing. | [Architecture §Security and trust boundaries](../ARCHITECTURE.md#security-and-trust-boundaries), [Data and privacy §2.3](../DATA_AND_PRIVACY.md#23-local-and-external-computation) |

## 4. Failure behavior

Every way this contract's checks can be unavailable resolves to not installing:

- **Signature or attestation missing, malformed, or unverifiable.** Failed verification; nothing installs, the artifact is retained, the user is warned (`SC-PUB-002`, `SC-PUB-006`).
- **Pinned identity unreadable, or the artifact presents an unpinned identity.** Failed verification (`SC-PUB-002`); an identity change requires approved rotation (`SC-PUB-003`), never acceptance in passing.
- **Checksum matches but no signature is present.** Not sufficient; automatic installation is refused (`SC-PUB-001`).
- **A condition in `SC-PUB-004` cannot be evaluated** — release type, contract integers, permission delta, key-handling delta, or migration reversibility unknown. Counted as failed; approval is required (`SC-PUB-005`).
- **Pre-update backup fails.** Automatic installation is refused (`SC-PUB-004`); there is no "install anyway" path.
- **Migration validation fails, or health checks fail.** The update does not complete and rollback is exercised; rollback remains available because health checks have not passed (`SC-PUB-007`).
- **Trusted confirmation channel unavailable when approval is required.** The update does not proceed, under `SC-CONF-008`.
- **Release endpoint unreachable.** No update is installed; the absence of a check is not an installation condition, and this is the only unsolicited outbound use in the first place (`SC-PUB-008`).

## 5. Verification owed

This contract owes evidence for these verification classes in [Data and privacy §12](../DATA_AND_PRIVACY.md#12-required-security-verification):

- update tampering, migration failure, and rollback tests;
- trusted-human confirmation tests against model-generated requests, replay, expired confirmations, capability escalation, and same-user spoofing — for the approval path `SC-PUB-005` routes into;
- database and backup confidentiality at rest, and Windows key scoping and recovery-key exercises, to the extent `SC-PUB-004`'s pre-update backup and `SC-PUB-003`'s identity rotation depend on them.

The threat model records which `THR-NNN` entries these rules govern. No test for any of these classes has been written.

## 6. Change procedure

This contract is frozen. A rule changes only through a new entry in the [decision register](../DECISION_REGISTER.md) plus a migration-impact note stating what already-written code, documentation, or test the change invalidates. A rule is never changed by an edit made in passing.

The **Version** integer above increments when a rule's meaning changes — including adding to or removing from `SC-PUB-004`'s condition set. Editorial clarification that does not change what is required or refused does not increment it. A withdrawn rule keeps its identifier and is marked withdrawn; identifiers are never reused or renumbered.

Resolving `F-007` does not by itself change this contract: choosing a signing provider satisfies `SC-PUB-002` rather than amending it. Rotating the pinned identity is likewise not a contract change; it is the approved, recorded event `SC-PUB-003` requires.
