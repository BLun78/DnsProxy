using ARSoft.Tools.Net.Dns;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DnsProxy.Dns
{
    internal interface IDnsResolverStrategy : IDisposable, IOrder
    {
        Task<DnsMessage> ResolveAsync(DnsMessage dnsMessage, CancellationToken cancellationToken = default);

    }

    internal interface IOrder
    {
        int Order { get; }
    }
}
