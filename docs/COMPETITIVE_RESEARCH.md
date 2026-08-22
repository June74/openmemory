# Competitive research

Status: planning baseline, first written 2026-08-16, refreshed 2026-08-22. OpenMemory implementation has not begun.

## How to read this document

OpenMemory was planned after reviewing public descriptions, documentation, and repositories for other memory systems. These projects are references, not dependencies or proof that a particular design will work for OpenMemory. Capabilities can change after this snapshot, and claims below are intentionally limited to what the linked projects publicly describe.

OpenMemory adopts public architectural patterns and product lessons. It does not copy another project's source code or claim compatibility with its internal data model. Before implementation, every borrowed idea must be expressed as an OpenMemory requirement, tested against OpenMemory's privacy model, and implemented independently under the repository's license and contribution rules.

## Projects and patterns reviewed

### Graphiti

[Graphiti](https://github.com/getzep/graphiti) describes a temporal context graph that connects entities, relationships, source episodes, and validity windows. It also documents hybrid semantic, keyword, and graph retrieval.

Ideas incorporated:

- facts retain both their source evidence and their history instead of being overwritten;
- temporal queries distinguish what was believed at different times;
- entity and relationship retrieval complements flat document search;
- new evidence can invalidate a current claim without erasing the earlier claim.

Not adopted for v1:

- a separate Neo4j, FalkorDB, or other graph service;
- Graphiti's implementation, schema, provider defaults, or deployment model.

OpenMemory instead plans a full temporal graph inside its encrypted SQLCipher database. This choice favors a single-user Windows installation over graph-database scale.

### Hindsight

[Hindsight](https://github.com/vectorize-io/hindsight) presents agent memory as more than transcript recall and exposes retain, recall, and reflection concepts. Its public material emphasizes memories that support learning over time.

Ideas incorporated:

- reflections can derive lessons or mental models from accumulated evidence;
- retrieval should consider experience and outcomes, not only similarity;
- memory quality should be evaluated over time.

Not adopted for v1:

- autonomous reflections with the same authority as approved facts;
- a general agent runtime or hosted deployment.

OpenMemory reflections remain provisional. They show supporting and opposing evidence and cannot override an approved memory without review.

### Memorix

[Memorix](https://github.com/AVIDS2/memorix) describes a local-first shared memory layer for multiple coding agents, with MCP access, Git-oriented memory, and transfer tooling.

Ideas incorporated:

- one provider-neutral memory can serve several coding clients;
- project, code, Git, and workstream context belong in the same retrieval system;
- transfer, approvals, and diagnostics are first-class product workflows;
- client adapters should converge on one stable contract.

Not adopted for v1:

- agent orchestration;
- every client or IDE integration supported by Memorix;
- its storage format or implementation.

OpenMemory v1 limits capture integrations to Codex CLI, Claude Code, and Antigravity CLI.

### memoirs

[memoirs](https://github.com/misaelzapata/memoirs) publicly describes a Python local-memory engine using SQLite/FTS5, optional SQLCipher, hybrid retrieval, bi-temporal validity, provenance, import/export, and MCP interfaces.

Ideas incorporated:

- encrypted local storage can combine raw evidence, full-text search, vectors, and provenance;
- hybrid retrieval should be measurable and explainable;
- portable exports need manifests and integrity checks;
- time-travel and source tracing are useful inspection tools.

Not adopted for v1:

- the Python implementation or local generative-model fallback;
- automatic pruning or deletion;
- its exact schema, command surface, or deployment model.

The reference helps establish feasibility, not equivalence. OpenMemory selected a C#/.NET Windows service and will prove SQLCipher packaging, retrieval, and recovery independently.

### Karpathy-style Markdown and Obsidian workflows

The public [LLM Wiki prompt and notes](https://gist.github.com/karpathy/442a6bf555914893e9891c11519de94f), and later community workflows built around them, popularized a simple pattern: preserve raw sources, compile them into a navigable Markdown knowledge base, and let a coding agent search and maintain the result. Obsidian is a convenient editor and navigator for that portable Markdown.

Community write-ups describe the pattern as three layers: immutable raw sources, an LLM-maintained wiki whose pages cross-reference each other and flag contradictions, and a schema or conventions file that fixes page naming, tag vocabulary, and the minimum structure of an entry. The framing is that knowledge is compiled once and then kept current, rather than re-retrieved and re-synthesized on every query. It is a discipline and a prompt, not a service: there is no server, no index contract, no encryption boundary, and no capture mechanism beyond whatever the operator pastes in.

Ideas incorporated:

- human-readable project indexes and linked notes;
- separation between raw evidence, curated knowledge, and generated reports;
- an AI-maintained table of contents rather than injecting every past chat;
- a declared schema for naming, tagging, and minimum record structure, so curated knowledge stays consistent as it grows;
- contradictions surfaced when knowledge is written, not only when it is queried;
- Obsidian as the primary human interface while keeping the memory usable without Obsidian.

Not adopted for v1:

- treating a plaintext vault as the complete private database;
- loading an entire vault or complete chat history into every model turn;
- rewriting a curated page in place, which discards the earlier claim and its evidence;
- depending on operator discipline instead of automatic, resumable capture;
- granting AI-generated notes automatic authority.

OpenMemory keeps sensitive raw history encrypted and projects selected, editable knowledge into Markdown.

### akitaonrails/ai-memory

[akitaonrails/ai-memory](https://github.com/akitaonrails/ai-memory) describes sanitized lifecycle capture, a Markdown wiki usable in Obsidian, bounded handoffs, local indexing, shared use across several agent harnesses, and backup-friendly files.

Its public documentation describes Markdown in a Git repository as the source of truth with SQLite and FTS5 as a derived, rebuildable index, and a sleep-style consolidation cycle in which recent records keep raw detail while older records are summarized, with access counts and exponential decay influencing what is retained. The author also documents it as a response to problems observed in the earlier `agentmemory` project, including reindexing, data loss, and broken hooks.

Ideas incorporated:

- hooks should be lightweight and capture lifecycle events without blocking the client;
- one serialized database writer reduces corruption and concurrency risk;
- a derived index should be rebuildable from durable records, so index damage is recoverable rather than fatal;
- consolidation belongs on a scheduled cycle rather than in the interactive response path;
- retrieval packets must be bounded;
- project and worktree identity need explicit handling;
- raw evidence and curated Markdown serve different purposes.

Not adopted for v1:

- Markdown as the sole source of truth for complete private history;
- decay or access-count pressure that removes records rather than deprioritizing them;
- its launcher, Rust implementation, or exact capture limits;
- assuming lifecycle observations are complete transcripts.

OpenMemory inverts the authority direction: the encrypted database is authoritative and Markdown is a projection of selected knowledge, because a plaintext Git wiki cannot hold complete private history under this project's privacy model.

### memem

[memem](https://github.com/TT-Wang/memem) describes a Claude Code plugin that stores lessons and decisions as Obsidian Markdown, indexes transcripts with FTS5, mines earlier sessions, and scans writes for security risks.

Ideas incorporated:

- Obsidian notes should remain useful outside the AI client;
- transcript search can be separate from curated durable memory;
- importing historical transcripts needs an explicit consent and security boundary;
- prompt-injection and credential checks belong before durable writes.

Not adopted for v1:

- a Claude-only product boundary;
- automatic installation or mutation of client configuration without approval;
- its bootstrap process or plugin implementation.

### Cognee

[Cognee](https://github.com/topoteretes/cognee) describes knowledge pipelines that combine ingestion, graph relationships, vector retrieval, provenance, and feedback.

Ideas incorporated:

- graph, text, and semantic retrieval should be complementary;
- retrieval feedback can improve ranking when its authority is bounded;
- ingestion and transformation should remain traceable.

Not adopted for v1:

- a general knowledge-engine platform, cloud control plane, or pluggable graph database;
- multimodal or multi-user scope beyond OpenMemory's approved personal workflow.

### Letta

[Letta](https://github.com/letta-ai/letta) and [Letta Code](https://github.com/letta-ai/letta-code) describe stateful agents with editable memory blocks, message search, skills, and long-lived agent behavior.

Ideas incorporated:

- agents need explicit tools to inspect and update memory;
- global, project, and task-level context should have different scopes;
- scheduled reflection and memory-quality checks can be useful when reviewable.

Not adopted for v1:

- an agent runtime, autonomous self-editing identity, orchestration, or cloud synchronization;
- automatically installed skills;
- giving generated reflections authority over user-approved memories.

OpenMemory is a memory service for existing tools, not an agent harness.

### projectmem

[projectmem](https://github.com/riponcm/projectmem) emphasizes local project memory that records issues, attempts, decisions, and reusable library lessons.

Ideas incorporated:

- failed attempts are first-class evidence, not disposable noise;
- a new task should receive warnings when it resembles a previous failure;
- stale memories should be marked and challenged rather than silently deleted;
- cross-project lessons require deliberate scope and sensitivity rules.

Not adopted for v1:

- a plain-file-only architecture;
- assuming every project lesson can cross project boundaries;
- its MCP surface or storage conventions.

### Graphify

The name Graphify is used by several graph-building projects. The relevant pattern here is represented by [rhanka/graphify](https://github.com/rhanka/graphify), which itself builds on an earlier code-structure graph tool of the same name: turn project material into a queryable knowledge graph with canonical entities, typed relationships, reconciliation, and scope-aware operations. This is distinct from Graphiti: Graphiti is primarily a temporal knowledge-graph framework for agent memory, while Graphify is primarily a project, code, and document graph-building pattern, distributed as a skill for coding assistants rather than as a running service.

Two published details matter for OpenMemory. Graphify describes tagging each relation by how it was obtained — found in the source, inferred with a confidence score, or ambiguous and flagged for review — so a reader can separate extraction from guesswork. It also describes extracting code structure locally through AST parsing without model calls, and clustering by graph topology rather than by embeddings, with export targets including an agent-crawlable wiki and an Obsidian vault.

Ideas incorporated:

- canonical entities and typed, explainable relations;
- every relation records how it was obtained, so inferred and ambiguous claims are never presented as observed facts;
- structural extraction that does not require a model call, which keeps indexing available when model processing is unavailable;
- entity reconciliation rather than creating a new node for every mention;
- scope-aware project graphs connected to source evidence;
- structural code and artifact relationships that complement conversational memory.

Not adopted for v1:

- treating a standalone generated graph as the memory authority;
- automatic skill installation or agent-environment modification;
- removing vector retrieval in favor of graph traversal alone;
- importing its implementation or ontology unchanged.

OpenMemory keeps its approved hybrid search and stores graph claims inside the same encrypted authority and provenance system as other memory.

### Microsoft SkillOpt

[Microsoft SkillOpt](https://github.com/microsoft/SkillOpt), likely the project meant in early discussion by “OptSkills,” treats agent skill documents as state that can be improved through trajectory reflection, bounded edits, rejected-edit history, and held-out evaluation. Microsoft's [SkillOpt research overview](https://www.microsoft.com/en-us/research/blog/skillopt-agent-skills-as-trainable-parameters/) also separates rapid task execution from slower improvement work.

Ideas incorporated:

- stage proposed changes before they affect trusted behavior;
- preserve rejected proposals as evidence instead of repeatedly rediscovering them;
- use bounded, reviewable updates and validation cases;
- run slower daily quality and consolidation work outside the live task path.

Not adopted for v1:

- turning OpenMemory into a skill optimizer or training system;
- replaying tasks to tune agent behavior automatically;
- automatically adopting generated skills or instructions;
- allowing evaluation output to override user-approved memory.

### Hermes Agent

[Hermes Agent](https://github.com/NousResearch/hermes-agent) demonstrates a compact always-available memory split, including user-focused and general memory, backed by searchable session history and provider-style memory integrations.

Ideas incorporated:

- separate durable user profile from project and task memory;
- inject only a bounded working packet while keeping deeper history searchable;
- define a stable provider-neutral interface around capture, prefetch, search, and lifecycle processing;
- keep scheduled consolidation separate from the interactive response path.

Not adopted for v1:

- fixed Markdown files as the only durable authority;
- injecting the full static memory on every turn;
- adopting a general agent runtime, shell, or orchestration layer;
- copying its plugin contract instead of defining OpenMemory's versioned MCP and service contracts.

### OpenHuman

[OpenHuman](https://github.com/tinyhumansai/openhuman) was published in May 2026 and was missed by the first pass of this document. It is the most widely adopted project in this survey and the closest public system to OpenMemory's local-first ambition, while differing from it on almost every product boundary. Its public material describes a GPL-3.0 desktop application with a Rust core and a TypeScript front end, running on macOS, Linux, and Windows, and it remains labelled early beta under active development.

The memory design is described as a Memory Tree: a hierarchical graph of scored Markdown compressed into a local SQLite database and mirrored as an editable Obsidian vault, explicitly positioned against opaque vector stores. Public write-ups describe the tree in roughly three planes — thematic nodes, entities such as people and repositories, and the underlying raw documents. Around that sit an auto-fetch cycle that pulls from connected accounts on a fixed interval, a large catalogue of OAuth connectors and MCP servers, and a tool-output compression step intended to cut token cost. Stated safety properties include on-device encryption, an approval gate, secrets in the OS keyring, opt-in sandboxing, a privacy mode enforced in the Rust core, and end-to-end encryption between agents.

Ideas incorporated:

- a hierarchical, scored memory tree is a credible alternative to flat ranked chunks, and node scoring is worth evaluating against OpenMemory's ranking rules;
- human-readable curated memory and a machine index can be maintained together rather than chosen between;
- a compression step between tool output and model context is a distinct, measurable concern from retrieval itself;
- a privacy mode enforced in the core, rather than by convention in each adapter, is the correct enforcement location;
- an approval gate and OS-keyring secret storage belong in the product from the start, not after launch;
- background ingestion on a schedule keeps the interactive path free of capture work.

Not adopted for v1:

- agent orchestration, sub-agent fleets, durable workflow execution, and deep research, which OpenMemory excludes by decision `D-005`;
- broad personal-life ingestion from mail, calendar, messaging, and financial accounts; OpenMemory v1 captures coding-assistant conversations and tool evidence only;
- a large third-party connector and skill catalogue, each entry of which is an unreviewed trust and egress surface;
- a mirrored plaintext Obsidian vault as part of the authoritative private store; OpenMemory's authority is the encrypted SQLCipher database, and the vault holds only selected projected knowledge;
- agent-to-agent networking of any kind, which places private memory on a wire;
- GPL-3.0 material of any kind: GPL-3.0 code cannot be redistributed under this repository's Apache-2.0 terms, so no OpenHuman source, schema, or prompt text may be copied into OpenMemory, and its patterns may only be reimplemented independently from public descriptions.

The interesting overlap is that OpenHuman documents optional use of a separate `agentmemory` backend so several coding tools can share one persistent store. That is the same cross-client goal as OpenMemory, reached by delegating to a second component rather than by defining one provider-neutral contract. OpenMemory keeps the contract in the service itself, because provider neutrality and encrypted provenance are properties this project has to be able to prove, not configure.

## Other OpenMemory projects and name collisions

### CaviraOSS/OpenMemory

[CaviraOSS/OpenMemory](https://github.com/CaviraOSS/OpenMemory) describes a broad self-hosted memory platform with SQLite or PostgreSQL, SDK and MCP access, a server, temporal and graph-oriented memory behavior, explainable traces, and external integrations.

OpenMemory adopts the value of explainable retrieval, temporal relationships, reinforcement and decay signals, and multiple access surfaces. It does not adopt hosted or multi-user operation, PostgreSQL, a listening web service, broad connector coverage, or its implementation. This project remains single-user, Windows-local, named-pipe based, and SQLCipher-authoritative in v1.

### AndroidPoet/openmemory

[AndroidPoet/openmemory](https://github.com/AndroidPoet/openmemory) demonstrates a local TypeScript/Bun memory server with SQLite, vector and keyword fusion, atomic fact or triple extraction, contradiction supersession, decay, MCP, REST, and a knowledge graph.

OpenMemory adopts the need to evaluate atomic extraction, contradiction history, graph links, and rank fusion. It does not adopt automatic forgetting or deletion, a localhost REST authority, grammar-based extraction as the sole truth mechanism, or its implementation. OpenMemory's user authority, encrypted evidence, approval workflow, and bitemporal history remain mandatory.

### Mem0 OpenMemory

Mem0 uses the OpenMemory name for two things. The name collision that matters most is [OpenMemory MCP](https://mem0.ai/blog/introducing-openmemory-mcp), described as a local-first memory server that exposes one persistent memory layer over MCP so that Claude, Cursor, Windsurf, and other MCP clients can write context in one tool and read it in another, with the data held on the user's machine. That is the same headline promise as this project, from a vendor with an established hosted product, and the overlap is in the product claim rather than only in the name. Mem0's separately documented self-hosted stack describes containers for an API service, PostgreSQL with pgvector, and Neo4j for entity relationships.

Mem0's archived [OpenMemory browser extension](https://github.com/mem0ai/mem0-chrome-extension) explored the same continuity idea in the browser: cross-site memory, automatic capture and retrieval, and a user-facing memory dashboard across browser AI products.

OpenMemory adopts the product lessons that cross-client continuity should feel automatic and inspectable, and that a local MCP server is a viable delivery shape for it. It does not adopt a browser extension, Google sign-in, a hosted API dependency, a container stack with separate vector and graph services, a listening HTTP surface, or sending private conversations to a remote memory provider in v1. The substantive differences remain bitemporal history, encrypted evidence, explicit authority and conflict review, and secret redaction as product requirements rather than deployment options.

### Naming consequence

The name **OpenMemory** is already used by unrelated or overlapping public projects, including [CaviraOSS/OpenMemory](https://github.com/CaviraOSS/OpenMemory), [AndroidPoet/openmemory](https://github.com/AndroidPoet/openmemory), Mem0's [OpenMemory MCP](https://mem0.ai/blog/introducing-openmemory-mcp), and Mem0's archived [OpenMemory browser extension](https://github.com/mem0ai/mem0-chrome-extension). Search results, package names, executable names, domains, and trademark availability therefore cannot be assumed. Mem0's OpenMemory MCP is the sharpest case, because it is an actively promoted product occupying both the name and the local cross-client memory-server description.

The project owner explicitly chose to retain the repository name `openmemory`. Before publishing packages or a signed application, Stage 0 must perform a fresh package, executable, domain, and legal-name review. The documentation should identify this repository by owner plus name where ambiguity matters.

## Comparison summary

This table is a navigation aid for the sections above, not an evaluation or a benchmark. Every entry describes what a project publicly claims as of this snapshot; nothing here was measured, and OpenMemory's own row describes a planning target rather than a shipped capability.

| Project | Product category | Authoritative store | Retrieval approach | Privacy posture as described | Nearest overlap with OpenMemory |
|---|---|---|---|---|---|
| **OpenMemory (this project, planned)** | Memory service for coding assistants | Encrypted SQLCipher database; Markdown is a projection | Hybrid keyword, semantic, metadata, and graph over one local store | Local-only by default, encrypted at rest, named pipe rather than a listening port | — |
| OpenHuman | Personal agent harness, orchestrator, and life memory | Scored Markdown Memory Tree in local SQLite, mirrored to Obsidian | Hierarchical tree traversal with scoring, plus compression before context | On-device encryption, privacy mode in the core, OS-keyring secrets, encrypted agent-to-agent links | Local-first, Markdown-and-database pairing, Obsidian surface, approval gate |
| Mem0 OpenMemory MCP | Local memory server for MCP clients | Vendor-defined local store; self-hosted stack adds vector and graph services | Vector-first with optional graph | Local-first, with a hosted product alongside | Cross-client memory over MCP; the closest name and claim collision |
| CaviraOSS/OpenMemory | Self-hosted memory platform | SQLite or PostgreSQL behind a server | Temporal and graph-oriented with explainable traces | Self-hosted rather than local-only; a listening service | Explainable retrieval, temporal relations, multiple access surfaces |
| AndroidPoet/openmemory | Local memory server | SQLite | Vector and keyword fusion over extracted atomic facts | Local, but a localhost REST authority | Atomic extraction, contradiction supersession, rank fusion |
| Graphiti / Zep | Temporal knowledge-graph framework | External graph database | Hybrid semantic, keyword, and graph with validity windows | Library or managed service; privacy is the operator's problem | Bitemporal facts, invalidation without erasure |
| Graphify | Graph-building skill for coding assistants | Generated graph artifacts and exports | Graph traversal and topology clustering; no embeddings | Runs locally, but produces plain output files | Canonical entities, typed relations, provenance on every relation |
| Karpathy LLM Wiki | A prompt and a discipline | Plaintext Markdown wiki over immutable raw sources | Agent reads and greps the wiki; knowledge compiled ahead of time | Whatever the operator's filesystem provides | Curated human-readable knowledge separated from raw evidence |
| akitaonrails/ai-memory | Memory system for coding CLIs | Markdown in Git; SQLite with FTS5 is a derived index | Full-text search over a curated wiki and raw transcripts | Local files, no encryption boundary described | Cross-vendor handoff, lifecycle hooks, bounded retrieval packets |
| memoirs | Local memory engine | SQLite with FTS5, optional SQLCipher | Hybrid, with bitemporal validity and provenance | Local, optionally encrypted | The closest match to OpenMemory's storage thesis |
| Letta | Stateful agent runtime | Service-managed agent state | Message search plus editable memory blocks | Service or cloud oriented | Scoped memory blocks and explicit memory tools |
| Cognee | Knowledge pipeline engine | Pluggable graph and vector stores | Graph plus vector with provenance and feedback | Deployable locally, platform shaped | Complementary graph, text, and semantic retrieval |
| Hindsight | Agent memory with reflection | Service-managed | Recall informed by experience and outcomes | Service oriented | Reflections derived from accumulated evidence |
| Memorix | Shared memory layer for coding agents | Local store behind MCP | Project, code, and Git context in one system | Local-first | One provider-neutral memory serving several clients |
| memem | Claude Code plugin | Obsidian Markdown plus an FTS5 transcript index | Full-text search over transcripts, separate from curated notes | Local, with pre-write security scanning | Consent-gated transcript import and injection checks before durable writes |
| projectmem | Local project memory | Plain files | Lookup of prior issues, attempts, and decisions | Local files | Failed attempts kept as first-class evidence; stale memories challenged, not deleted |
| Microsoft SkillOpt | Skill-optimization research system | Skill documents as trainable state | Held-out evaluation rather than user retrieval | Research setting | Staged proposals, rejected-edit history, bounded reviewable updates |
| Hermes Agent | Agent with a compact memory split | Markdown memory files plus searchable session history | Small always-on packet with deeper history searchable | Local or provider-backed, depending on configuration | Bounded working packet; durable profile separated from project and task memory |

Three distinctions separate OpenMemory from every row above, and they are the ones the staged gates have to prove:

1. **Encrypted evidence with a readable projection.** Most projects choose either plaintext Markdown as the truth or an opaque index as the truth. OpenMemory keeps complete history encrypted and authoritative, and projects only selected, editable knowledge into Markdown.
2. **Authority is a product feature.** Extraction, reflection, inference, and imported evidence are all provisional here. Promotion, conflict review, and supersession are user-facing workflows rather than implementation details, and generated content never silently outranks an approved memory.
3. **Provider neutrality is enforced, not incidental.** Client identity survives only in encrypted private provenance. No durable memory, ranking signal, or report is labelled as belonging to the assistant that happened to capture it.

The corresponding honest weaknesses are equally clear at this stage: OpenMemory has no implementation, no benchmark position, one target operating system, a smaller integration surface than any funded competitor, and a name that at least four public projects already use.

## Resulting OpenMemory position

OpenMemory combines ideas that commonly appear separately:

- complete encrypted evidence plus selective human-readable projections;
- full temporal and structural code graphs inside one local database;
- hybrid retrieval available silently to multiple terminal agents;
- provider-neutral durable memory with private, auditable provenance;
- explicit authority, conflict, deletion, secret, and transfer workflows;
- Obsidian as an optional human interface rather than the database itself.

This is a planning target, not a current capability claim. Implementation, security, performance, and interoperability remain to be proved through the staged feasibility and acceptance gates.
