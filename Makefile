.PHONY: ntpc
ntpc: ntpc_win-x64.exe ntpc_osx-x64 ntpc_linux-x64

ntpc_win-x64.exe:
	dotnet publish --runtime win-x64
	cp src/Cli/bin/Release/net8.0/win-x64/publish/RobertHodgen.Ntp.Cli.exe ntpc_win-x64.exe

ntpc_osx-x64:
	dotnet publish --runtime osx-x64
	cp src/Cli/bin/Release/net8.0/osx-x64/publish/RobertHodgen.Ntp.Cli ntpc_osx-x64

ntpc_linux-x64:
	dotnet publish --runtime linux-x64
	cp src/Cli/bin/Release/net8.0/linux-x64/publish/RobertHodgen.Ntp.Cli ntpc_linux-x64

.PHONY: clean
clean:
	dotnet clean -c Release
	rm -f ntpc_win-x64.exe ntpc_osx-x64 ntpc_linux-x64
