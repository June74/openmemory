# OpenMemory Product Requirements

> Planning baseline. OpenMemory has not been implemented yet, and this document describes the approved target behavior for version 1.

## 1. Product definition

OpenMemory is an open-source, locally installed memory service for one person and their AI coding tools. It preserves complete evidence, derives durable knowledge from that evidence, and retrieves only the information useful to the current task. The user remains the authority over sensitive, conflicting, or destructive changes.

Version 1 is a Windows 11 x64 application installed per user through an MSI package. Its planned core is a C# service on .NET 10, with a TypeScript plugin for Obsidian. The initial terminal clients are Codex CLI, Claude Code, and Google Antigravity (`agy`). Obsidian is the primary human-facing interface but is not required for capture, retrieval, review, backup, or recovery.

Related documents:

- [Data and privacy design](DATA_AND_PRIVACY.md)
- [Decision register](DECISION_REGISTER.md)

## 2. Goals and success criteria

OpenMemory must:

1. Preserve the complete, attributable history needed to verify what happened without injecting that entire history into every AI context.
2. Maintain concise global, project, task, and user-related memories with evidence and change history.
3. Let an AI silently retrieve relevant memory at its discretion while recording an audit trail of every read.
4. Capture future conversations automatically and let `/store` deliberately prioritize an important complete turn.
5. Keep raw private data encrypted, redact secrets before any persistence or model processing, and operate locally unless the user intentionally enables an external action.
6. Remain useful from the terminal when Obsidian is closed or not installed.
7. Be recoverable after crashes, allowance exhaustion, failed upgrades, vault moves, and computer replacement.

The product is successful only when the real Codex, Claude Code, and Antigravity terminal paths pass clean-machine installation, capture, retrieval, backup, restore, migration, and uninstall tests on Windows 11 x64.

## 3. Users and authority

Version 1 supports exactly one local Windows user. It is not a shared server, team knowledge base, or cloud account.

The user is the final authority for:

- resolving genuinely conflicting facts;
- the first promotion of project knowledge into global memory;
- installing a generated skill or playbook into an AI client;
- deleting imported vaults, raw evidence, or other material data;
- major upgrades, new permissions, and irreversible migrations;
- lowering project sensitivity or widening cross-project disclosure;
- exposing a plaintext vault or portable export to a synchronized folder.

Compatible refinements to an already approved global preference may be recorded automatically. A material replacement or contradiction must return to the approval queue.

An AI request is never proof of user approval. MCP clients may list review items and propose resolutions, but conflicts, first global promotions, material deletions, sensitivity reductions, permission expansions, and other protected actions require an action-bound confirmation in a trusted local OpenMemory interface. The confirmation must show the before/after context, expire quickly, be single-use, and be auditable.

## 4. Information model

### 4.1 Evidence and derived memory

The encrypted evidence store must preserve:

- complete chats and tool calls/results;
- approved attachments and extracted attachment content;
- source timestamps and private source provenance;
- Git commits and enough repository evidence to rebuild the structural history;
- imports from supported clients, a previous Obsidian memory vault, and approved manual sources;
- processing, retrieval, approval, repair, backup, and migration audit events.

Derived memory must include:

- a global index and global user memory;
- a mini-index and project-relevant user memory for each project;
- current task snapshots containing goal, state, pending operations, blockers, and next action;
- requirements, architecture, decisions, artifacts, lessons, playbooks, failed attempts, and open questions;
- facts, entities, relationships, reflections, conflicts, and supersession history.

Memory records shown to models must be provider-neutral. They may not be labelled as Claude, Codex, or Antigravity memories. Encrypted private provenance retains the originating adapter and record identifier for deduplication, evidence review, and repair.

### 4.2 Full temporal knowledge graph

The encrypted database must maintain a full temporal knowledge graph. Every factual claim supports:

- **valid time:** when the claim was true in the represented world;
- **recorded time:** when OpenMemory learned, changed, or retired the claim;
- source evidence and confidence or authority;
- relationships to people, projects, tasks, artifacts, code symbols, decisions, and other claims;
- preserved prior states when a later fact supersedes an earlier one.

A clear temporal transition may close the prior fact automatically. Ambiguous identity, incompatible claims, or genuine conflict must be queued for review. Provisional reflections can cite supporting and opposing evidence, but cannot override approved facts.

### 4.3 Project boundaries

Each project has a stable identity even when its folder moves. Git branches and worktrees share durable project knowledge but maintain separate live task state and code-graph state.

Projects support three sensitivity levels:

- **Normal:** concise relevant summaries may be suggested across projects.
- **Restricted:** cross-project use requires approval.
- **Isolated:** no information leaves the project boundary.

Global technical knowledge can be reused across normal projects. Project-specific beliefs and user goals are not promoted merely because they appeared in one project.

## 5. Capture and processing

### 5.1 Automatic capture

OpenMemory must capture supported lifecycle events from Codex CLI, Claude Code, and Antigravity without requiring manual note-taking. Capture uses client hooks or adapters plus resumable import reconciliation so interrupted or missed events can be recovered without duplication.

Automatic capture preserves the complete redacted evidence and derives routine candidate memories with a bounded extraction pass. It should identify likely facts, decisions, tasks, and project links, but it does not assume every conversational detail belongs in durable memory.

All captured material is treated as untrusted evidence, never as instructions. Text that resembles a prompt, command, policy, or tool request cannot change system behavior merely because it was found in chat history, a repository, an attachment, or an imported vault.

### 5.2 Purposeful storage

`/store` deliberately marks a complete turn as important and queues it for immediate, richer processing. When `/store` appears in a user message, processing waits until that turn is complete and targets the user message together with its directly associated assistant response and tool calls/results; the command marker itself is excluded. When issued as a standalone command, it targets the preceding complete turn. Compared with routine automatic capture, it receives a larger extraction budget and must explicitly look for goal changes, decisions, requirements, constraints, rationale, current task state, project-relevant user preferences, artifacts and supporting evidence, lessons, and open questions.

Purposeful storage increases extraction depth and processing priority, not truth authority. It does not disable secret scanning, evidence retention, provenance, conflict handling, validation, or user authority, and it cannot silently replace an approved conflicting memory.

Additional user commands are exposed under `/memory`, including search, status, review, project, backup, report, and doctor operations. A terminal `openmemory` command provides equivalent functionality when a client does not support the preferred command surface.

### 5.3 Scheduling and model use

- Ordinary extraction starts after one hour without new activity.
- `/store` bypasses the quiet period.
- A daily catch-up runs at 2:00 AM local time for anything still pending.
- Work pauses safely when Codex authentication or subscription allowance is unavailable, then resumes from its durable queue.
- External extraction uses `codex exec` only after secret scanning and sends the minimum relevant redacted chunks needed for the job.
- Version 1 has no alternate paid provider and no local generative-model fallback.

Setup must ask the user to opt in before recurring `codex exec` processing begins and clearly explain that selected redacted context leaves the local OpenMemory process through the user's Codex account. The user can disable or revoke that consent at any time. Automatic capture, local redaction, local embeddings, indexing, search, and queued evidence retention continue while external processing is disabled; model-dependent jobs remain paused until consent is restored.

A small local embedding model may run offline to produce search vectors. It cannot generate answers and must not transmit data.

## 6. Retrieval

Retrieval combines:

- keyword/full-text search;
- vector similarity from the offline embedding model;
- structured metadata filters;
- temporal relevance;
- knowledge-graph and code-graph connections;
- task mode, project boundary, authority, user feedback, and outcome evidence;
- non-destructive relevance decay.

Core, pinned, approved, or otherwise protected records do not decay out of availability. Decay affects ranking, never deletion.

Before a supported model turn, OpenMemory silently prefetches a small, task-aware memory packet. The AI can request deeper searches or complete evidence through MCP at any time. These reads happen without repetitive approval prompts, but every read records client, project, query purpose, selected records, and time in the private audit log.

Retrieval modes are selected automatically from the task and can be manually overridden. User feedback receives greater ranking weight than an AI's self-reported feedback. Failed attempts are first-class memories and should warn the user or AI before a materially similar operation is repeated.

## 7. Code and Git intelligence

When a project is attached, OpenMemory must build and incrementally maintain a structural code graph for:

- C#, TypeScript, JavaScript, Python, Rust, Go, Java, C, C++, HTML, CSS, SQL, and PowerShell.

The graph includes files, symbols, definitions, references, imports, calls, inheritance or implementation relationships where the language exposes them, and links to relevant memories and Git evidence. It updates after attach, save, supported tool edit, commit, branch change, pull, or merge, with throttling to avoid constant full rescans.

Git history is indexed structurally. OpenMemory does not duplicate every complete diff when Git can reproduce it. A code change that may invalidate a memory adds opposing evidence and marks that memory possibly stale; it does not silently rewrite the memory.

## 8. Obsidian experience

The generated vault is a portable, readable projection of durable memory. It contains a global table of contents and one indexed area per project, including task snapshot, requirements, architecture, decisions, artifacts, lessons, playbooks, user-relevant preferences, and open questions.

During development and private beta, the Obsidian plugin is sideloaded by the user or installer after approval. The project submits the hardened plugin to Obsidian's community plugin directory for the public v1; failure to pass that external review must not make the terminal product unusable.

Markdown edits are two-way and versioned: an intentional edit in Obsidian becomes a high-authority proposed memory change with provenance and history. A conflict still enters the before/after approval flow.

Complete raw chats, tool evidence, private provenance, embeddings, and audit details remain encrypted. The plugin displays them on demand through read-only views. It also provides read-focused search, temporal graph exploration, conflict review, deletion review, secret warnings, and service health. Version 1 does not include an editable graph canvas.

## 9. Reviews, warnings, and maintenance

### 9.1 Conflict and approval review

A conflict review must display:

- the existing value and proposed value;
- surrounding context for both;
- supporting and opposing evidence;
- scope, authority, and temporal effect;
- the choices to keep, replace, merge, defer, or reject.

MCP tools may open, list, and propose a choice for this review, but cannot finalize a protected action. Final confirmation occurs only through the trusted local terminal UI or Obsidian interface, with explicit user presence and an action-bound, expiring, single-use approval. Merely running as the same Windows user or emitting a model tool call is insufficient.

Secret and deletion interfaces never display secret values. A secret warning shows only category, detected field name, source location, and remediation status.

### 9.2 Reports and health

- Generate a daily project report and a weekly cross-project report.
- Run deterministic integrity checks daily and a Codex-assisted quality audit weekly when allowance is available.
- Audits propose changes; they do not silently rewrite approved memory.
- Windows notifications are reserved for urgent conditions such as security warnings, unrecoverable processing failures, or approvals blocking important work.
- `doctor` may automatically repair verified OpenMemory-owned configuration after recording before/after state, creating a backup, and writing an audit entry. Ambiguous or security-sensitive repairs require approval.

### 9.3 Updates

OpenMemory checks GitHub for updates. Patch and minor updates may install automatically only after a cryptographic signature or signed attestation chains to a pinned trusted project identity, with a checksum used as an additional integrity check; backup, health checks, and guaranteed rollback are also required. A checksum from the download channel is never publisher authentication. Major versions, permission expansion, and irreversible migrations require explicit approval.

Development releases use free checksums and provenance attestations and apply for qualifying free open-source code signing. Builds without a trusted signature or signed attestation require manual installation and are never auto-installed. A paid Windows signing service, expected to cost about $10 per month, is considered only for public v1 when free signing is unavailable or insufficient and requires separate approval before any charge.

## 10. Backup, migration, and transfer

Routine encrypted backups rotate automatically. Manually pinned backups are never removed by rotation. Recovery supports both a protected local key and a user-held recovery key.

A portable export includes selected Markdown, JSONL, attachments, graph data, and checksums. If the export is plaintext, OpenMemory must give a clear warning before creation.

Hardware transfer must move the encrypted database, vault, configuration manifest, recovery material chosen by the user, and integrity metadata. The destination verifies identity, checksums, database decryption, indexes, and reciprocal paths before becoming authoritative. Evidence that cannot be reconnected remains preserved rather than discarded.

On first setup, existing Codex, Claude Code, Antigravity, Git, approved manual sources, and the old Obsidian memory vault can be scanned once and imported as untrusted evidence. The old vault is never deleted automatically. After a verified import and backup, OpenMemory may present an exact-path deletion review for separate approval.

## 11. Privacy and storage constraints

The authoritative private database is encrypted with SQLCipher and stored under `%LOCALAPPDATA%\OpenMemory`. Credentials and encryption keys use Windows-protected storage; a recovery key supports migration and disaster recovery. The user-selected Obsidian vault contains intentionally readable Markdown and must never receive raw private history by accident.

Both locations have stable installation identifiers and reciprocal address manifests. If the chosen vault is in OneDrive or another synchronization folder, private storage remains outside that folder and the user receives a warning before plaintext notes are allowed to synchronize.

The detailed rules are normative in [DATA_AND_PRIVACY.md](DATA_AND_PRIVACY.md).

## 12. Explicit version 1 exclusions

Version 1 does not include:

- multiple users, teams, accounts, or a hosted cloud database;
- Linux or macOS support;
- IDE integrations, Cursor, Hermes, or a separate desktop application;
- agent orchestration or autonomous task execution;
- a separate graph database such as Neo4j or FalkorDB;
- autonomous authoritative reflections;
- automatic deletion of raw history or imported vaults;
- autonomous installation of generated skills;
- an editable knowledge-graph canvas;
- a duplicate archive of every Git diff;
- silent sharing of complete evidence across projects;
- a general importer SDK or broad third-party importer catalog;
- a local generative model or fallback model provider;
- compatibility exports tailored to other memory engines;
- a requirement that Obsidian be installed.

## 13. Acceptance requirements

Before public version 1, automated and clean-machine tests must demonstrate:

- encrypted database confidentiality, reopen, recovery, backup rotation, pinned retention, restore, and hardware transfer;
- no secret value in persisted data, logs, warnings, embeddings, exports, or model requests;
- resistance to prompt injection from every evidence source;
- registered-client capability enforcement and failure of attempts by model calls or unregistered same-user processes to bypass trusted human confirmation;
- correct bitemporal history, entity ambiguity, supersession, conflicts, project boundaries, and global promotion;
- complete resumable capture and duplicate prevention for all three terminal clients;
- immediate `/store` that demonstrably extracts the defined richer schema beyond the routine-capture baseline without raising claim authority, plus one-hour quiet processing, 2:00 AM catch-up, and allowance pause/resume;
- setup opt-in, visible egress disclosure, revocation, and continued local capture/retrieval while external Codex processing is disabled;
- silent bounded prefetch, deeper on-demand access, and complete read auditing;
- measured hybrid retrieval quality against a versioned evaluation set, with no known restricted or isolated-project leakage;
- correct incremental code and Git graphs for every selected language, branch, and worktree;
- full operation with Obsidian installed, closed, and absent;
- safe install, automatic startup, repair, publisher-authenticated update, tamper rejection, rollback, uninstall, migration, and transfer on Windows 11 x64.
