#region Apache License-2.0
// Copyright 2020 Bjoern Lundstroem
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

using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Server.Models;
using DnsProxy.Server.Strategies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace DnsProxy.Server
{
    internal class DnsServer : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IDisposable _dnsHostConfigListener;
        private readonly IOptionsMonitor<DnsHostConfig> _dnsHostConfigOptionsMonitor;
        private readonly ILogger<DnsServer> _logger;
        private readonly IDictionary<IPAddress, int> _networkWhitelist;
        private readonly StrategyManager _strategyManager;
        private readonly int DefaultDnsPort = 53;

        private ARSoft.Tools.Net.Dns.DnsServer _server;

        public DnsServer(ILogger<DnsServer> logger,
            IOptionsMonitor<DnsHostConfig> dnsHostConfigOptionsMonitor,
            StrategyManager strategyManager,
            CancellationTokenSource cancellationTokenSource)
        {
            _networkWhitelist = new Dictionary<IPAddress, int>();
            _logger = logger;
            _dnsHostConfigOptionsMonitor = dnsHostConfigOptionsMonitor;
            _strategyManager = strategyManager;
            _cancellationTokenSource = cancellationTokenSource;
            _dnsHostConfigListener = _dnsHostConfigOptionsMonitor.OnChange(DnsHostConfigListener);
            CreateNetworkWhitelist();
            DefaultDnsPort = _dnsHostConfigOptionsMonitor.CurrentValue.ListenerPort;
            StartServer(_dnsHostConfigOptionsMonitor.CurrentValue.ListenerPort);
        }

        public void Dispose()
        {
            StopServer();
            _dnsHostConfigListener?.Dispose();
            ((IDisposable)_server)?.Dispose();
            GC.SuppressFinalize(this);
        }

        [SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "<Pending>")]
        [SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
        private void CreateNetworkWhitelist()
        {
            _networkWhitelist.Clear();
            foreach (var network in _dnsHostConfigOptionsMonitor.CurrentValue.NetworkWhitelist)
            {
                var items = network.Split('/');
                if (items.Length != 2)
                    throw new ArgumentOutOfRangeException(
                        nameof(_dnsHostConfigOptionsMonitor.CurrentValue.NetworkWhitelist),
                        _dnsHostConfigOptionsMonitor.CurrentValue.NetworkWhitelist, null);
                var ip = IPAddress.Parse(items[0]);
                var mask = int.Parse(items[1]);
                _networkWhitelist.Add(ip, mask);
            }
        }

        private void DnsHostConfigListener(DnsHostConfig dnsHostConfig, string arg)
        {
            CreateNetworkWhitelist();
            StopServer();
            StartServer(dnsHostConfig.ListenerPort);
        }

        public void StartServer(int? listnerPort = null)
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Any, listnerPort ?? DefaultDnsPort);
            _server = new ARSoft.Tools.Net.Dns.DnsServer(ipEndPoint, 10000, 10000);

            _server.ClientConnected += OnClientConnected;
            _server.QueryReceived += OnQueryReceived;

            _server.Start();
        }

        public void StopServer()
        {
            _server.Stop();
        }

        private async Task<DnsMessage> DoQuery(DnsMessage dnsMessage, string ipEndPoint)
        {
            var upstreamResponse = await _strategyManager
                .ResolveAsync(dnsMessage, ipEndPoint, _cancellationTokenSource.Token)
                .ConfigureAwait(false);
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
                var upstreamResponse =
                    await DoQuery(message, e.RemoteEndpoint.Address.ToString()).ConfigureAwait(false);
                if (upstreamResponse != null)
                {
                    upstreamResponse.ReturnCode = ReturnCode.NoError;
                    e.Response = upstreamResponse;
                }
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        private Task OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            if (!_dnsHostConfigOptionsMonitor.CurrentValue.NetworkWhitelist.Any()
                && !IPAddress.IsLoopback(e.RemoteEndpoint.Address))
                e.RefuseConnect = true;

            if (_dnsHostConfigOptionsMonitor.CurrentValue.NetworkWhitelist.Any())
                if (_networkWhitelist.All(pair =>
                    !pair.Key.GetNetworkAddress(pair.Value)
                        .Equals(e.RemoteEndpoint.Address.GetNetworkAddress(pair.Value))))
                    e.RefuseConnect = true;

            return Task.CompletedTask;
        }
    }
}