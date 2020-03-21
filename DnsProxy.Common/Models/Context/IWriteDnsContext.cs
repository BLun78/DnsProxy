using ARSoft.Tools.Net.Dns;
using DnsProxy.Common.Models.Rules;
using DnsProxy.Common.Strategies;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DnsProxy.Common.Models.Context
{
    public interface IWriteDnsContext : IDnsCtx, IDisposable
    {
        new ILogger<IDnsCtx> Logger { get; set; }
        new List<IRule> Rules { get; set; }
        new DnsMessage Request { get; set; }
        new DnsMessage Response { get; set; }
        new IDnsResolverStrategy DefaultDnsStrategy { get; set; }
        new IDnsResolverStrategy CacheResolverStrategy { get; set; }
        new List<IDnsResolverStrategy> DnsResolverStrategies { get; set; }
        new CancellationToken RootCancellationToken { get; set; }
        new string IpEndPoint { get; set; }
    }
}