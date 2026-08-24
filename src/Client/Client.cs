namespace RobertHodgen.Ntp.Client;

using System.Net;
using System.Net.Sockets;
using Remote;
using Remote.Fields;
using Serilog;

/// <summary>
/// NTP client that sends requests to a remote NTP server and calculates clock offset and delay.
/// </summary>
public sealed class Client
{
    private readonly IPEndPoint _endPoint;
    private readonly IMonotonicClock _clock;

    private Client(IPAddress ipAddress, IMonotonicClock clock)
    {
        _endPoint = new IPEndPoint(ipAddress, 123);
        _clock = clock;
    }

    /// <summary>
    /// Factory method to create a new <see cref="Client"/> instance from a server host name.
    /// </summary>
    /// <remarks>
    /// Will use one of the IP addresses that is resolved for the host name. Prefer <see cref="CreateForIpAddress"/> for
    /// connecting to multiple remote NTP servers.
    /// </remarks>
    public static async Task<Client> CreateForHostAsync(
        string server,
        IMonotonicClock clock,
        CancellationToken ct = default)
    {
        var ipAddresses = await Dns.GetHostAddressesAsync(server, ct);
        return new Client(
            ipAddresses.FirstOrDefault() ?? throw new ApplicationException("Could not resolve any IP addresses."),
            clock);
    }

    /// <summary>
    /// Factory method to create a new <see cref="Client"/> instance from an IP address.
    /// </summary>
    /// <param name="clock">The monotonic clock to use for timestamp capture.</param>
    /// <param name="address">The IP address of the NTP server.</param>
    /// <returns>A new <see cref="Client"/> instance.</returns>
    public static Client CreateForIpAddress(IMonotonicClock clock, IPAddress address) => new (address, clock);

    /// <summary>
    /// Sends an NTP request to the configured server and returns the request/response exchange.
    /// </summary>
    /// <param name="ct">Cancellation token for the network operation.</param>
    /// <returns>A <see cref="Request"/> containing the exchange data with calculated offset and delay.</returns>
    // TODO rename to SampleAsync
    public async Task<Request> ConnectAsync(CancellationToken ct = default)
    {
        using var client = new UdpClient();
        var requestPacket = TransmitPacketHeader.CreateNewPacket(_clock);
        var requestBytes = requestPacket.Encode();
        var sent = await client.Client.SendToAsync(requestBytes, SocketFlags.None, _endPoint, ct);
        Log.Debug("Sent {Bytes} bytes to `{endpoint}`.", sent, _endPoint);

        Memory<byte> buffer = new byte[48];
        var response = await client.Client.ReceiveFromAsync(buffer, SocketFlags.None, _endPoint, ct);
        var receivePacket = ReadResponse(response, buffer, _clock, requestPacket.Header.TransmitTimestamp);
        Log.Debug("Received {Bytes} bytes from `{endpoint}`.", response.ReceivedBytes, _endPoint);
        return new Request(requestPacket, receivePacket);
    }

    private static Packet<ReceivePacketHeader> ReadResponse(
        int receivedBytes,
        Memory<byte> buffer,
        IMonotonicClock clock,
        TransmitTimestamp requestTransmitTimestamp)
    {
        var destinationTimestamp = clock.Capture();
        var actualReceived = buffer[..receivedBytes];
        var receivePacket = ReceivePacketHeader.Parse(actualReceived, destinationTimestamp);
        receivePacket.Header.ValidateKissODeath();
        receivePacket.Header.ValidateOriginTimestamp(requestTransmitTimestamp);
        return receivePacket;
    }

    private static Packet<ReceivePacketHeader> ReadResponse(
        SocketReceiveFromResult response,
        Memory<byte> buffer,
        IMonotonicClock clock,
        TransmitTimestamp requestTransmitTimestamp)
    {
        return ReadResponse(response.ReceivedBytes, buffer, clock, requestTransmitTimestamp);
    }
}
