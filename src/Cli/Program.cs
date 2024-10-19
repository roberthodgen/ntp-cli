// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using RobertHodgen.Ntp.Client;

Console.WriteLine("Network Time Protocol (NTP) Command Line Interface (CLI)");

var addresses = await Dns.GetHostAddressesAsync("pool.ntp.org");
if (addresses.Any() == false)
{
    throw new ApplicationException("Could not resolve any IP addresses.");
}

var endpoint = new IPEndPoint(addresses.First(), 123);
Console.WriteLine($"Resolved: {endpoint}");
var client = new UdpClient();
var requestHeader = TransmitPacketHeader.CreateNew();
var request = Packet.CreateNew(requestHeader);

var sent = await client.Client.SendToAsync(request.Encode(), SocketFlags.None, endpoint);

Console.WriteLine($"Sent {sent} bytes.");

Memory<byte> buffer = new byte[1024];
var result = await client.Client.ReceiveFromAsync(buffer, SocketFlags.None, endpoint);
var actualReceived = buffer[..result.ReceivedBytes];
Console.WriteLine($"Received {result.ReceivedBytes} bytes.");

// TODO decode response
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

Console.WriteLine("Done.");
