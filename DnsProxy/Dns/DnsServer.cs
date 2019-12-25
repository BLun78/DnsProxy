using ARSoft.Tools.Net.Dns;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DnsProxy.Dns
{
    internal class DnsServer : IDisposable
    {
        private const int DefaultDnsPort = 53;
        private readonly ILogger<DnsServer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly SortedList<int, IDnsResolverStrategy> _dnsResolverStrategies;

        private ARSoft.Tools.Net.Dns.DnsServer _server;

        public DnsServer(ILogger<DnsServer> logger, IServiceProvider serviceProvider, IEnumerable<IDnsResolverStrategy> dnsResolverStrategies)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _dnsResolverStrategies = new SortedList<int, IDnsResolverStrategy>();
            foreach (var dnsResolverStrategy in dnsResolverStrategies)
            {
                _dnsResolverStrategies.Add(dnsResolverStrategy.Order, dnsResolverStrategy);
            }
        }

        public void StartServer(int? listnerPort = null)
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Any, listnerPort ?? DefaultDnsPort);
            _server = new ARSoft.Tools.Net.Dns.DnsServer(ipEndPoint, 10, 10);

            _server.ClientConnected += OnClientConnected;
            _server.QueryReceived += OnQueryReceived;

            _server.Start();
        }

        private async Task<DnsMessage> DoQuery(DnsMessage dnsMessage)
        {
            using (var logger = _logger.BeginScope("OnQueryReceived")) ;

            foreach (var strategy in _dnsResolverStrategies)
            {
                DnsMessage upstreamResponse = await strategy.Value.ResolveAsync(dnsMessage).ConfigureAwait(false);
                if (upstreamResponse?.AnswerRecords != null
                    && upstreamResponse.AnswerRecords.Any())
                {
                    return upstreamResponse;
                }
            }

            return await Task.FromResult((DnsMessage)null).ConfigureAwait(false);
        }

        private async Task OnQueryReceived(object sender, QueryReceivedEventArgs e)
        {
            if (e.Query is DnsMessage message 
                && message.Questions.Count == 1)
            {
                DnsMessage upstreamResponse = await DoQuery(message).ConfigureAwait(false);
                if (upstreamResponse != null)
                {
                    upstreamResponse.ReturnCode = ReturnCode.NoError;
                    e.Response = upstreamResponse;
                }
            }
            await Task.CompletedTask.ConfigureAwait(false);
        }

        private async Task OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            if (!IPAddress.IsLoopback(e.RemoteEndpoint.Address))
                e.RefuseConnect = true;

            await Task.CompletedTask.ConfigureAwait(false);
        }

        public void Dispose()
        {
            ((IDisposable)_server)?.Dispose();
        }
    }
}
