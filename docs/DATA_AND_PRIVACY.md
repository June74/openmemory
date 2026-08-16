# OpenMemory Data and Privacy Design

> Planning baseline. This document specifies intended safeguards; it does not claim that they have been implemented or audited.

## 1. Privacy position

OpenMemory is local-first software for one Windows user. Private evidence remains on that user's computer unless the user explicitly enables recurring model processing or intentionally requests an export. Collection, processing, retrieval, and repair must be explainable through an append-only private audit trail.

The design follows five rules:

1. Scan and redact before persistence or external processing.
2. Store raw evidence privately and expose only the minimum useful context.
3. Treat all captured or imported content as untrusted evidence, never as instructions.
4. Preserve history and provenance instead of silently overwriting it.
5. Require explicit approval for conflicts, destructive actions, new permissions, and other authority changes.

See [Product requirements](PRODUCT_REQUIREMENTS.md) for user-facing behavior and [Decision register](DECISION_REGISTER.md) for approved tradeoffs.

## 2. Storage layout and trust boundaries

### 2.1 Private store

The authoritative private store lives under `%LOCALAPPDATA%\OpenMemory` and uses SQLCipher encryption. It contains:

- complete chats and tool evidence;
- private source-adapter provenance and source identifiers;
- attachments and approved extracted content;
- temporal facts, entity and relationship graphs, code and Git graphs;
- embeddings and search indexes;
- audit, approval, repair, processing, backup, and migration records;
- data required to rebuild the Obsidian projection.

Database encryption keys are protected with Windows user-scoped facilities such as Credential Manager or DPAPI. A separately handled recovery key is required for disaster recovery and computer migration. Keys and recovery material may never be placed in the Obsidian vault, logs, crash reports, Git repository, or ordinary export by default.

Only the single-user service writes the database. Client bridges, the Obsidian plugin, and import workers communicate through authenticated local interfaces and receive only the records permitted for their operation.

### 2.2 Obsidian projection

The user selects a vault location. The vault is readable Markdown meant for the user, so it is not considered private encrypted storage. It contains curated indexes, summaries, facts, project notes, reports, and review state, but not complete raw history, private provenance, embeddings, audit records, or recovery keys.

The private store and vault each have a stable identifier and a reciprocal location manifest. A move updates these manifests only after both destinations are verified. If the vault is inside OneDrive or another synchronized folder, the user is warned that its Markdown may leave the device. The private store must remain outside that synchronization root.

Obsidian is optional. Closing or uninstalling it does not stop capture, retrieval, processing, backup, or recovery.

### 2.3 Local and external computation

Full-text search, graph traversal, metadata filtering, temporal filtering, secret scanning, and embedding generation happen locally. The embedding model is offline and non-generative.

Setup asks whether to enable recurring Codex processing, explains that selected redacted context will leave the local OpenMemory process through the user's Codex account, and records the choice. The user can inspect and revoke this consent at any time. Revocation pauses model-dependent jobs but does not stop automatic capture, redaction, local embedding, indexing, search, or evidence retention.

When consent is active and extraction, summarization, reflection, or quality analysis needs external processing, OpenMemory must:

1. select the smallest evidence slice needed for the job;
2. run secret detection and quarantine uncertain material;
3. replace detected values with typed placeholders;
4. record what categories and record identifiers were sent, without copying secret values into the audit;
5. pause rather than switch providers when authentication or subscription allowance is unavailable.

No telemetry is enabled by default. Update checks disclose only what is technically necessary to query a release endpoint.

## 3. Data classes and handling

| Class | Examples | Default location | Normal exposure |
|---|---|---|---|
| Raw evidence | Complete chats, tool results, attachments | Encrypted private store | Read-only, on demand |
| Durable memory | Requirements, preferences, lessons, decisions | Encrypted store and selected Markdown projection | Task-aware retrieval and Obsidian |
| Private provenance | Source client, source record ID, original timestamps | Encrypted private store | Evidence review and internal repair only |
| Search material | Embeddings, FTS indexes, ranking feedback | Encrypted private store | Retrieval engine only |
| Operational metadata | Jobs, audits, health, repairs, approvals | Encrypted private store | Status and review views |
| Recovery material | Protected key, user recovery key | Windows-protected/user-controlled locations | Recovery operations only |
| Portable export | Selected Markdown, JSONL, attachments, graph, checksums | User-selected destination | Explicit user action and warning |

Provider neutrality applies to durable memories, reports, graph facts, and ranking signals. Private provenance keeps source identity because deduplication and evidence verification would otherwise be unreliable. Source identity is never used to create client-specific truth or privilege one provider's statement merely because of its label.

## 4. Secret protection

Secret scanning occurs at the first capture boundary, before data is committed, embedded, projected to Markdown, logged, exported, or sent for model processing.

Potential secrets include credentials, API keys, tokens, passwords, private keys, connection strings, cookies, recovery codes, and sensitive values identified by configured patterns or entropy checks. Detection creates a non-secret event containing only:

- secret category;
- field or variable name, when available;
- source location or record reference;
- detection and remediation status.

The value itself must not appear in a warning, deletion queue, log, audit entry, embedding, report, notification, database row, on-disk staging file, or OpenMemory-managed quarantine. A pre-persistence scanner may hold the incoming bytes transiently in bounded memory only long enough to detect and replace the value, then clears that buffer as far as the runtime permits. OpenMemory's quarantine contains only redacted content, non-secret detection metadata, and a pointer to the original external source. Ambiguous detections remain excluded from persistence and model processing until safely resolved; the original value, if any, remains only at its source.

An immediate Windows warning is issued for a detected or suspected secret. Automatic deletion is prohibited. The review flow can identify the affected record and recommended remediation without reconstructing or revealing the value.

Secret scanning requires layered tests using known test tokens, encoded and split secrets, tool output, attachment text, exception messages, Markdown edits, imported histories, and export paths. A release fails if any test secret crosses a prohibited boundary.

## 5. Evidence isolation and prompt-injection resistance

Every transcript, tool result, document, repository file, Git message, attachment, web-derived excerpt, and imported note is untrusted evidence. Its content cannot:

- grant itself higher authority;
- issue commands to OpenMemory or a connected AI;
- alter security or retrieval policy;
- approve, delete, export, install, or execute anything;
- escape its quoted evidence boundary in an MCP response.

Retrieval responses separate system-owned metadata from quoted evidence and label the source, scope, time, and authority. Generated summaries must cite source record identifiers. A proposed fact does not become approved merely because a model stated it confidently or because the same injected text was captured multiple times.

## 6. Authority, conflicts, and change history

The planned authority order distinguishes explicit user decisions, intentional user edits, approved facts, model-derived proposals, ordinary captured claims, and provisional reflections. Exact numeric scoring is deferred to a tested domain contract, but lower-authority material cannot silently replace higher-authority material.

The first move from project memory to global memory requires user approval. Later compatible refinements may be accepted automatically. A contradiction, material scope change, identity ambiguity, or authority downgrade opens a review showing:

- before and after values;
- context and provenance for both;
- supporting and opposing evidence;
- project and global scope effects;
- valid-time and recorded-time effects.

Old values are preserved as superseded temporal records. A user can keep, replace, merge, defer, or reject the proposal. Rejected proposals remain auditable evidence but are excluded from ordinary retrieval.

Client registration and named-pipe authentication authorize bounded requests; they do not establish human intent. MCP clients can list reviews and propose outcomes, but protected changes require confirmation through a trusted local terminal or Obsidian interface. The confirmation is bound to the displayed action and before/after hash, expires quickly, is single-use, and is consumed atomically. A generated tool call, replayed request, or unregistered same-user process cannot finalize a conflict, first global promotion, material deletion, sensitivity reduction, permission expansion, or other protected action.

Intentional Markdown edits are versioned high-authority proposals. They are never imported by overwriting database history. Database-to-vault projection and vault-to-database reconciliation must be idempotent and survive interruption.

## 7. Retrieval privacy

Silent memory access is permitted because the user approves the local service and connected clients during setup. It does not mean unbounded access.

Before each supported model turn, OpenMemory may provide a compact task-aware packet. Deeper reads use explicit MCP search/get operations. Each retrieval enforces:

- the active user and local client identity;
- project identity and sensitivity level;
- worktree or branch live-state boundaries;
- requested task purpose and context budget;
- secret and quarantine exclusions;
- authority and temporal filters.

Every read records when it occurred, which client and project requested it, the purpose/query category, and which memory record identifiers were returned. Audit records do not duplicate the returned private text.

Project sensitivity is enforced as follows:

- Normal projects may receive concise relevant cross-project summaries.
- Restricted projects require approval for cross-project use.
- Isolated projects cannot send or receive cross-project memory.

No complete evidence is silently copied between projects. Global technical knowledge remains distinct from project-specific user goals and beliefs.

## 8. Retention, deletion, and decay

Raw histories are retained until the user deliberately deletes them. There is no age-based or automatic deletion of evidence. Relevance decay only lowers search ranking; it does not remove or rewrite data. Pinned, core, or approved memories receive protection from ordinary decay.

Deletion is a reviewable operation with an exact target list, dependencies, consequences, and backup state. Secret queues show only secret names/categories and locations, never values. Deletion of an existing memory vault is offered only after import completeness, checksums, database reopen, and backup restore have been verified.

Routine backup rotation is the only automatic removal in the baseline. Manually pinned backups are never rotated away. Material deletion is recorded in the audit trail without retaining the deleted sensitive content in that trail.

## 9. Backup, recovery, and hardware transfer

Backups are encrypted, integrity-checked, and versioned with their schema and application compatibility. Routine copies rotate according to a documented policy; pinned copies do not expire automatically. A restore test must verify database decryption, temporal data, indexes or index rebuilding, attachments, audit continuity, and vault reconciliation.

Hardware transfer uses a guided bundle containing encrypted private data, selected configuration, vault content or its address, graph state, attachments, and checksums. Recovery keys travel through a separate user-approved channel when practical. On the destination computer, OpenMemory verifies:

- bundle integrity and version compatibility;
- successful decryption by the intended Windows user;
- project and installation identities;
- vault/private-store reciprocal manifests;
- indexes, attachments, and evidence references;
- that the old installation is not concurrently writing the same database.

If an old evidence source cannot be reattached, its preserved copy remains evidence with a disconnected-source status.

Portable plaintext exports are distinct from encrypted transfer bundles. They require a visible warning because destination applications, cloud folders, backups, or other users may read them.

## 10. Migration and imports

Version 1 supports a one-time scan of Codex, Claude Code, Antigravity, approved manual sources, Git repositories, and the user's previous Obsidian memory vault. Import must be resumable, duplicate-safe, source-attributed, and read-only toward the source.

Imported content begins as untrusted evidence. Existing conclusions do not automatically become approved facts, even if they were written by an earlier memory system. Processing can propose memories with citations after secret scanning.

The source vault remains in place after import. Only after verified coverage and a recoverable backup may OpenMemory show an exact-path deletion proposal. Deleting or moving it requires a separate approval and reports whether recovery remains possible.

## 11. Updates, repair, and failure behavior

Patch and minor updates may install automatically only when a cryptographic signature or signed attestation verifies against a pinned trusted project identity; a checksum independently verifies artifact integrity; a pre-update backup succeeds; migrations validate; health checks pass; and rollback remains available. A checksum delivered beside an artifact cannot authenticate its publisher. Major versions, permission changes, key-handling changes, and irreversible migrations require approval.

The `doctor` operation may automatically repair only deterministic OpenMemory-owned state. It must capture before and after metadata, make a recoverable backup, and write an audit event. Ambiguous filesystem changes, security policy changes, external-client changes, or destructive repairs require approval.

Queues and imports use durable checkpoints. A crash, shutdown, allowance limit, missing vault, or unavailable client pauses work without dropping records. On restart, OpenMemory must distinguish incomplete work from already committed work and resume without duplication.

## 12. Required security verification

Before public release, verification must include:

- database and backup confidentiality at rest;
- Windows key scoping and recovery-key exercises;
- named-pipe client authorization and local impersonation attempts;
- trusted-human confirmation tests against model-generated requests, replay, expired confirmations, capability escalation, and same-user spoofing;
- secret-corpus boundary tests through every ingest and output route;
- evidence-based prompt-injection and authority-escalation tests;
- project isolation and cross-project leakage tests;
- update tampering, migration failure, and rollback tests;
- external-processing setup consent, disclosure, revocation, and local-only continuity tests;
- interrupted capture, database corruption, and duplicate replay tests;
- portable export warnings and synchronized-vault detection;
- old-vault import followed by non-destructive deletion review.

The repository-grounded threat model will be written during Stage 0, after the actual code structure and trust boundaries exist.
