.PHONY: ntpc ntpc_win-x64.exe ntpc_linux-x64 ntpc_macos-x64 ntpc_macos-arm64 package clean

CLIENT_PROJECT := src/Client/RobertHodgen.Ntp.Client.csproj
CLI_PROJECT := src/Cli/RobertHodgen.Ntp.Cli.csproj
PACKAGE_VERSION := $(if $(VERSION),/p:PackageVersion=$(VERSION),)

ntpc: ntpc_win-x64.exe ntpc_linux-x64 ntpc_macos-x64 ntpc_macos-arm64

ntpc_win-x64.exe:
	dotnet publish $(CLI_PROJECT) -c Release --runtime win-x64
	cp src/Cli/bin/Release/net10.0/win-x64/publish/RobertHodgen.Ntp.Cli.exe ntpc_win-x64.exe

ntpc_linux-x64:
	dotnet publish $(CLI_PROJECT) -c Release --runtime linux-x64
	cp src/Cli/bin/Release/net10.0/linux-x64/publish/RobertHodgen.Ntp.Cli ntpc_linux-x64

ntpc_macos-x64:
	dotnet publish $(CLI_PROJECT) -c Release --runtime osx-x64
	cp src/Cli/bin/Release/net10.0/osx-x64/publish/RobertHodgen.Ntp.Cli ntpc_macos-x64

ntpc_macos-arm64:
	dotnet publish $(CLI_PROJECT) -c Release --runtime osx-arm64
	cp src/Cli/bin/Release/net10.0/osx-arm64/publish/RobertHodgen.Ntp.Cli ntpc_macos-arm64

package:
	mkdir -p artifacts
	dotnet pack $(CLIENT_PROJECT) -c Release -o artifacts $(PACKAGE_VERSION)

clean:
	dotnet clean -c Release
	rm -rf artifacts
	rm -f ntpc_win-x64.exe ntpc_linux-x64 ntpc_osx-x64 ntpc_macos-x64 ntpc_macos-arm64
