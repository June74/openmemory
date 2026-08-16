# Stage 0 Wave A — Governance and records (design)

- **Date:** 2026-08-16
- **Stage:** 0 (Program foundation)
- **Wave:** A of four
- **Owner:** Root integrator
- **Branch:** `codex/stage0-wave-a`
- **Status:** Awaiting user review

## 1. Why this wave exists

[Stage 0](../../IMPLEMENTATION_PLAN.md) lists nine independent workstreams. Specifying all nine at once would produce a document too vague to implement, so Stage 0 is split into four dependency-ordered waves:

| Wave | Content | Depends on |
|---|---|---|
| **A** | Governance files and records | Nothing |
| B | Toolchain pinning and solution boundaries | .NET 10 SDK install |
| C | CI pipeline | Wave B |
| D | Security contracts and repository-grounded threat model | Wave B |

Wave A is first because it has no toolchain dependency. Wave D is last because [`F-010`](../../DECISION_REGISTER.md) requires the threat model to be created *after* the repository structure and executable trust boundaries exist.

This wave covers two Stage 0 bullets:

- "establish Apache-2.0 licensing, DCO contribution policy, security policy, ownership, issue templates, and pull-request gates";
- "record product requirements, architecture decisions, data classifications, stable identifiers, protocol versions, and compatibility policy".

## 2. Current state

Verified on 2026-08-16 against commit `0939e71`.

**Already present and correct:** `LICENSE` (Apache-2.0), `NOTICE`, `CODE_OF_CONDUCT.md`, `CONTRIBUTING.md` (including the DCO sign-off requirement and a written PR evidence list), `SECURITY.md`, `AGENTS.md`, and a data-class table at [DATA_AND_PRIVACY.md §3](../../DATA_AND_PRIVACY.md).

**Gaps this wave closes:**

1. `.github/` does not exist. There is no `CODEOWNERS`, no issue template, and no pull-request template.
2. `main` has no branch protection (confirmed: the protection endpoint returns 404), while `CONTRIBUTING.md` already states that direct implementation commits to `main` are not allowed.
3. No document defines how any surface is versioned, yet [ARCHITECTURE.md](../../ARCHITECTURE.md) already promises that the named pipe "rejects unsupported protocol versions or capabilities".
4. Four identifier schemes (`D-*`, `X-*`, `F-*`, `SET-*`) are in active use across the documentation but are defined nowhere, and no runtime identifier format has been decided.

## 3. Decisions taken during brainstorming

| Ref | Decision | Rationale |
|---|---|---|
| A-1 | Independent review is performed by `codex exec` locally after implementation, not by a GitHub App or CI job. | [AGENTS.md](../../../AGENTS.md) requires *independent* specification and quality review. Self-review of one's own diff is not independent. Codex CLI 0.147.0 is installed, uses the existing subscription, and needs no repository secret or CI spend. |
| A-2 | The versioning policy is written now, but GitHub branch protection is not enabled until Wave C. | A protection rule cannot require status checks that do not exist yet. Wave C creates the checks, then enables the rule. |
| A-3 | One SemVer product version, plus independent integer versions per contract surface. | Decouples unrelated surfaces. A database migration should not force a major product release, and the pipe can reject on a protocol integer while the updater reasons about the product major. |
| A-4 | Apache-2.0 file headers are defined now and applied in Wave B. | There are no source files yet. A header convention with nothing to apply it to is ceremony. |

## 4. Deliverables

### 4.1 New file — `docs/COMPATIBILITY.md`

Defines eight independently-versioned surfaces.

| Surface | Scheme | Notes |
|---|---|---|
| Product | SemVer 2.0.0 | Covers the service, CLI, and installer as one release unit. This is the version the updater in `D-071` reasons about. |
| MCP protocol | Integer | Negotiated per connection. |
| Named-pipe envelope | Integer | The framing and capability contract described in `ARCHITECTURE.md`. |
| Database schema | Integer | Monotonic migration number. Forward-only; rollback is by restoring a backup, never by reversing a migration in place. |
| Normalized event envelope | Integer | The frozen client-neutral contract all three adapters emit. |
| Markdown projection protocol | Integer | Governs the two-way Obsidian projection in `D-065`. |
| Portable export format | Integer | The format in `D-083`; must stay readable without the encrypted database. |
| Obsidian plugin | SemVer | **External constraint:** Obsidian's `manifest.json` requires a SemVer version and a `minAppVersion`, so this surface cannot use an integer. |

The document defines, for each surface, what counts as a breaking change and what the support window is. The working rules:

- **Breaking** means an existing peer that was previously accepted would be rejected, misread, or silently lose data. Adding an optional field is not breaking; changing a field's meaning is.
- The service accepts the **current and immediately previous** integer for the MCP protocol and pipe envelope. Anything older is rejected with a version-mismatch error naming the supported range, never by best-effort parsing.
- Database schema is forward-only. The installer refuses to open a database whose schema integer exceeds the one the binary knows, which is what makes rollback-after-failed-update safe.
- **Pre-1.0 policy:** before `1.0.0` every contract may break without a support window, because no supported release exists. This is stated explicitly so that development builds are not mistaken for compatible ones.

**Tie-in to `D-071`:** automatic installation is permitted only for a product MINOR or PATCH release in which no contract integer increases. Any contract increment, or a product MAJOR, requires explicit approval. This gives the "major or permission-changing updates require approval" rule a mechanical test rather than a judgement call.

### 4.2 New file — `docs/IDENTIFIERS.md`

**Documentation identifiers** — formalizes what is already in use and adds one:

| Pattern | Meaning | Status |
|---|---|---|
| `D-NNN` | Approved decision | In use |
| `X-NNN` | Explicit v1 exclusion | In use |
| `F-NNN` | Deferred decision | In use |
| `SET-YYYYMMDD-NNN` | Setback record | In use |
| `REQ-<AREA>-NNN` | Product requirement | **New** — `PRODUCT_REQUIREMENTS.md` has no citable IDs today |

**On architecture decision records:** Stage 0 asks to "record architecture decisions", and `D-*` already does this in index form. Rather than run two competing decision systems, `D-*` remains the single register, and `ADR-NNNN` is reserved for the case where a decision needs context, alternatives, and consequences at a length the register table cannot hold. No `ADR-*` document is created in this wave. The prefix is registered so that the first long-form record does not have to invent a scheme under time pressure.

**Runtime identifiers:**

- Time-ordered entities (event, project, repository, installation, private store, vault) use **UUIDv7** (RFC 9562). Chosen over ULID because it is a standard, is directly supported by .NET (`Guid.CreateVersion7()`), and sorts by creation time, which gives good index locality for the append-only journal.
- `source_record_id` is an opaque adapter-namespaced string, `{adapter}:{native_id}`, because native IDs from Codex, Claude Code, and Antigravity have no common format and must not be forced into one.
- `content_hash` is SHA-256, lowercase hex, used for idempotency and deduplication.

**Stability rules**, which are the actual point of the document:

1. An identifier is never reused, even after the thing it names is deleted.
2. An identifier is never derived from mutable data. Paths, folder names, project names, and branch names all change; identifiers must survive rename, move, and hardware transfer.
3. An identifier must not encode user content or any secret value. This follows from the secret rules in `DATA_AND_PRIVACY.md §4` — an identifier is a value that gets logged, exported, and displayed.

### 4.3 New directory — `.github/`

| File | Content |
|---|---|
| `CODEOWNERS` | Root integrator owns all paths, with `docs/` and `.github/` called out explicitly. Enforces the AGENTS.md rule that shared files have a named owner. |
| `PULL_REQUEST_TEMPLATE.md` | The five-item evidence list `CONTRIBUTING.md` already requires: what changed and why, requirements and decisions satisfied, security/privacy effects, verification commands and real user paths, remaining limitations. |
| `ISSUE_TEMPLATE/bug_report.yml` | Requires affected revision, expected versus actual, reproduction. Warns against pasting transcripts or secrets. |
| `ISSUE_TEMPLATE/feature_request.yml` | Requires the requester to state which decision or requirement the request relates to, and confirms it is not in the `X-*` exclusion list. |
| `ISSUE_TEMPLATE/documentation.yml` | For corrections and unclear documentation. |
| `ISSUE_TEMPLATE/config.yml` | Disables blank issues; routes vulnerability reports to `SECURITY.md` private reporting rather than the public tracker. |
| `branch-protection.md` | Records the exact ruleset Wave C will enable, so the deferral in A-2 is a documented plan rather than an omission. |

### 4.4 Edits to existing files

Narrow additions only. No rewrites, no reformatting.

- `CONTRIBUTING.md` — add the required-checks table and the Apache-2.0 file-header policy; link the two new documents.
- `DATA_AND_PRIVACY.md` — add canonical `DC-1`…`DC-7` labels to the existing data-class table so other documents can cite a class instead of restating it. **The table's content does not change**; only a label column is added.
- `DECISION_REGISTER.md` — record decisions A-2 and A-3 as new `D-*` entries. A-1 and A-4 are process choices, not product decisions, and stay in this spec.
- `README.md` — link `COMPATIBILITY.md` and `IDENTIFIERS.md`.

## 5. Verification

Documentation has no unit tests, so the test-first discipline applies to the *checks*: each is run before the change to observe it fail, then after to observe it pass.

| Check | Command | Fails before because |
|---|---|---|
| CODEOWNERS validity | `gh api repos/June74/openmemory/codeowners/errors` | The file does not exist |
| Issue-template YAML | Parsed locally, then confirmed server-side after push | GitHub validates issue forms only on the pushed ref |
| Repo-local Markdown links | Bash link check, repository-internal links only | New documents are linked before they exist |
| Independent review | `codex exec` spec and quality review of the diff | — |

**Two constraints carried from prior setbacks.** The link check runs in bash, not a PowerShell pipeline, because [SET-20260816-001](../../operations/setbacks/SET-20260816-001-link-check-parser.md) was a PowerShell parse failure. It checks repository-internal links only, because [SET-20260816-002](../../operations/setbacks/SET-20260816-002-link-check-sandbox.md) was the sandbox blocking outbound requests. Repeating either would be a self-inflicted setback.

## 6. Out of scope

Deliberately excluded, so their absence is not read as an oversight:

- `.github/workflows/` — Wave C. A workflow referencing a build that does not exist would fail on its first run.
- Enabling branch protection — Wave C, per decision A-2.
- Any toolchain file (`global.json`, `.nvmrc`, `package.json`) — Wave B.
- Any source code, project file, or solution file — Wave B.
- The threat model and the four frozen security contracts — Wave D.

Wave A adds **zero product capability**, which is what the Stage 0 exit gate requires: "no product capability is claimed."

## 7. Risks

| Risk | Mitigation |
|---|---|
| The versioning policy is written before any code exists, so a contract boundary may be wrong. | Contract integers all start at 1 and are explicitly unfrozen until Stage 2 freezes them. The pre-1.0 policy makes this safe. |
| `REQ-*` IDs are introduced but `PRODUCT_REQUIREMENTS.md` is not yet renumbered. | Wave A defines the scheme; applying IDs to existing requirement text is a separate, larger edit deferred to its own task so this wave's diff stays reviewable. |
| Issue templates cannot be fully validated until pushed. | Local YAML parse first, server-side confirmation after push, before the wave is called complete. |
