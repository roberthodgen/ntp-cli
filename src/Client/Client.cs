namespace RobertHodgen.Ntp.Client;

using System.Net;
using System.Net.Sockets;
using System.Text;
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
        var endpoint = new IPEndPoint(_addresses.First(), 123);
        Log.Debug("Using: {Endpoint}", endpoint);
        var client = new UdpClient();

        var request = TransmitPacketHeader.CreateNewPacket();
        var sent = await client.Client.SendToAsync(request.Encode(), SocketFlags.None, endpoint, ct);
        Log.Debug("Sent {Bytes} bytes to `{endpoint}`.", sent, endpoint);

        // TODO receive next
        Memory<byte> buffer = new byte[48];
        var received = await client.Client.ReceiveFromAsync(buffer, SocketFlags.None, endpoint, ct);
        var destinationTimestamp = NtpTimestamp.Now;
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

        var receivePacket = ReceivePacketHeader.Parse(actualReceived, destinationTimestamp);
        return new Request(request, receivePacket);
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
}
