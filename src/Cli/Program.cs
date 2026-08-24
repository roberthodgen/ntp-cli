// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using RobertHodgen.Ntp.Client;
using RobertHodgen.Ntp.Client.Remote;
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
var verboseOption = new Option<bool>("--verbose")
{
    Description = "Enable verbose logging"
};
var checkCommand = new Command("check", "Check an NTP server for a time offset");
rootCommand.Add(checkCommand);
checkCommand.Add(verboseOption);

checkCommand.SetAction(
    async (parseResult, ct) =>
    {
        var verbose = parseResult.GetValue(verboseOption);
        if (verbose)
        {
            levelSwitch.MinimumLevel = LogEventLevel.Verbose;
        }

        var clock = new MonotonicClock();
        var client = await Client.CreateForHostAsync("pool.ntp.org", clock, ct);
        var sample = await client.SampleAsync(ct);
        
        Log.Debug("Server response headers:");
        sample.ServerResponse.Header.LogDebugData();

        Log.Debug("Local receive timestamp: {receiveTimestamp:O}", sample.ServerResponse.DestinationTimestamp);

        Log.Information($"Theta: {sample.Theta():c} (absolute time difference between client and server clocks)");
        Log.Information($"Delta: {sample.Delta():c} (round-trip delay)");

    });

return await rootCommand.Parse(args).InvokeAsync(cancellationToken: cts.Token);
