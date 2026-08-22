# SET-20260821-007: Link checker reported success over files it never opened

- **First observed:** 2026-08-21
- **Last observed:** 2026-08-21
- **Status:** Closed
- **Phase/task:** Stage 0 Wave D, Tasks 2–4 verification
- **Environment:** Git Bash / Linux, `tools/check-links.sh` as written in Wave A
- **Version/commit:** `50f280e`
- **Owner:** Root integrator

## Symptom and impact

`bash tools/check-links.sh` was run four times over newly created documents and reported `0 broken` every time. It had not opened any of them. The script's file list came from `git ls-files '*.md'`, which lists only tracked files; a newly created document is untracked until it is staged, so every new document was invisible to the check that was being cited as evidence about it.

The impact was false confidence, not a broken link: when the files were finally staged, the same script reported `230 internal links checked` against the earlier `75`. Those 155 links had never been checked at the moment their verification was recorded as passing. A broken link among them would have reached `main` with a green check beside it.

## Safe evidence

- Before staging: `75 internal links checked, 0 broken`.
- After staging the same tree: `230 internal links checked, 0 broken`.
- The 155-link jump was the only signal, and it appeared in output that was read as success four times.
- Found by Worker A, which ran the command as specified and reported the discrepancy rather than working around it.
- No credential, token, or private value was involved.

## Cause analysis

- **Confirmed cause:** `git ls-files '*.md'` enumerates the index, not the working tree. Untracked files are absent by design.
- **Rejected hypotheses:** a link-parsing bug (parsing was correct on tracked files), a path-resolution bug (relative resolution was correct), and fenced-code exclusion (unrelated to which files were listed).
- **Why review missed it:** the output was success-shaped. A count was present, it was non-zero, and `0 broken` followed it. Nothing in the output said which files produced that count.

## Correction and prevention

Two changes to `tools/check-links.sh`:

1. The file list is now `git ls-files --cached --others --exclude-standard '*.md'`, which adds untracked files while still honouring `.gitignore`, so a document is checked from the moment it exists rather than from the moment it is staged.
2. The summary now prints the **file count** alongside the link count: `41 files, 233 internal links checked, 0 broken`. A green result that states its denominator cannot hide a run that checked nothing.

The generalizable rule, recorded here because it applies well beyond this script: when a tool reports success, the report must say what set it operated on. A pass over an empty set is indistinguishable from a real pass at a glance, and is the more dangerous of the two precisely because it looks identical.

## Verification

The corrected script was run against a deliberately planted untracked file containing a broken link. It reported `BROKEN docs/_probe.md -> ./no-such-file.md`, `42 files, 234 internal links checked, 1 broken`, exit `1` — the exact case it was previously blind to. The probe was removed and the script returned `41 files, 233 internal links checked, 0 broken`, exit `0`.
