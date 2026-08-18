#!/usr/bin/env bash
set -euo pipefail

assert_contains() {
  local haystack="$1"
  local needle="$2"
  local message="$3"

  if [[ "$haystack" != *"$needle"* ]]; then
    printf 'FAIL: %s\nExpected to find: %s\n' "$message" "$needle" >&2
    exit 1
  fi
}

assert_not_contains() {
  local haystack="$1"
  local needle="$2"
  local message="$3"

  if [[ "$haystack" == *"$needle"* ]]; then
    printf 'FAIL: %s\nDid not expect to find: %s\n' "$message" "$needle" >&2
    exit 1
  fi
}

package_output=$(make -n package VERSION=1.2.3)
assert_contains "$package_output" "mkdir -p build" "package target creates the build directory"
assert_contains "$package_output" "-o build" "package target writes NuGet artifacts under build"
assert_contains "$package_output" "/p:PackageVersion=1.2.3" "explicit VERSION flows into PackageVersion"
assert_not_contains "$package_output" "artifacts" "package target no longer uses artifacts"

tag_package_output=$(make -n package GITHUB_REF_NAME=v2.3.4)
assert_contains "$tag_package_output" "/p:PackageVersion=2.3.4" "GITHUB_REF_NAME tag flows into PackageVersion when VERSION is omitted"

ntpc_output=$(make -n ntpc)
assert_contains "$ntpc_output" "build/ntpc_win-x64.exe" "Windows CLI artifact is staged under build"
assert_contains "$ntpc_output" "build/ntpc_linux-x64" "Linux CLI artifact is staged under build"
assert_contains "$ntpc_output" "build/ntpc_macos-x64" "macOS x64 CLI artifact is staged under build"
assert_contains "$ntpc_output" "build/ntpc_macos-arm64" "macOS arm64 CLI artifact is staged under build"

clean_output=$(make -n clean)
assert_contains "$clean_output" "rm -rf build" "clean removes build output"

printf 'release Makefile checks passed\n'
