# OpenMemory Launch Checklist

- **Scope:** the Stage 8 release obligations in [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md#stage-8--release)
- **Status:** Stage 0. Nothing on this list is discharged, and no release commit exists yet.
- **Kind:** a checklist of evidence, per decision `D-6` of the Wave D design — one row per obligation, each naming the artifact, command, or job that produces the evidence

> **Status note:** this document is a target, not a report. Every row below is `Not started`. None of the capabilities named in the `Evidence` column exists yet; the column says what will discharge the obligation, not what has been produced.

## How a row is ticked

A row is ticked only when the artifact named in its `Evidence` column **exists and is current on the release commit**. Three consequences follow, and they are the reason this checklist is written as evidence rather than intent:

1. **Recollection is not evidence.** "We tested that" ticks nothing. A row needs a named, retrievable artifact — a file, a recorded command output, a CI run, a review record — that someone other than its author can open.
2. **Evidence from an earlier commit is not evidence for the release commit.** A green CI run, a passing acceptance record, or a checksum produced against an older tree says nothing about the tree being released. The release commit's own gates must have been run on the release commit, which is what the Stage 8 exit gate means by "every preceding gate passes on the release commit."
3. **An obligation with no evidence yet is listed, not omitted.** Where the evidence is not yet decidable, the row names the deferred decision in [DECISION_REGISTER.md](DECISION_REGISTER.md) §9 that must supply it. Dropping such a row would make the checklist shorter and the release less checkable.

The obligations come from the Stage 8 text itself — its opening paragraph, the five bullets of what the public v1 release includes, and its exit gate. Nothing has been added that Stage 8 does not state, with one identified exception: the required-security-verification row, which [DATA_AND_PRIVACY.md](DATA_AND_PRIVACY.md#12-required-security-verification) §12 states as a precondition of public release and which Stage 8's exit gate therefore reaches through "every preceding gate passes."

## The checklist

| Obligation | Evidence | Produced by | Status |
|---|---|---|---|
| Independent specification review of the release commit | A written review record naming the reviewed commit, each finding, and how each finding was resolved | Stage 8 root-integrator review, recorded under `docs/operations/` in the clause/evidence/verification-command form [`docs/operations/STAGE0_EXIT.md`](operations/STAGE0_EXIT.md) establishes | Not started |
| Independent security review of the release commit | A review record covering every `SC-*` rule and every `THR-NNN` entry, stating which are exercised by a test and which are not | Review against [contracts/README.md](contracts/README.md) and [THREAT_MODEL.md](THREAT_MODEL.md), recorded under `docs/operations/` | Not started |
| Independent quality review of the release commit | A review record covering the acceptance requirements in [PRODUCT_REQUIREMENTS.md](PRODUCT_REQUIREMENTS.md#13-acceptance-requirements) §13, each marked demonstrated or not | Stage 8 quality review, recorded under `docs/operations/` | Not started |
| A private beta exercises real workflows before publication | A beta record naming the workflows exercised, the defects found, and the resolution of each, dated before the publication date | Stage 8 private beta, recorded under `docs/operations/` | Not started |
| All approved v1 features are present in the release | A mapping from each acceptance requirement in `PRODUCT_REQUIREMENTS.md` §13 to the passing run that demonstrates it on the release commit | `dotnet test` on the release commit plus the Stage 7 clean-machine acceptance record; the local embedding model and its redistribution terms are deferred to `F-004` | Not started — embedding model blocked on `F-004` |
| All supported clients are published and verified | Per-client capability verification against the client versions current at release, for each adapter the release claims to support | Stage 3 adapter acceptance runs re-run on the release commit; client hook and slash-command packaging deferred to `F-005`; the Obsidian community submission date and review checklist deferred to `F-008` | Not started — blocked on `F-005` and `F-008` |
| Reproducible build instructions | Documented steps that a clean Windows 11 x64 machine follows to rebuild the published artifact, plus a rebuild whose SHA-256 matches the published checksum | `dotnet publish` as the `artifact` job in [`.github/workflows/ci.yml`](../.github/workflows/ci.yml) invokes it, compared against the release checksum file | Not started |
| Software bill of materials | A CycloneDX JSON SBOM generated from the release commit and published as a release asset — the release equivalent of `openmemory-sbom.cyclonedx.json` | The CI `artifact` job's "Generate SBOM" step (`anchore/sbom-action`, syft pinned) | Not started |
| Checksums for every published artifact | A SHA-256 file per published archive, lowercase hex with LF endings so `sha256sum -c` verifies it — the release equivalent of `openmemory-dev.zip.sha256` | The CI `artifact` job's "Package and checksum" step | Not started |
| A signature or signed attestation anchored to the documented trusted project identity | A signature or attestation over each published artifact that verifies against the pinned trusted project identity, plus the record of how that identity was pinned and how it may rotate | The requirement is fixed by [contracts/PUBLISHER_AUTHENTICATION.md](contracts/PUBLISHER_AUTHENTICATION.md) §3 (`SC-PUB-002`, `SC-PUB-003`); the signing provider and attestation implementation are deferred to `F-007` | Not started — blocked on `F-007` |
| Installer documentation | An install guide matching the shipped installer, exercised end to end during the clean-machine acceptance run | Stage 7 Lane C install acceptance run; the MSI authoring tool, bootstrapper, and upgrade implementation are deferred to `F-001` | Not started — blocked on `F-001` |
| Upgrade documentation | An upgrade guide, plus a publisher-authenticated update run showing which of `SC-PUB-004`'s conditions were evaluated and how each was decided | Stage 7 Lane B update tests and Lane C upgrade acceptance; [contracts/PUBLISHER_AUTHENTICATION.md](contracts/PUBLISHER_AUTHENTICATION.md) §5; installer implementation deferred to `F-001` | Not started — blocked on `F-001` |
| Rollback documentation | A rollback guide, plus a rollback exercised from a deliberately failed update, showing rollback was retained until post-update health checks passed per `SC-PUB-007` | Stage 7 Lane B update-rollback tests; [contracts/PUBLISHER_AUTHENTICATION.md](contracts/PUBLISHER_AUTHENTICATION.md) §5 | Not started |
| Uninstall documentation | An uninstall guide, plus a clean-machine uninstall run recording exactly what remains on disk afterwards | Stage 7 Lane C uninstall acceptance run; installer implementation deferred to `F-001` | Not started — blocked on `F-001` |
| Migration documentation | A migration and import guide, plus resumable import runs with the coverage report that names every skipped or failed record | Stage 7 Lane A migration runs; the database schema and migrations themselves are deferred to `F-002` | Not started — blocked on `F-002` |
| Backup documentation | A backup guide stating the shipped cadence, rotation count, and retention defaults, plus a restore exercised from a routine and from a pinned backup | Stage 6 Lane C backup and restore runs; the cadence, rotation counts, and retention defaults are deferred to `F-006` | Not started — blocked on `F-006` |
| Recovery documentation | A recovery guide, plus recorded recovery-key and key-loss exercises showing what is and is not recoverable | Stage 7 Lane B key-loss and recovery-key tests, against the accepted residual risk recorded in [THREAT_MODEL.md](THREAT_MODEL.md) §6 | Not started |
| Hardware transfer documentation | A transfer guide, plus a transfer exercised end to end onto a second clean machine with the restored store verified | Stage 7 Lane C transfer acceptance run; the storage-size and transfer measurements it depends on are deferred to `F-006` | Not started — blocked on `F-006` |
| Privacy documentation | [DATA_AND_PRIVACY.md](DATA_AND_PRIVACY.md) published with the release and current on the release commit, together with the four contracts and the threat model it relies on | `docs/DATA_AND_PRIVACY.md`, [contracts/README.md](contracts/README.md), and [THREAT_MODEL.md](THREAT_MODEL.md), each verified current on the release commit | Not started |
| Evaluation results with limitations stated plainly | An evaluation report over the versioned retrieval evaluation set, naming the corpus version, the measured numbers, and a limitations section that states what was not measured | Stage 5 and Stage 7 quality-lane evaluation runs against `PRODUCT_REQUIREMENTS.md` §13's measured-retrieval-quality requirement; quantitative targets are deferred to `F-009` and the ranking weights to `F-003` | Not started — targets blocked on `F-009` |
| A clean issue tracker | Recorded issue-tracker output for the release commit, in which every remaining open item is a deliberate, labelled deferral rather than an unreviewed one | `gh issue list --state open` output recorded in the Stage 8 release record under `docs/operations/` | Not started |
| Published compatibility boundaries | [COMPATIBILITY.md](COMPATIBILITY.md) published with the release, with the §1 contract integers frozen and the §3 support windows stated for the released version | `docs/COMPATIBILITY.md` §1, §3, and §6; the integers are unfrozen until Stage 2 freezes them | Not started |
| Every preceding stage gate passes on the release commit | One exit record per stage, each with clause, evidence, and verification command, every command re-run on the release commit — Stage 0's is [`docs/operations/STAGE0_EXIT.md`](operations/STAGE0_EXIT.md) | `dotnet format --verify-no-changes`, `dotnet build OpenMemory.sln`, `dotnet test`, `bash tools/check-links.sh`, and all CI jobs green on a run whose `headSha` equals the release commit | Not started |
| Required security verification complete | One recorded test result per verification class in [DATA_AND_PRIVACY.md](DATA_AND_PRIVACY.md#12-required-security-verification) §12, each mapped to the rules and threats it exercises | Each contract's §5 "Verification owed" — [REGISTERED_CLIENT_CAPABILITIES.md](contracts/REGISTERED_CLIENT_CAPABILITIES.md), [TRUSTED_HUMAN_CONFIRMATION.md](contracts/TRUSTED_HUMAN_CONFIRMATION.md), [EXTERNAL_PROCESSING_CONSENT.md](contracts/EXTERNAL_PROCESSING_CONSENT.md), [PUBLISHER_AUTHENTICATION.md](contracts/PUBLISHER_AUTHENTICATION.md) — plus the coverage table in [THREAT_MODEL.md](THREAT_MODEL.md) §7, run by `dotnet test` and the clean-machine acceptance runs | Not started — the same-user spoofing class is blocked on `F-011` |
| The remote tag and artifacts match local evidence | The published tag resolves to the release commit, and every downloaded release asset's SHA-256 equals its published checksum | `git rev-parse <tag>^{commit}` compared with the release commit, and `sha256sum -c` run against the downloaded assets | Not started |
| The actual user-facing paths are tested before the user is asked to test them | A clean Windows 11 x64 install-to-transfer acceptance record, and the private beta record, both dated before the publication date | Stage 7 Lane C acceptance run and the Stage 8 private beta record, under `docs/operations/` | Not started |

## Deferred decisions this checklist depends on

Each entry below is recorded in [DECISION_REGISTER.md](DECISION_REGISTER.md) §9 with the evidence its decision requires. A row above that names one of these cannot be ticked until the decision is made and its evidence produced.

- `F-001` — installer authoring, bootstrapper, and upgrade implementation: gates the installer, upgrade, and uninstall documentation rows.
- `F-002` — database schema and migrations: gates the migration documentation row.
- `F-003` — authority and hybrid-ranking weights: gates part of the evaluation row.
- `F-004` — local embedding model and redistribution terms: gates the feature-completeness row.
- `F-005` — client hook and slash-command packaging: gates the supported-clients row.
- `F-006` — backup cadence, rotation counts, and retention defaults: gates the backup and transfer documentation rows.
- `F-007` — signing provider and attestation implementation: gates the signature row. `SC-PUB-002` fixes the requirement; only the provider is open.
- `F-008` — Obsidian community submission date and review checklist: gates the supported-clients row.
- `F-009` — quantitative retrieval, indexing, and startup targets: gates the evaluation row.
- `F-011` — user-presence mechanism satisfying `SC-CONF-007`: gates the same-user spoofing part of the required-security-verification row.

`F-010` is not listed: it deferred the repository-grounded threat model, which [THREAT_MODEL.md](THREAT_MODEL.md) now supplies.

## Maintaining this document

This checklist is written at Stage 0 and will be wrong in detail long before Stage 8 arrives — job names change, artifacts get renamed, deferrals are discharged. Two rules keep it honest:

1. When a deferred decision is discharged, replace the `F-*` reference in the affected row with the evidence the decision selected. A discharged deferral left in place reads as an open question and hides a row that is now checkable.
2. When an obligation's evidence changes producer, update the `Produced by` cell in the same commit that changes the producer. A `Produced by` cell that names a job or artifact which no longer exists is worse than an empty one, because it will be read as evidence that something ran.

Adding or removing a row requires the corresponding change in [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md#stage-8--release) §Stage 8 first. This document does not decide what the release owes; it records how each owed thing is proved.
