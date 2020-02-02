using System;
using System.Collections.Generic;
using System.Threading;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Models.Rules;
using DnsProxy.Strategies;

namespace DnsProxy.Models.Context
{
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
}