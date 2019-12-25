using System.Collections.Generic;

namespace DnsProxy.Models
{
    internal class DnsHostConfig
    {
        public int ListenerPort { get; set; }
        public List<string> NetworkWhitelist { get; set; }
    }
}