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
