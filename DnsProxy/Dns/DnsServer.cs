#region Apache License-2.0
// Copyright 2019 Bjoern Lundstroem
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
#endregion

using ARSoft.Tools.Net.Dns;
using DnsProxy.Models;
using DnsProxy.Strategies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DnsProxy.Dns
{
    internal class DnsServer : IDisposable
    {
        private const int DefaultDnsPort = 53;
        private readonly ILogger<DnsServer> _logger;
        private readonly IOptionsMonitor<DnsHostConfig> _dnsHostConfigOptionsMonitor;
        private readonly StrategyManager _strategyManager;

        private ARSoft.Tools.Net.Dns.DnsServer _server;

        public DnsServer(ILogger<DnsServer> logger,
            IOptionsMonitor<DnsHostConfig> dnsHostConfigOptionsMonitor,
            StrategyManager strategyManager)
        {
            _logger = logger;
            _dnsHostConfigOptionsMonitor = dnsHostConfigOptionsMonitor;
            _strategyManager = strategyManager;
            _dnsHostConfigOptionsMonitor.OnChange(DnsHostConfigListener);
            StartServer(_dnsHostConfigOptionsMonitor.CurrentValue.ListenerPort);
        }

        private void DnsHostConfigListener(DnsHostConfig dnsHostConfig, string arg)
        {
            StopServer();
            StartServer(dnsHostConfig.ListenerPort);
        }

        public void Dispose()
        {
            ((IDisposable)_server)?.Dispose();
        }

        public void StartServer(int? listnerPort = null)
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Any, listnerPort ?? DefaultDnsPort);
            _server = new ARSoft.Tools.Net.Dns.DnsServer(ipEndPoint, 10, 10);

            _server.ClientConnected += OnClientConnected;
            _server.QueryReceived += OnQueryReceived;

            _server.Start();
        }

        public void StopServer()
        {
            _server.Stop();
        }

        private async Task<DnsMessage> DoQuery(DnsMessage dnsMessage)
        {
            DnsMessage upstreamResponse = await _strategyManager.ResolveAsync(dnsMessage).ConfigureAwait(false);
            if (upstreamResponse?.AnswerRecords != null
                && upstreamResponse.AnswerRecords.Any())
                return upstreamResponse;

            return await Task.FromResult((DnsMessage)null).ConfigureAwait(false);
        }

        private async Task OnQueryReceived(object sender, QueryReceivedEventArgs e)
        {
            if (e.Query is DnsMessage message
                && message.Questions.Count == 1)
            {
                var upstreamResponse = await DoQuery(message).ConfigureAwait(false);
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
    }
}