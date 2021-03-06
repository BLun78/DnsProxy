﻿#region Apache License-2.0
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

using ARSoft.Tools.Net.Dns;
using DnsProxy.Common.Models.Context;
using DnsProxy.Plugin.Models.Rules;
using DnsProxy.Plugin.Strategies;
using DnsProxy.Server.Models;
using DnsProxy.Server.Models.Models;
using DnsProxy.Server.Models.Rules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DnsProxy.Server.Strategies
{
    internal class StrategyManager : IDisposable
    {
        private readonly IDisposable _dnsDefaultServerListener;
        private readonly IOptionsMonitor<DnsDefaultServer> _dnsDefaultServerOptionsMonitor;
        private readonly IDisposable _dnsHostConfigListener;
        private readonly IOptionsMonitor<DnsHostConfig> _dnsHostConfigOptionsMonitor;
        private readonly ILogger<StrategyManager> _logger;
        private readonly IDisposable _hostsConfigListener;
        private readonly IOptionsMonitor<HostsConfig> _hostsConfigOptionsMonitor;
        private readonly object _lockObjectRules;
        private readonly IDisposable _rulesConfigListener;
        private readonly IOptionsMonitor<RulesConfig> _rulesConfigOptionsMonitor;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRuleFactories _ruleFactories;
        private readonly List<IRule> _rules;

        public StrategyManager(
            IServiceProvider serviceProvider,
            IRuleFactories ruleFactories,
            IOptionsMonitor<RulesConfig> rulesConfigOptionsMonitor,
            IOptionsMonitor<HostsConfig> hostsConfigOptionsMonitor,
            IOptionsMonitor<DnsDefaultServer> dnsDefaultServerOptionsMonitor,
            IOptionsMonitor<DnsHostConfig> dnsHostConfigOptionsMonitor,
            ILogger<StrategyManager> logger)
        {
            _serviceProvider = serviceProvider;
            _ruleFactories = ruleFactories;
            _lockObjectRules = new object();
            _rules = new List<IRule>();

            _rulesConfigOptionsMonitor = rulesConfigOptionsMonitor;
            _hostsConfigOptionsMonitor = hostsConfigOptionsMonitor;
            _dnsDefaultServerOptionsMonitor = dnsDefaultServerOptionsMonitor;
            _dnsHostConfigOptionsMonitor = dnsHostConfigOptionsMonitor;
            _logger = logger;
            _rulesConfigListener = _rulesConfigOptionsMonitor.OnChange(RulesConfigListener);
            _hostsConfigListener = _hostsConfigOptionsMonitor.OnChange(HostsConfigListener);
            _dnsDefaultServerListener = _dnsDefaultServerOptionsMonitor.OnChange(DnsDefaultServerListener);
            _dnsHostConfigListener = _dnsDefaultServerOptionsMonitor.OnChange(DnsHostConfigListener);
            DnsDefaultServerListener(_dnsDefaultServerOptionsMonitor.CurrentValue, null);
            RulesConfigListener(_rulesConfigOptionsMonitor.CurrentValue, null);
        }

        public void Dispose()
        {
            _rulesConfigListener?.Dispose();
            _dnsDefaultServerListener?.Dispose();
            _dnsHostConfigListener?.Dispose();
            _hostsConfigListener?.Dispose();
            (_rulesConfigOptionsMonitor as IDisposable)?.Dispose();
            (_dnsDefaultServerOptionsMonitor as IDisposable)?.Dispose();
            (_dnsHostConfigOptionsMonitor as IDisposable)?.Dispose();
            (_hostsConfigOptionsMonitor as IDisposable)?.Dispose();
        }

        private void DnsHostConfigListener(DnsDefaultServer dnsHostConfig, string name)
        {
        }

        private void HostsConfigListener(HostsConfig hostsConfig, string name)
        {
        }

        private void DnsDefaultServerListener(DnsDefaultServer dnsDefaultServer, string name)
        {
        }

        private void RulesConfigListener(RulesConfig rulesConfig, string name)
        {
            lock (_lockObjectRules)
            {
                try
                {
                    _rules.Clear();
                    _rules.AddRange(rulesConfig.Rules.Select(x => x.GetInternalRule(_ruleFactories.Factories)).ToList());
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, e.Message);
                    throw;
                }
            }
        }

        private static IDnsResolverStrategy CreateStrategy(IRule rule, IServiceScope scope)
        {
            var strategy = (IDnsResolverStrategy)scope.ServiceProvider.GetService(rule.GetStrategy());
            strategy.SetRule(rule);
            return strategy;
        }

        public async Task<DnsMessage> ResolveAsync(DnsMessage dnsMessage, string ipEndPoint, CancellationToken cancellationToken)
        {
            if (dnsMessage == null) throw new ArgumentNullException(nameof(dnsMessage));
            if (cancellationToken == null) throw new ArgumentNullException(nameof(cancellationToken));
            if (string.IsNullOrWhiteSpace(ipEndPoint))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(ipEndPoint));

            using (var scope = _serviceProvider.CreateScope())
            using (var dnsWriteContext = GetWriteDnsContext(scope, dnsMessage, ipEndPoint, cancellationToken))
            {
                foreach (var dnsQuestion in dnsWriteContext.Response.Questions)
                {
                    try
                    {
                        await DoStrategyAsync(dnsWriteContext.CacheResolverStrategy, dnsQuestion, dnsWriteContext,
                            cancellationToken).ConfigureAwait(false);
                        if (dnsWriteContext.Response.AnswerRecords.Any())
                        {
                            continue;
                        }

                        var patternList = dnsWriteContext.DnsResolverStrategies
                            .Where(dnsResolverStrategy => dnsResolverStrategy.MatchPattern(dnsQuestion)).ToList();

                        foreach (var dnsResolverStrategy in patternList)
                        {
                            await DoStrategyAsync(dnsResolverStrategy, dnsQuestion, dnsWriteContext,
                                cancellationToken).ConfigureAwait(false);
                            if (dnsWriteContext.Response.AnswerRecords.Any())
                            {
                                break;
                            }
                        }

                        if (dnsWriteContext.Response.AnswerRecords.Any())
                        {
                            continue;
                        }

                        await DoStrategyAsync(dnsWriteContext.DefaultDnsStrategy, dnsQuestion, dnsWriteContext,
                            cancellationToken).ConfigureAwait(false);
                        if (dnsWriteContext.Response.AnswerRecords.Any())
                        {
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        dnsWriteContext.Logger.LogError(e, "At the resolving process is an error raised: [{0}]", e.Message);
                        throw;
                    }
                }

                dnsWriteContext.Response.ReturnCode = ReturnCode.NoError;
                dnsWriteContext.Response.IsQuery = false;
                dnsWriteContext.Response.IsRecursionAllowed = true;

                if (!dnsWriteContext.Response.AnswerRecords.Any())
                    dnsWriteContext.Response.ReturnCode = ReturnCode.ServerFailure;

                return dnsWriteContext.Response;
            }
        }

        private static async Task DoStrategyAsync(IDnsResolverStrategy dnsResolverStrategy, DnsQuestion dnsQuestion,
            IWriteDnsContext dnsWriteContext, CancellationToken joinedGlobalCtx)
        {

            if (!dnsWriteContext.Response.AnswerRecords.Any() && dnsResolverStrategy != null)
            {
                try
                {
                    if (!dnsResolverStrategy.NeedsQueryTimeout)
                    {
                        await DoResolveAsync(dnsResolverStrategy, dnsQuestion, dnsWriteContext, joinedGlobalCtx).ConfigureAwait(false);
                    }
                    else
                    {
                        using (var strategyQueryTimeoutCts = new CancellationTokenSource(dnsResolverStrategy.Rule.QueryTimeout * 2))
                        using (var joinedStrategyCts = CancellationTokenSource.CreateLinkedTokenSource(strategyQueryTimeoutCts.Token, joinedGlobalCtx))
                        {
                            await DoResolveAsync(dnsResolverStrategy, dnsQuestion, dnsWriteContext, joinedStrategyCts.Token).ConfigureAwait(false);
                        }
                    }
                }
                catch (ArgumentOutOfRangeException aoore)
                {
                    dnsWriteContext.Logger.LogWarning(aoore, "A mapping is not supported [{0}] for that dns question! >> [{1}]",
                        aoore.ActualValue, aoore.Message);
                }
                catch (Exception e)
                {
                    dnsWriteContext.Logger.LogError(e, "A strategy error for the dns question {0} with the error message >> [{1}]",
                        dnsQuestion?.Name?.ToString(), e.Message);
                }
            }
            else
            {
                dnsWriteContext.Logger.LogInformation("Query is found! {0}", dnsResolverStrategy?.GetType()?.ToString());
            }
        }

        private static async Task DoResolveAsync(IDnsResolverStrategy dnsResolverStrategy, DnsQuestion dnsQuestion,
            IWriteDnsContext dnsWriteContext, CancellationToken cancellationToken)
        {
            var answer = await dnsResolverStrategy
                .ResolveAsync(dnsQuestion, cancellationToken)
                .ConfigureAwait(false);
            var response = (DnsMessage)dnsWriteContext.Response;
            response.AnswerRecords.AddRange(answer.Cast<DnsRecordBase>());
        }

        private IWriteDnsContext GetWriteDnsContext(IServiceScope scope, DnsMessage dnsMessage, string ipEndPoint,
            CancellationToken cancellationToken)
        {
            var dnsContextAccessor = _serviceProvider.GetService<IWriteDnsContextAccessor>();
            var dnsWriteContext = _serviceProvider.GetService<IWriteDnsContext>();
            dnsContextAccessor.WriteDnsContext = dnsWriteContext;

            dnsWriteContext.IpEndPoint = ipEndPoint;
            dnsWriteContext.RootCancellationToken = cancellationToken;
            dnsWriteContext.Request = dnsMessage;
            dnsWriteContext.Response = dnsMessage.CreateResponseInstance();
            dnsWriteContext.Logger = _serviceProvider.GetService<ILogger<IDnsCtx>>();

            dnsWriteContext.DefaultDnsStrategy =
                CreateStrategy(_dnsDefaultServerOptionsMonitor.CurrentValue.Servers.GetInternalRule(_ruleFactories.Factories), scope);
            dnsWriteContext.CacheResolverStrategy = _hostsConfigOptionsMonitor.CurrentValue.Rule.IsEnabled
                ? CreateStrategy(_hostsConfigOptionsMonitor.CurrentValue.Rule, scope)
                : null;

            lock (_lockObjectRules)
            {
                dnsWriteContext.DnsResolverStrategies = _rules
                    .Where(y => y.IsEnabled)
                    .Select(x => CreateStrategy(x, scope)).ToList();
            }

            return dnsWriteContext;
        }
    }
}