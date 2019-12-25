using ARSoft.Tools.Net.Dns;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DnsProxy.Strategies
{
    internal class StrategyManager
    {
        private readonly ILogger<StrategyManager> _logger;

        public StrategyManager(ILogger<StrategyManager> logger)
        {
            _logger = logger;
        }

        public Task<DnsMessage> ResolveAsync(DnsMessage dnsMessage, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
