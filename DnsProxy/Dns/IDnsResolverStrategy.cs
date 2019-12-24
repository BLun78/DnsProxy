using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;

namespace DnsProxy.Dns
{
    internal interface IDnsResolverStrategy: IDisposable, IOrder
    {
        Task<DnsMessage> ResolveAsync(DnsMessage dnsMessage, CancellationToken cancellationToken = default);
        
    }

   internal interface IOrder
    {
        int Order { get; }
    }
}
