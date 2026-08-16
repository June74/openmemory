# SET-20260816-005: Active workspace prevented folder rename

- **First observed:** 2026-08-16T01:58:52-05:00
- **Last observed:** 2026-08-16T01:58:52-05:00
- **Status:** Closed
- **Phase/task:** Local folder migration
- **Environment:** Windows 11, active Codex workspace
- **Version/commit:** `24814cbb8d4bc97ed6eec3ff00f00d5376433ec5`
- **Owner:** Root integrator

## Symptom and impact

After exact-path validation confirmed that the source existed and the destination did not, Windows rejected the requested folder rename because the active workspace was in use. Nothing moved, and the source repository remained intact.

## Safe evidence

- Exact source: the current `ai_memory` project folder under the user's projects directory.
- Exact destination: the sibling `openmemory` project folder.
- The destination was absent immediately before the attempt.
- Native PowerShell `Move-Item` returned an in-use error without a partial move.
- No file was deleted, overwritten, or made unrecoverable.

## Cause analysis

- **Confirmed cause:** at least one process retained an open handle in the active workspace tree.
- **Expected context:** the approved migration plan anticipated that Windows might prevent renaming the currently active workspace.
- **Rejected hypotheses:** destination collision and wrong-path resolution were excluded by the preceding safety check.

## Correction and prevention

Use the approved non-destructive fallback: clone the verified remote into the exact `openmemory` destination, compare repository identity, commit, and files, and retain `ai_memory` until the user reopens the new workspace and separately approves old-folder removal.

## Verification

The remote was cloned into the exact `openmemory` destination. The destination was clean on `main`, its `origin` was `https://github.com/June74/openmemory.git`, and its commit exactly matched both the published remote and the source repository before this incident record was added. The old folder remains intact pending user reopen and separate deletion approval.
