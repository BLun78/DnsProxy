using System.Collections.Generic;

namespace DnsProxy.Models
{
    public class Host
    {
        public List<string> IPAddresses { get; set; }

        public List<string> DomainNames { get; set; }
    }
}