# Branch protection

> **Status:** The required checks below are now real and verified green.
> Enablement to GitHub is pending explicit approval from the repository owner.

## Enablement is pending repository owner approval

Originally, protection was deferred because a protection rule cannot require
status checks that do not exist, and the CI workflow did not exist yet. That
prerequisite has been satisfied: the required checks are now defined, verified
green, and documented in the section below.

The remaining gate is explicit approval from the repository owner to enable
GitHub branch protection. Until that approval is given, `main` remains
protected by process: `CONTRIBUTING.md` states that direct implementation
commits to `main` are not allowed. That rule is currently enforced by policy
and code review, not by GitHub. Independent review is performed locally with
`codex exec` after implementation and before integration, and this remains
the operative control.

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

These are the job names from [`ci.yml`](workflows/ci.yml). Branch protection
matches required checks by name, so **renaming a job here or in the workflow
without updating the other blocks merging instead of un-enforcing anything**
— GitHub fails closed: a required context that never reports leaves the pull
request waiting on it indefinitely, blocking the merge rather than silently
letting it through. This differs from a job that reports as *skipped*, which
GitHub treats as passing; a renamed job's old context reports nothing at
all, so the two behave oppositely. Any job rename must update both files
together to avoid a permanently blocked pull request.

| Check | What it verifies |
|---|---|
| `build-and-test` | C# formatting, restore, build, tests, and the minimum discovered-test count |
| `plugin` | The Obsidian plugin installs from the committed lockfile and type-checks |
| `docs` | Every repository-internal Markdown link resolves |
| `secret-scan` | gitleaks finds no secret in the full history |
| `dependency-review` | No known-vulnerable dependency is introduced by the pull request |
| `artifact` | Publish, checksum, and SBOM generation succeed |

Licence policy is not configured for `dependency-review`: the action runs
without `allow-licenses`/`deny-licenses` inputs, so it checks vulnerabilities
only. Configuring licence enforcement requires first deciding which licences
are acceptable — a product decision that has not been made.
