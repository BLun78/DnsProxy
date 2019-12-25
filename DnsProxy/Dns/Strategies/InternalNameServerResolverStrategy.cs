using ARSoft.Tools.Net.Dns;
using Makaretu.Dns.Resolving;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DnsProxy.Dns.Strategies
{
    internal class InternalNameServerResolverStrategy : BaseResolverStrategy, IDnsResolverStrategy
    {
        private readonly IOptionsMonitor<NameServerOptions> _nameServerOptions;
        private NameServer _resolver;

        public InternalNameServerResolverStrategy(ILogger<DnsResolverStrategy> logger,
            IOptionsMonitor<NameServerOptions> nameServerOptions) : base(logger)
        {
            _nameServerOptions = nameServerOptions;
            _nameServerOptions.OnChange(OptionsListener);
            var catalog = new Catalog();

            catalog.IncludeRootHints();
            _resolver = new NameServer { Catalog = catalog };
            Order = 100;
        }

        private void OptionsListener(NameServerOptions nameServerOptions, string arg2)
        {
            throw new NotImplementedException();
        }

        public Task<DnsMessage> ResolveAsync(DnsMessage dnsMessage, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    internal class NameServerOptions
    {

    }
}