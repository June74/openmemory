# OpenMemory Staged Implementation Plan

> **Status:** planning only. No application code exists yet. This plan begins after the documentation baseline is published and the workspace is verified at its final location.

OpenMemory will be built in gated stages. Stages may contain parallel work, but a later stage cannot treat an earlier design as stable until its exit gate passes. The public v1 release includes every approved v1 capability; stages are internal development milestones, not reduced public editions.

See [Architecture](ARCHITECTURE.md), [Product requirements](PRODUCT_REQUIREMENTS.md), [Data and privacy](DATA_AND_PRIVACY.md), and [Roadmap](ROADMAP.md).

## Team model

One root integrator owns the product contract, architecture, shared schemas, integration branch, and final acceptance evidence. At most three worker agents run concurrently.

After the baseline commit:

- every worker uses an isolated Git worktree and a short `codex/<description>` branch;
- a worker owns a disjoint subsystem or explicit file set for the duration of a wave;
- shared contracts are written, tested, reviewed, and frozen before dependent parallel work begins;
- contract changes return to the root integrator and are merged before workers continue;
- agents never resolve integration problems by overwriting another worker's changes;
- every work item has an implementer, specification reviewer, quality/security reviewer when relevant, and root integration check;
- focused tests run before review, and the complete affected suite runs after integration;
- unexpected failures are diagnosed and recorded before the plan is changed.

The root integrator may run three makers in parallel, then rotate available agents into review roles. Review independence matters more than keeping every slot occupied.

## Stage 0 — Program foundation

**Owner:** root integrator, with narrow documentation or infrastructure tasks delegated only after ownership is clear.

Build on the published documentation baseline and establish the engineering foundation:

- verify the published repository, renamed local workspace, GitHub identity, `origin`, `main`, and clean worktree;
- install and pin the .NET 10 SDK and Node/TypeScript toolchain without adding product features;
- create solution boundaries for the service, contracts, MCP bridge, adapters, storage, indexing, Obsidian plugin, installer, and tests;
- establish Apache-2.0 licensing, DCO contribution policy, security policy, ownership, issue templates, and pull-request gates;
- record product requirements, architecture decisions, data classifications, stable identifiers, protocol versions, and compatibility policy;
- create a repository-grounded threat model before security-sensitive implementation;
- freeze registered-client capabilities, trusted-human confirmation, consent/revocation, and publisher-authentication contracts;
- define deterministic test fixtures and the launch checklist;
- establish CI for formatting, build, unit tests, dependency review, secret scanning, SBOM generation, and checksummed development artifacts.

**Exit gate:** a clean Windows build produces a documentation/dev artifact; CI and security checks pass; contracts and repository ownership are approved; no product capability is claimed.

## Stage 1 — Three parallel feasibility proofs

These are disposable proofs used to remove foundational risk. They are not silently promoted into production code.

### Worker A: encrypted storage and recovery

Prove SQLCipher creation, writes, reopen, concurrent-read behavior, transactional migrations, Credential Manager/DPAPI key protection, recovery-key use, integrity checks, rotated backups, pinned backups, and restore on a second Windows user profile.

### Worker B: Windows packaging and client connection

Prove per-user MSI installation, repair, upgrade, uninstall, singleton startup, user-restricted named-pipe access, thin stdio MCP bridging, protocol negotiation, and one neutral MCP call from Codex, Claude Code, and Antigravity. Prove an ephemeral `codex exec` job can pause cleanly when authentication or allowance is unavailable.

### Worker C: indexing feasibility

Prove FTS5, a compact local ONNX embedding model, deterministic vector search, Tree-sitter grammars for every required language, Git/worktree identity, file watching, incremental parsing, and bounded indexing of a representative repository.

**Exit gate:** all three clients reach one neutral service contract; encrypted backup and recovery work; required parsers load; semantic and keyword retrieval meet baseline correctness and latency; the per-user package installs and uninstalls on a clean Windows 11 x64 environment. A failed core technology is replaced and re-proved before Stage 2.

## Stage 2 — Secure data plane

Freeze the first production contracts, then run three lanes:

- **Lane A — durable ingestion:** append-only event journal, normalized event envelope, idempotency, bounded payloads, secret quarantine, single database writer, checkpoints, and crash recovery.
- **Lane B — memory and temporal state:** entities, claims, provenance, valid and recorded time, authority levels, conflicts, supersession, failed attempts, approval state, and schema migration rules.
- **Lane C — identity and projection foundation:** projects, repositories, branches, worktrees, installations, reciprocal vault/private manifests, jobs, leases, audit events, and versioned Markdown projection protocol.

**Exit gate:** duplicate delivery, interruption, database reopen, backup/restore, migration rollback, temporal transitions, authority conflicts, project sensitivity, and concurrent Markdown edits all pass contract tests. No client adapter may invent a private schema.

## Stage 3 — Three client adapters

Assign one worker to each supported terminal client:

- Codex CLI hooks, import checkpoints, source mapping, MCP configuration, and `/store` translation;
- Claude Code hooks, import checkpoints, source mapping, MCP configuration, and `/store` translation;
- Antigravity CLI hooks, import checkpoints, source mapping, MCP configuration, and `/store` translation.

Each adapter emits the frozen neutral event envelope. Provider-specific details stay in encrypted private provenance. Hooks provide immediate capture; importers reconcile missed history without duplication. An inline `/store` waits for and targets the complete current user/assistant/tool turn, while a standalone `/store` targets the previous complete turn; all adapters resolve these forms to the same stable turn-ID contract.

**Exit gate:** the same black-box adapter suite passes for all three clients; interrupted imports resume; duplicated source records collapse safely; tool results and attachments retain evidence links; no provider label leaks into semantic memory.

## Stage 4 — Memory engine

Run three lanes against a shared evaluation corpus:

- **Lane A — processing and authority:** one-hour quiet period, immediate `/store` with its larger extraction budget and richer goal/decision/requirement/state/evidence schema, 2:00 AM catch-up, allowance-aware `codex exec` queue, routine extraction, reflection, reports, global promotion, approvals, and deterministic validation.
- **Lane B — retrieval:** FTS5, local embeddings, metadata filtering, graph/temporal signals, ranking fusion, task-aware modes, manual overrides, context budgets, relevance decay, feedback, and auditable reads.
- **Lane C — temporal knowledge:** entity resolution, bitemporal claims, provenance, supersession, conflicting evidence, provisional reflections, cross-project portability, sensitivity boundaries, and failed-attempt warnings.

Ranking, promotion, matching, and conflict rules require explicit domain-logic contracts rather than informal scoring embedded in code.

**Exit gate:** versioned retrieval evaluations meet agreed relevance and latency thresholds; tests prove `/store` runs immediately, extracts the complete richer schema beyond the routine-capture baseline, and does not raise claim authority; user feedback outranks model feedback; normal/restricted/isolated projects show no unauthorized leakage; conflicts preserve before/after evidence; injected evidence cannot become instructions; allowance exhaustion pauses rather than loses work.

## Stage 5 — Three code-intelligence lanes

Freeze a common language and code-graph contract, then split ownership:

- **Lane A:** C#, TypeScript, JavaScript, HTML, and CSS.
- **Lane B:** Rust, Go, C, and C++.
- **Lane C:** Python, Java, SQL, and PowerShell.

All lanes produce the same core concepts: files, symbols, definitions, references, calls, imports, inheritance, parse status, Git evidence, and changed-symbol links. Full Git history is represented structurally without copying every diff. Durable project knowledge is shared across worktrees, while live task and code states remain branch/worktree-specific.

**Exit gate:** representative repositories for every language pass parser, relationship, incremental-update, rename, branch, merge, and worktree tests. Unsupported syntax produces partial-state evidence instead of corrupting the graph. Code contradictions mark memories possibly stale without silently rewriting them.

## Stage 6 — User experience and operations

- **Lane A — Obsidian:** global/project indexes, task snapshots, editable projections, transcript evidence, temporal graph explorer, search, conflicts, ambiguous entities, secret warnings, deletion review, reports, and health views.
- **Lane B — local operation:** MCP tools, `openmemory` CLI, `/memory` commands, project attachment, startup registration, Windows urgent notifications, diagnostics, and safe doctor repairs.
- **Lane C — lifecycle:** routine and pinned backups, restore, hardware transfer, portable export, deterministic daily and consented model-assisted weekly audits, publisher-authenticated update verification, health checks, and rollback.

The terminal path must remain complete when Obsidian is absent. The graph explorer is read-focused; an editable graph canvas is outside v1.

**Exit gate:** users can install, attach a project, store and retrieve memory, review a conflict, edit a projected note, inspect evidence, restore a backup, export data, and run doctor through real interfaces. Obsidian-closed and Obsidian-not-installed cases pass. Tests prove that MCP/model calls cannot finalize protected actions, external-processing consent can be revoked without stopping local capture/retrieval, and unsafe repair, deletion, export, or update actions require the approved warning and trusted confirmation.

## Stage 7 — Migration and hardening

- **Lane A — migration:** resumable Codex, Claude, Antigravity, approved old-vault, manual-file, and Git imports; coverage reporting; evidence-only treatment; verified cutover; and exact-path deletion review.
- **Lane B — security and recovery:** secret classes, prompt injection, malicious tool output, path traversal, pipe impersonation, corrupted database, key loss, recovery-key use, backup tampering, dependency risks, and update rollback.
- **Lane C — quality and performance:** retrieval and temporal correctness, graph completeness, large-history behavior, indexing throttles, queue pressure, crash loops, installation, upgrade, uninstall, transfer, and accessibility/usability acceptance.

**Exit gate:** a clean Windows 11 x64 machine passes install-to-transfer acceptance; zero known project-sensitivity leaks remain; secret values never appear in storage, logs, embeddings, warnings, model requests, or exports; migration reports every skipped or failed record; old data deletion is never automatic.

## Stage 8 — Release

The root integrator assembles release evidence and uses independent specification, security, and quality reviews. A private beta exercises real workflows before publication.

The public v1 release includes:

- all approved v1 features and supported clients;
- reproducible build instructions, SBOM, checksums, and a signature or signed attestation anchored to the documented trusted project identity;
- installer, upgrade, rollback, uninstall, migration, backup, recovery, transfer, and privacy documentation;
- evaluation results with limitations stated plainly;
- a clean issue tracker and published compatibility boundaries.

**Exit gate:** every preceding gate passes on the release commit; the remote tag and artifacts match local evidence; the actual user-facing paths are tested successfully before the user is asked to test them.

## Required skills

Agents must read and follow applicable skill instructions before acting.

| Purpose | Skills |
|---|---|
| Discovery and specification | `using-superpowers`, `brainstorming`, `grill-me`, `scope-gate`, `writing-plans`, `explaining-unfamiliar-terms` |
| Parallel development | `using-git-worktrees`, `dispatching-parallel-agents`, `subagent-driven-development` |
| Implementation correctness | `test-driven-development`, `domain-logic-contract`, `trace-live-call-path` |
| Security | `security-threat-model`, `api-security-best-practices`, `secret-scanning` |
| Client and extension work | `openai-docs`, `plugin-creator`, `skill-creator`, `writing-skills` |
| Obsidian user experience | `frontend-design`, `ui-ux-pro-max`, `web-design-guidelines` |
| Diagnosis | `systematic-debugging`, `setback-logger` |
| Review | `requesting-code-review`, `receiving-code-review` |
| GitHub and CI | `github:github`, `github:yeet`, `github:gh-fix-ci`, `github:gh-address-comments` |
| Acceptance and integration | `verification-before-completion`, `deliverable-acceptance-check`, `finishing-a-development-branch` |

## Standard task lifecycle

1. Confirm scope, owner, frozen interfaces, and acceptance tests.
2. Create an isolated worktree and branch.
3. Write a failing test for the required behavior.
4. Implement the smallest correct change.
5. Run focused tests and inspect the diff for secrets and unrelated files.
6. Perform self-review, specification review, and quality/security review as applicable.
7. Integrate through the root owner and run the complete affected suite.
8. Record unexpected failures, verified outcomes, and any decision change.

No stage is declared complete from unit tests alone when its exit gate describes an installer, client, Obsidian, backup, migration, or other user-facing workflow.
