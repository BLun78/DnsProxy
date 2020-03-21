using ARSoft.Tools.Net.Dns;
using DnsProxy.Common.Models.Rules;
using DnsProxy.Common.Strategies;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DnsProxy.Common.Models.Context
{
    public interface IDnsCtx : IDisposable
    {
        ILogger<IDnsCtx> Logger { get; }
        List<IRule> Rules { get; }
        DnsMessage Request { get; }
        DnsMessage Response { get; }

        IDnsResolverStrategy DefaultDnsStrategy { get; }
        IDnsResolverStrategy CacheResolverStrategy { get; }
        List<IDnsResolverStrategy> DnsResolverStrategies { get; }
        CancellationToken RootCancellationToken { get; }
        string IpEndPoint { get; }
    }
}