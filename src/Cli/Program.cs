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

var response = await new Client().ConnectAsync(cts.Token);
var headers = response.Header;

Log.Debug("Server decoded response:");
Log.Debug("  Leap indicator: {LeapIndicator}", headers.LeapIndicator);
Log.Debug("  Version number: {VersionNumber}", headers.VersionNumber);
Log.Debug("  Mode: {Mode}", headers.Mode);
Log.Debug("  Stratum: {Stratum}", headers.Stratum);
Log.Debug("  Poll: {Poll}", headers.Poll);
Log.Debug("  Precision: {Precision}", headers.Precision);
Log.Debug("  Root delay: {RootDelay}", headers.RootDelay);
Log.Debug("  Root dispersion: {RootDispersion}", headers.RootDispersion);
Log.Debug("  Reference ID: {ReferenceId}", headers.ReferenceId);
Log.Debug("  Reference timestamp: {ReferenceTimestamp}", headers.ReferenceTimestamp);
Log.Debug("  Origin timestamp: {OriginTimestamp}", headers.OriginTimestamp);
Log.Debug("  Receive timestamp: {ReceiveTimestamp}", headers.ReceiveTimestamp);
Log.Debug("  Transmit timestamp: {TransmitTimestamp}", headers.TransmitTimestamp);

Log.Debug("Local receive timestamp: {receiveTimestamp:O}", response.DestinationTimestamp);
