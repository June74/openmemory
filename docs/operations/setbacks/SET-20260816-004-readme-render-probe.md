# SET-20260816-004: Rendered README probe mixed HTML and JSON modes

- **First observed:** 2026-08-16T01:56:43-05:00
- **Last observed:** 2026-08-16T01:56:43-05:00
- **Status:** Closed
- **Phase/task:** Post-push GitHub verification
- **Environment:** GitHub CLI and GitHub API
- **Version/commit:** `a4e038a87d8b1e5597cbb02b548bf1c45c1a5797`
- **Owner:** Root integrator

## Symptom and impact

The combined remote-verification command successfully confirmed repository metadata, visibility, default branch, remote commit, and tree contents. Its final README-rendering probe requested GitHub's HTML media type but also applied a JSON query, so the CLI rejected the leading HTML character as invalid JSON. No repository or content failure was established.

## Safe evidence

- `June74/openmemory` was confirmed public with default branch `main`.
- Remote `main` matched the local commit before the rendering probe.
- The API returned the complete expected documentation tree.
- The only failure was a response-format mismatch in the read-only probe.
- No credential, token, private value, or response body was persisted.

## Cause analysis

- **Confirmed cause:** the command requested raw rendered HTML and simultaneously used a JSON-only `--jq` filter.
- **Rejected hypotheses:** repository absence, wrong visibility, wrong branch, missing files, and commit mismatch were excluded by earlier output from the same command.

## Correction and prevention

Request the rendered HTML without a JSON filter, then search the raw response for the two planning-status statements. Keep media-type and parser expectations aligned in future API checks.

## Verification

The corrected probe requested raw rendered HTML without a JSON filter and confirmed both planning-only statements: the repository is in the planning/documentation stage, and no application code has been implemented.
