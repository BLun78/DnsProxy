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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Common;
using DnsProxy.Models;
using DnsProxy.Models.Context;
using DnsProxy.Models.Rules;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Strategies
{
    internal abstract class BaseResolverStrategy<TRule> : IDisposable, IOrder, IDnsResolverStrategy<TRule>,
        IDnsResolverStrategy
        where TRule : IRule
    {
        protected readonly IDnsContextAccessor DnsContextAccessor;
        protected readonly ILogger<BaseResolverStrategy<TRule>> Logger;
        protected readonly IMemoryCache MemoryCache;

        protected BaseResolverStrategy(ILogger<BaseResolverStrategy<TRule>> logger,
            IDnsContextAccessor dnsContextAccessor,
            IMemoryCache memoryCache)
        {
            Logger = logger;
            DnsContextAccessor = dnsContextAccessor;
            MemoryCache = memoryCache;
        }

        public TRule Rule { get; protected set; }
        IRule IDnsResolverStrategy.Rule => Rule;

        public abstract Task<List<DnsRecordBase>> ResolveAsync(DnsQuestion dnsQuestion,
            CancellationToken cancellationToken);

        public abstract Models.Strategies GetStrategy();

        void IDnsResolverStrategy.SetRule(IRule rule)
        {
            Rule = (TRule) rule;
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
            Logger.LogTrace("--> Pattern: {pattern} --> Question {Question}  ->> IsMatch=={match}", pattern,
                dnsQuestion.Name.ToString(), match.Success);
            return match.Success;
        }

        public int Order { get; protected set; }

        protected void LogDnsQuestion(DnsQuestion dnsQuestion)
        {
            var dnsContext = DnsContextAccessor.DnsContext;
            Logger.LogDebug("ClientIpAddress: {0} requested {1} (#{2}, {3}).", dnsContext?.IpEndPoint, dnsQuestion.Name,
                dnsContext?.Request?.TransactionID.ToString(), dnsQuestion.RecordType);
        }

        protected void LogDnsCanncelQuestion(DnsQuestion dnsQuestion, OperationCanceledException operationCanceledException)
        {
            var dnsContext = DnsContextAccessor.DnsContext;
            Logger.LogDebug("Timeout for ClientIpAddress: {0} requested {1} (#{2}, {3}).", dnsContext?.IpEndPoint, dnsQuestion.Name,
                dnsContext?.Request?.TransactionID.ToString(), dnsQuestion.RecordType);
        }

        protected void LogDnsQuestionAndResult(DnsQuestion dnsQuestion, List<DnsRecordBase> answers)
        {
            var dnsContext = DnsContextAccessor.DnsContext;
            Logger.LogDebug("ClientIpAddress: {0} resolve {1} (#{2}, {3}).", dnsContext?.IpEndPoint,
                answers?.FirstOrDefault()?.ToString(),
                dnsContext?.Request?.TransactionID.ToString(), dnsQuestion.RecordType);
        }


        protected void StoreInCache(List<DnsRecordBase> data, string key, MemoryCacheEntryOptions cacheEntryOptions)
        {
            var cacheItem = new CacheItem(data);
            var lastChar = key.Substring(key.Length - 1, 1);
            MemoryCache.Set(lastChar == "."
                ? key
                : $"{key}.", cacheItem, cacheEntryOptions);
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
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}