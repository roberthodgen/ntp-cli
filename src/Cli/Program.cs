// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using RobertHodgen.Ntp.Client;
using Serilog;
using Serilog.Core;
using Serilog.Events;

var levelSwitch = new LoggingLevelSwitch
{
    MinimumLevel = LogEventLevel.Information, // default log level
};

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.ControlledBy(levelSwitch)
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss.fffffff} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        standardErrorFromLevel: LogEventLevel.Warning)
    .CreateLogger();

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    Log.Warning("Stopping...");
    cts.Cancel();
    e.Cancel = true;
};

Log.Information("Network Time Protocol (NTP) Command Line Interface (CLI)");

var rootCommand = new RootCommand("NTP CLI");
var verboseOption = new Option<bool>("--verbose", description: "Enable verbose logging");
var checkCommand = new Command("check", "Check an NTP server for a time offset");
rootCommand.Add(checkCommand);
checkCommand.AddOption(verboseOption);

checkCommand.SetHandler(
    async verbose =>
    {
        if (verbose)
        {
            levelSwitch.MinimumLevel = LogEventLevel.Verbose; // TODO add option from CLI
        }

        var request = await new Client().ConnectAsync(cts.Token);
        
        Log.Debug("Server response headers:");
        request.ServerResponse.Header.LogDebugData();

        Log.Debug("Local receive timestamp: {receiveTimestamp:O}", request.ServerResponse.DestinationTimestamp);

        Log.Information($"Theta: {request.Theta():c} (absolute time difference between client and server clocks)");
        Log.Information($"Delta: {request.Delta():c} (round-trip delay)");

    }, verboseOption);

await rootCommand.InvokeAsync(args);
