# Contributing to OpenMemory

OpenMemory is currently documentation-only. Implementation contributions should wait until the Stage 0 contracts and repository-grounded threat model have been merged.

## Before opening a change

1. Read the [product requirements](docs/PRODUCT_REQUIREMENTS.md), [architecture](docs/ARCHITECTURE.md), and [agent agreement](AGENTS.md).
2. Open or reference an issue that defines the intended behavior and acceptance evidence.
3. Do not mix unrelated changes in one branch.
4. Never place real credentials, private transcripts, or personal memory databases in the repository, including test fixtures.
5. Read [Compatibility](docs/COMPATIBILITY.md) and [Identifiers](docs/IDENTIFIERS.md).

## Development workflow

- Branch from `main` using `codex/<short-description>`.
- Use an isolated Git worktree for implementation.
- Freeze shared interfaces before parallel implementation begins.
- Add tests before or with behavior changes.
- Run focused checks and the complete affected suite.
- Request specification review before code-quality and security review.
- Submit changes through a pull request. Direct implementation commits to `main` are not allowed after the initial documentation baseline.

### Required checks

Checks are not yet enforced by GitHub. The intended ruleset is recorded in [.github/branch-protection.md](.github/branch-protection.md). Until it is enabled in Wave C, every change instead receives an independent `codex exec` review before integration.

| Check | What it verifies | Enforcement status |
|---|---|---|
| CODEOWNERS validity | The `CODEOWNERS` file parses and every path has a resolvable owner. | Not enforced — Wave C |
| Issue-template schema | `.github/ISSUE_TEMPLATE/*.yml` files are valid GitHub issue forms. | Not enforced — Wave C |
| Repo-internal link check | Every repository-internal Markdown link resolves to an existing file or anchor. | Not enforced — Wave C |
| Independent specification and quality review | The diff satisfies the relevant spec and contains no defect an independent reviewer would flag. | Not enforced — Wave C (performed manually via `codex exec` in the interim) |

### Placeholder test projects

A test project with no behavior to cover yet must contain exactly one skipped test, not zero tests. The skip reason must name what is missing and when it is expected. `dotnet test` exits 0 for a project with no tests at all, printing only "No test is available", so an empty project is indistinguishable from a broken test discovery; a skipped test still appears in every run's skip count. No test project currently needs this — all three have real tests — so this applies to test projects added from Stage 2 onward.

```csharp
[Fact(Skip = "Stage 2: no Storage behavior exists yet")]
public void Placeholder()
{
}
```

### License headers

Source files added from Wave B onward carry an Apache-2.0 header. Wave A adds no source files. `LICENSE` and `NOTICE` remain authoritative.

## Developer Certificate of Origin

Every commit must include a `Signed-off-by` line certifying the [Developer Certificate of Origin 1.1](https://developercertificate.org/). Sign a commit with:

```powershell
git commit -s -m "Describe the change"
```

The sign-off certifies that you have the right to submit the contribution under this repository's license. It is not a copyright assignment.

## Documentation

Use plain language, define unfamiliar terms, distinguish planned behavior from implemented behavior, and link to the decision or evidence that supports a claim.

## Pull-request evidence

A pull request should state:

- what changed and why;
- which requirements and decisions it satisfies;
- security or privacy effects;
- commands and real user paths used for verification;
- remaining limitations or deferred work.
