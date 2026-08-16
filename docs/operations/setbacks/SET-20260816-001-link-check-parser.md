# SET-20260816-001: PowerShell link-check pipeline parse failure

- **First observed:** 2026-08-16T01:39:50-05:00
- **Last observed:** 2026-08-16T01:39:50-05:00
- **Status:** Closed
- **Phase/task:** Documentation baseline external-link verification
- **Environment:** Windows 11, PowerShell
- **Version/commit:** Pre-initial-commit working tree
- **Owner:** Root integrator

## Symptom and impact

The first read-only external-link checker stopped at PowerShell parse time with an empty-pipeline-element error. It did not make network requests, edit files, or change remote state. External-link verification was delayed until a simpler command could be used.

## Reproduction conditions

A compound `foreach` expression containing nested `try`/`catch` blocks was sent directly into a formatting pipeline. The parser rejected the pipeline boundary following the loop.

## Safe evidence

- Command category: read-only Markdown external-link status check.
- Result category: PowerShell parser error before execution.
- No private data, authorization values, or response bodies were emitted.

## Attempts and outcomes

1. **Compound loop piped directly to formatting:** failed during parsing; no requests ran.
2. **Corrected collection-based loop:** completed successfully and checked every external URL.

## Cause analysis

- **Confirmed cause:** the command's direct pipeline placement after the compound `foreach`/`try` expression was not valid in the submitted PowerShell form, as shown by the parser location.
- **Hypothesis:** none remains necessary for the contained parser failure.
- **Rejected hypotheses:** network restriction and broken documentation links were not evaluated because parsing stopped before any request.
- **Known exclusions:** no repository mutation, remote mutation, credential exposure, or link request occurred.

## Correction and prevention

- Build an explicit `$results` collection inside the loop and pipe only that collection to the formatter.
- Prefer small verification scripts or simple statements over deeply nested one-line PowerShell commands.
- Do not mark this incident closed until the replacement checker completes and reports every URL.

## Verification

The corrected checker completed on 2026-08-16. All 15 external documentation URLs returned HTTP status 200, and the command exited successfully.
