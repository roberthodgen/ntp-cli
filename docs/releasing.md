# Releasing

This repository publishes only `src/Client/RobertHodgen.Ntp.Client.csproj` to NuGet. The CLI is built as standalone release artifacts and attached to GitHub Releases, but it is not published as a NuGet package.

## Local Packaging

Create the NuGet package and symbols package locally:

```bash
make package VERSION=0.1.0-local
```

Output is written to `artifacts/`:

- `RobertHodgen.Ntp.Client.<version>.nupkg`
- `RobertHodgen.Ntp.Client.<version>.snupkg`

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
- Pack `RobertHodgen.Ntp.Client` with the version from the tag.
- Publish the `.nupkg` and `.snupkg` to NuGet.
- Build Windows, Linux, and macOS CLI binaries.
- Create a GitHub Release using generated release notes.
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
- Add environment secret: `NUGET_USER`
- Set `NUGET_USER` to the NuGet.org username, not an email address.

Optional: add required reviewers to the `release` environment to require manual approval before publishing.

## Release Notes

GitHub generated release notes are configured by `.github/release.yml`. Pull requests with labels are grouped as breaking changes, features, fixes, documentation, maintenance, or other changes.

Use `skip-changelog` on pull requests that should be excluded from generated release notes.
