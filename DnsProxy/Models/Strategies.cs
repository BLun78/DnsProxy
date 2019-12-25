namespace DnsProxy.Models
{
    internal enum Strategies : int
    {
        Hosts = 0,
        NameServer = 1,
        Dns = 2,
        DoH = 3,
        Multicast = 4
    }
}
