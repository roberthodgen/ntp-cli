namespace RobertHodgen.Ntp.Client;

using System.Net;
using System.Net.Sockets;
using Remote;
using Remote.Fields;
using Serilog;

public sealed class Client
{
    private const string DefaultServer = "pool.ntp.org";

    private readonly string _host;
    private readonly List<IPAddress> _addresses = [];

    public Client(string server = DefaultServer)
    {
        _host = server;
    }

    public async Task<Request> ConnectAsync(CancellationToken ct = default)
    {
        await InitializeClientAsync(ct);
        using var client = new UdpClient();
        var endpoint = CreateEndpoint();
        var requestPacket = TransmitPacketHeader.CreateNewPacket();
        var sent = await client.Client.SendToAsync(requestPacket.Encode(), SocketFlags.None, endpoint, ct);
        Log.Debug("Sent {Bytes} bytes to `{endpoint}`.", sent, endpoint);

        Memory<byte> buffer = new byte[48];
        var response = await client.Client.ReceiveFromAsync(buffer, SocketFlags.None, endpoint, ct);
        var receivePacket = ReadResponse(response, buffer);
        Log.Debug("Received {Bytes} bytes from `{endpoint}`.", response.ReceivedBytes, endpoint);
        return new Request(requestPacket, receivePacket);
    }

    private static Packet<ReceivePacketHeader> ReadResponse(SocketReceiveFromResult response, Memory<byte> buffer)
    {
        var destinationTimestamp = NtpTimestamp.Now;
        var actualReceived = buffer[..response.ReceivedBytes];
        return ReceivePacketHeader.Parse(actualReceived, destinationTimestamp);
    }

    private async Task InitializeClientAsync(CancellationToken ct = default)
    {
        if (_addresses.Any())
        {
            return;
        }

        Log.Information("Using host: {defaultServer}", _host);
        _addresses.AddRange(await Dns.GetHostAddressesAsync("pool.ntp.org", ct));
        if (_addresses.Count == 0)
        {
            throw new ApplicationException("Could not resolve any IP addresses.");
        }

        Log.Debug("Resolved {Count} IPs for host: {IpAddresses}", _addresses.Count, _addresses);
    }

    private IPEndPoint CreateEndpoint()
    {
        var endpoint = new IPEndPoint(_addresses.First(), 123);
        Log.Debug("Using: {Endpoint}", endpoint);
        return endpoint;
    }
}
