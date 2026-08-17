# Stage 0 Wave C — Continuous Integration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Establish CI for formatting, build, tests, dependency review, secret scanning, SBOM, and a checksummed development artifact — then enable branch protection against those checks.

**Architecture:** One workflow, `.github/workflows/ci.yml`, with six independently-required-able jobs. .NET jobs run on `windows-latest` because `D-003` fixes Windows as the only supported platform; jobs that do not touch the build run on `ubuntu-latest`. The repository is public, so Actions minutes are free and runner choice is driven by correctness rather than cost.

**Tech Stack:** GitHub Actions, .NET SDK 10.0.400, pnpm 11 / Node 24, gitleaks, Syft (SBOM), `actions/dependency-review-action`.

**Spec:** [docs/superpowers/specs/2026-08-16-stage0-wave-c-design.md](../specs/2026-08-16-stage0-wave-c-design.md)

## Global Constraints

Every task's requirements implicitly include this section.

- **No product behavior.** No `.cs` or `.ts` file gains logic. This wave adds automation only.
- **Repository:** `June74/openmemory`. **Branch:** `codex/stage0-wave-c`. Never commit to `main`.
- **DCO required.** Every commit uses `git commit -s`.
- **Never write a secret value** into any file, including examples. This wave configures secret scanning; a fake-but-realistic token in a workflow would be self-defeating and would trip the very scanners being installed.
- **No repository secrets are needed.** Every action used here works on a public repository with the default `GITHUB_TOKEN`. If a step appears to need a secret, stop and report — do not create one.
- **Pin every third-party action to an explicit major version tag** (`@v4`, not `@main`). An action referenced by a moving ref is an unpinned dependency in the security-critical path.
- **`permissions:` is declared explicitly** and kept least-privilege. Do not grant `write` where `read` suffices.
- **Never commit build output**, `node_modules`, SBOM files, or artifacts. They are produced by CI, not stored in the repository.
- **Use Bash** locally (Git Bash available), never a PowerShell pipeline. Inside the workflow, Windows steps use `shell: pwsh` where a Windows-native cmdlet is the right tool.
- **A workflow cannot be verified locally.** Tasks 1 and 2 validate YAML structure only; Task 3 is where real verification happens, on the pushed branch.

---

### Task 1: Workflow skeleton and the correctness jobs

**Files:**
- Create: `.github/workflows/ci.yml`

**Interfaces:**
- Consumes: `OpenMemory.sln`, `global.json`, `tools/check-links.sh`, and `src/OpenMemory.ObsidianPlugin/package.json` — all from `main`.
- Produces: job names `build-and-test`, `plugin`, and `docs`, which Task 4 records as required checks and Task 5 enforces. **These exact strings matter** — branch protection matches required checks by name, and a renamed job silently stops being enforced.

- [ ] **Step 1: Verify no workflow exists**

```bash
cd "C:/Users/2006i/projects/openmemory" && ls .github/workflows 2>&1
```

Expected: `No such file or directory`.

- [ ] **Step 2: Write the workflow with three jobs**

Create `.github/workflows/ci.yml`:

```yaml
name: CI

on:
  pull_request:
    branches: [main]
  push:
    branches: [main]

permissions:
  contents: read

concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true

jobs:
  build-and-test:
    name: build-and-test
    runs-on: windows-latest
    steps:
      - name: Check out
        uses: actions/checkout@v4

      - name: Set up .NET
        uses: actions/setup-dotnet@v4
        with:
          global-json-file: global.json

      - name: Verify formatting
        run: dotnet format OpenMemory.sln --verify-no-changes

      - name: Restore
        run: dotnet restore OpenMemory.sln

      - name: Build
        run: dotnet build OpenMemory.sln --no-restore --nologo

      - name: Run tests
        run: dotnet test OpenMemory.sln --no-build --nologo

      - name: Assert minimum discovered test count
        shell: pwsh
        run: |
          $expected = 4
          $listed = dotnet test OpenMemory.sln --no-build --nologo --list-tests
          $count = ($listed | Where-Object { $_ -match '^\s+\S+\.\S+\.\S+$' }).Count
          Write-Host "Discovered $count tests (floor $expected)"
          if ($count -lt $expected) {
            throw "Discovered $count tests, expected at least $expected. Either tests were removed, or test discovery is broken. dotnet test exits 0 in both cases, which is why this assertion exists."
          }

  plugin:
    name: plugin
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: src/OpenMemory.ObsidianPlugin
    steps:
      - name: Check out
        uses: actions/checkout@v4

      - name: Set up Node
        uses: actions/setup-node@v4
        with:
          node-version: 24

      - name: Enable corepack
        run: corepack enable

      - name: Install
        run: pnpm install --frozen-lockfile

      - name: Type-check
        run: pnpm run typecheck

  docs:
    name: docs
    runs-on: ubuntu-latest
    steps:
      - name: Check out
        uses: actions/checkout@v4

      - name: Check repository-internal links
        run: bash tools/check-links.sh
```

Three things in here are load-bearing and a reasonable-looking edit breaks them:

1. **`--list-tests` counts discovery, not results.** The point of C-5 is that `dotnet test` exits 0 both when there are no tests and when discovery is broken. Counting *discovered* tests distinguishes those. Do not replace this with a check on the exit code or on "Passed!" text.
2. **`corepack enable` before `pnpm`.** The plugin's `package.json` declares `packageManager: pnpm@11.9.0`; Corepack reads that field and provisions exactly that version. Installing pnpm some other way discards the pin.
3. **`--frozen-lockfile`.** This is why Wave B committed `pnpm-lock.yaml`. It fails rather than silently resolving different versions when lockfile and manifest disagree.

- [ ] **Step 3: Validate the YAML parses**

```bash
cd "C:/Users/2006i/projects/openmemory" && node -e '
const fs=require("fs");
const t=fs.readFileSync(".github/workflows/ci.yml","utf8");
if(t.includes("\t")){console.error("FAIL: tab character in YAML");process.exit(1)}
for(const j of ["build-and-test","plugin","docs"]){
  if(!t.includes("name: "+j)){console.error("FAIL: missing job name "+j);process.exit(1)}
}
console.log("PASS: structural check");'
```

Expected: `PASS: structural check`.

This is a structural check only. GitHub validates workflow schema when it runs the file, which is Task 3.

- [ ] **Step 4: Confirm the expected test count is correct**

```bash
cd "C:/Users/2006i/projects/openmemory" && dotnet test OpenMemory.sln --nologo --list-tests 2>&1 | grep -cE '^\s+\S+\.\S+\.\S+$'
```

Expected: `4`, matching the `$expected` floor in the workflow.

If this returns a different number, the regex does not match this environment's output format. **Report the actual output rather than loosening the pattern** — a matcher that matches nothing would make the assertion pass vacuously, which is the exact defect it exists to prevent.

- [ ] **Step 5: Commit**

```bash
git add .github/workflows/ci.yml
git commit -s -m "Add CI workflow with build, plugin, and docs jobs

The test step asserts a minimum discovered-test count because dotnet
test exits 0 both when no tests exist and when discovery is broken."
```

---

### Task 2: Security and release jobs

**Files:**
- Modify: `.github/workflows/ci.yml`

**Interfaces:**
- Consumes: the workflow from Task 1.
- Produces: job names `secret-scan`, `dependency-review`, and `artifact`. As in Task 1, **the exact strings matter** for Task 4 and Task 5.

- [ ] **Step 1: Determine the gitleaks version to pin**

```bash
gh api repos/gitleaks/gitleaks/releases/latest --jq '.tag_name'
```

Record the tag it returns. Pin that exact version in Step 2 — do not use `latest` in the workflow, because an unpinned scanner in the security-critical path is a supply-chain dependency that can change under you.

The binary is downloaded directly rather than using a marketplace action, because gitleaks' own action has had licensing conditions for some account types and this repository must not depend on that resolving favourably.

- [ ] **Step 2: Append the three jobs**

Add to `.github/workflows/ci.yml`, at the same indentation level as the existing jobs. Replace `<VERSION>` with the tag from Step 1 (including its leading `v`) and `<VERSION_NO_V>` with the same value without the leading `v`.

```yaml
  secret-scan:
    name: secret-scan
    runs-on: ubuntu-latest
    steps:
      - name: Check out
        uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - name: Install gitleaks
        run: |
          curl -sSfL -o gitleaks.tar.gz \
            "https://github.com/gitleaks/gitleaks/releases/download/<VERSION>/gitleaks_<VERSION_NO_V>_linux_x64.tar.gz"
          tar -xzf gitleaks.tar.gz gitleaks
          chmod +x gitleaks

      - name: Scan repository history
        run: ./gitleaks detect --source . --no-banner --redact --exit-code 1

  dependency-review:
    name: dependency-review
    runs-on: ubuntu-latest
    if: github.event_name == 'pull_request'
    steps:
      - name: Check out
        uses: actions/checkout@v4

      - name: Review dependencies
        uses: actions/dependency-review-action@v4

  artifact:
    name: artifact
    runs-on: windows-latest
    steps:
      - name: Check out
        uses: actions/checkout@v4

      - name: Set up .NET
        uses: actions/setup-dotnet@v4
        with:
          global-json-file: global.json

      - name: Publish executables
        shell: pwsh
        run: |
          foreach ($p in "Service", "Cli", "McpBridge") {
            dotnet publish "src/OpenMemory.$p/OpenMemory.$p.csproj" `
              --configuration Release `
              --output "artifacts/publish/OpenMemory.$p"
          }

      - name: Package and checksum
        shell: pwsh
        run: |
          New-Item -ItemType Directory -Force -Path artifacts/out | Out-Null
          Compress-Archive -Path artifacts/publish/* -DestinationPath artifacts/out/openmemory-dev.zip
          $hash = Get-FileHash -Algorithm SHA256 -Path artifacts/out/openmemory-dev.zip
          "$($hash.Hash.ToLower())  openmemory-dev.zip" |
            Out-File -FilePath artifacts/out/openmemory-dev.zip.sha256 -Encoding ascii
          Get-Content artifacts/out/openmemory-dev.zip.sha256

      - name: Generate SBOM
        uses: anchore/sbom-action@v0
        with:
          path: .
          format: cyclonedx-json
          output-file: artifacts/out/openmemory-sbom.cyclonedx.json
          upload-artifact: false

      - name: Upload artifact
        uses: actions/upload-artifact@v4
        with:
          name: openmemory-dev
          path: artifacts/out/
          if-no-files-found: error
```

Notes the YAML does not convey:

- **`fetch-depth: 0` on the secret-scan checkout** gives gitleaks the full history. The default shallow clone would scan only the tip commit, so a secret introduced and later removed would go undetected.
- **`--exit-code 1`** makes a finding fail the job. Without it gitleaks reports and exits 0, and the required check would pass while reporting leaks.
- **`--redact`** keeps any detected value out of the workflow log. Public repository: the log is world-readable, and a scanner that prints the secret it found has published it.
- **`if: github.event_name == 'pull_request'`** on dependency-review because that action only operates on a pull-request diff.
- **`shell: pwsh` on the Windows steps** because `Compress-Archive` and `Get-FileHash` are the native tools. These are single cmdlets, not chained pipelines — the setback that produced `SET-20260816-001` was a PowerShell *pipeline* parse failure.
- **`if-no-files-found: error`** so a silently empty artifact fails rather than uploading nothing.

Two version-drift traps to check rather than assume, because both fail in confusing ways:

- **The gitleaks subcommand.** Recent versions moved from `gitleaks detect` to `gitleaks git`, with `detect` deprecated. After pinning the version in Step 1, run `./gitleaks --help` (or check that release's notes) and use whichever subcommand that version actually documents. Record which you used. A deprecated subcommand may still run but emit a warning, and a removed one fails the job with an unhelpful error.
- **`anchore/sbom-action` input names.** The inputs used here (`path`, `format`, `output-file`, `upload-artifact`) are correct for recent v0 releases, but if the step errors on an unrecognised input, read the action's own `action.yml` rather than guessing a substitute — and record what you changed.

- [ ] **Step 3: Re-run the structural check for all six jobs**

```bash
cd "C:/Users/2006i/projects/openmemory" && node -e '
const fs=require("fs");
const t=fs.readFileSync(".github/workflows/ci.yml","utf8");
if(t.includes("\t")){console.error("FAIL: tab in YAML");process.exit(1)}
const jobs=["build-and-test","plugin","docs","secret-scan","dependency-review","artifact"];
for(const j of jobs){ if(!t.includes("name: "+j)){console.error("FAIL: missing "+j);process.exit(1)} }
if(/uses:\s+\S+@main/.test(t)){console.error("FAIL: action pinned to @main");process.exit(1)}
if(/gitleaks\/releases\/download\/latest/.test(t)){console.error("FAIL: gitleaks not version-pinned");process.exit(1)}
console.log("PASS: 6 jobs, all actions pinned");'
```

Expected: `PASS: 6 jobs, all actions pinned`.

- [ ] **Step 4: Commit**

```bash
git add .github/workflows/ci.yml
git commit -s -m "Add secret scan, dependency review, and artifact jobs

gitleaks is version-pinned and downloaded directly rather than through
a marketplace action with licensing conditions. It runs with full
history, redacted output, and a failing exit code on any finding.

The artifact job publishes the three executables, checksums the
archive, and uploads it with a CycloneDX SBOM. Per D-018 the checksum
is integrity evidence only; it is not publisher authentication."
```

---

### Task 3: Push and drive every job green

This is where the wave is actually verified. A workflow's first real execution happens on the pushed branch, so unlike Waves A and B, local checks cannot complete this one.

**Files:** `.github/workflows/ci.yml` and possibly source files, if the format check fails.

**Interfaces:**
- Consumes: Tasks 1 and 2.
- Produces: a green run whose job names Task 4 records.

- [ ] **Step 1: Push the branch**

```bash
cd "C:/Users/2006i/projects/openmemory" && git push -u origin codex/stage0-wave-c
```

- [ ] **Step 2: Watch the run**

```bash
cd "C:/Users/2006i/projects/openmemory" && gh run list --branch codex/stage0-wave-c --limit 1
gh run watch $(gh run list --branch codex/stage0-wave-c --limit 1 --json databaseId --jq '.[0].databaseId')
```

- [ ] **Step 3: Diagnose any failure at its root**

For each failing job:

```bash
gh run view <RUN_ID> --log-failed
```

Fix the cause and push again. Expected failures, and the correct response to each:

| Failure | Correct response |
|---|---|
| `dotnet format --verify-no-changes` fails | Wave B's files were never format-checked. Run `dotnet format OpenMemory.sln` locally, commit the result as one formatting commit. **Do not** remove or weaken the format step. |
| gitleaks reports a finding | Investigate it. If a real secret exists, stop and report immediately — do not commit over it. If it is a false positive, add a narrowly-scoped `.gitleaks.toml` allowlist entry with a comment explaining why, never a blanket ignore. |
| The test-count assertion fails | The regex does not match this runner's output format. Report the actual `--list-tests` output. **Do not lower `$expected` or loosen the regex** — that would restore the vacuous pass the assertion exists to prevent. |
| `dependency-review` fails | Read what it flagged. A vulnerable or incompatible-licence dependency is a real finding, not a check to disable. |
| An action version does not exist | Bump to the current major tag and record which version you used. |

Push after each fix and re-watch. Repeat until every job is green.

- [ ] **Step 4: Confirm the artifact is real**

```bash
cd "C:/Users/2006i/projects/openmemory"
RUN=$(gh run list --branch codex/stage0-wave-c --limit 1 --json databaseId --jq '.[0].databaseId')
mkdir -p /tmp/wave-c-artifact && gh run download "$RUN" --dir /tmp/wave-c-artifact
ls -R /tmp/wave-c-artifact
```

Expected: the zip, its `.sha256`, and the SBOM.

Then verify the checksum actually matches — a checksum file nobody has checked is decoration:

```bash
cd /tmp/wave-c-artifact/openmemory-dev && sha256sum -c openmemory-dev.zip.sha256
```

Expected: `openmemory-dev.zip: OK`.

- [ ] **Step 5: Record the final green run**

```bash
cd "C:/Users/2006i/projects/openmemory" && gh run list --branch codex/stage0-wave-c --limit 1 --json databaseId,conclusion,url --jq '.[0]'
```

Expected: `"conclusion": "success"`. Record the URL — it is this wave's primary acceptance evidence.

---

### Task 4: Record the required checks

**Files:**
- Modify: `.github/branch-protection.md`

**Interfaces:**
- Consumes: the green run's job names from Task 3.
- Produces: the documented check list Task 5 applies.

- [ ] **Step 1: Confirm the current placeholder**

```bash
cd "C:/Users/2006i/projects/openmemory" && sed -n '30,40p' .github/branch-protection.md
```

Expected: the "Required checks" section still says it is to be filled in by Wave C.

- [ ] **Step 2: Replace the status blockquote and the Required checks section**

Change the status blockquote at the top from "not yet enabled" to enabled, dated 2026-08-16, and replace the "Required checks" section body with:

```markdown
## Required checks

These are the job names from [`ci.yml`](workflows/ci.yml). Branch protection
matches required checks by name, so **renaming a job here or in the workflow
silently un-enforces it** — a required check that never reports simply never
blocks. Any job rename must update both files together.

| Check | What it verifies |
|---|---|
| `build-and-test` | C# formatting, restore, build, tests, and the minimum discovered-test count |
| `plugin` | The Obsidian plugin installs from the committed lockfile and type-checks |
| `docs` | Every repository-internal Markdown link resolves |
| `secret-scan` | gitleaks finds no secret in the full history |
| `dependency-review` | No vulnerable or incompatible-licence dependency is introduced |
| `artifact` | Publish, checksum, and SBOM generation succeed |
```

**Additions and replacements only.** Do not alter the ruleset table above it — that was settled in Wave A and is not this task's business.

- [ ] **Step 3: Verify**

```bash
cd "C:/Users/2006i/projects/openmemory" && git add .github/branch-protection.md && bash tools/check-links.sh; echo "exit=$?"
grep -c 'build-and-test\|plugin\|docs\|secret-scan\|dependency-review\|artifact' .github/branch-protection.md
```

Expected: `exit=0`, and at least `6` matches.

- [ ] **Step 4: Commit**

```bash
git add .github/branch-protection.md
git commit -s -m "Record the required checks from the CI workflow

Names the six job names branch protection will require, and states
that renaming a job silently un-enforces it unless both files change."
```

---

### Task 5: Repository settings and branch protection

**Files:** none. This task makes API calls against repository settings.

**STOP — this task requires explicit user approval before any call.** These changes take effect outside the repository tree, change how every future merge works, and cannot be reviewed in a diff. Present the exact calls, get a clear yes, then execute. Do not proceed on the plan's authority alone.

**Interfaces:**
- Consumes: Task 4's documented check list, and a green run from Task 3.
- Produces: enforced branch protection.

- [ ] **Step 1: Record the "before" state**

```bash
gh api repos/June74/openmemory --jq '.security_and_analysis'
gh api repos/June74/openmemory/branches/main/protection 2>&1 | head -3
```

Expected: non-provider patterns and validity checks `disabled`; protection returns 404 "Branch not protected".

- [ ] **Step 2: Enable the two secret-scanning settings**

```bash
gh api -X PATCH repos/June74/openmemory \
  -f 'security_and_analysis[secret_scanning_non_provider_patterns][status]=enabled' \
  -f 'security_and_analysis[secret_scanning_validity_checks][status]=enabled'
```

- [ ] **Step 3: Enable branch protection**

The ruleset below is transcribed from `.github/branch-protection.md`, which was settled in Wave A. `enforce_admins: true` is deliberate — a rule the maintainer can silently bypass is not a control.

```bash
cd "C:/Users/2006i/projects/openmemory" && cat > /tmp/protection.json <<'JSON'
{
  "required_status_checks": {
    "strict": true,
    "contexts": ["build-and-test", "plugin", "docs", "secret-scan", "dependency-review", "artifact"]
  },
  "enforce_admins": true,
  "required_pull_request_reviews": {
    "required_approving_review_count": 0,
    "dismiss_stale_reviews": false,
    "require_code_owner_reviews": false
  },
  "restrictions": null,
  "allow_force_pushes": false,
  "allow_deletions": false
}
JSON
gh api -X PUT repos/June74/openmemory/branches/main/protection --input /tmp/protection.json
```

- [ ] **Step 4: Verify the "after" state**

```bash
gh api repos/June74/openmemory --jq '.security_and_analysis'
gh api repos/June74/openmemory/branches/main/protection --jq '{
  checks: .required_status_checks.contexts,
  strict: .required_status_checks.strict,
  admins: .enforce_admins.enabled,
  force_push: .allow_force_pushes.enabled,
  deletions: .allow_deletions.enabled
}'
```

Expected: both secret-scanning settings `enabled`; six contexts listed; `strict: true`; `admins: true`; both `force_push` and `deletions` false.

---

### Task 6: Integration verification and independent review

**Files:** none created or modified except a possible setback record.

- [ ] **Step 1: Confirm no product behavior was added**

```bash
cd "C:/Users/2006i/projects/openmemory"
git diff --name-only main...HEAD
git diff main...HEAD -- '*.cs' '*.ts' | head -20
```

Expected: only `.github/` and `docs/` paths, unless Task 3 required a formatting commit — in which case `.cs` changes must be whitespace only, with no logic added.

- [ ] **Step 2: DCO and secret scan**

```bash
cd "C:/Users/2006i/projects/openmemory"
for sha in $(git log main..HEAD --format=%H); do
  git log -1 --format=%B "$sha" | grep -qE '^Signed-off-by:' || echo "MISSING SIGN-OFF: $sha"
done; echo "sign-off check complete"
git diff main...HEAD | grep -inE '(sk-[A-Za-z0-9]{8,}|ghp_[A-Za-z0-9]{8,}|-----BEGIN [A-Z ]*PRIVATE KEY-----)' && echo "FAIL" || echo "PASS: no secret-shaped content"
```

- [ ] **Step 3: Links**

```bash
cd "C:/Users/2006i/projects/openmemory" && bash tools/check-links.sh; echo "exit=$?"
```

Expected: `exit=0`.

- [ ] **Step 4: Confirm CI is green on the final commit**

```bash
cd "C:/Users/2006i/projects/openmemory" && gh run list --branch codex/stage0-wave-c --limit 1 --json headSha,conclusion --jq '.[0]'
git rev-parse HEAD
```

Expected: `"conclusion": "success"`, and `headSha` matching local `HEAD`. A green run against an older commit is not evidence about the current tree.

- [ ] **Step 5: Independent review by Codex**

```bash
cd "C:/Users/2006i/projects/openmemory"
git diff main...HEAD > "C:/Users/2006i/AppData/Local/Temp/claude/C--Users-2006i-projects-openmemory/321382d3-fcf5-498c-8a6d-843b2f666ac1/scratchpad/wave-c.diff"
codex exec "Review this diff as an independent specification and quality reviewer for the OpenMemory project. Read AGENTS.md, docs/IMPLEMENTATION_PLAN.md, and docs/superpowers/specs/2026-08-16-stage0-wave-c-design.md first. Verify: (1) the diff implements the Wave C spec with no gap and no scope creep; (2) no product behavior was added; (3) every third-party action and downloaded tool is pinned to an explicit version, and permissions are least-privilege; (4) the secret-scan job cannot pass while leaking — check exit code handling, history depth, and log redaction; (5) the test-count assertion cannot pass vacuously; (6) the required-check names in .github/branch-protection.md exactly match the job names in ci.yml. Report findings by severity with concrete evidence. Do not modify files." < "C:/Users/2006i/AppData/Local/Temp/claude/C--Users-2006i-projects-openmemory/321382d3-fcf5-498c-8a6d-843b2f666ac1/scratchpad/wave-c.diff"
```

Write the diff to a scratch directory **outside** the repository so it does not appear as an untracked file. Record Codex's findings verbatim; per `receiving-code-review`, verify each technically rather than accepting it performatively.

- [ ] **Step 6: Record any unexpected failure**

If any step failed unexpectedly, create a setback record in `docs/operations/setbacks/` following the existing five, add it to `INDEX.md`, and commit.

---

## Wave C completion criteria

1. Tasks 1–6 complete.
2. All six CI jobs green on a run whose `headSha` matches the branch tip.
3. The downloaded artifact's SHA-256 verifies against the archive.
4. `.github/branch-protection.md` names the six real job names.
5. Branch protection active on `main` with those six contexts, `strict: true`, `enforce_admins: true`, force-pushes and deletions disabled.
6. Non-provider patterns and validity checks enabled.
7. Every commit DCO-signed; no secret-shaped content.
8. Codex independent review findings resolved with evidence.
9. No product behavior added beyond whitespace-only formatting changes, if any.
