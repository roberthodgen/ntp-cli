# Releasing

This repository publishes only `src/Client/RobertHodgen.Ntp.Client.csproj` to NuGet. The CLI is built as standalone release artifacts and attached to GitHub Releases, but it is not published as a NuGet package.

## Local Packaging

Create the NuGet package and symbols package locally:

```bash
dotnet pack src/Client -c Release -o build /p:PackageVersion=0.1.0-local
```

Output is written to `build/`:

- `RobertHodgen.Ntp.Client.<version>.nupkg`
- `RobertHodgen.Ntp.Client.<version>.snupkg`

The package version is passed explicitly with `/p:PackageVersion=<semver>`. For local builds, use a non-released version like `0.1.0-local`.

Build a CLI release binary locally:

```bash
dotnet publish src/Cli -c Release --runtime osx-arm64 -o build/ntpc_macos-arm64
```

Replace `osx-arm64` with `win-x64`, `linux-x64`, or `osx-x64` as needed.

Clean generated release outputs:

```bash
dotnet clean -c Release
rm -rf build
```

## Automated Releases

The release workflow runs when a tag matching `v*` is pushed. The package version comes from the `version` field in root `package.json`, and the tag must match it with a leading `v`. For example, a `package.json` version of `1.2.3` must be released from tag `v1.2.3`.

Create and push a release tag:

```bash
git tag v0.1.0
git push origin v0.1.0
```

The release workflow will:

- Read the package version from `package.json` and fail if the tag does not match `v<version>`.
- Restore, build, and test the solution.
- Pack `src/Client` with the `package.json` version and build Windows and Linux CLI release artifacts under `build/`.
- Build, sign, zip, and notarize macOS CLI binaries on a macOS runner.
- Publish the `.nupkg` and `.snupkg` to NuGet.
- Create a GitHub Release with `gh release create` using generated release notes.
- Attach the NuGet and CLI artifacts to the GitHub Release.

The CLI artifacts attached to each GitHub Release are:

- `ntpc_win-x64.exe`
- `ntpc_linux-x64`
- `ntpc_macos-x64.zip`
- `ntpc_macos-arm64.zip`

See [Apple Signing and Notarization](apple-signing-notarization.md) for the Apple Developer setup, GitHub Secrets, and verification commands required for macOS releases.

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
