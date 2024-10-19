// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Text;
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

levelSwitch.MinimumLevel = LogEventLevel.Verbose; // TODO add option from CLI

const string defaultServer = "pool.ntp.org";

Log.Information("Using: {defaultServer}", defaultServer);
var addresses = await Dns.GetHostAddressesAsync("pool.ntp.org");
if (addresses.Length == 0)
{
    throw new ApplicationException("Could not resolve any IP addresses.");
}

var endpoint = new IPEndPoint(addresses.First(), 123);
Log.Debug("Resolved `{defaultServer}` as: {Address}", defaultServer, endpoint.Address);
var client = new UdpClient();
var requestHeader = TransmitPacketHeader.CreateNew();
var request = Packet.CreateNew(requestHeader);
var sent = await client.Client.SendToAsync(request.Encode(), SocketFlags.None, endpoint, cts.Token);
Log.Debug("Sent {Bytes} bytes to `{endpoint}`.", sent, endpoint);

Memory<byte> buffer = new byte[48];
var received = await client.Client.ReceiveFromAsync(buffer, SocketFlags.None, endpoint, cts.Token);
var receiveTimestamp = DateTime.UtcNow;
var actualReceived = buffer[..received.ReceivedBytes];
Log.Debug("Received {Bytes} bytes from `{endpoint}`.", received.ReceivedBytes, endpoint);

var nth = 1;
var stringBuilder = new StringBuilder();
foreach (var b in actualReceived.ToArray())
{
    stringBuilder.Append($"{Convert.ToString(b, toBase: 2).PadLeft(8, '0'),8}");
    if (nth % 4 == 0)
    {
        stringBuilder.AppendLine();
    }
    else
    {
        stringBuilder.Append(' ');
    }

    nth++;
}

Log.Debug("Server raw response:\n{Response}", stringBuilder);

var response = Packet.ParseResponse(actualReceived);

Log.Debug("Server decoded response:");
Log.Debug("  Leap indicator: {LeapIndicator}", response.Header.LeapIndicator);
Log.Debug("  Version number: {VersionNumber}", response.Header.VersionNumber);
Log.Debug("  Mode: {Mode}", response.Header.Mode);
Log.Debug("  Stratum: {Stratum}", response.Header.Stratum);
Log.Debug("  Poll: {Poll}", response.Header.Poll);
Log.Debug("  Precision: {Precision}", response.Header.Precision);
Log.Debug("  Root delay: {RootDelay}", response.Header.RootDelay);
Log.Debug("  Root dispersion: {RootDispersion}", response.Header.RootDispersion);
Log.Debug("  Reference ID: {ReferenceId}", response.Header.ReferenceId);
Log.Debug("  Reference timestamp: {ReferenceTimestamp}", response.Header.ReferenceTimestamp);
Log.Debug("  Origin timestamp: {OriginTimestamp}", response.Header.OriginTimestamp);
Log.Debug("  Receive timestamp: {ReceiveTimestamp}", response.Header.ReceiveTimestamp);
Log.Debug("  Transmit timestamp: {TransmitTimestamp}", response.Header.TransmitTimestamp);

Log.Debug("Local receive timestamp: {receiveTimestamp:O}", receiveTimestamp);
