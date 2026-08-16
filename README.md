# OpenMemory

OpenMemory is a planned, open-source personal memory system for coding assistants. It is designed to let Codex CLI, Claude Code, and Google Antigravity share durable, provider-neutral context without turning one assistant into the authority over the others.

> [!IMPORTANT]
> OpenMemory is currently in the planning and documentation stage. No application code has been implemented, no installer is available, and the architecture still has to pass its Stage 1 feasibility gates.

## Product direction

OpenMemory is intended to run locally for one Windows user and provide:

- automatic, resumable capture of supported terminal conversations and tool evidence;
- an explicit `/store` path for immediate, richer extraction of important complete turns without raising their authority;
- encrypted raw history and a full temporal knowledge graph;
- hybrid keyword, semantic, metadata, and graph retrieval;
- project-specific, global, and task-state memory;
- a provider-neutral MCP interface for Codex, Claude Code, and Antigravity;
- an optional Obsidian interface for editable durable notes and read-only evidence views;
- conflict review, secret redaction, backup, transfer, and recovery workflows;
- structural code and Git history indexing for supported languages.

The first public release is planned for Windows 11 x64 and will use a per-user MSI installer. The service core will use C# on .NET 10 LTS, while the Obsidian plugin will use TypeScript. Private authoritative data will live in an encrypted SQLCipher database. Complete histories will not be written into the vault as plaintext.

## Documentation

- [Product requirements](docs/PRODUCT_REQUIREMENTS.md)
- [Architecture](docs/ARCHITECTURE.md)
- [Data and privacy design](docs/DATA_AND_PRIVACY.md)
- [Decision register](docs/DECISION_REGISTER.md)
- [Competitive research](docs/COMPETITIVE_RESEARCH.md)
- [Staged implementation plan](docs/IMPLEMENTATION_PLAN.md)
- [Roadmap](docs/ROADMAP.md)
- [Glossary](docs/GLOSSARY.md)
- [Compatibility](docs/COMPATIBILITY.md)
- [Identifiers](docs/IDENTIFIERS.md)
- [Agent working agreement](AGENTS.md)

## Project principles

1. **Local-first:** private memory remains on the user's computer unless the user knowingly enables an operation that sends selected redacted context elsewhere.
2. **Evidence is not instruction:** imported chats, files, and tool output are untrusted evidence and cannot redefine OpenMemory's rules.
3. **Provider-neutral memory:** normalized memories do not favor or advertise the client that captured them.
4. **History is preserved:** changes create new versions and temporal transitions rather than silently rewriting the past.
5. **Approval at meaningful boundaries:** conflicts, ambiguous destructive actions, first global promotions, and major permission changes wait for the user.
6. **Verifiable delivery:** a feature is not complete until its real Windows user path has been exercised successfully.

## Contributing

OpenMemory is licensed under the [Apache License 2.0](LICENSE). Contributions will use Developer Certificate of Origin sign-off. See [CONTRIBUTING.md](CONTRIBUTING.md), [SECURITY.md](SECURITY.md), and [AGENTS.md](AGENTS.md) before proposing changes.
