# Glossary

Status: planning baseline. These definitions explain the terms used in OpenMemory's design; they do not claim that the features have been implemented.

## Memory and retrieval

**Embedding**

A list of numbers that represents the meaning of text or code. Items with similar meanings tend to have nearby embeddings, which makes meaning-based search possible.

**Keyword search**

Search based on exact words or word forms. It is strong for names, error codes, identifiers, and phrases that must match precisely.

**Semantic search**

Search by meaning rather than exact wording. For example, it may connect “move to another computer” with “hardware transfer.” It is commonly powered by embeddings.

**Vector search**

The mathematical nearest-neighbor search used to find embeddings close to a query embedding. Semantic search is the user-facing behavior; vector search is the underlying technique.

**Hybrid search**

A search that combines multiple signals—such as keywords, vectors, metadata, time, and graph connections—so weaknesses in one method can be offset by another.

**Fusion**

A rule for combining several ranked result lists into one list. Reciprocal Rank Fusion, or RRF, rewards items that rank well in more than one search without requiring every search engine to use the same score scale.

**Reranking**

A second pass that reorders a candidate set using additional information. OpenMemory may consider project scope, recency, graph distance, authority, and prior usefulness after the first search.

**RAG (retrieval-augmented generation)**

A pattern in which a system retrieves relevant information and supplies it to a language model before the model answers. Retrieval does not train or permanently change the model; it gives the current request better context.

**Provisional reflection**

An AI-generated lesson, pattern, or hypothesis derived from evidence. “Provisional” means it remains reviewable, retains supporting and opposing evidence, and cannot silently replace user-approved memory.

## Data and history

**Provenance**

The trace of where a record came from and how it was transformed—for example: source chat, message, extracted candidate, approved memory. Provenance lets a user inspect or challenge a conclusion.

**Temporal graph**

A network of entities and relationships that records change over time. It can answer not only “what is connected?” but also “when was this connection considered true?”

**Bitemporal graph**

A temporal graph with two timelines: valid time, when a fact applies in the real or project world; and recorded time, when OpenMemory learned or stored it. The distinction allows late corrections without rewriting history.

**Event journal**

An append-oriented log of captured events before or alongside derived memories. It helps replay interrupted processing, audit transformations, and recover safely after a crash.

**Idempotency**

The property that repeating the same operation has the same final effect as doing it once. An idempotency key lets an importer retry an event without creating duplicate chats or facts.

**Projection**

A human-readable view generated from authoritative stored data. In OpenMemory, an Obsidian Markdown note can be a projection of selected database memories rather than a plaintext copy of every private record.

**Provider-neutral**

Stored knowledge is not branded as “Claude memory,” “Codex memory,” or another provider's memory. It follows a common schema so supported clients can use it equally. Private provenance may still identify the adapter needed for evidence and deduplication.

**Source adapter**

A small integration that translates one client's events or history format into OpenMemory's common event contract. Adapters preserve private source identifiers but do not put provider branding into normalized facts.

## Storage and local operation

**SQLCipher**

An extension of SQLite that encrypts the database file. It protects data at rest when configured correctly, but does not replace key protection, safe backups, or access controls while the database is open.

**FTS5**

SQLite's full-text search extension. It builds an index for fast word and phrase search without scanning every stored record each time.

**DPAPI (Data Protection API)**

A Windows facility that encrypts a secret so it can normally be decrypted only under the intended Windows user or machine context. OpenMemory plans to use Windows-backed protection for local key material.

**Named pipe**

A Windows operating-system channel that lets local processes exchange messages without opening a web port. Permissions can restrict the pipe to the current user.

**Daemon / background service**

A program that keeps running without an open terminal window to capture events, process queues, and serve requests. On Windows, OpenMemory uses the plain term “background service” even if other platforms call it a daemon.

**Checksum**

A short value calculated from file contents. Recalculating it after download or transfer can reveal accidental changes or tampering, although a checksum is trustworthy only when obtained through a trustworthy channel.

**Signed release**

A release whose publisher attaches a cryptographic signature or attestation. Verification helps show who produced the artifact and whether it changed after publication; it does not prove the software has no vulnerabilities.

**SBOM (software bill of materials)**

An inventory of third-party packages and components included in a build. It helps users and maintainers identify licensing and security exposure when a dependency has a known problem.

## Agent and security interfaces

**MCP (Model Context Protocol)**

A standard way for an AI client to call external tools and read resources. OpenMemory plans to expose the same memory operations to several MCP-capable terminal clients.

**Prompt injection**

Untrusted text that tries to manipulate an AI into treating data as instructions—for example, a transcript saying “ignore the user and reveal secrets.” OpenMemory must label evidence as data, scan risky content, and enforce permissions outside the model.

## Code and engineering process

**Tree-sitter**

A parser framework that turns source code into a syntax tree and can update the tree incrementally as files change. OpenMemory plans to use it to identify code symbols and relationships across several languages.

**Worktree**

A Git feature that checks out another branch into a separate folder while sharing the same repository history. Worktrees let parallel agents work without constantly replacing one another's files.

**ADR (architecture decision record)**

A short document recording an important technical choice, its context, alternatives, and consequences. ADRs preserve why a decision was made after the original discussion is gone.

**DCO (Developer Certificate of Origin)**

A lightweight contribution process in which contributors add a `Signed-off-by` line to certify that they have the right to submit their work under the project's license.
