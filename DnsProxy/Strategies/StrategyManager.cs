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
using DnsProxy.Models.Context;
using DnsProxy.Models.Rules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DnsProxy.Strategies
{
    internal class StrategyManager : IDisposable
    {
        private readonly object _lockObjectRules;
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
            _lockObjectRules = new object();

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
            lock (_lockObjectRules)
            {
                Rules.Clear();
                Rules = rulesConfig.Rules.Select(x => x.GetInternalRule()).ToList();
            }
        }

        private void DnsDefaultServerListener(DnsDefaultServer dnsDefaultServer, string name)
        {
        }

        private static IDnsResolverStrategy CreateStrategy(IRule rule, IServiceScope scope)
        {
            var strategy = (IDnsResolverStrategy)scope.ServiceProvider.GetService(rule.GetStraegy());
            strategy.SetRule(rule);
            return strategy;
        }

        public async Task<DnsMessage> ResolveAsync(DnsMessage dnsMessage, CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dnsWriteContext = GetWriteDnsContext(scope, dnsMessage, cancellationToken);

                using (var globalQueryTimeoutCts = new CancellationTokenSource(_dnsDefaultServerOptionsMonitor.CurrentValue.Servers.QueryTimeout))
                using (var joinedGlobalCts = CancellationTokenSource.CreateLinkedTokenSource(globalQueryTimeoutCts.Token, cancellationToken))
                {
                    foreach (DnsQuestion dnsQuestion in dnsWriteContext.Response.Questions)
                    {
                        foreach (IDnsResolverStrategy dnsResolverStrategy in dnsWriteContext.DnsResolverStrategies)
                        {
                            if (dnsResolverStrategy.MatchPattern(dnsQuestion))
                            {
                                using (var strategyQueryTimeoutCts = new CancellationTokenSource(dnsResolverStrategy.Rule.QueryTimeout))
                                using (var joinedStrategyCts = CancellationTokenSource.CreateLinkedTokenSource(strategyQueryTimeoutCts.Token, joinedGlobalCts.Token))
                                {
                                    var answer = await dnsResolverStrategy
                                        .ResolveAsync(dnsQuestion, joinedStrategyCts.Token)
                                        .ConfigureAwait(false);
                                    dnsWriteContext.Response.AnswerRecords.AddRange(answer);
                                    break;
                                }
                            }
                        }

                        if (!dnsWriteContext.Response.AnswerRecords.Any() && dnsWriteContext.HostsResolverStrategy != null)
                        {
                            using (var strategyQueryTimeoutCts = new CancellationTokenSource(dnsWriteContext.HostsResolverStrategy.Rule.QueryTimeout))
                            using (var joinedStrategyCts = CancellationTokenSource.CreateLinkedTokenSource(strategyQueryTimeoutCts.Token, joinedGlobalCts.Token))
                            {
                                var answer = await dnsWriteContext.HostsResolverStrategy
                                    .ResolveAsync(dnsQuestion, joinedStrategyCts.Token)
                                    .ConfigureAwait(false);
                                dnsWriteContext.Response.AnswerRecords.AddRange(answer);
                            }
                        }

                        if (!dnsWriteContext.Response.AnswerRecords.Any() && dnsWriteContext.InternalNameServerResolverStrategy != null)
                        {
                            using (var strategyQueryTimeoutCts = new CancellationTokenSource(dnsWriteContext.InternalNameServerResolverStrategy.Rule.QueryTimeout))
                            using (var joinedStrategyCts = CancellationTokenSource.CreateLinkedTokenSource(strategyQueryTimeoutCts.Token, joinedGlobalCts.Token))
                            {
                                var answer = await dnsWriteContext.InternalNameServerResolverStrategy
                                    .ResolveAsync(dnsQuestion, joinedStrategyCts.Token)
                                    .ConfigureAwait(false);
                                dnsWriteContext.Response.AnswerRecords.AddRange(answer);
                            }
                        }

                        if (!dnsWriteContext.Response.AnswerRecords.Any())
                        {
                            using (var strategyQueryTimeoutCts = new CancellationTokenSource(dnsWriteContext.DefaultDnsStrategy.Rule.QueryTimeout))
                            using (var joinedStrategyCts = CancellationTokenSource.CreateLinkedTokenSource(strategyQueryTimeoutCts.Token, joinedGlobalCts.Token))
                            {
                                var answer = await dnsWriteContext.DefaultDnsStrategy
                                    .ResolveAsync(dnsQuestion, joinedStrategyCts.Token)
                                    .ConfigureAwait(false);
                                dnsWriteContext.Response.AnswerRecords.AddRange(answer);
                            }
                        }
                    }

                    dnsWriteContext.Response.ReturnCode = ReturnCode.NoError;
                    dnsWriteContext.Response.IsQuery = false;
                   

                    if (!dnsWriteContext.Response.AnswerRecords.Any()) dnsWriteContext.Response.ReturnCode = ReturnCode.ServerFailure;

                    return dnsWriteContext.Response;
                }
            }
        }

        private IWriteDnsContext GetWriteDnsContext(IServiceScope scope, DnsMessage dnsMessage, CancellationToken cancellationToken)
        {
            var dnsContextAccessor = scope.ServiceProvider.GetService<IWriteDnsContextAccessor>();
            var dnsWriteContext = scope.ServiceProvider.GetService<IWriteDnsContext>();
            dnsContextAccessor.WriteDnsContext = dnsWriteContext;

            dnsWriteContext.RootCancellationToken = cancellationToken;
            dnsWriteContext.Request = dnsMessage;
            dnsWriteContext.Response = dnsMessage.CreateResponseInstance();
            dnsWriteContext.DefaultDnsStrategy = CreateStrategy(_dnsDefaultServerOptionsMonitor.CurrentValue.Servers, scope);

            dnsWriteContext.HostsResolverStrategy = _hostsConfigOptionsMonitor.CurrentValue.Rule.IsEnabled
                ? CreateStrategy(_hostsConfigOptionsMonitor.CurrentValue.Rule, scope)
                : null;

            dnsWriteContext.InternalNameServerResolverStrategy = _internalNameServerConfigOptionsMonitor.CurrentValue.Rule.IsEnabled
                ? CreateStrategy(_internalNameServerConfigOptionsMonitor.CurrentValue.Rule, scope)
                : null;

            var strategies = new List<Models.Strategies>() { Models.Strategies.Hosts, Models.Strategies.InternalNameServer };
            dnsWriteContext.DnsResolverStrategies = Rules.Where(y => !strategies.Contains(y.Strategy)).Select(x => CreateStrategy(x, scope)).ToList();

            return dnsWriteContext;
        }
    }
}