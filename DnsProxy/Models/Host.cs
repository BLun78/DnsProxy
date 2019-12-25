using System.Collections.Generic;

namespace DnsProxy.Models
{

#pragma warning disable CA2227 // Collection properties should be read only
    public class Host
    {
        public List<string> IpAddresses { get; set; }

        public List<string> DomainNames { get; set; }
    }
#pragma warning restore CA2227 // Collection properties should be read only
}
