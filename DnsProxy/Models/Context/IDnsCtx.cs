using System;
using System.Collections.Generic;
using System.Threading;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Models.Rules;
using DnsProxy.Strategies;
using Microsoft.Extensions.Logging;

namespace DnsProxy
{
    internal interface IDnsCtx : IDisposable
    {
        ILogger<IDnsCtx> Logger { get; }
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