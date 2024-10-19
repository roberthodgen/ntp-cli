namespace RobertHodgen.Ntp.Client;

using System.Net;
using System.Net.Sockets;

public class Client
{
    private const string DefaultServer = "pool.ntp.org";

    private readonly UdpClient _client = new();
    private readonly DnsEndPoint _endpoint;

    public Client(string server = DefaultServer)
    {
        _endpoint = new DnsEndPoint(server, 123);
    }

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        // await _client.Client.SendToAsync(_endpoint);
        // await _client.Client.ReceiveFromAsync(_endpoint);
    }
}
