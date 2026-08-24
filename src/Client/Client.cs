namespace RobertHodgen.Ntp.Client;

using System.Net;
using System.Net.Sockets;
using Remote;
using Remote.Fields;
using Serilog;

public sealed class Client
{
    private const string DefaultServer = "pool.ntp.org";

    private readonly List<IPAddress> _addresses = [];
    private readonly IMonotonicClock? _clock;

    public Client(IPAddress[] addresses, IMonotonicClock? clock = null)
    {
        if (addresses.Length == 0)
        {
            throw new ApplicationException("Could not resolve any IP addresses.");
        }

        _addresses.AddRange(addresses);
        _clock = clock;
    }

    public static async Task<Client> CreateWithHostAsync(string server = DefaultServer, CancellationToken ct = default, IMonotonicClock? clock = null)
    {
        return new Client(await Dns.GetHostAddressesAsync(server, ct), clock);
    }

    public static Client CreateWithIpAddresses(params IPAddress[] addresses) => new(addresses);
    public static Client CreateWithIpAddresses(IMonotonicClock clock, IPAddress[] addresses) => new(addresses, clock);

    // TODO: rename to SampleAsync
    public async Task<Request> ConnectAsync(CancellationToken ct = default)
    {
        using var client = new UdpClient();
        var endpoint = CreateEndpoint();
        var requestPacket = TransmitPacketHeader.CreateNewPacket(_clock ?? new MonotonicClock());
        var sent = await client.Client.SendToAsync(requestPacket.Encode(), SocketFlags.None, endpoint, ct);
        Log.Debug("Sent {Bytes} bytes to `{endpoint}`.", sent, endpoint);

        Memory<byte> buffer = new byte[48];
        var response = await client.Client.ReceiveFromAsync(buffer, SocketFlags.None, endpoint, ct);
        var receivePacket = ReadResponse(response, buffer, _clock ?? new MonotonicClock());
        Log.Debug("Received {Bytes} bytes from `{endpoint}`.", response.ReceivedBytes, endpoint);
        return new Request(requestPacket, receivePacket);
    }

    internal static Packet<ReceivePacketHeader> ReadResponse(int receivedBytes, Memory<byte> buffer, IMonotonicClock clock)
    {
        var destinationTimestamp = clock.Capture();
        var actualReceived = buffer[..receivedBytes];
        var receivePacket = ReceivePacketHeader.Parse(actualReceived, destinationTimestamp);
        receivePacket.Header.ValidateKissODeath();
        return receivePacket;
    }

    private static Packet<ReceivePacketHeader> ReadResponse(SocketReceiveFromResult response, Memory<byte> buffer, IMonotonicClock clock)
    {
        return ReadResponse(response.ReceivedBytes, buffer, clock);
    }

    private IPEndPoint CreateEndpoint()
    {
        var endpoint = new IPEndPoint(_addresses.First(), 123);
        Log.Debug("Using: {Endpoint}", endpoint);
        return endpoint;
    }
}
