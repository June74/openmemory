# OpenMemory Versioning and Compatibility Policy

> **Status:** approved planning-stage policy. These rules govern how OpenMemory's surfaces will be versioned once they exist. No version of any surface has been released, so nothing described below is currently in force; there is no shipped contract for a client or peer to be compatible with yet.

This document defines how OpenMemory versions its product and its individual wire and storage contracts, what counts as a breaking change, how long old versions are supported, and how those rules constrain automatic updates. The [architecture](ARCHITECTURE.md) describes the surfaces themselves; the [data and privacy design](DATA_AND_PRIVACY.md) describes the update and migration safeguards this policy assumes. Stage 2 freezes the contract version integers this document introduces, and later stages — including the named-pipe version negotiation and each adapter's wire contract — are written against the frozen values.

## §1 Versioned surfaces

OpenMemory does not use one version number for everything. The product ships as a single unit, but its wire and storage contracts change on their own schedules, so each is versioned independently.

| Surface | Scheme | Initial value | Notes |
|---|---|---|---|
| Product | SemVer 2.0.0 | `0.1.0` | Service, CLI, and installer released as one unit. This is the version `D-071`'s updater reasons about. |
| MCP protocol | Integer | `1` | Negotiated per connection. |
| Named-pipe envelope | Integer | `1` | Framing and capability contract from `ARCHITECTURE.md`. |
| Database schema | Integer | `1` | Monotonic migration number. Forward-only. |
| Normalized event envelope | Integer | `1` | The client-neutral contract all three adapters emit. |
| Markdown projection protocol | Integer | `1` | Governs the two-way projection in `D-065`. |
| Portable export format | Integer | `1` | The format in `D-083`. Must stay readable without the encrypted database. |
| Obsidian plugin | SemVer | `0.1.0` | Obsidian's `manifest.json` requires SemVer and a `minAppVersion`, so this surface cannot use an integer. |

The Obsidian plugin is the one exception to the integer scheme, and it is not a stylistic choice. Obsidian's plugin loader reads `manifest.json` and requires a SemVer `version` field plus a `minAppVersion` field to decide whether the installed Obsidian build can run the plugin at all. An OpenMemory-internal integer would have no meaning to that loader, so the plugin surface follows Obsidian's external requirement rather than the integer scheme used everywhere else in this table.

## §2 What "breaking" means

A change is breaking when an existing peer that was previously accepted would be rejected, misread, or would silently lose data. This definition applies uniformly to every integer-versioned surface in §1.

- Adding an optional field is not breaking.
- Changing an existing field's meaning, type, or required-ness is breaking.
- Removing a field is breaking.
- Tightening validation on previously accepted input is breaking.

A change that is not breaking under this definition does not require a contract version increment. A change that is breaking always does.

## §3 Support windows

Support windows describe how much of a surface's version history the service must keep accepting, not how long a version is merely documented.

- The service accepts the **current and immediately previous** integer for the MCP protocol and the named-pipe envelope. Anything older is rejected with a version-mismatch error that names the supported range. A rejected version is never handled by best-effort parsing.
- The **database schema is forward-only**. The service refuses to open a database whose schema integer exceeds the value the running binary knows. This refusal is what makes rollback after a failed update safe: an older binary declines rather than corrupting newer data.
- The **portable export format** is readable by every later version. This is the promise behind `D-083`, so it has no support window; support is permanent.
- The **event envelope** and **projection protocol** follow the current-and-previous rule.

## §4 Pre-1.0 policy

Before product `1.0.0`, every contract may break without a support window, because no supported release exists. Development builds are explicitly not compatible with each other. A development artifact must never be mistaken for a compatible one: until product `1.0.0` ships, matching contract integers between two builds is not a guarantee of interoperability, only a coincidence of the current development state.

## §5 Relationship to automatic updates

Automatic installation is permitted only for a product MINOR or PATCH release in which **no contract integer increases**. Any contract increment, and any product MAJOR release, requires explicit approval.

This is a deliberately stricter reading of decision `D-071` than its literal wording. `D-071` requires approval for "major or permission-changing updates," which leaves the boundary to judgment. This policy replaces that judgment call with a mechanical test: compare the contract version integers in §1 before and after the candidate update, and if any of them increases, the update is not eligible for automatic installation regardless of what the product's SemVer field does. A consequence worth stating explicitly is that a product PATCH release can still require approval — for example, a patch that carries a database schema migration increments the database schema integer, so it fails the no-increase test and is not auto-installable even though it is a patch by SemVer.

## §6 Freeze status

Every contract integer listed in §1 is **unfrozen** until Stage 2 freezes it. Stage 2 is the freezing stage: it is where the MCP protocol, named-pipe envelope, database schema, event envelope, projection protocol, and portable export format versions are fixed as stable contracts. Stage 3 adapters consume the frozen contract produced by Stage 2 rather than extending or renegotiating it.
