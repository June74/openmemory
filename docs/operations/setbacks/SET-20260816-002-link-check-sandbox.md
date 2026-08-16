# SET-20260816-002: Sandbox blocked external-link requests

- **First observed:** 2026-08-16T01:44:37-05:00
- **Last observed:** 2026-08-16T01:44:37-05:00
- **Status:** Closed
- **Phase/task:** Documentation baseline external-link verification
- **Environment:** Windows 11, PowerShell, restricted network sandbox
- **Version/commit:** Pre-initial-commit working tree
- **Owner:** Root integrator

## Symptom and impact

A read-only external-link checker parsed the documentation correctly, but every HTTPS request was denied by the restricted sandbox. No documentation defect was established. External-link verification paused until the same bounded check received network permission.

## Safe evidence

- Command category: read-only HTTP `HEAD` requests to documentation links.
- Result category: socket access denied before remote responses were received.
- Scope: 19 unique HTTPS links extracted from Markdown files.
- No private data, authorization values, or response bodies were emitted.

## Cause analysis

- **Confirmed cause:** the default sandbox prohibited outbound sockets.
- **Rejected hypothesis:** the documentation links were not all simultaneously broken; the permitted rerun received successful responses from every target.
- **Known exclusions:** no repository mutation, remote mutation, credential use, or secret exposure occurred.

## Correction and prevention

The same read-only checker was rerun with explicit network permission. Future publication checks should expect the default sandbox denial and request narrowly scoped access rather than treating it as a link failure.

## Verification

The permitted checker completed on 2026-08-16. All 19 unique external documentation URLs returned HTTP status 200, and the command exited successfully.
