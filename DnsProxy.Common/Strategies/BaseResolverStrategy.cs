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

using ARSoft.Tools.Net.Dns;
using DnsProxy.Common.Models;
using DnsProxy.Common.Models.Context;
using DnsProxy.Common.Models.Rules;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DnsProxy.Common.Strategies
{
    public abstract class BaseResolverStrategy<TRule> : IDisposable, IOrder, IDnsResolverStrategy<TRule>,
        IDnsResolverStrategy
        where TRule : IRule
    {
        protected readonly IDnsContextAccessor DnsContextAccessor;
        protected readonly IMemoryCache MemoryCache;
        protected readonly IOptionsMonitor<CacheConfig> CacheConfigOptionsMonitor;

        protected BaseResolverStrategy(
            IDnsContextAccessor dnsContextAccessor,
            IMemoryCache memoryCache,
            IOptionsMonitor<CacheConfig> cacheConfigOptionsMonitor)
        {
            DnsContextAccessor = dnsContextAccessor;
            MemoryCache = memoryCache;
            CacheConfigOptionsMonitor = cacheConfigOptionsMonitor;
            Order = 1000;
            IsCache = false;
            NeedsQueryTimeout = true;
        }

        public bool NeedsQueryTimeout { get; protected set; }
        public TRule Rule { get; protected set; }
        IRule IDnsResolverStrategy.Rule => Rule;

        public bool IsCache { get; protected internal set; }
        public string StrategyName { get; protected set; }

        public abstract Task<List<DnsRecordBase>> ResolveAsync(DnsQuestion dnsQuestion,
            CancellationToken cancellationToken);

        public int Order { get; protected set; }

        void IDnsResolverStrategy.SetRule(IRule rule)
        {
            Rule = (TRule)rule;
        }

        public void SetRule(TRule rule)
        {
            Rule = rule;
        }

        public virtual bool MatchPattern(DnsQuestion dnsQuestion)
        {
            string pattern = null;
            if (!string.IsNullOrWhiteSpace(Rule.DomainNamePattern))
            {
                pattern = Rule.DomainNamePattern;
            }
            else if (!string.IsNullOrWhiteSpace(Rule.DomainName))
            {
                pattern = $"^{Rule.DomainName.Replace(".", @"\.", StringComparison.InvariantCulture)}$";
            }

            var match = Rule.GetDomainNameRegex().Match(dnsQuestion.Name.ToString());
            DnsContextAccessor.DnsContext.Logger.LogTrace("--> Pattern: {pattern} --> Question {Question}  ->> IsMatch=={match}", pattern,
                dnsQuestion.Name.ToString(), match.Success);
            return match.Success;
        }

        protected void LogDnsQuestion(DnsQuestion dnsQuestion, Stopwatch stopwatch)
        {
            stopwatch.Start();
            var dnsContext = DnsContextAccessor.DnsContext;
            DnsContextAccessor.DnsContext.Logger.LogDebug("ClientIpAddress: {0} requested {1} ({2}, {3})",
                dnsContext?.IpEndPoint,
                dnsQuestion.Name.ToString(),
                dnsQuestion.RecordType.ToString(),
                dnsQuestion.RecordClass.ToString());
        }

        protected void LogDnsCanncelQuestion(DnsQuestion dnsQuestion, OperationCanceledException operationCanceledException, Stopwatch stopwatch)
        {
            stopwatch.Stop();
            var dnsContext = DnsContextAccessor.DnsContext;
            DnsContextAccessor.DnsContext.Logger.LogDebug(operationCanceledException, @"Timeout for ClientIpAddress: {0} requested {1} ({2}, {3}) after [{4} ms]",
                dnsContext?.IpEndPoint,
                dnsQuestion.Name.ToString(),
                dnsQuestion.RecordType.ToString(),
                dnsQuestion.RecordClass.ToString(),
                stopwatch.ElapsedMilliseconds);
        }

        protected void LogDnsQuestionAndResult(DnsQuestion dnsQuestion, List<DnsRecordBase> answers, Stopwatch stopwatch)
        {
            stopwatch.Stop();
            var logger = DnsContextAccessor.DnsContext.Logger;
            var dnsContext = DnsContextAccessor.DnsContext;
            var i = 1;
            var count = answers.Count(x => x != null);

            foreach (DnsRecordBase dnsRecordBase in answers.Where(x => x != null))
            {
                if (i == 1)
                {
                    if (count == 1)
                    {
                        logger.LogDebug("ClientIpAddress: {0} resolve {1} ({2}, {3}) after [{4} ms]",
                            dnsContext?.IpEndPoint,
                            dnsRecordBase?.ToString(),
                            dnsQuestion.RecordType.ToString(),
                            dnsQuestion.RecordClass.ToString(),
                            stopwatch.ElapsedMilliseconds);
                    }
                    else
                    {

                        LogMultiResolverLog(stopwatch, logger, dnsContext, i, count, dnsRecordBase);
                    }
                }
                else
                {
                    LogMultiResolverLog(stopwatch, logger, dnsContext, i, count, dnsRecordBase);
                }
                ++i;
            }

        }

        private static void LogMultiResolverLog(Stopwatch stopwatch, ILogger<IDnsCtx> logger, IDnsCtx dnsContext, int i, int count, DnsRecordBase dnsRecordBase)
        {
            var messageTemplate = "ClientIpAddress: {0} resolve[{1}/{2}] {3} => {4} ({5}, {6}) after [{7} ms]";
            if (dnsRecordBase is CNameRecord cNameRecord)
            {
                logger.LogDebug(messageTemplate,
                 dnsContext?.IpEndPoint,
                    i,
                    count,
                    cNameRecord.Name.ToString(),
                    cNameRecord.CanonicalName.ToString(),
                    cNameRecord.RecordType.ToString(),
                    cNameRecord.RecordClass.ToString(),
                    stopwatch.ElapsedMilliseconds);
            }
            else if (dnsRecordBase is ARecord aRecord)
            {
                logger.LogDebug(messageTemplate,
                 dnsContext?.IpEndPoint,
                    i,
                    count,
                    aRecord.Name.ToString(),
                    aRecord.Address.ToString(),
                    aRecord.RecordType.ToString(),
                    aRecord.RecordClass.ToString(),
                    stopwatch.ElapsedMilliseconds);
            }
            else if (dnsRecordBase is AaaaRecord aaaaRecord)
            {
                logger.LogDebug(messageTemplate,
                 dnsContext?.IpEndPoint,
                    i,
                    count,
                    aaaaRecord.Name.ToString(),
                    aaaaRecord.Address.ToString(),
                    aaaaRecord.RecordType.ToString(),
                    aaaaRecord.RecordClass.ToString(),
                    stopwatch.ElapsedMilliseconds);
            }
            else if (dnsRecordBase is PtrRecord ptrRecord)
            {
                logger.LogDebug(messageTemplate,
                    dnsContext?.IpEndPoint,
                    i,
                    count,
                    ptrRecord.Name.ToString(),
                    ptrRecord.PointerDomainName.ToString(),
                    ptrRecord.RecordType.ToString(),
                    ptrRecord.RecordClass.ToString(),
                    stopwatch.ElapsedMilliseconds);
            }
            else if (dnsRecordBase is MxRecord mxRecord)
            {
                logger.LogDebug(messageTemplate,
                    dnsContext?.IpEndPoint,
                    i,
                    count,
                    mxRecord.Name.ToString(),
                    mxRecord.ExchangeDomainName.ToString(),
                    mxRecord.RecordType.ToString(),
                    mxRecord.RecordClass.ToString(),
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                logger.LogDebug("ClientIpAddress: {0} resolve[{1}/{2}] {3} ({4}, {5}) after [{6} ms]",
                    dnsContext?.IpEndPoint,
                    i,
                    count,
                    dnsRecordBase.Name.ToString(),
                    dnsRecordBase.RecordType.ToString(),
                    dnsRecordBase.RecordClass.ToString(),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        protected void StoreInCache(DnsQuestion dnsQuestion, List<DnsRecordBase> data,
            MemoryCacheEntryOptions cacheEntryOptions)
        {
            var key = dnsQuestion.ToString();
            var key2 = new DnsQuestion(dnsQuestion.Name, RecordType.A, dnsQuestion.RecordClass).ToString();

            var cacheItem = new CacheItem(dnsQuestion.RecordType, data);
            MemoryCache.Set(key, cacheItem, cacheEntryOptions);

            if (dnsQuestion.RecordType != RecordType.A)
            {
                MemoryCache.Set(key2, cacheItem, cacheEntryOptions);
            }
        }

        #region IDisposable Support

        protected bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BaseResolverStrategy()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.


        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}