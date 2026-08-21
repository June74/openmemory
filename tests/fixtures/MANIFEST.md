# Committed test fixture manifest

Every committed file under `tests/fixtures/` except this manifest is listed below with the SHA-256 of its bytes, in lowercase hex, as decision `D-095` requires (see [Decision register](../../docs/DECISION_REGISTER.md)). `FixtureManifestTests` in `tests/OpenMemory.Contracts.Tests/FixtureManifestTests.cs` recomputes every digest, so a fixture and this table cannot drift apart unnoticed.

**Changing a fixture is a deliberate act.** Edit the fixture and regenerate this table in the same commit; a commit that changes one without the other fails the build. The reason is not bookkeeping: a fixture quietly edited to make a failing test pass looks identical in review to one edited to fix a real defect, and the manifest is what tells those two apart. Regenerate with:

```bash
cd tests/fixtures && find . -type f -name '*.json' | sort | while read -r f; do echo "$f  $(sha256sum "$f" | cut -d' ' -f1)"; done
```

Digests are taken over the file's bytes, so they depend on line endings. `.gitattributes` pins `tests/fixtures/** text eol=lf`; a fixture checked out or written with CRLF produces a different digest and fails the checksum test.

**No secret-detection corpus is listed here, and none may be added.** Secret-detection corpora are generated at test run time from documented synthetic patterns and are never committed, per `D-094`. Two independent reasons: [Agent instructions](../../AGENTS.md) prohibit writing a secret value into any file, including examples, placeholders, and test data; and the CI `secret-scan` job runs gitleaks over the tree, so a committed corpus would fail the build on the very content it exists to exercise. If a secret-scanner test needs input, generate it inside the test — see [Test fixtures](../../docs/TEST_FIXTURES.md). Do not "fix" the absence of a corpus file by committing one.

## Fixtures

| Fixture | SHA-256 | Purpose | Consumed by |
|---|---|---|---|
| `events/neutral-event-envelope.sample.json` | `e802b8e96f9aed8d5945aa6521331d04b6c030c4c39696cd27652122a888ea9f` | One provider-neutral event envelope carrying the fields [Architecture](../../docs/ARCHITECTURE.md) names under "Capture and normalized events": stable event identifier, source record identifier, content hash, schema version, capture time, and processing status. | Stage 1 event-envelope validation and normalization tests. |
| `transcripts/conversation-turn.sample.json` | `9adb7a041721c1227bac0947db0b8039114d19f41b41ac7f0311ab8fa4926642` | One complete user / assistant / tool / assistant turn — the unit that an inline or standalone `/store` targets. | Stage 1 `/store` complete-turn resolution and extraction tests. |
| `repositories/synthetic-repository-tree.sample.json` | `9dbd1bc99eb55d00a99ec3f7bc118afd2fa0d3c661d49e2e2fc6a20351ccf2b2` | A small synthetic file, symbol, and language tree that describes a repository rather than containing one, for repository-indexing work. | Stage 4 repository-indexing tests. |

No fixture listed above contains real conversation data, real repository content, a credential, or a machine-varying value. Every identifier is a fixed literal typed into the file; nothing in a fixture is generated at read time. Time-valued and hash-valued fields are fixed literals for the same reason — they are illustrative shapes, not values recomputed from the payload, because Stage 0 has no canonical serializer to recompute them with.

Nothing consumes these fixtures yet. Stage 0 adds no product code, so the "Consumed by" column names the stage that will consume each fixture, not an existing caller.
