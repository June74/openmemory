# SET-20260821-006: .NET toolchain unavailable in the remote session environment

- **First observed:** 2026-08-21
- **Last observed:** 2026-08-21
- **Status:** Closed with a compensating control
- **Phase/task:** Stage 0 Wave D, Task 1
- **Environment:** Claude Code remote execution container (Linux), agent proxy network policy
- **Version/commit:** `b791775`
- **Owner:** Root integrator

## Symptom and impact

Wave D's plan verifies its three new tests locally with `dotnet build`, `dotnet test`, and `dotnet format --verify-no-changes`. In this session's environment `dotnet` is not installed, and installing it fails: the agent proxy answered `403` to `CONNECT builds.dotnet.microsoft.com:443`, which the environment's network policy does not permit.

The impact is confined to *where* the verification runs, not whether it runs. Every documentation deliverable remains locally verifiable; the C# test code cannot be compiled or executed in this container.

## Safe evidence

- `which dotnet` returned nothing.
- `dotnet-install.sh` could not be fetched: `curl: (56) CONNECT tunnel failed, response 403`.
- The proxy status endpoint recorded `connect_rejected` for `builds.dotnet.microsoft.com:443` — "gateway answered 403 to CONNECT (policy denial or upstream failure)".
- No credential, token, or private value was involved or persisted.

## Cause analysis

- **Confirmed cause:** the environment's network policy does not allow the .NET distribution host, and the image ships no .NET SDK.
- **Rejected hypotheses:** a missing `PATH` entry (no SDK exists anywhere in the image), a TLS trust failure (the rejection is at `CONNECT`, before TLS), and a transient outage (the policy denial is deterministic across retries).

## Correction and prevention

Verification of C# code moves to the Wave C CI pipeline, which installs the pinned SDK from `global.json` on `windows-latest` — the platform `D-003` fixes as the supported one, and therefore better evidence than a Linux run would have been. The workflow's `workflow_dispatch` trigger runs it on this branch without opening a pull request.

The test-first discipline is preserved rather than abandoned: the guards are proved able to fail by pushing one deliberate three-way breakage — a single fixture byte changed, a duplicated `SC-*` identifier, and a removed coverage row — observing the CI run go red on all three, then reverting and observing it go green. What changes is the latency of the feedback loop, not the evidence.

## Verification

Recorded at the point of discovery, before the plan's verification steps were adapted, per `AGENTS.md`: "Record unexpected failures, verified outcomes, and any decision change."
