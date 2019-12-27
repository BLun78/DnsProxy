using System;
using System.Collections.Generic;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Models.Rules;
using DnsProxy.Strategies;

namespace DnsProxy.Models.Context
{
    internal class DnsContext : IWriteDnsContext, IDnsContext, IDisposable
    {
        public void Dispose()
        {

        }

        public List<IRule> Rules { get; set; }
        public DnsMessage Request { get; set; }
        public DnsMessage Response { get; set; }
        public IDnsResolverStrategy DefaultDnsStrategy { get; set; }
        public IDnsResolverStrategy HostsResolverStrategy { get; set; }
        public IDnsResolverStrategy InternalNameServerResolverStrategy { get; set; }
        public List<IDnsResolverStrategy> DnsResolverStrategies { get; set; }
    }

    internal interface IDnsContext : IDisposable
    {
        public List<IRule> Rules { get; }
        public DnsMessage Request { get; }
        public DnsMessage Response { get; }

        public IDnsResolverStrategy DefaultDnsStrategy { get; }
        public IDnsResolverStrategy HostsResolverStrategy { get; }
        public IDnsResolverStrategy InternalNameServerResolverStrategy { get; }
        public List<IDnsResolverStrategy> DnsResolverStrategies { get; }

    }

    internal interface IWriteDnsContext : IDnsContext, IDisposable
    {
        public new List<IRule> Rules { get; set; }
        public new DnsMessage Request { get; set; }
        public new DnsMessage Response { get; set; }
        public new IDnsResolverStrategy DefaultDnsStrategy { get; set; }
        public new IDnsResolverStrategy HostsResolverStrategy { get; set; }
        public new IDnsResolverStrategy InternalNameServerResolverStrategy { get; set; }
        public new List<IDnsResolverStrategy> DnsResolverStrategies { get; set; }
    }
}

