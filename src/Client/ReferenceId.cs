namespace RobertHodgen.Ntp.Client;

using System.Net;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// Reference ID: 32-bit code identifying the particular server or reference clock.  The interpretation depends on the
/// value in the stratum field.  For packet stratum 0 (unspecified or invalid), this is a four-character ASCII [RFC1345]
/// string, called the "kiss code", used for debugging and monitoring purposes.  For stratum 1 (reference clock), this
/// is a four-octet, left-justified, zero-padded ASCII string assigned to the reference clock.  The authoritative list
/// of Reference Identifiers is maintained by IANA; however, any string beginning with the ASCII character "X" is
/// reserved for unregistered experimentation and development.  The identifiers in Figure 12 have been used as ASCII
/// identifiers:
/// <code>
/// +------+----------------------------------------------------------+
/// | ID   | Clock Source                                             |
/// +------+----------------------------------------------------------+
/// | GOES | Geosynchronous Orbit Environment Satellite               |
/// | GPS  | Global Position System                                   |
/// | GAL  | Galileo Positioning System                               |
/// | PPS  | Generic pulse-per-second                                 |
/// | IRIG | Inter-Range Instrumentation Group                        |
/// | WWVB | LF Radio WWVB Ft. Collins, CO 60 kHz                     |
/// | DCF  | LF Radio DCF77 Mainflingen, DE 77.5 kHz                  |
/// | HBG  | LF Radio HBG Prangins, HB 75 kHz                         |
/// | MSF  | LF Radio MSF Anthorn, UK 60 kHz                          |
/// | JJY  | LF Radio JJY Fukushima, JP 40 kHz, Saga, JP 60 kHz       |
/// | LORC | MF Radio LORAN C station, 100 kHz                        |
/// | TDF  | MF Radio Allouis, FR 162 kHz                             |
/// | CHU  | HF Radio CHU Ottawa, Ontario                             |
/// | WWV  | HF Radio WWV Ft. Collins, CO                             |
/// | WWVH | HF Radio WWVH Kauai, HI                                  |
/// | NIST | NIST telephone modem                                     |
/// | ACTS | NIST telephone modem                                     |
/// | USNO | USNO telephone modem                                     |
/// | PTB  | European telephone modem                                 |
/// +------+----------------------------------------------------------+
/// </code>
///
/// Above stratum 1 (secondary servers and clients): this is the reference identifier of the server and can be used to
/// detect timing loops.  If using the IPv4 address family, the identifier is the four- octet IPv4 address.  If using
/// the IPv6 address family, it is the first four octets of the MD5 hash of the IPv6 address.  Note that, when using the
/// IPv6 address family on an NTPv4 server with a NTPv3 client, the Reference Identifier field appears to be a random
/// value and a timing loop might not be detected.
/// </summary>
public sealed record ReferenceId : EncodableBase
{
    public static ReferenceId Empty => new ("    ");

    public string Value { get; }

    public override int SizeInBits => 32;

    private ReferenceId(string value)
    {
        if (value.Length != 4)
        {
            throw new ArgumentException("Reference ID must be exactly 4 characters.", nameof(value));
        }

        Value = value;
    }

    public static ReferenceId CreateNewFromIpAddress(IPAddress address)
    {
        if (address.AddressFamily != AddressFamily.InterNetwork)
        {
            throw new ArgumentException("IP address must be an IPv4 address.", nameof(address));
        }

        return new (Encoding.ASCII.GetString(address.GetAddressBytes()));
    }

    public static ReferenceId CreateNew(string referenceId) => new (referenceId);

    public static ReferenceId Reconstitute(string referenceId) => new (referenceId);

    public override byte[] Encode()
    {
        var bytes = Encoding.ASCII.GetBytes(Value);
        if (bytes.Length != 4)
        {
            throw new ApplicationException("Reference ID is of an invalid size; must be 4-bytes.");
        }

        return bytes;
    }
}
