// See https://aka.ms/new-console-template for more information

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

levelSwitch.MinimumLevel = LogEventLevel.Verbose; // TODO add option from CLI

var request = await new Client().ConnectAsync(cts.Token);
var responseHeaders = request.ServerResponse.Header;

Log.Debug("Server decoded response:");
Log.Debug("  Leap indicator: {LeapIndicator}", responseHeaders.LeapIndicator);
Log.Debug("  Version number: {VersionNumber}", responseHeaders.VersionNumber);
Log.Debug("  Mode: {Mode}", responseHeaders.Mode);
Log.Debug("  Stratum: {Stratum}", responseHeaders.Stratum);
Log.Debug("  Poll: {Poll}", responseHeaders.Poll);
Log.Debug("  Precision: {Precision}", responseHeaders.Precision);
Log.Debug("  Root delay: {RootDelay}", responseHeaders.RootDelay);
Log.Debug("  Root dispersion: {RootDispersion}", responseHeaders.RootDispersion);
Log.Debug("  Reference ID: {ReferenceId}", responseHeaders.ReferenceId);
Log.Debug("  Reference timestamp: {ReferenceTimestamp}", responseHeaders.ReferenceTimestamp);
Log.Debug("  Origin timestamp: {OriginTimestamp}", responseHeaders.OriginTimestamp);
Log.Debug("  Receive timestamp: {ReceiveTimestamp}", responseHeaders.ReceiveTimestamp);
Log.Debug("  Transmit timestamp: {TransmitTimestamp}", responseHeaders.TransmitTimestamp);

Log.Debug("Local receive timestamp: {receiveTimestamp:O}", request.ServerResponse.DestinationTimestamp);

Log.Information("Theta: {Theta:c}", request.Theta());
Log.Information("Delta: {Delta:c}", request.Delta());
