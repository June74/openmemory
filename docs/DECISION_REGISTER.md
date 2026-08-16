# OpenMemory Decision Register

> Planning baseline. “Approved” means the product direction is settled; it does not mean the capability has been implemented. Deferred items require a future decision or are outside version 1.

Related documents:

- [Product requirements](PRODUCT_REQUIREMENTS.md)
- [Data and privacy design](DATA_AND_PRIVACY.md)

## 1. Approved product decisions

| ID | Approved decision | Consequence |
|---|---|---|
| D-001 | Keep the product and repository name **OpenMemory** despite existing projects with similar names. | Documentation must distinguish this project clearly; package, executable, and publication-name collision checks remain part of Stage 0. |
| D-002 | OpenMemory is an open-source, personal, single-user, local application. | Version 1 does not design for tenants, collaboration, accounts, or a hosted control plane. |
| D-003 | Version 1 supports Windows 11 x64 only. | Windows integration, installation, startup, security, and clean-machine testing take priority over portability. |
| D-004 | Use a staged program: private development milestones followed by one complete public v1. | Partial internal builds are allowed, but the public v1 must contain every approved v1 feature and pass release gates. |
| D-005 | OpenMemory is a memory system, not an agent orchestrator. | It supplies evidence and durable context but does not autonomously plan or execute users' projects. |
| D-006 | Obsidian is the primary human-facing interface but remains optional. | All core operations need terminal/MCP equivalents, and closing Obsidian cannot stop the service. |
| D-007 | Keep raw evidence and curated memory separate. | Complete evidence can be inspected for verification without flooding normal AI context. |

## 2. Approved technology decisions

| ID | Approved decision | Consequence |
|---|---|---|
| D-010 | Plan the Windows service in C# on .NET 10 and the Obsidian plugin in TypeScript. | Stage 1 must prove the complete Windows packaging, SQLCipher, MCP, parsing, and clean-machine path before feature construction. |
| D-011 | Use SQLCipher as the encrypted authoritative private database under `%LOCALAPPDATA%\OpenMemory`. | Database migrations, backup, restore, and key recovery become release-critical contracts. |
| D-012 | Protect the data key with Windows user-scoped storage and provide a recovery key. | Normal use stays transparent to the local user while hardware transfer and disaster recovery remain possible. |
| D-013 | Keep the full temporal and code graphs inside SQLCipher rather than operate a separate graph database. | The service owns graph traversal, indexing, and migration; deployment remains one local data system. |
| D-014 | Use a small offline embedding model for vector search but no local generative model. | Semantic indexing can continue locally; extraction and reflection wait when Codex processing is unavailable. |
| D-015 | Communicate locally through MCP and a user-restricted Windows named pipe rather than a listening network service. | Each client can use neutral tools while the core remains a singleton with one database writer. |
| D-016 | Store provider identity only in encrypted private provenance. | Facts, notes, relationships, reports, and ranking signals remain client-neutral while evidence stays traceable. |
| D-017 | Distribute the Windows application through a per-user MSI installer; portable export moves memory but is not the primary runnable installation. | Startup registration, repair, upgrade, and uninstall have one conventional Windows lifecycle while transfer remains a separate data workflow. |
| D-018 | Use free checksums and provenance attestations during development, apply for qualifying free open-source signing, and consider a roughly $10/month Microsoft signing service only for public v1 with separate spending approval. Automatic installation requires a signature or signed attestation anchored to a pinned trusted project identity; a checksum is integrity evidence only. | Development remains free and no paid service is activated implicitly; unsigned development builds are manual-install only and may receive Windows reputation warnings. |
| D-019 | Sideload the Obsidian plugin during development/private beta, then submit the hardened plugin to the Obsidian community directory for public v1. | Core terminal operation cannot depend on external plugin-review timing. |

## 3. Approved capture and processing decisions

| ID | Approved decision | Consequence |
|---|---|---|
| D-020 | Initial terminal clients are Codex CLI, Claude Code, and Google Antigravity (`agy`). | Three adapters and importers share one frozen, provider-neutral event contract. Other clients wait until after v1. |
| D-021 | Capture supported future chats and tool results automatically. | Hooks/adapters and reconciliation importers must be resumable and duplicate-safe. |
| D-022 | An inline `/store` waits for and prioritizes the current complete user/assistant/tool turn; a standalone `/store` targets the preceding complete turn. It uses a larger extraction budget and richer schema than routine capture to seek goal changes, decisions, requirements, constraints, rationale, task state, project-relevant user preferences, artifacts and evidence, lessons, and open questions. | Purposeful storage increases processing depth and priority, not authority, and remains subject to secret scanning, provenance, validation, and conflict rules. |
| D-023 | Ordinary processing waits for a one-hour quiet period, with a daily catch-up at 2:00 AM local time. | Queues need durable scheduling and restart-safe checkpoints. |
| D-024 | After explicit setup opt-in, use the user's Codex subscription through `codex exec`, sending the minimum redacted evidence needed; allow consent to be revoked. | Model-dependent work pauses on revocation, authentication failure, or allowance exhaustion without silently switching providers, while local capture and retrieval continue. |
| D-025 | Automatically analyze approved attachments after pre-persistence secret scanning; quarantine only redacted content, non-secret metadata, and source pointers. | OpenMemory never persists the suspected value, and attachment processing cannot bypass the normal privacy boundary. |
| D-026 | Historical v1 imports cover Codex, Claude Code, Antigravity, Git, the old Obsidian vault, and approved manual sources. | A broad importer SDK and unrelated service importers are deferred. |
| D-027 | Treat all captured and imported material as untrusted evidence. | Prompt-like text has no execution or approval authority, and generated memories must retain citations. |

## 4. Approved memory and retrieval decisions

| ID | Approved decision | Consequence |
|---|---|---|
| D-030 | Preserve complete encrypted chats and tool evidence while deriving concise memories. | Normal retrieval returns bounded context; full evidence stays available on demand. |
| D-031 | Maintain global, project, task-snapshot, and project-relevant user memory. | Cross-project user preferences can be shared, but project-specific beliefs and goals remain scoped. |
| D-032 | Implement a full temporal knowledge graph with valid time, recorded time, provenance, and preserved supersession. | The system can answer both “when was this true?” and “when did we know it?” without overwriting history. |
| D-033 | Allow provisional reflections with supporting and opposing evidence, but never let them override approved facts. | Reflection helps discovery without becoming autonomous authority. |
| D-034 | Make failed attempts first-class memories and warn before materially similar operations. | Retrieval and code/task matching need outcome-aware similarity, not only topic similarity. |
| D-035 | Use hybrid keyword, vector, metadata, graph, and temporal retrieval. | Ranking is a versioned, tested domain rule rather than a single vector nearest-neighbor query. |
| D-036 | Select task-aware retrieval mode automatically and allow manual override. | The default stays low-friction while advanced users can control retrieval behavior. |
| D-037 | Silently prefetch a compact memory packet and allow the AI to read deeper memory at will. | Reads do not require repeated approval, but every returned record is audited. |
| D-038 | Apply relevance decay only to ranking and never to retention. | Core, pinned, and approved records remain available; no memory disappears due to age alone. |
| D-039 | Weight explicit user feedback more strongly than AI self-reported retrieval feedback. | Retrieval can improve from outcomes without letting a model unilaterally define success. |
| D-040 | Permit portable global technical knowledge and provisional cross-project suggestions. | Cross-project retrieval remains subject to project sensitivity and does not silently move complete evidence. |
| D-041 | Support Normal, Restricted, and Isolated project sensitivity. | Normal permits concise cross-project summaries, Restricted requires approval, and Isolated blocks cross-project flow. |
| D-042 | Use conservative entity matching and review ambiguous identity. | Avoiding a false merge takes priority; uncertain records remain separate until approved. |
| D-043 | Automatically close an old claim only for a clear temporal transition. | Genuine contradictions, identity ambiguity, and authority conflicts go to review. |
| D-044 | Mark memory possibly stale and add opposing evidence when code changes contradict it. | Code-derived signals do not silently rewrite human-approved memory. |

## 5. Approved authority and privacy decisions

| ID | Approved decision | Consequence |
|---|---|---|
| D-049 | MCP and model calls may list or propose protected changes but cannot approve them; conflicts, first global promotions, material deletions, sensitivity reductions, and permission expansions require a trusted local, action-bound, expiring, single-use human confirmation. | Same-user pipe access is not treated as human intent; Stage 0 must define registered-client capabilities and prove resistance to replay and local spoofing. |
| D-050 | The first promotion of project knowledge to global memory requires approval. | Global scope is deliberate and reviewable. |
| D-051 | Compatible refinements to already approved global information can be stored automatically; real replacements or conflicts require review. | Everyday learning remains automatic without removing user control over meaning. |
| D-052 | Conflict review shows before, after, context, and supporting/opposing evidence. | The user can understand the complete scope before choosing keep, replace, merge, defer, or reject. |
| D-053 | Redact secrets before persistence, embedding, display, export, or Codex processing. | Secret detection is a first-boundary control, not later cleanup. |
| D-054 | Issue an immediate secret warning that shows category/name, location, and status but never the value. | The user can remediate exposure without OpenMemory repeating it. |
| D-055 | Do not automatically delete raw histories or imported data. | Deletion requires an exact target review and separate approval. |
| D-056 | Keep routine encrypted backup rotation, while manually pinned backups never expire automatically. | Storage stays bounded for ordinary backups without risking deliberately preserved recovery points. |
| D-057 | Support encrypted recovery, portable export, and complete hardware transfer. | Changing computers must preserve identities, evidence, graph history, checksums, and vault relationships. |
| D-058 | Warn before creating plaintext portable exports or allowing the vault to synchronize. | The user can knowingly choose portability or synchronization without exposing the encrypted store. |
| D-059 | Keep private storage outside detected cloud-sync roots; vault and private store have stable IDs and reciprocal addresses. | The two folders can move independently and find each other without putting raw private data in OneDrive. |

## 6. Approved code, UX, and operations decisions

| ID | Approved decision | Consequence |
|---|---|---|
| D-060 | Build a complete structural code graph for C#, TypeScript, JavaScript, Python, Rust, Go, Java, C, C++, HTML, CSS, SQL, and PowerShell. | Language-specific parsing can be parallelized behind a common graph contract and shared acceptance suite. |
| D-061 | Index full Git history structurally without duplicating every complete diff. | Git remains the source for reproducible diffs while memory links decisions and symbols over time. |
| D-062 | Update code graphs incrementally after attach, save, supported tool edit, commit, branch change, pull, or merge. | Watchers must be throttled and branch/worktree aware. |
| D-063 | Share durable project knowledge across branches/worktrees but keep live task and code state separate. | Parallel development does not contaminate the active state of another worktree. |
| D-064 | Provide a read-focused temporal graph explorer in Obsidian. | Version 1 supports navigation and evidence review, not free-form graph editing. |
| D-065 | Make intentional Markdown edits two-way, high-authority, versioned proposals. | Obsidian remains genuinely editable without making Markdown an unsafe overwrite channel. |
| D-066 | Show raw history through read-only on-demand plugin views while it stays encrypted. | The vault remains useful without becoming a plaintext transcript archive. |
| D-067 | Generate daily project reports and weekly cross-project reports. | Summaries remain regular artifacts, subject to project-sensitivity rules. |
| D-068 | Run daily deterministic integrity checks and weekly Codex-assisted quality audits. | Automated audits propose repairs or memory changes; they do not silently alter approved knowledge. |
| D-069 | Let `doctor` automatically repair only verified OpenMemory-owned state with before/after, backup, and audit. | Ambiguous, external, destructive, or security-sensitive repairs require approval. |
| D-070 | Reserve Windows notifications for urgent conditions. | Background operation stays quiet while security and blocking failures surface promptly. |
| D-071 | Automatically install patch/minor updates only after publisher authentication by a signature or signed attestation anchored to a pinned project identity, plus checksum, backup, health check, and rollback; require approval for major or permission-changing updates. | Routine maintenance is low-friction without treating same-channel checksums as identity or silently expanding authority. |
| D-072 | Provide `/memory` commands and an `openmemory` terminal fallback. | Users retain a consistent control surface even when native slash-command integration differs by client. |
| D-073 | Generated project playbooks or client skills require explicit approval before installation. | OpenMemory can propose reusable behavior but cannot alter an agent's capabilities autonomously. |

## 7. Approved migration decisions

| ID | Approved decision | Consequence |
|---|---|---|
| D-080 | Scan the previous Obsidian memory vault once and import it as untrusted evidence. | Old summaries can inform new memory but receive no automatic authority. |
| D-081 | Preserve sources that cannot be reattached after hardware transfer. | Disconnected evidence remains searchable and auditable rather than being discarded. |
| D-082 | Offer deletion of the previous vault only after verified import and backup. | The deletion review must list exact paths and requires a separate approval. |
| D-083 | Use a portable format containing selected Markdown, JSONL, attachments, graph data, and checksums. | Users are not locked into the encrypted database for long-term access, but plaintext exposure is warned clearly. |

## 8. Explicit version 1 exclusions

The following are settled as outside version 1, not accidental omissions:

| ID | Excluded from v1 | Reason or consequence |
|---|---|---|
| X-001 | Separate graph database | SQLCipher remains the only authoritative data service. |
| X-002 | Autonomous authoritative reflections | Reflections stay provisional unless approved through normal authority rules. |
| X-003 | Automatic raw-history or old-vault deletion | Destructive operations remain user-approved. |
| X-004 | Autonomous skill installation | Generated skills are proposals only. |
| X-005 | Editable graph canvas | The Obsidian graph view is read-focused. |
| X-006 | Duplicate archive of every Git diff | Structural history references Git evidence. |
| X-007 | Silent complete-evidence sharing across projects | Retrieval uses summaries and sensitivity rules. |
| X-008 | Broad importer SDK or third-party importer catalog | Initial migration focuses on the approved clients and sources. |
| X-009 | Local generative model or fallback paid provider | Codex-dependent work pauses when unavailable. |
| X-010 | Agent orchestration | OpenMemory supplies memory only. |
| X-011 | Other-engine compatibility exports | Version 1 provides neutral portable data, not engine-specific adapters. |
| X-012 | Mandatory Obsidian installation | Terminal and MCP workflows remain complete. |
| X-013 | Multi-user, cloud-hosted, macOS, or Linux operation | Windows single-user reliability is the initial target. |
| X-014 | IDE, Cursor, Hermes, or separate desktop-app integration | Codex, Claude Code, and Antigravity terminal clients are the v1 integration set. |

## 9. Deferred implementation decisions

These items are intentionally left for Stage 0 design or Stage 1 feasibility testing. Implementers must not silently choose them in unrelated feature work.

| ID | Deferred item | Required decision evidence |
|---|---|---|
| F-001 | Exact MSI authoring tool, bootstrapper, and upgrade implementation | Clean Windows 11 install/update/uninstall proof and reproducible release design; the approved MSI distribution format does not change. |
| F-002 | Exact database schema, migrations, graph indexes, and vector representation | Prototype measurements, migration/rollback tests, and the repository-grounded threat model. |
| F-003 | Exact authority and hybrid-ranking weights | Versioned domain-logic contract and retrieval evaluation corpus. |
| F-004 | Local embedding model and redistribution terms | Quality, latency, footprint, offline behavior, license, and security review. |
| F-005 | Client-specific hook and slash-command packaging details | Live capability verification against current Codex, Claude Code, and Antigravity versions. |
| F-006 | Backup cadence, rotation counts, and retention defaults | Storage-size measurements plus restore and computer-transfer exercises. |
| F-007 | Exact free or paid signing provider and attestation implementation | Threat model, available open-source signing programs, and explicit approval before any paid service; the staged signing policy remains fixed. |
| F-008 | Exact Obsidian community-submission date and review checklist | Plugin maturity, privacy review, and current community-directory requirements; sideload-then-submit remains fixed. |
| F-009 | Quantitative retrieval, indexing, and startup performance targets | Stage 1 baselines on representative personal repositories and histories. |
| F-010 | Repository-grounded threat model and subsystem implementation plans | Create after the repository structure and executable trust boundaries exist. |

## 10. Source-choice traceability

The planning interview used temporary A/B/C/D option labels. The normalized `D-*` entries above are the durable decisions; this appendix preserves which option produced them so the baseline can be audited. Repeated decision IDs are intentional where a later question refined an earlier choice.

The pre-interview naming decision was stated directly rather than selected by letter: retain **OpenMemory** despite name-collision warnings (`D-001`).

Two preliminary choices established the product and Obsidian boundaries before the later feature interview:

| Seq. | Choice question | Selected | Resulting decision(s) |
|---:|---|:---:|---|
| P1 | What should v1 be: personal local, multi-user, or developer framework? | **A** — personal-first and local-only | `D-002` |
| P2 | What should editing in Obsidian do? | **A** — curated memory is editable while raw history is locked; the user also requested `/store` | `D-007`, `D-022`, `D-065`, `D-066` |

The subsequent feature and competitor-inspired interview produced these selections:

| Seq. | Choice question | Selected | Resulting decision(s) |
|---:|---|:---:|---|
| 1 | Memory relationships: lightweight layer, none, or full temporal knowledge graph | **C**, answered in words as “full temporal knowledge graph” | `D-032` |
| 2 | Where should the graph be stored? | **A** — inside encrypted SQLCipher | `D-013` |
| 3 | Should OpenMemory derive new insights? | **A** — provisional reflections | `D-033` |
| 4 | Remembering failed approaches | **A** — store attempts and warn proactively | `D-034` |
| 5 | Turning lessons into reusable procedures | **B** — playbooks plus reviewed skill drafts | `D-073` |
| 6 | Should OpenMemory index project code like Graphify? | **B** — complete structural code graph | `D-060` |
| 7 | Which programming languages receive deep understanding? | **B** — expanded language set | `D-060` |
| 8 | How much Git history should become memory? | **A** — complete structural history without duplicating every diff | `D-061` |
| 9 | Should retrieval adapt to the type of work? | **D** — automatic mode with manual override | `D-036` |
| 10 | Should old memories gradually rank lower? | **A** — non-destructive relevance decay | `D-038` |
| 11 | How should the temporal graph appear in Obsidian? | **A** — focused read-only explorer | `D-064` |
| 12 | Should OpenMemory learn which retrieved memories are useful? | **A** — outcome-aware feedback, user weighted more strongly | `D-039` |
| 13 | Can technical lessons move between projects? | **D** — approved global technical knowledge plus provisional cross-project suggestions | `D-040`, `D-050` |
| 14 | What may cross-project suggestions reveal? | **D** — Normal, Restricted, and Isolated sensitivity levels | `D-041` refining `D-040` |
| 15 | Which historical sources should v1 import? | **A** — approved sources only | `D-026` |
| 16 | What happens when Codex processing is unavailable? | **A** — queue and wait for allowance; no local or fallback LLM | `D-014`, `D-024` |
| 17 | Should OpenMemory repair broken integrations? | **D** — automatically repair verified owned entries with review history | `D-069` |
| 18 | How should software updates work? | **D** — authenticated updates with backup and rollback | `D-071` |
| 19 | Which releases may install automatically? | **A** — patch/minor only; major, permission, and irreversible changes require approval | `D-071` |
| 20 | Which explicit memory commands should v1 expose? | **C** — comprehensive commands | `D-072` |
| 21 | How should these commands be named? | **A** — `/store` plus the `/memory …` namespace | `D-072` |
| 22 | How should screenshots, PDFs, and attachments become searchable? | **B** — automatic Codex analysis after secret scanning and safe quarantine | `D-025` |
| 23 | When should the complete code graph update? | **A** — throttled background incremental updates | `D-062` |
| 24 | How should Git branches and worktrees share memory? | **A** — shared durable project knowledge, separate live states | `D-063` |
| 25 | Should OpenMemory coordinate multiple working agents? | **A** — memory only, no orchestration | `D-005` |
| 26 | Should OpenMemory generate activity summaries? | **B** — daily project and weekly cross-project reports | `D-067` |
| 27 | How should OpenMemory notify the user? | **B** — Windows notifications for urgent events only | `D-070` |
| 28 | Should users be able to leave OpenMemory without losing portability? | **B** — documented portable export plus full transfer | `D-057`, `D-058`, `D-083` |
| 29 | How should the graph decide that two names mean the same thing? | **A** — conservative identity matching | `D-042` |
| 30 | Is an explicit change over time a conflict? | **A** — clear transitions are historical versions, not conflicts | `D-043` |
| 31 | What happens when code changes make a memory potentially outdated? | **A** — mark for revalidation and attach opposing evidence | `D-044` |
| 32 | Should OpenMemory perform scheduled memory-quality audits? | **A** — daily deterministic plus weekly Codex audit, proposals only | `D-068` |
| 33 | What if OpenMemory is placed in OneDrive or another synchronized folder? | **A** — warn, split storage, and keep reciprocal stable addresses | `D-058`, `D-059` |
| 34 | Is Obsidian required to use OpenMemory? | **A** — recommended but optional | `D-006` |
| 35 | How should this large v1 be delivered? | **B** — staged internal program, complete public v1 | `D-004` |
