# Security Policy

OpenMemory is currently in design and documentation. There are no supported binaries or production versions yet.

## Reporting a vulnerability

Do not open a public issue containing exploit details, credentials, private memory content, or a working proof of concept. Use GitHub's private vulnerability-reporting feature when it becomes available for this repository. If private reporting is unavailable, open a minimal public issue asking the maintainer to establish a private contact channel without revealing the vulnerability.

Include the affected revision, required conditions, impact, reproduction outline, and suggested mitigation when safe to do so. Never include real user transcripts or secrets.

## Security boundaries

The planned product handles highly sensitive local material. Changes involving encryption, key storage, secret redaction, imports, prompt-injection defenses, MCP permissions, updater trust, backup, export, or deletion require threat-model review and adversarial tests before release.

Imported text and tool output are evidence, not instructions. Normalized memories must remain provider-neutral. No secret value may be persisted, displayed in warnings, written to logs, committed as a fixture, or sent to an external model.

## Supported versions

No version is currently supported because implementation has not begun. A supported-version table and response targets will be added before the first public release.
