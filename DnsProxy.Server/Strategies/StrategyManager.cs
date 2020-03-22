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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Common.Models.Context;
using DnsProxy.Common.Models.Rules;
using DnsProxy.Common.Strategies;
using DnsProxy.Models;
using DnsProxy.Server.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DnsProxy.Server.Strategies
{
    internal class StrategyManager : IDisposable
    {
        private readonly IDisposable _dnsDefaultServerListener;
        private readonly IOptionsMonitor<DnsDefaultServer> _dnsDefaultServerOptionsMonitor;
        private readonly IDisposable _dnsHostConfigListener;
        private readonly IOptionsMonitor<DnsHostConfig> _dnsHostConfigOptionsMonitor;
        private readonly object _lockObjectRules;
        private readonly IDisposable _rulesConfigListener;
        private readonly IOptionsMonitor<RulesConfig> _rulesConfigOptionsMonitor;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<IRule> Rules;

        public StrategyManager(
            IServiceProvider serviceProvider,
            IOptionsMonitor<RulesConfig> rulesConfigOptionsMonitor,
            IOptionsMonitor<DnsDefaultServer> dnsDefaultServerOptionsMonitor,
            IOptionsMonitor<DnsHostConfig> dnsHostConfigOptionsMonitor)
        {
            _serviceProvider = serviceProvider;
            _lockObjectRules = new object();
            Rules = new List<IRule>();

            _rulesConfigOptionsMonitor = rulesConfigOptionsMonitor;
            _dnsDefaultServerOptionsMonitor = dnsDefaultServerOptionsMonitor;
            _dnsHostConfigOptionsMonitor = dnsHostConfigOptionsMonitor;
            _rulesConfigListener = _rulesConfigOptionsMonitor.OnChange(RulesConfigListener);
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
            (_rulesConfigOptionsMonitor as IDisposable)?.Dispose();
            (_dnsDefaultServerOptionsMonitor as IDisposable)?.Dispose();
            (_dnsHostConfigOptionsMonitor as IDisposable)?.Dispose();
        }

        private void DnsHostConfigListener(DnsDefaultServer dnsHostConfig, string name)
        {
        }

        private void DnsDefaultServerListener(DnsDefaultServer dnsDefaultServer, string name)
        {
        }

        private void RulesConfigListener(RulesConfig rulesConfig, string name)
        {
            lock (_lockObjectRules)
            {
                Rules.Clear();
                Rules.AddRange(rulesConfig.Rules.Select(x => x.GetInternalRule()).ToList());
            }
        }

        private static IDnsResolverStrategy CreateStrategy(IRule rule, IServiceScope scope)
        {
            var strategy = (IDnsResolverStrategy)scope.ServiceProvider.GetService(rule.GetStrategy());
            strategy.SetRule(rule);
            return strategy;
        }

        public async Task<DnsMessage> ResolveAsync(DnsMessage dnsMessage, string ipEndPoint,
            CancellationToken cancellationToken)
        {
            if (dnsMessage == null) throw new ArgumentNullException(nameof(dnsMessage));
            if (cancellationToken == null) throw new ArgumentNullException(nameof(cancellationToken));
            if (string.IsNullOrWhiteSpace(ipEndPoint))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(ipEndPoint));

            using (var scope = _serviceProvider.CreateScope())
            {
                var dnsWriteContext = GetWriteDnsContext(scope, dnsMessage, ipEndPoint, cancellationToken);

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

        private async Task DoStrategyAsync(IDnsResolverStrategy dnsResolverStrategy, DnsQuestion dnsQuestion,
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
                    dnsWriteContext.Logger.LogWarning(aoore, "A mapping is not supportet [{0}] for that dns question! >> [{1}]",
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
            dnsWriteContext.Response.AnswerRecords.AddRange(answer);
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
            dnsWriteContext.DefaultDnsStrategy =
                CreateStrategy(_dnsDefaultServerOptionsMonitor.CurrentValue.Servers.GetInternalRule(), scope);

            dnsWriteContext.Logger = _serviceProvider.GetService<ILogger<IDnsCtx>>();
            lock (_lockObjectRules)
            {
                var strategies = new List<string>
                { dnsWriteContext.DefaultDnsStrategy.StrategyName};

                dnsWriteContext.CacheResolverStrategy = Rules
                    .Where(y => !strategies.Contains(y.StrategyName) && y.IsEnabled && y.IsCache)
                    .Select(x => CreateStrategy(x, scope)).FirstOrDefault();

                if (dnsWriteContext.CacheResolverStrategy != null)
                {
                    strategies.Add(dnsWriteContext.CacheResolverStrategy.StrategyName);
                }
                
                dnsWriteContext.DnsResolverStrategies = Rules
                    .Where(y => !strategies.Contains(y.StrategyName) && y.IsEnabled && y.IsCache == false)
                    .Select(x => CreateStrategy(x, scope)).ToList();
            }

            return dnsWriteContext;
        }
    }
}