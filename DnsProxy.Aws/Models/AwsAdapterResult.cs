using System;
using System.Collections.Generic;
using System.Text;
using ARSoft.Tools.Net.Dns;

namespace DnsProxy.Aws.Models
{
    internal class AwsAdapterResult
    {
        public AwsAdapterResult()
        {
            DnsRecords = new List<DnsRecordBase>();
            ProxyBypassList = new List<string>();
        }

        public List<DnsRecordBase> DnsRecords { get; }

        public List<string> ProxyBypassList { get; }
    }
}
