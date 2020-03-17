using System;
using System.Collections.Generic;
using System.Threading;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Models.Rules;
using DnsProxy.Strategies;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Models.Context
{
    internal interface IWriteDnsContext : IDnsCtx, IDisposable
    {
        new ILogger<IDnsCtx> Logger { get; set; }
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