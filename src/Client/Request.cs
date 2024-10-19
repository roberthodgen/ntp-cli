namespace RobertHodgen.Ntp.Client;

using Remote;

public sealed class Request
{
    public Packet<TransmitPacketHeader> ClientRequest { get; }

    public Packet<ReceivePacketHeader> ServerResponse { get; }

    internal Request(Packet<TransmitPacketHeader> clientRequest, Packet<ReceivePacketHeader> serverResponse)
    {
        ClientRequest = clientRequest;
        ServerResponse = serverResponse;
    }

    /// <summary>
    /// The difference in absolute time between client and server clocks.
    /// </summary>
    /// <remarks>
    /// Calculated as:
    /// <code>
    /// theta = ((t1 - t0) + (t2 - t3)) / 2
    /// </code>
    /// </remarks>
    public TimeSpan Theta()
    {
        // clientRequestPacketTransmissionTime
        var t0 = ClientRequest.Header.OriginTimestamp.Value.ToDateTime();
        
        // serverRequestPacketReceptionTime
        var t1 = ServerResponse.Header.ReceiveTimestamp.Value.ToDateTime();
        
        // serverResponseTransmissionTime
        var t2 = ServerResponse.Header.TransmitTimestamp.Value.ToDateTime();
        
        // clientResponsePacketReception
        if (ServerResponse.DestinationTimestamp is null)
        {
            throw new ApplicationException("Server response packet reception time is required for theta calculation.");
        }

        var t3 = ServerResponse.DestinationTimestamp.ToDateTime();
        return ((t1 - t0) + (t2 - t3)) / 2.0;
    }

    /// <summary>
    /// The round-trip delay.
    /// </summary>
    /// <remarks>
    /// Calculated as:
    /// <code>
    /// delta = (t3 − t0) − (t2 − t1)
    /// </code>
    /// </remarks>
    public TimeSpan Delta()
    {
        // clientRequestPacketTransmissionTime
        var t0 = ClientRequest.Header.OriginTimestamp.Value.ToDateTime();
        
        // serverRequestPacketReceptionTime
        var t1 = ServerResponse.Header.ReceiveTimestamp.Value.ToDateTime();
        
        // serverResponseTransmissionTime
        var t2 = ServerResponse.Header.TransmitTimestamp.Value.ToDateTime();
        
        // clientResponsePacketReception
        if (ServerResponse.DestinationTimestamp is null)
        {
            throw new ApplicationException("Server response packet reception time is required for delta calculation.");
        }

        var t3 = ServerResponse.DestinationTimestamp.ToDateTime();
        return (t3 - t0) - (t2 - t1);
    }
}
