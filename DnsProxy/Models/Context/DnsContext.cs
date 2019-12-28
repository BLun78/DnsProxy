using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
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
        public CancellationToken RootCancellationToken { get; set; }
        public string IpEndPoint { get; set; }
    }

    internal interface IDnsContext : IDisposable
    {
        List<IRule> Rules { get; }
        DnsMessage Request { get; }
        DnsMessage Response { get; }

        IDnsResolverStrategy DefaultDnsStrategy { get; }
        IDnsResolverStrategy HostsResolverStrategy { get; }
        IDnsResolverStrategy InternalNameServerResolverStrategy { get; }
        List<IDnsResolverStrategy> DnsResolverStrategies { get; }
        CancellationToken RootCancellationToken { get; }
        string IpEndPoint { get; }
    }

    internal interface IWriteDnsContext : IDnsContext, IDisposable
    {
        new List<IRule> Rules { get; set; }
        new DnsMessage Request { get; set; }
        new DnsMessage Response { get; set; }
        new IDnsResolverStrategy DefaultDnsStrategy { get; set; }
        new IDnsResolverStrategy HostsResolverStrategy { get; set; }
        new IDnsResolverStrategy InternalNameServerResolverStrategy { get; set; }
        new List<IDnsResolverStrategy> DnsResolverStrategies { get; set; }
        new CancellationToken RootCancellationToken { get; set; }
        new string IpEndPoint { get; set; }
    }
}

