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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Models;
using DnsProxy.Models.Rules;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DnsProxy.Strategies
{
    internal class StrategyManager : IDisposable
    {
        private readonly IDisposable _dnsDefaultServerListener;
        private readonly IOptionsMonitor<DnsDefaultServer> _dnsDefaultServerOptionsMonitor;
        private readonly IDisposable _dnsHostConfigListener;
        private readonly IOptionsMonitor<DnsHostConfig> _dnsHostConfigOptionsMonitor;
        private readonly IDisposable _hostsConfigListener;
        private readonly IOptionsMonitor<HostsConfig> _hostsConfigOptionsMonitor;
        private readonly IDisposable _internalNameServerConfigListener;
        private readonly IOptionsMonitor<InternalNameServerConfig> _internalNameServerConfigOptionsMonitor;
        private readonly ILogger<StrategyManager> _logger;
        private readonly IDisposable _rulesConfigListner;
        private readonly IOptionsMonitor<RulesConfig> _rulesConfigOptionsMonitor;
        private readonly IServiceProvider _serviceProvider;
        private CancellationTokenSource _cts;
        private CancellationTokenSource _timeoutCts;
        private IDnsResolverStrategy defaultstrategy;
        private List<IRule> Rules;

        public StrategyManager(ILogger<StrategyManager> logger,
            IServiceProvider serviceProvider,
            IOptionsMonitor<RulesConfig> rulesConfigOptionsMonitor,
            IOptionsMonitor<DnsDefaultServer> dnsDefaultServerOptionsMonitor,
            IOptionsMonitor<HostsConfig> hostsConfigOptionsMonitor,
            IOptionsMonitor<InternalNameServerConfig> internalNameServerConfigOptionsMonitor,
            IOptionsMonitor<DnsHostConfig> dnsHostConfigOptionsMonitor)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

            _rulesConfigOptionsMonitor = rulesConfigOptionsMonitor;
            _dnsDefaultServerOptionsMonitor = dnsDefaultServerOptionsMonitor;
            _hostsConfigOptionsMonitor = hostsConfigOptionsMonitor;
            _internalNameServerConfigOptionsMonitor = internalNameServerConfigOptionsMonitor;
            _dnsHostConfigOptionsMonitor = dnsHostConfigOptionsMonitor;
            _rulesConfigListner = _rulesConfigOptionsMonitor.OnChange(RulesConfigListener);
            _dnsDefaultServerListener = _dnsDefaultServerOptionsMonitor.OnChange(DnsDefaultServerListener);
            _hostsConfigListener = _hostsConfigOptionsMonitor.OnChange(HostsConfigListener);
            _internalNameServerConfigListener =
                _internalNameServerConfigOptionsMonitor.OnChange(InternalNameServerConfigListener);
            _dnsHostConfigListener = _dnsDefaultServerOptionsMonitor.OnChange(DnsHostConfigListener);
            DnsDefaultServerListener(_dnsDefaultServerOptionsMonitor.CurrentValue, null);
        }

        public void Dispose()
        {
            _rulesConfigListner?.Dispose();
            _dnsDefaultServerListener?.Dispose();
            _hostsConfigListener?.Dispose();
            _internalNameServerConfigListener?.Dispose();
            _dnsHostConfigListener?.Dispose();
            (_rulesConfigOptionsMonitor as IDisposable)?.Dispose();
            (_dnsDefaultServerOptionsMonitor as IDisposable)?.Dispose();
            (_hostsConfigOptionsMonitor as IDisposable)?.Dispose();
            (_internalNameServerConfigOptionsMonitor as IDisposable)?.Dispose();
            (_dnsHostConfigOptionsMonitor as IDisposable)?.Dispose();
        }

        private void DnsHostConfigListener(DnsDefaultServer dnsHostConfig, string name)
        {
        }

        private void InternalNameServerConfigListener(InternalNameServerConfig internalNameServerConfig, string name)
        {
        }

        private void HostsConfigListener(HostsConfig hostsConfig, string name)
        {
        }

        private void RulesConfigListener(RulesConfig rulesConfig, string name)
        {
            Rules = rulesConfig.Rules)
        }

        private void DnsDefaultServerListener(DnsDefaultServer dnsDefaultServer, string name)
        {
            defaultstrategy = CreateStrategy(dnsDefaultServer.Servers.GetInternalRule());
        }

        private IDnsResolverStrategy CreateStrategy(IRule rule)
        {
            var strategy = (IDnsResolverStrategy)_serviceProvider.GetService(rule.GetStraegy());
            strategy.SetRule(rule);
            return strategy;
        }

        public async Task<DnsMessage> ResolveAsync(DnsMessage dnsMessage, CancellationToken cancellationToken)
        {
            DnsMessage result = null;
            var resultDnsMessage = dnsMessage.CreateResponseInstance();

            // TODO: Pattern Matching 
            // Todo: Hosts and NameServer, etc
                


            if (!resultDnsMessage.AnswerRecords.Any())
                resultDnsMessage = await defaultstrategy.ResolveAsync(resultDnsMessage, cancellationToken)
                    .ConfigureAwait(false);

            result = resultDnsMessage;
            result.IsQuery = false;

            if (!result.AnswerRecords.Any()) result.ReturnCode = ReturnCode.ServerFailure;

            return result;
        }

        protected CancellationToken CreateCancellationToken(CancellationToken cancellationToken)
        {
            _timeoutCts = new CancellationTokenSource(_dnsHostConfigOptionsMonitor.CurrentValue.DefaultQueryTimeout);
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _timeoutCts.Token);
            return _cts.Token;
        }

        protected void DisposeCancellationToken()
        {
            _timeoutCts?.Dispose();
            _cts?.Dispose();
        }
    }
}