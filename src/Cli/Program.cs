// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using RobertHodgen.Ntp.Client;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    Console.WriteLine("Stopping...");
    cts.Cancel();
    e.Cancel = true;
};

Console.WriteLine("Network Time Protocol (NTP) Command Line Interface (CLI)");
Console.WriteLine();

Console.WriteLine("Using: pool.ntp.org");
var addresses = await Dns.GetHostAddressesAsync("pool.ntp.org");
if (addresses.Any() == false)
{
    throw new ApplicationException("Could not resolve any IP addresses.");
}

var endpoint = new IPEndPoint(addresses.First(), 123);
Console.WriteLine($"Resolved: {endpoint}");
Console.WriteLine();
var client = new UdpClient();
var requestHeader = TransmitPacketHeader.CreateNew();
var request = Packet.CreateNew(requestHeader);
var sent = await client.Client.SendToAsync(request.Encode(), SocketFlags.None, endpoint, cts.Token);
Console.WriteLine($"Sent {sent} bytes.");
Console.WriteLine();

Memory<byte> buffer = new byte[48];
var received = await client.Client.ReceiveFromAsync(buffer, SocketFlags.None, endpoint, cts.Token);
var receiveTimestamp = DateTime.UtcNow;
var actualReceived = buffer[..received.ReceivedBytes];
Console.WriteLine($"Received {received.ReceivedBytes} bytes.");

var nth = 1;
foreach (var b in actualReceived.ToArray())
{
    Console.Write($"{Convert.ToString(b, toBase: 2).PadLeft(8, '0'),8}");
    if (nth % 4 == 0)
    {
        Console.WriteLine();
    }
    else
    {
        Console.Write(" ");
    }

    nth++;
}

var response = Packet.ParseResponse(actualReceived);

Console.WriteLine();
Console.WriteLine("SERVER Response:");
Console.WriteLine($"  Leap indicator: {response.Header.LeapIndicator}");
Console.WriteLine($"  Version number: {response.Header.VersionNumber}");
Console.WriteLine($"  Mode: {response.Header.Mode}");
Console.WriteLine($"  Stratum: {response.Header.Stratum}");
Console.WriteLine($"  Poll: {response.Header.Poll}");
Console.WriteLine($"  Precision: {response.Header.Precision}");
Console.WriteLine($"  Root delay: {response.Header.RootDelay}");
Console.WriteLine($"  Root dispersion: {response.Header.RootDispersion}");
Console.WriteLine($"  Reference ID: {response.Header.ReferenceId}");
Console.WriteLine($"  Reference timestamp: {response.Header.ReferenceTimestamp}");
Console.WriteLine($"  Origin timestamp: {response.Header.OriginTimestamp}");
Console.WriteLine($"  Receive timestamp: {response.Header.ReceiveTimestamp}");
Console.WriteLine($"  Transmit timestamp: {response.Header.TransmitTimestamp}");

Console.WriteLine();
Console.WriteLine($"LOCAL Receive timestamp: {receiveTimestamp:O}");

Console.WriteLine("Done.");
