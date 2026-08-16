# SET-20260816-003: Sandbox blocked Git index write

- **First observed:** 2026-08-16T01:52:43-05:00
- **Last observed:** 2026-08-16T01:52:43-05:00
- **Status:** Closed
- **Phase/task:** Initial documentation-baseline staging
- **Environment:** Windows 11, Git, workspace sandbox
- **Version/commit:** Empty repository before first commit
- **Owner:** Root integrator

## Symptom and impact

Repository initialization succeeded, but the first explicit `git add` could not create `.git/index.lock` because Git metadata was read-only in the default sandbox. No file was staged, committed, overwritten, or published.

## Safe evidence

- Git reported permission denied for the exact repository index-lock path.
- `git status --short` continued to show every intended file as untracked.
- The command also warned that the user's global excludes file was unreadable; the repository's explicit `.gitignore` remained available.
- No credential, token, or private content was emitted.

## Cause analysis

- **Confirmed causes:** the execution sandbox allowed workspace-file writes but not the required `.git` metadata write; the permitted process then correctly rejected the empty repository because `.git` was created under the sandbox identity rather than the interactive Windows identity.
- **Rejected hypotheses:** an existing lock and a partial staging operation were excluded because the repository was newly initialized and status showed no staged paths. The later cached-diff option errors were downstream of Git treating the directory as no repository after its ownership rejection.

## Correction and prevention

Rerun only the approved, explicit Git staging and commit operations with repository-metadata permission. Use a per-command `safe.directory` override for this exact path first, without altering global Git configuration. Continue to inspect the exact staged paths and cached diff before committing.

## Verification

The permitted staging rerun succeeded with the exact documentation path set, and `git diff --cached --check` passed. Publication remains governed separately by the commit and remote-verification gates.
