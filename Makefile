.PHONY: ntpc build/ntpc_win-x64.exe build/ntpc_linux-x64 build/ntpc_macos-x64 build/ntpc_macos-arm64 package validate-version clean

CLIENT_PROJECT := src/Client/RobertHodgen.Ntp.Client.csproj
CLI_PROJECT := src/Cli/RobertHodgen.Ntp.Cli.csproj
BUILD_DIR := build
CURRENT_TAG := $(or $(shell git describe --tags --exact-match 2>/dev/null),$(GITHUB_REF_NAME))
VERSION ?= $(patsubst v%,%,$(CURRENT_TAG))
PACKAGE_VERSION := /p:PackageVersion=$(VERSION)

ntpc: build/ntpc_win-x64.exe build/ntpc_linux-x64 build/ntpc_macos-x64 build/ntpc_macos-arm64

build/ntpc_win-x64.exe:
	mkdir -p $(BUILD_DIR)
	dotnet publish $(CLI_PROJECT) -c Release --runtime win-x64
	cp src/Cli/bin/Release/net10.0/win-x64/publish/RobertHodgen.Ntp.Cli.exe $(BUILD_DIR)/ntpc_win-x64.exe

build/ntpc_linux-x64:
	mkdir -p $(BUILD_DIR)
	dotnet publish $(CLI_PROJECT) -c Release --runtime linux-x64
	cp src/Cli/bin/Release/net10.0/linux-x64/publish/RobertHodgen.Ntp.Cli $(BUILD_DIR)/ntpc_linux-x64

build/ntpc_macos-x64:
	mkdir -p $(BUILD_DIR)
	dotnet publish $(CLI_PROJECT) -c Release --runtime osx-x64
	cp src/Cli/bin/Release/net10.0/osx-x64/publish/RobertHodgen.Ntp.Cli $(BUILD_DIR)/ntpc_macos-x64

build/ntpc_macos-arm64:
	mkdir -p $(BUILD_DIR)
	dotnet publish $(CLI_PROJECT) -c Release --runtime osx-arm64
	cp src/Cli/bin/Release/net10.0/osx-arm64/publish/RobertHodgen.Ntp.Cli $(BUILD_DIR)/ntpc_macos-arm64

package: validate-version
	mkdir -p $(BUILD_DIR)
	dotnet pack $(CLIENT_PROJECT) -c Release -o $(BUILD_DIR) $(PACKAGE_VERSION)

validate-version:
	@if [ -z "$(VERSION)" ]; then \
		printf 'VERSION is required. Pass VERSION=1.2.3 or run from an exact v<semver> tag.\n' >&2; \
		exit 1; \
	fi
	@if ! printf '%s\n' "$(VERSION)" | grep -Eq '^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z][0-9A-Za-z.-]*)?$$'; then \
		printf 'VERSION must be SemVer like 1.2.3 or 1.2.3-preview.1; got %s\n' "$(VERSION)" >&2; \
		exit 1; \
	fi

clean:
	dotnet clean -c Release
	rm -rf $(BUILD_DIR)
	rm -f ntpc_win-x64.exe ntpc_linux-x64 ntpc_osx-x64 ntpc_macos-x64 ntpc_macos-arm64
