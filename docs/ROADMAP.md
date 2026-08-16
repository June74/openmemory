# OpenMemory Roadmap

> **Current status:** planning baseline only. No application code, installer, service, plugin, or supported integration exists yet.

OpenMemory will be developed through internal milestones and private test builds. There is one public v1 target: the complete approved Windows product. Milestone completion does not mean a partial public edition is ready.

The detailed work and exit gates live in the [implementation plan](IMPLEMENTATION_PLAN.md). The planned system boundaries live in [Architecture](ARCHITECTURE.md).

## Internal milestones

| Milestone | Intended outcome | Public availability |
|---|---|---|
| **0 — Foundation** | Repository, contracts, security process, build skeleton, CI, and launch criteria. | Documentation only. |
| **1 — Risk proofs** | SQLCipher/key recovery, Windows/MCP/client connectivity, packaging, embeddings, Tree-sitter, and incremental indexing proven on clean Windows. | No supported release. |
| **2 — Secure data plane** | Durable journal, single writer, bitemporal schema, authority rules, identities, jobs, audit, and two-way projection foundations. | Private development builds only. |
| **3 — Client capture** | Codex, Claude Code, and Antigravity hooks, importers, MCP setup, and `/store` conform to one neutral contract. | Private development builds only. |
| **4 — Memory engine** | Allowance-aware processing, hybrid retrieval, temporal graph, conflicts, feedback, project sensitivity, and failed-attempt memory. | Private evaluation builds only. |
| **5 — Code intelligence** | Structural parsing for every approved language plus Git, branches, and worktrees. | Private evaluation builds only. |
| **6 — User experience and operations** | Obsidian views and projections, complete terminal path, reports, doctor, backups, transfer, export, updates, and rollback. | Private beta candidate. |
| **7 — Migration and hardening** | Historical import, security testing, clean-machine lifecycle, performance, recovery, and acceptance evidence. | Private beta. |
| **8 — Public v1** | Every approved capability passes release gates with documentation, SBOM, checksums, publisher-authenticating signature or signed attestation, and reproducible evidence. | Public stable release. |

## Public v1 promise

Public v1 is not released until all of these are present and verified together:

- a local per-user Windows 11 x64 service with encrypted SQLCipher authority;
- silent, audited, on-demand memory for Codex CLI, Claude Code, and Antigravity CLI;
- automatic capture, complete supported history import, immediate `/store`, one-hour quiet processing, and daily catch-up;
- pre-persistence secret scanning with only redacted content, non-secret metadata, and source pointers in quarantine, plus warnings that never reveal secret values;
- global and project memory, task snapshots, approvals, before/after conflicts, provenance, failed attempts, and a full bitemporal knowledge graph;
- hybrid keyword, local semantic, metadata, temporal, and graph retrieval with versioned evaluation evidence;
- structural code intelligence for C#, TypeScript, JavaScript, Python, Rust, Go, Java, C, C++, HTML, CSS, SQL, and PowerShell, including Git branches and worktrees;
- an optional Obsidian interface with editable durable projections and read-focused evidence, graph, review, report, and health views;
- encrypted rotating backups, non-expiring pinned backups, recovery, hardware transfer, portable export, diagnostics, safe updates, and rollback;
- successful clean-machine installation, startup, upgrade, restore, migration, transfer, and uninstall tests;
- accurate documentation that distinguishes verified behavior from limitations.

## Explicitly after v1

The following are not permitted to delay or blur the approved v1 scope by quietly entering implementation:

- macOS or Linux support;
- multi-user accounts or a hosted cloud service;
- IDE-native integrations, Cursor, Hermes Agent, or other additional clients;
- a standalone desktop application;
- a separate graph database;
- a local generative model or fallback provider;
- agent orchestration or autonomous task execution;
- automatic skill installation;
- an editable knowledge-graph canvas;
- broad third-party importer SDKs;
- automatic deletion of raw history;
- autonomous overwriting of approved memories;
- direct compatibility exports tailored to other memory engines.

These may be proposed after v1 based on real use, security impact, maintenance cost, and contributor interest. Each requires a new product decision and design review.

## Release progression

Development artifacts are explicitly unsupported and may change format. Private beta begins only after Milestone 6 passes its own user-facing gates. Beta focuses on migration safety, recovery, retrieval quality, and daily use on the owner's real Windows workflow.

The v1 release candidate is cut only after Milestone 7. It is accepted when:

1. the release commit passes CI, security, migration, evaluation, and clean-machine suites;
2. restored and transferred data matches source checksums and graph counts;
3. all three clients complete real capture, `/store`, silent retrieval, and evidence-inspection workflows;
4. no known secret disclosure or project-sensitivity leak remains;
5. update failure rolls back without loss;
6. documentation, artifacts, tag, SBOM, checksums, and available signing/attestation agree;
7. independent specification, security, and quality reviews are resolved.

There is no automatic date promise. A stage advances when its evidence passes, not when code merely exists.
