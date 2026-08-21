# Frozen security contracts

> **Status note:** nothing implements any of these contracts yet. They fix required behavior before implementation, per the Stage 0 exit gate.

This directory holds the four security contracts frozen at Stage 0. Each document states the behavior that OpenMemory's security depends on — what may authorize an action, what must be refused, and what must fail closed — in numbered, citable rules.

## What a frozen security contract is

A frozen security contract is a versioned document whose rules are normative and stable. A rule may be added, corrected, or withdrawn only through the change procedure each document's §6 states: a decision-register entry plus a migration-impact note, never an edit made in passing. Later stages cite a rule by identifier when they claim to satisfy it, adapt to it, or attack it in an adversarial test.

Freezing fixes *behavior*, not *encoding*. These four contracts say nothing about wire formats, field names, schemas, or timeout values; those are Stage 2's freeze, as [Compatibility](../COMPATIBILITY.md) §6 records.

## Why these four freeze at Stage 0 while the contract integers do not

[Compatibility](../COMPATIBILITY.md) §1 lists the wire and storage surfaces — MCP protocol, named-pipe envelope, database schema, event envelope, projection protocol, portable export format — and §6 declares every one of their integers **unfrozen** until Stage 2. The four contracts here freeze now, at Stage 0, because Stage 1 cannot be built or reviewed without knowing what must be refused.

Decision `D-092` in the [decision register](../DECISION_REGISTER.md) records the consequence: these four are versioned in their own documents and are deliberately **not** added to `COMPATIBILITY.md` §1. Listing two opposite freeze states in one table would contradict §6.

## The `SC-<AREA>-NNN` identifier scheme

Decision `D-093` in the [decision register](../DECISION_REGISTER.md) gives every rule a stable identifier, registered in the [identifier registry](../IDENTIFIERS.md) §1. The scheme is namespaced by contract area — `CAP`, `CONF`, `CONSENT`, `PUB` — rather than running as one flat series, because the four contracts are owned, versioned, and frozen as four separate documents: `SC-PUB` can gain a rule without changing the meaning of any `SC-CAP` rule.

An identifier, once allocated, is never reused or renumbered, because other documents cite it. A rule withdrawn through the change procedure keeps its number and is marked withdrawn.

## The four contracts

| Contract | Document | Rule prefix | Rules | Version |
|---|---|---|---|---|
| Registered client capabilities | [REGISTERED_CLIENT_CAPABILITIES.md](REGISTERED_CLIENT_CAPABILITIES.md) | `SC-CAP-` | 8 | 1 |
| Trusted human confirmation | [TRUSTED_HUMAN_CONFIRMATION.md](TRUSTED_HUMAN_CONFIRMATION.md) | `SC-CONF-` | 9 | 1 |
| External processing consent and revocation | [EXTERNAL_PROCESSING_CONSENT.md](EXTERNAL_PROCESSING_CONSENT.md) | `SC-CONSENT-` | 9 | 1 |
| Publisher authentication | [PUBLISHER_AUTHENTICATION.md](PUBLISHER_AUTHENTICATION.md) | `SC-PUB-` | 8 | 1 |

## Sources these contracts freeze

No rule in this directory is new. Each one restates an approved position already recorded in [Architecture](../ARCHITECTURE.md), [Data and privacy](../DATA_AND_PRIVACY.md), [Compatibility](../COMPATIBILITY.md), or the [decision register](../DECISION_REGISTER.md), and every rule's `Source` column links to where it comes from. Stage 0 has authority to freeze approved security requirements, not to invent them.

## What is still deferred

Two deferred decisions in the [decision register](../DECISION_REGISTER.md) sit underneath these contracts and are not resolved by freezing them:

- `F-007` — the exact signing provider and attestation implementation behind `SC-PUB-002`.
- `F-011` — the exact user-presence mechanism satisfying `SC-CONF-007`, and its resistance to same-user process spoofing.

In both cases the requirement is frozen here and only the mechanism is deferred.
