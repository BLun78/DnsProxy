using System.Collections.Generic;
using ARSoft.Tools.Net.Dns;

namespace DnsProxy.Models
{
    internal class CacheItem
    {
        public CacheItem(List<DnsRecordBase> dnsRecordBases)
        {
            DnsRecordBases = dnsRecordBases;
        }

        public List<DnsRecordBase> DnsRecordBases { get; }
    }
}