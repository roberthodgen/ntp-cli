# NuGet Packaging and Release Automation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Package and publish only `RobertHodgen.Ntp.Client` to NuGet with tag-based versioning, local Makefile output, GitHub Actions validation, GitHub Releases, and NuGet trusted publishing.

**Architecture:** Package metadata lives in the client project. Local builds use `make package`; CI validates restore/build/test/pack without publishing; tag-triggered releases let the `Makefile` derive the package version from the Git tag, publish via NuGet trusted publishing, build CLI binaries, create a GitHub Release with the `gh` CLI, and attach all artifacts.

**Tech Stack:** .NET 10 SDK, MSBuild packing, NuGet trusted publishing/OIDC, `NuGet/login@v1`, GitHub Actions, GitHub CLI, GitHub generated release notes, Makefile.

**Spec:** Chat-reviewed plan from 2026-08-18.

## Global Constraints

- Publish only `src/Client/RobertHodgen.Ntp.Client.csproj` to NuGet.
- Do not make the CLI project packable for NuGet.
- Package version comes from tags shaped like `v1.2.3` or `v1.2.3-preview.1`.
- Build artifacts are written under `build/`.
- GitHub publishing uses NuGet trusted publishing with OIDC, not a long-lived NuGet API key.
- GitHub Release creation happens from the same tag with the `gh` CLI and attaches all built artifacts.
- Release notes use GitHub's release notes configuration template.
- Follow Microsoft package authoring guidance as much as practical.

---

### Task 1: Package Metadata And Assets

**Files:**
- Modify: `src/Client/RobertHodgen.Ntp.Client.csproj`
- Create: `LICENSE`
- Create: `src/Client/README.md`
- Create: `CHANGELOG.md`

**Interfaces:**
- Consumes: existing SDK-style client project.
- Produces: a NuGet package with package ID `RobertHodgen.Ntp.Client`, README metadata, MIT license expression, repository metadata, Source Link, `.nupkg`, and `.snupkg` outputs.

- [x] Add package metadata required by NuGet package authoring guidance.
- [x] Add package README with install and usage sample.
- [x] Add MIT license file.
- [x] Add changelog pointer to GitHub Releases.
- [x] Add Source Link package reference.

### Task 2: Makefile Packaging

**Files:**
- Modify: `Makefile`

**Interfaces:**
- Consumes: `src/Client/RobertHodgen.Ntp.Client.csproj`.
- Produces: `make package` and `make package VERSION=<version>` commands that write NuGet artifacts to `build/`.

- [x] Add project variables for the client and CLI projects.
- [x] Keep `make ntpc` for CLI binaries and make it target the CLI project explicitly.
- [x] Add `make package` for client package output.
- [x] Update `make clean` to remove generated package output.
- [x] Move all release outputs to `build/`.
- [x] Derive package versions from the current exact `v<semver>` tag when `VERSION` is omitted.

### Task 3: GitHub CI

**Files:**
- Create: `.github/workflows/ci.yml`

**Interfaces:**
- Consumes: solution, tests, client project.
- Produces: PR/main validation and uploaded NuGet package artifacts without publishing.

- [x] Add pull request and main branch push triggers.
- [x] Restore, build, test, and pack using .NET 10.
- [x] Upload `.nupkg` and `.snupkg` artifacts for inspection.

### Task 4: Tag Release Automation

**Files:**
- Create: `.github/workflows/release.yml`
- Create: `.github/release.yml`

**Interfaces:**
- Consumes: Git tags named `v<semver>`, NuGet trusted publishing policy, GitHub `release` environment, NuGet user `roberthodgen`.
- Produces: published NuGet package, published symbols package, GitHub Release, and attached artifacts.

- [x] Trigger release workflow from tags matching `v*`.
- [x] Validate SemVer after the leading `v` through the `Makefile`.
- [x] Pack only `RobertHodgen.Ntp.Client` with `Makefile`-derived `PackageVersion`.
- [x] Use `NuGet/login@v1` with `id-token: write`.
- [x] Push `.nupkg` and `.snupkg` to NuGet.
- [x] Use `roberthodgen` directly as the NuGet trusted publishing user, without a `NUGET_USER` secret.
- [x] Build CLI binaries and stage them under `build/`.
- [x] Create GitHub Release with `gh release create`, generated notes, and attached `build/*` artifacts.
- [x] Configure generated release note categories.

**NuGet trusted publishing connection:** NuGet.org authorizes the workflow by matching GitHub's OIDC token to a trusted publishing policy. Configure NuGet.org with repository owner `roberthodgen`, repository `ntp-cli`, workflow file `release.yml`, and environment `release`. The workflow keeps `permissions: id-token: write`, calls `NuGet/login@v1`, and passes user `roberthodgen`; `NuGet/login@v1` returns a short-lived API key for the push commands.

### Task 5: Documentation And Verification

**Files:**
- Modify: `README.md`
- Create: `docs/releasing.md`

**Interfaces:**
- Consumes: Makefile targets and GitHub workflows.
- Produces: contributor instructions for packaging and release setup.

- [x] Document local package creation.
- [x] Document tag-driven release flow.
- [x] Document NuGet trusted publishing setup values.
- [ ] Run final verification commands before merging.
