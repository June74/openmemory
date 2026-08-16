# Stage 0 Wave A — Governance and Records Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Establish OpenMemory's repository governance files and its versioning, compatibility, and identifier records, without adding any product capability.

**Architecture:** Wave A is documentation and repository configuration only. It creates a `.github/` directory that does not exist yet, adds two records documents (`docs/COMPATIBILITY.md`, `docs/IDENTIFIERS.md`), adds one repeatable verification script, and makes four narrow edits to existing documents. Because documentation has no unit tests, the test-first discipline applies to the *verification checks*: each check is run before the change to observe it fail, then after to observe it pass.

**Tech Stack:** Markdown, GitHub issue-forms YAML, GitHub CODEOWNERS syntax, Bash (Git Bash on Windows), GitHub CLI (`gh` 2.95.0), Codex CLI (`codex` 0.147.0).

**Spec:** [docs/superpowers/specs/2026-08-16-stage0-wave-a-design.md](../specs/2026-08-16-stage0-wave-a-design.md)

## Global Constraints

Every task's requirements implicitly include this section.

- **No application code.** No `.cs`, `.ts`, project file, solution file, dependency manifest, or CI workflow. Those are Waves B and C. Wave A adds zero product capability.
- **Repository:** `June74/openmemory`. **Branch:** `codex/stage0-wave-a`. Never commit to `main`.
- **Owner handle for CODEOWNERS:** `@June74`.
- **DCO required.** Every commit uses `git commit -s`. A commit without a `Signed-off-by` line violates `CONTRIBUTING.md`.
- **Never write a secret value** into any file, including examples, placeholders, and templates. Example tokens are prohibited even when obviously fake.
- **Distinguish planned from implemented behavior** in all prose. Nothing in this wave may describe an unimplemented capability in the present tense as though it exists.
- **Scripts are Bash, never PowerShell pipelines.** `SET-20260816-001` was a PowerShell pipeline parse failure.
- **Verification makes no outbound network request** except authenticated `gh api` calls to this repository. `SET-20260816-002` was the sandbox blocking outbound traffic.
- **Contract version integers all start at `1`** and are explicitly unfrozen until Stage 2.
- **Do not renumber existing requirement text.** `REQ-*` is defined in this wave but applied later (spec §7).

---

### Task 1: Repository link-check tool

Creates the verification tool the later tasks depend on. It must exist first, because Tasks 4–6 add cross-document links that need checking.

**Files:**
- Create: `tools/check-links.sh`
- Test: `tools/fixtures/broken-link-sample.md` (temporary; deleted in Step 5)

**Interfaces:**
- Consumes: nothing.
- Produces: `tools/check-links.sh`, run as `bash tools/check-links.sh`. Exits `0` when every repository-internal Markdown link resolves, `1` otherwise. Prints one `BROKEN <file> -> <target>` line per failure and a `N internal links checked, M broken` summary. Tasks 2–7 rely on this exact contract.

- [ ] **Step 1: Write the failing test fixture**

A checker that never reports a failure is worthless, so prove it detects one before trusting it.

Create `tools/fixtures/broken-link-sample.md`:

```markdown
# Link checker fixture

This file exists only to prove the link checker detects a broken link.

- A link that resolves: [the license](../../LICENSE)
- A link that does not resolve: [missing document](./this-file-does-not-exist.md)
```

- [ ] **Step 2: Run the checker to verify it fails**

Run: `bash tools/check-links.sh`

Expected: FAIL — `bash: tools/check-links.sh: No such file or directory`. The script does not exist yet.

- [ ] **Step 3: Write the checker**

Create `tools/check-links.sh`:

```bash
#!/usr/bin/env bash
# Verify that repository-internal Markdown links resolve to real paths.
#
# External links are deliberately NOT requested. SET-20260816-002 recorded the
# sandbox blocking outbound requests, which made an external link check report
# false failures. This script is Bash rather than a PowerShell pipeline because
# SET-20260816-001 recorded a PowerShell pipeline parse failure.
set -uo pipefail

root=$(git rev-parse --show-toplevel) || exit 1
cd "$root" || exit 1

broken=0
checked=0

while IFS= read -r file; do
  dir=$(dirname "$file")
  while IFS= read -r target; do
    [ -z "$target" ] && continue
    case "$target" in
      http://*|https://*|mailto:*|\#*) continue ;;
    esac
    path=${target%%#*}
    [ -z "$path" ] && continue
    checked=$((checked + 1))
    if [ ! -e "$dir/$path" ]; then
      printf 'BROKEN %s -> %s\n' "$file" "$target"
      broken=$((broken + 1))
    fi
  done < <(awk '/^```/{fence = !fence; next} !fence' "$file" \
           | grep -oE '\]\([^)]+\)' | sed -E 's/^\]\(//; s/\)$//')
done < <(git ls-files '*.md')

printf '%d internal links checked, %d broken\n' "$checked" "$broken"
[ "$broken" -eq 0 ]
```

Two notes for the implementer, both of which cause silent wrong behavior if missed:

1. Both loops use process substitution (`< <(...)`) rather than a pipe. A pipe runs the loop body in a subshell, so the `broken` counter is discarded and the script always exits `0` — a checker that can never fail.

2. The `awk` filter strips fenced code blocks before links are extracted. Specification and plan documents legitimately contain example links inside code fences that are *content for other files*, not links from the document itself. Without this filter, `docs/superpowers/plans/` and `docs/superpowers/specs/` report false failures and block every task that requires a clean run.

- [ ] **Step 4: Run the checker to verify it detects the fixture**

Run: `git add tools/ && bash tools/check-links.sh; echo "exit=$?"`

(`git add` is required first: the script enumerates files with `git ls-files`, which does not see untracked files.)

Expected: PASS-as-designed — output contains `BROKEN tools/fixtures/broken-link-sample.md -> ./this-file-does-not-exist.md` and `exit=1`.

- [ ] **Step 5: Delete the fixture and confirm the repository is clean**

```bash
git rm -f --cached tools/fixtures/broken-link-sample.md
rm -rf tools/fixtures
bash tools/check-links.sh; echo "exit=$?"
```

Expected: no `BROKEN` lines, and `exit=0`. This is the baseline: every link in the repository resolves today.

- [ ] **Step 6: Commit**

```bash
git add tools/check-links.sh
git commit -s -m "Add repository-internal Markdown link checker

External links are not requested, per SET-20260816-002. Bash rather than
a PowerShell pipeline, per SET-20260816-001."
```

---

### Task 2: Ownership and branch-protection policy

**Files:**
- Create: `.github/CODEOWNERS`
- Create: `.github/branch-protection.md`

**Interfaces:**
- Consumes: `bash tools/check-links.sh` from Task 1.
- Produces: `.github/branch-protection.md`, referenced by `CONTRIBUTING.md` in Task 6 and enabled in Wave C.

- [ ] **Step 1: Run the CODEOWNERS check to verify it fails**

Run: `gh api repos/June74/openmemory/codeowners/errors`

Expected: FAIL — HTTP 404, because no CODEOWNERS file exists on the default branch.

Record this output. It is the "before" evidence.

- [ ] **Step 2: Write CODEOWNERS**

Create `.github/CODEOWNERS`:

```
# OpenMemory code owners.
#
# AGENTS.md requires that shared files are owned by the root integrator or a
# named contract owner. Every path below has a named owner.

# Default owner for everything not matched more specifically.
*                     @June74

# Licensing, security policy, and the agent agreement.
/LICENSE              @June74
/NOTICE               @June74
/SECURITY.md          @June74
/CODE_OF_CONDUCT.md   @June74
/CONTRIBUTING.md      @June74
/AGENTS.md            @June74

# Repository configuration and contributor templates.
/.github/             @June74

# Approved product documents and frozen contracts.
/docs/                @June74
```

- [ ] **Step 3: Write the branch-protection record**

Create `.github/branch-protection.md`:

```markdown
# Branch protection

> **Status:** not yet enabled. This document records the ruleset that Wave C
> will apply to `main` once continuous integration produces checks worth
> requiring.

## Why it is deferred

`CONTRIBUTING.md` states that direct implementation commits to `main` are not
allowed. That rule is currently enforced by process, not by GitHub. A
protection rule cannot require status checks that do not exist, so enabling
protection before Wave C would either require nothing or block every merge.

Until Wave C, independent review is performed locally with `codex exec` after
implementation and before integration.

## Ruleset to enable in Wave C

Applied to `main`:

| Setting | Value | Reason |
|---|---|---|
| Require a pull request before merging | Enabled | Matches the rule already published in `CONTRIBUTING.md`. |
| Required approving reviews | 0 | OpenMemory has a single maintainer, who cannot approve their own pull request. Independent review is provided by `codex exec` and recorded in the pull-request evidence, not by a GitHub approval. |
| Require status checks to pass | Enabled | The check list is defined by the Wave C workflow. |
| Require branches to be up to date | Enabled | Prevents merging against a stale base. |
| Require signed commits | Not enabled | The project requires DCO sign-off, which is a `Signed-off-by` trailer, not a cryptographic signature. |
| Allow force pushes | Disabled | History is evidence. `AGENTS.md` prohibits destructive Git operations. |
| Allow deletions | Disabled | Same reason. |
| Enforce for administrators | Enabled | A rule the maintainer can silently bypass is not a control. |

## Required checks

To be filled in by Wave C with the exact job names from the CI workflow.
Wave C is not complete until this section names real, passing checks.
```

- [ ] **Step 4: Verify locally, then confirm CODEOWNERS server-side**

Local check first:

```bash
git add .github/
bash tools/check-links.sh; echo "exit=$?"
```

Expected: `exit=0`.

The `codeowners/errors` endpoint reads the **default branch**, so it cannot validate work on a feature branch. Confirm syntax locally by checking that every rule line has a pattern and at least one owner:

```bash
grep -vE '^\s*(#|$)' .github/CODEOWNERS | grep -vE '\S+\s+@\S+' && echo "MALFORMED" || echo "syntax OK"
```

Expected: `syntax OK`.

Re-run `gh api repos/June74/openmemory/codeowners/errors` after this branch merges to `main`; it must return an empty `errors` array. Record that as the closing evidence for this task in Task 7.

- [ ] **Step 5: Commit**

```bash
git add .github/CODEOWNERS .github/branch-protection.md
git commit -s -m "Add CODEOWNERS and record deferred branch-protection ruleset

Ownership is assigned for every path, per AGENTS.md. Branch protection is
documented but not enabled until Wave C produces status checks to require."
```

---

### Task 3: Issue and pull-request templates

**Files:**
- Create: `.github/PULL_REQUEST_TEMPLATE.md`
- Create: `.github/ISSUE_TEMPLATE/config.yml`
- Create: `.github/ISSUE_TEMPLATE/bug_report.yml`
- Create: `.github/ISSUE_TEMPLATE/feature_request.yml`
- Create: `.github/ISSUE_TEMPLATE/documentation.yml`

**Interfaces:**
- Consumes: `bash tools/check-links.sh` from Task 1.
- Produces: nothing later tasks depend on.

- [ ] **Step 1: Write the YAML validity check and verify it fails**

Run:

```bash
node -e '
const fs=require("fs"),d=".github/ISSUE_TEMPLATE";
const f=fs.existsSync(d)?fs.readdirSync(d).filter(n=>n.endsWith(".yml")):[];
if(f.length===0){console.error("FAIL: no issue templates found");process.exit(1)}
console.log("found",f.length)'
```

Expected: FAIL — `FAIL: no issue templates found`, exit 1. The directory does not exist.

- [ ] **Step 2: Write the pull-request template**

Create `.github/PULL_REQUEST_TEMPLATE.md`:

```markdown
## What changed and why

<!-- The behavior delivered and the reason for it. Not a file list. -->

## Requirements and decisions satisfied

<!-- Cite the D-*, F-*, X-*, or REQ-* identifiers this change satisfies or
     affects. Write "none" only for repository maintenance. -->

## Security and privacy effects

<!-- State explicitly whether this changes the trust surface: encryption, key
     storage, secret redaction, imports, prompt-injection defenses, MCP
     permissions, updater trust, backup, export, or deletion.
     Write "no trust-surface change" if none apply. -->

## Verification

<!-- The exact commands run and their results, plus the real user-facing path
     exercised. Per AGENTS.md, no change is "done" or "working" without
     current command output or an equivalent acceptance check. -->

## Remaining limitations or deferred work

<!-- Known gaps, assumptions, and anything intentionally left for later. -->

---

- [ ] Every commit is signed off (`git commit -s`), per the Developer Certificate of Origin.
- [ ] This change contains no secret value, credential, private transcript, or personal memory content.
- [ ] Planned behavior is not described as though it were implemented.
```

- [ ] **Step 3: Write the issue-template configuration**

Create `.github/ISSUE_TEMPLATE/config.yml`:

```yaml
blank_issues_enabled: false
contact_links:
  - name: Report a security vulnerability privately
    url: https://github.com/June74/openmemory/security/advisories/new
    about: Never open a public issue containing exploit details, credentials, or private memory content.
  - name: Read the security policy
    url: https://github.com/June74/openmemory/blob/main/SECURITY.md
    about: Reporting process, security boundaries, and supported versions.
```

- [ ] **Step 4: Write the bug-report form**

Create `.github/ISSUE_TEMPLATE/bug_report.yml`:

```yaml
name: Bug report
description: Report incorrect behavior in OpenMemory or its documentation.
labels: ["bug", "needs-triage"]
body:
  - type: markdown
    attributes:
      value: |
        OpenMemory is in its planning and foundation stage. No supported binary
        exists yet, so most reports at this point concern documentation.

        Never paste credentials, API keys, private transcripts, or personal
        memory content into this form. Suspected vulnerabilities are reported
        privately instead — see the security policy.
  - type: input
    id: revision
    attributes:
      label: Affected revision
      description: The commit SHA or tag where you observed this.
    validations:
      required: true
  - type: textarea
    id: expected
    attributes:
      label: Expected behavior
    validations:
      required: true
  - type: textarea
    id: actual
    attributes:
      label: Actual behavior
    validations:
      required: true
  - type: textarea
    id: reproduction
    attributes:
      label: Reproduction steps
      description: Exact commands or navigation. Redact private paths and values.
    validations:
      required: true
  - type: checkboxes
    id: hygiene
    attributes:
      label: Confirmation
      options:
        - label: This report contains no secret value, credential, or private memory content.
          required: true
```

- [ ] **Step 5: Write the feature-request form**

Create `.github/ISSUE_TEMPLATE/feature_request.yml`:

```yaml
name: Feature request
description: Propose a capability or a change to approved behavior.
labels: ["enhancement", "needs-triage"]
body:
  - type: markdown
    attributes:
      value: |
        Version 1 scope is fixed and several capabilities are explicitly
        excluded. Check the `X-*` exclusion list in the decision register
        before opening this request.
  - type: textarea
    id: problem
    attributes:
      label: Problem
      description: The user-facing problem, described without a proposed solution.
    validations:
      required: true
  - type: textarea
    id: proposal
    attributes:
      label: Proposed behavior
    validations:
      required: true
  - type: input
    id: decision
    attributes:
      label: Related decision or requirement
      description: The D-*, F-*, or REQ-* identifier this relates to, or "none".
    validations:
      required: true
  - type: checkboxes
    id: scope
    attributes:
      label: Scope confirmation
      options:
        - label: I checked the X-* exclusion list and this is not already excluded from version 1.
          required: true
```

- [ ] **Step 6: Write the documentation form**

Create `.github/ISSUE_TEMPLATE/documentation.yml`:

```yaml
name: Documentation issue
description: Report unclear, incorrect, or missing documentation.
labels: ["documentation", "needs-triage"]
body:
  - type: input
    id: location
    attributes:
      label: Document and section
      description: For example, docs/ARCHITECTURE.md and the section heading.
    validations:
      required: true
  - type: textarea
    id: problem
    attributes:
      label: What is wrong or unclear
    validations:
      required: true
  - type: textarea
    id: suggestion
    attributes:
      label: Suggested correction
    validations:
      required: false
```

- [ ] **Step 7: Run the YAML validity check to verify it passes**

Run:

```bash
node -e '
const fs=require("fs"),d=".github/ISSUE_TEMPLATE";
const f=fs.readdirSync(d).filter(n=>n.endsWith(".yml"));
if(f.length!==4){console.error("FAIL: expected 4 yml files, found",f.length);process.exit(1)}
for(const n of f){
  const t=fs.readFileSync(d+"/"+n,"utf8");
  if(t.includes("\t")){console.error("FAIL: tab character in",n);process.exit(1)}
  if(n!=="config.yml"&&!/^name:\s+\S/m.test(t)){console.error("FAIL: missing name in",n);process.exit(1)}
}
console.log("PASS:",f.length,"template files well-formed")'
```

Expected: PASS — `PASS: 4 template files well-formed`.

This is a structural check only. GitHub validates issue-form schema exclusively on the pushed ref, so Task 7 confirms rendering server-side.

- [ ] **Step 8: Commit**

```bash
git add .github/PULL_REQUEST_TEMPLATE.md .github/ISSUE_TEMPLATE/
git commit -s -m "Add issue forms and pull-request template

The pull-request template encodes the evidence list CONTRIBUTING.md already
requires. Blank issues are disabled and vulnerability reports are routed to
private reporting rather than the public tracker."
```

---

### Task 4: Compatibility and versioning policy

**Files:**
- Create: `docs/COMPATIBILITY.md`

**Interfaces:**
- Consumes: `bash tools/check-links.sh` from Task 1.
- Produces: `docs/COMPATIBILITY.md`, linked from `README.md` and `CONTRIBUTING.md` in Task 6, and cited by Stage 2 contract work.

Implements spec §4.1. Write the prose from that section; the normative content below is decision-complete and must appear.

- [ ] **Step 1: Verify the document is absent**

Run: `test -f docs/COMPATIBILITY.md && echo "exists" || echo "absent"`

Expected: `absent`.

- [ ] **Step 2: Write the document**

Create `docs/COMPATIBILITY.md` with these sections, in order.

**Front matter.** A status line stating this is a planning-stage policy: the rules are approved, but no released version exists to which they yet apply.

**§1 Versioned surfaces.** This exact table:

| Surface | Scheme | Initial value | Notes |
|---|---|---|---|
| Product | SemVer 2.0.0 | `0.1.0` | Service, CLI, and installer released as one unit. This is the version `D-071`'s updater reasons about. |
| MCP protocol | Integer | `1` | Negotiated per connection. |
| Named-pipe envelope | Integer | `1` | Framing and capability contract from `ARCHITECTURE.md`. |
| Database schema | Integer | `1` | Monotonic migration number. Forward-only. |
| Normalized event envelope | Integer | `1` | The client-neutral contract all three adapters emit. |
| Markdown projection protocol | Integer | `1` | Governs the two-way projection in `D-065`. |
| Portable export format | Integer | `1` | The format in `D-083`. Must stay readable without the encrypted database. |
| Obsidian plugin | SemVer | `0.1.0` | Obsidian's `manifest.json` requires SemVer and a `minAppVersion`, so this surface cannot use an integer. |

**§2 What "breaking" means.** A change is breaking when an existing peer that was previously accepted would be rejected, misread, or would silently lose data. Adding an optional field is not breaking. Changing an existing field's meaning, type, or required-ness is breaking. Removing a field is breaking. Tightening validation on previously accepted input is breaking.

**§3 Support windows.**

- The service accepts the **current and immediately previous** integer for the MCP protocol and the named-pipe envelope. Anything older is rejected with a version-mismatch error that names the supported range. A rejected version is never handled by best-effort parsing.
- The **database schema is forward-only**. The service refuses to open a database whose schema integer exceeds the value the running binary knows. This refusal is what makes rollback after a failed update safe: an older binary declines rather than corrupting newer data.
- The **portable export format** is readable by every later version. This is the promise behind `D-083`, so it has no support window; support is permanent.
- The **event envelope** and **projection protocol** follow the current-and-previous rule.

**§4 Pre-1.0 policy.** Before product `1.0.0`, every contract may break without a support window, because no supported release exists. Development builds are explicitly not compatible with each other. State this plainly so a development artifact is never mistaken for a compatible one.

**§5 Relationship to automatic updates.** Automatic installation is permitted only for a product MINOR or PATCH release in which **no contract integer increases**. Any contract increment, and any product MAJOR, requires explicit approval. Note in the text that this is a deliberately stricter reading of `D-071` than its literal wording: it converts "major or permission-changing updates require approval" from a judgement call into a mechanical test, and it means a patch release carrying a database migration still requires approval.

**§6 Freeze status.** Every contract integer is **unfrozen** until Stage 2 freezes it. Name Stage 2 as the freezing stage and state that Stage 3 adapters consume the frozen contract rather than extending it.

- [ ] **Step 3: Verify links and content**

```bash
git add docs/COMPATIBILITY.md
bash tools/check-links.sh; echo "exit=$?"
grep -c '^## ' docs/COMPATIBILITY.md
```

Expected: `exit=0`, and at least `6` top-level sections.

- [ ] **Step 4: Commit**

```bash
git add docs/COMPATIBILITY.md
git commit -s -m "Add versioning and compatibility policy

One SemVer product version plus independent integer versions for the MCP
protocol, pipe envelope, database schema, event envelope, projection
protocol, and portable export. All contract integers start at 1 and remain
unfrozen until Stage 2."
```

---

### Task 5: Identifier registry

**Files:**
- Create: `docs/IDENTIFIERS.md`

**Interfaces:**
- Consumes: `bash tools/check-links.sh` from Task 1.
- Produces: `docs/IDENTIFIERS.md`, linked from `README.md` in Task 6. Defines the `REQ-<AREA>-NNN` scheme that later requirement work applies.

Implements spec §4.2.

- [ ] **Step 1: Verify the document is absent**

Run: `test -f docs/IDENTIFIERS.md && echo "exists" || echo "absent"`

Expected: `absent`.

- [ ] **Step 2: Write the document**

Create `docs/IDENTIFIERS.md` with these sections.

**§1 Documentation identifiers.** This exact table:

| Pattern | Meaning | Status | Defined in |
|---|---|---|---|
| `D-NNN` | Approved decision | In use | `DECISION_REGISTER.md` |
| `X-NNN` | Explicit version 1 exclusion | In use | `DECISION_REGISTER.md` |
| `F-NNN` | Deferred decision | In use | `DECISION_REGISTER.md` |
| `SET-YYYYMMDD-NNN` | Setback record | In use | `docs/operations/setbacks/` |
| `REQ-<AREA>-NNN` | Product requirement | Defined here, applied later | `PRODUCT_REQUIREMENTS.md` |
| `ADR-NNNN` | Long-form architecture decision record | Reserved, none created | — |

State that `D-*` remains the single decision register and `ADR-*` is used only when a decision needs context, alternatives, and consequences at a length the register table cannot hold. Two competing decision systems are explicitly not wanted.

State that `REQ-*` is defined but not yet applied to existing requirement text, and that applying it is a separate task (spec §7).

**§2 Runtime identifiers.**

| Identifier | Format | Rationale |
|---|---|---|
| `event_id` | UUIDv7 (RFC 9562) | Time-sortable, which gives index locality for the append-only journal. |
| `project_id` | UUIDv7 | Stable across rename and move. |
| `repository_id` | UUIDv7 | Stable across rename, move, and re-clone. |
| `installation_id` | UUIDv7 | Stable across upgrade; regenerated only on a genuinely new installation. |
| `private_store_id` | UUIDv7 | The stable identifier in the reciprocal manifest of `D-059`. |
| `vault_id` | UUIDv7 | The other half of that reciprocal pair. |
| `source_record_id` | `{adapter}:{native_id}` | Opaque, adapter-namespaced string. Codex, Claude Code, and Antigravity native IDs share no common format and must not be forced into one. |
| `content_hash` | SHA-256, lowercase hex | Idempotency and deduplication. |

Record why UUIDv7 was chosen over ULID: it is a published standard, .NET provides it directly through `Guid.CreateVersion7()`, and it sorts by creation time.

**§3 Stability rules.** These three rules, stated as requirements:

1. An identifier is never reused, even after the thing it names is deleted.
2. An identifier is never derived from mutable data. Paths, folder names, project names, and branch names all change; identifiers must survive rename, move, and hardware transfer.
3. An identifier must not encode user content or any secret value. This follows from `DATA_AND_PRIVACY.md §4`: an identifier is a value that gets logged, exported, and displayed, so anything encoded in it is effectively public to every one of those surfaces.

- [ ] **Step 3: Verify links and that no example secret was introduced**

```bash
git add docs/IDENTIFIERS.md
bash tools/check-links.sh; echo "exit=$?"
grep -inE '(sk-|ghp_|api[_-]?key\s*[:=]\s*\S)' docs/IDENTIFIERS.md && echo "FAIL: possible secret-shaped example" || echo "no secret-shaped example"
```

Expected: `exit=0` and `no secret-shaped example`.

- [ ] **Step 4: Commit**

```bash
git add docs/IDENTIFIERS.md
git commit -s -m "Add stable identifier registry

Formalizes the D-*, X-*, F-*, and SET-* schemes already in use, adds REQ-*,
reserves ADR-*, and specifies UUIDv7 runtime identifiers with three
stability rules."
```

---

### Task 6: Edits to existing documents

Narrow additions only. No rewrites and no reformatting: `AGENTS.md` prohibits reformatting files owned by another lane, and unrelated churn makes the diff unreviewable.

**Files:**
- Modify: `CONTRIBUTING.md`
- Modify: `docs/DATA_AND_PRIVACY.md:63-71`
- Modify: `docs/DECISION_REGISTER.md`
- Modify: `README.md`

**Interfaces:**
- Consumes: `docs/COMPATIBILITY.md` (Task 4), `docs/IDENTIFIERS.md` (Task 5), `.github/branch-protection.md` (Task 2).
- Produces: `DC-1`…`DC-7` data-class labels that later documents cite.

- [ ] **Step 1: Verify the new documents are currently unreferenced**

```bash
grep -rn "COMPATIBILITY.md\|IDENTIFIERS.md" README.md CONTRIBUTING.md docs/*.md | grep -v "^docs/superpowers" || echo "unreferenced"
```

Expected: `unreferenced`.

- [ ] **Step 2: Add data-class labels**

In `docs/DATA_AND_PRIVACY.md`, add a leading `Class ID` column to the existing table at lines 63–71, assigning `DC-1` through `DC-7` top to bottom: `DC-1` Raw evidence, `DC-2` Durable memory, `DC-3` Private provenance, `DC-4` Search material, `DC-5` Operational metadata, `DC-6` Recovery material, `DC-7` Portable export.

**Do not change any existing cell text.** Add one sentence below the table stating that other documents cite a class by its `DC-*` label rather than restating its handling rules.

- [ ] **Step 3: Add decision-register entries**

In `docs/DECISION_REGISTER.md`, append to the "Approved technology decisions" table:

```
| D-090 | Version the product with SemVer and each contract surface with an independent integer, as specified in [Compatibility](COMPATIBILITY.md). | A database migration does not force a major product release, while the pipe can reject on a protocol integer. Automatic installation requires that no contract integer increased. |
| D-091 | Document the branch-protection ruleset in Stage 0 Wave A but enable it in Wave C, once continuous integration produces checks worth requiring. | Until then, `main` is protected by process rather than by GitHub, and independent review is performed locally with `codex exec` before integration. |
```

- [ ] **Step 4: Update CONTRIBUTING.md**

Add a "Required checks" subsection under "Development workflow" stating that checks are not yet enforced by GitHub, that the ruleset is recorded in `.github/branch-protection.md`, and that until Wave C every change receives an independent `codex exec` review before integration.

The link written into `CONTRIBUTING.md` must use the repository-root-relative target `.github/branch-protection.md`, because `CONTRIBUTING.md` sits at the repository root. Writing `../.github/...` would escape the repository and fail the Step 6 link check.

Add a "License headers" subsection stating that source files added from Wave B onward carry an Apache-2.0 header, that Wave A adds no source files, and that `LICENSE` and `NOTICE` remain authoritative.

Add links to `docs/COMPATIBILITY.md` and `docs/IDENTIFIERS.md` in the "Before opening a change" list.

- [ ] **Step 5: Update README.md**

Add `docs/COMPATIBILITY.md` and `docs/IDENTIFIERS.md` to the existing documentation list, matching the surrounding link style exactly.

- [ ] **Step 6: Verify**

```bash
git add -A
bash tools/check-links.sh; echo "exit=$?"
grep -c 'DC-[1-7]' docs/DATA_AND_PRIVACY.md
grep -c 'D-09[01]' docs/DECISION_REGISTER.md
git diff --cached --stat
```

Expected: `exit=0`; at least `7` `DC-*` occurrences; `2` `D-09*` occurrences; and a diff touching exactly four files with a small line count. A large line count means something was reformatted — revert and redo.

- [ ] **Step 7: Commit**

```bash
git add CONTRIBUTING.md README.md docs/DATA_AND_PRIVACY.md docs/DECISION_REGISTER.md
git commit -s -m "Link Wave A records and label data classes

Adds DC-1..DC-7 labels to the existing data-class table without changing its
content, records D-090 and D-091, and documents the required-checks and
license-header policies."
```

---

### Task 7: Integration verification and independent review

The wave is not complete until this task passes. Per `AGENTS.md`, passing a focused check is not evidence that the wave is ready.

**Files:** none created or modified except a possible setback record.

**Interfaces:**
- Consumes: everything from Tasks 1–6.
- Produces: the evidence recorded in the pull-request body.

- [ ] **Step 1: Confirm no product capability was added**

```bash
git diff --stat main...HEAD
git diff --name-only main...HEAD | grep -E '\.(cs|ts|js|csproj|sln)$|package\.json|global\.json' && echo "FAIL: product code present" || echo "PASS: documentation and configuration only"
```

Expected: `PASS: documentation and configuration only`. This is the Stage 0 exit-gate requirement that no product capability is claimed.

- [ ] **Step 2: Confirm every commit is signed off**

```bash
git log main..HEAD --format='%H %s' | while read -r sha _; do
  git log -1 --format='%B' "$sha" | grep -q '^Signed-off-by:' || echo "MISSING SIGN-OFF: $sha"
done; echo "sign-off check complete"
```

Expected: no `MISSING SIGN-OFF` lines.

- [ ] **Step 3: Run the full link check**

Run: `bash tools/check-links.sh; echo "exit=$?"`

Expected: `exit=0`.

- [ ] **Step 4: Scan the whole diff for secret-shaped content**

```bash
git diff main...HEAD | grep -inE '(sk-[A-Za-z0-9]{8,}|ghp_[A-Za-z0-9]{8,}|-----BEGIN [A-Z ]*PRIVATE KEY-----|password\s*[:=]\s*\S+)' && echo "FAIL: investigate" || echo "PASS: no secret-shaped content"
```

Expected: `PASS: no secret-shaped content`.

- [ ] **Step 5: Independent review by Codex**

Per decision A-1, self-review is not independent review.

```bash
git diff main...HEAD > /tmp/wave-a.diff
codex exec "Review this diff as an independent specification and quality reviewer for the OpenMemory project. Read AGENTS.md, docs/IMPLEMENTATION_PLAN.md, and docs/superpowers/specs/2026-08-16-stage0-wave-a-design.md first. Verify: (1) the diff implements the Wave A spec with no gap and no scope creep; (2) no application code, dependency, or CI workflow was added; (3) no planned capability is described in the present tense as implemented; (4) no secret value appears anywhere including examples; (5) the versioning and identifier rules are internally consistent and consistent with the decision register. Report findings by severity with concrete evidence. Do not modify files." < /tmp/wave-a.diff
```

Record the findings. Per `receiving-code-review`, verify each finding technically rather than accepting it performatively, and resolve it with evidence before proceeding.

- [ ] **Step 6: Push and confirm server-side rendering**

```bash
git push -u origin codex/stage0-wave-a
```

Then confirm what only GitHub can validate:

```bash
gh api repos/June74/openmemory/contents/.github/ISSUE_TEMPLATE?ref=codex/stage0-wave-a --jq '.[].name'
```

Expected: the four template filenames.

Issue-form schema and CODEOWNERS validity are both evaluated against the **default branch**, so after this branch merges to `main`, run:

```bash
gh api repos/June74/openmemory/codeowners/errors --jq '.errors | length'
```

Expected: `0`. Until that returns `0`, Task 2 is not closed.

- [ ] **Step 7: Record any unexpected failure**

If any step above failed unexpectedly, create a setback record in `docs/operations/setbacks/` following the format of the existing five records, add it to `INDEX.md`, and commit it. Per `AGENTS.md`, unexpected failures are diagnosed and recorded before the plan is changed.

---

## Wave A completion criteria

All of the following, together:

1. Tasks 1–7 complete, every step checked.
2. `bash tools/check-links.sh` exits `0`.
3. No application code, dependency manifest, or CI workflow in the diff.
4. Every commit carries a `Signed-off-by` trailer.
5. Codex independent review findings resolved with evidence.
6. Issue templates confirmed present on the pushed ref.
7. `gh api repos/June74/openmemory/codeowners/errors` returns zero errors after merge to `main`.

Wave A adds **zero product capability**. Waves B, C, and D each get their own brainstorm, spec, and plan.
