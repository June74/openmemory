# Competitive research

Status: planning baseline, 2026-08-16. OpenMemory implementation has not begun.

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

Ideas incorporated:

- human-readable project indexes and linked notes;
- separation between raw evidence, curated knowledge, and generated reports;
- an AI-maintained table of contents rather than injecting every past chat;
- Obsidian as the primary human interface while keeping the memory usable without Obsidian.

Not adopted for v1:

- treating a plaintext vault as the complete private database;
- loading an entire vault or complete chat history into every model turn;
- granting AI-generated notes automatic authority.

OpenMemory keeps sensitive raw history encrypted and projects selected, editable knowledge into Markdown.

### akitaonrails/ai-memory

[akitaonrails/ai-memory](https://github.com/akitaonrails/ai-memory) describes sanitized lifecycle capture, a Markdown wiki usable in Obsidian, bounded handoffs, local indexing, shared use across several agent harnesses, and backup-friendly files.

Ideas incorporated:

- hooks should be lightweight and capture lifecycle events without blocking the client;
- one serialized database writer reduces corruption and concurrency risk;
- retrieval packets must be bounded;
- project and worktree identity need explicit handling;
- raw evidence and curated Markdown serve different purposes.

Not adopted for v1:

- Markdown as the sole source of truth for complete private history;
- its launcher, Rust implementation, or exact capture limits;
- assuming lifecycle observations are complete transcripts.

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

The name Graphify is used by several graph-building projects. The relevant pattern here is represented by [rhanka/graphify](https://github.com/rhanka/graphify): turn project material into a queryable knowledge graph with canonical entities, typed relationships, reconciliation, and scope-aware operations. This is distinct from Graphiti: Graphiti is primarily a temporal knowledge-graph framework for agent memory, while Graphify is primarily a project, code, and document graph-building pattern.

Ideas incorporated:

- canonical entities and typed, explainable relations;
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

## Other OpenMemory projects and name collisions

### CaviraOSS/OpenMemory

[CaviraOSS/OpenMemory](https://github.com/CaviraOSS/OpenMemory) describes a broad self-hosted memory platform with SQLite or PostgreSQL, SDK and MCP access, a server, temporal and graph-oriented memory behavior, explainable traces, and external integrations.

OpenMemory adopts the value of explainable retrieval, temporal relationships, reinforcement and decay signals, and multiple access surfaces. It does not adopt hosted or multi-user operation, PostgreSQL, a listening web service, broad connector coverage, or its implementation. This project remains single-user, Windows-local, named-pipe based, and SQLCipher-authoritative in v1.

### AndroidPoet/openmemory

[AndroidPoet/openmemory](https://github.com/AndroidPoet/openmemory) demonstrates a local TypeScript/Bun memory server with SQLite, vector and keyword fusion, atomic fact or triple extraction, contradiction supersession, decay, MCP, REST, and a knowledge graph.

OpenMemory adopts the need to evaluate atomic extraction, contradiction history, graph links, and rank fusion. It does not adopt automatic forgetting or deletion, a localhost REST authority, grammar-based extraction as the sole truth mechanism, or its implementation. OpenMemory's user authority, encrypted evidence, approval workflow, and bitemporal history remain mandatory.

### Mem0 OpenMemory browser extension

Mem0's archived [OpenMemory browser extension](https://github.com/mem0ai/mem0-chrome-extension) explored cross-site memory continuity, automatic capture and retrieval, and a user-facing memory dashboard across browser AI products.

OpenMemory adopts the product lesson that continuity should feel automatic and inspectable. It does not adopt a browser extension, Google sign-in, a hosted API dependency, or sending private conversations to a remote memory provider in v1.

### Naming consequence

The name **OpenMemory** is already used by unrelated or overlapping public projects, including [CaviraOSS/OpenMemory](https://github.com/CaviraOSS/OpenMemory), [AndroidPoet/openmemory](https://github.com/AndroidPoet/openmemory), and Mem0's archived [OpenMemory browser extension](https://github.com/mem0ai/mem0-chrome-extension). Search results, package names, executable names, domains, and trademark availability therefore cannot be assumed.

The project owner explicitly chose to retain the repository name `openmemory`. Before publishing packages or a signed application, Stage 0 must perform a fresh package, executable, domain, and legal-name review. The documentation should identify this repository by owner plus name where ambiguity matters.

## Resulting OpenMemory position

OpenMemory combines ideas that commonly appear separately:

- complete encrypted evidence plus selective human-readable projections;
- full temporal and structural code graphs inside one local database;
- hybrid retrieval available silently to multiple terminal agents;
- provider-neutral durable memory with private, auditable provenance;
- explicit authority, conflict, deletion, secret, and transfer workflows;
- Obsidian as an optional human interface rather than the database itself.

This is a planning target, not a current capability claim. Implementation, security, performance, and interoperability remain to be proved through the staged feasibility and acceptance gates.
