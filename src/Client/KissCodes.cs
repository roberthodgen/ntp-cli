namespace RobertHodgen.Ntp.Client;

/// <summary>
/// Kiss Code
/// 
/// If the Stratum field is 0, which implies unspecified or invalid, the Reference Identifier field can be used to
/// convey messages useful for status reporting and access control.  These are called Kiss-o'-Death (KoD) packets and
/// the ASCII messages they convey are called kiss codes.  The KoD packets got their name because an early use was to
/// tell clients to stop sending packets that violate server access controls.  The kiss codes can provide useful
/// information for an intelligent client, either NTPv4 or SNTPv4.  Kiss codes are encoded in four-character ASCII
/// strings that are left justified and zero filled.  The strings are designed for character displays and log files.  A
/// list of the currently defined kiss codes is given in Figure 13.  Recipients of kiss codes MUST inspect them and, in
/// the following cases, take these actions:
///  a.  For kiss codes DENY and RSTR, the client MUST demobilize any associations to that server and stop sending
/// packets to that server;
///  b.  For kiss code RATE, the client MUST immediately reduce its polling interval to that server and continue to
/// reduce it each time it receives a RATE kiss code.
///  c.  Kiss codes beginning with the ASCII character "X" are for unregistered experimentation and development and MUST
/// be ignored if not recognized.
///  d.  Other than the above conditions, KoD packets have no protocol significance and are discarded after inspection.
/// <code>
/// +------+------------------------------------------------------------+
/// | Code |                           Meaning                          |
/// +------+------------------------------------------------------------+
/// | ACST | The association belongs to a unicast server.               |
/// | AUTH | Server authentication failed.                              |
/// | AUTO | Autokey sequence failed.                                   |
/// | BCST | The association belongs to a broadcast server.             |
/// | CRYP | Cryptographic authentication or identification failed.     |
/// | DENY | Access denied by remote server.                            |
/// | DROP | Lost peer in symmetric mode.                               |
/// | RSTR | Access denied due to local policy.                         |
/// | INIT | The association has not yet synchronized for the first     |
/// |      | time.                                                      |
/// | MCST | The association belongs to a dynamically discovered server.|
/// | NKEY | No key found. Either the key was never installed or is     |
/// |      | not trusted.                                               |
/// | RATE | Rate exceeded. The server has temporarily denied access    |
/// |      | because the client exceeded the rate threshold.            |
/// | RMOT | Alteration of association from a remote host running       |
/// |      | ntpdc.                                                     |
/// | STEP | A step change in system time has occurred, but the         |
/// |      | association has not yet resynchronized.                    |
/// +------+------------------------------------------------------------+
/// </code>
/// </summary>
public sealed record KissCodes
{
    public string Value { get; } // TODO this needs to be merged into the reference ID type
}
