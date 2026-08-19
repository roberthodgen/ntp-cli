# Releasing

This repository publishes only `src/Client/RobertHodgen.Ntp.Client.csproj` to NuGet. The CLI is built as standalone release artifacts and attached to GitHub Releases, but it is not published as a NuGet package.

## Local Packaging

Create the NuGet package and symbols package locally:

```bash
make package VERSION=0.1.0-local
```

Output is written to `build/`:

- `RobertHodgen.Ntp.Client.<version>.nupkg`
- `RobertHodgen.Ntp.Client.<version>.snupkg`

When `VERSION` is omitted, the `Makefile` derives the package version from the current exact Git tag. For example, `v1.2.3` becomes `1.2.3`. If there is no exact tag, pass `VERSION=<semver>` explicitly.

Build CLI release binaries locally:

```bash
make ntpc
```

Clean generated release outputs:

```bash
make clean
```

## Automated Releases

The release workflow runs when a tag matching `v*` is pushed. Tags must use SemVer after the leading `v`, for example:

- `v1.2.3`
- `v1.2.3-preview.1`

Create and push a release tag:

```bash
git tag v0.1.0
git push origin v0.1.0
```

The release workflow will:

- Restore, build, and test the solution.
- Run `make package ntpc`; the `Makefile` derives the package version from the tag and writes all release artifacts under `build/`.
- Publish the `.nupkg` and `.snupkg` to NuGet.
- Build Windows, Linux, and macOS CLI binaries.
- Create a GitHub Release with `gh release create` using generated release notes.
- Attach the NuGet and CLI artifacts to the GitHub Release.

## NuGet Trusted Publishing Setup

Before the first automated publish, configure trusted publishing on NuGet.org.

Go to https://www.nuget.org/account/trustedpublishing and add a policy with these values:

- Repository Owner: `roberthodgen`
- Repository: `ntp-cli`
- Workflow File: `release.yml`
- Environment: `release`

Then create a GitHub environment:

- Repository Settings -> Environments -> New environment: `release`

No NuGet API key or `NUGET_USER` secret is required. The workflow passes `roberthodgen` directly to `NuGet/login@v1`, and NuGet.org authorizes the publish by matching the GitHub OIDC token to the trusted publishing policy.

Optional: add required reviewers to the `release` environment to require manual approval before publishing.

The release workflow must keep `permissions: id-token: write`; without it, `NuGet/login@v1` cannot exchange the GitHub OIDC token for a short-lived NuGet publishing key.

## Release Notes

GitHub generated release notes are configured by `.github/release.yml`. Pull requests with labels are grouped as breaking changes, features, fixes, documentation, maintenance, or other changes.

Use `skip-changelog` on pull requests that should be excluded from generated release notes.
