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
using System.Globalization;
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
using Microsoft.Extensions.Options;

namespace DnsProxy.Strategies
{
    internal class HostsResolverStrategy : BaseResolverStrategy<HostsRule>, IDnsResolverStrategy<HostsRule>
    {
        private readonly IOptionsMonitor<HostsConfig> _hostConfigOptionsMonitor;
        private readonly IDisposable _parseHostConfig;
        private HostsConfig _hostConfigCache;

        public HostsResolverStrategy(
            ILogger<HostsResolverStrategy> logger,
            IMemoryCache memoryCache,
            IOptionsMonitor<HostsConfig> hostConfigOptionsMonitor,
            IOptionsMonitor<CacheConfig> cacheConfigOptionsMonitor,
            IDnsContextAccessor dnsContextAccessor) 
            : base(logger, dnsContextAccessor, memoryCache, cacheConfigOptionsMonitor)
        {
            CacheCancellationToken = new CancellationToken();
            _hostConfigOptionsMonitor = hostConfigOptionsMonitor;
            ParseHostConfig(_hostConfigOptionsMonitor.CurrentValue);
            _parseHostConfig = _hostConfigOptionsMonitor.OnChange(ParseHostConfig);
            Order = 0;
        }

        internal static CancellationToken CacheCancellationToken { get; private set; }

        public override Task<List<DnsRecordBase>> ResolveAsync(DnsQuestion dnsQuestion,
            CancellationToken cancellationToken)
        {
            LogDnsQuestion(dnsQuestion);
            var result = new List<DnsRecordBase>();

            var cacheItem = MemoryCache.Get<CacheItem>(dnsQuestion.Name.ToString());
            if (cacheItem != null && cacheItem.DnsRecordBases.Any())
            {
                if (cacheItem.RecordType == dnsQuestion.RecordType)
                {
                    result.AddRange(cacheItem.DnsRecordBases);
                }
            }

            LogDnsQuestionAndResultFromCache(dnsQuestion, result);
            return Task.FromResult(result);
        }

        public override Models.Strategies GetStrategy()
        {
            return Models.Strategies.Hosts;
        }

        public override bool MatchPattern(DnsQuestion dnsQuestion)
        {
            return true;
        }

        protected void LogDnsQuestionAndResultFromCache(DnsQuestion dnsQuestion, List<DnsRecordBase> answers)
        {
            var dnsContext = DnsContextAccessor.DnsContext;
            Logger.LogDebug("Cache >> ClientIpAddress: {0} resolve by cache to {1} (#{2}, {3}).",
                dnsContext?.IpEndPoint, answers?.FirstOrDefault()?.ToString(),
                dnsContext?.Request?.TransactionID.ToString(CultureInfo.InvariantCulture), dnsQuestion.RecordType);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
                if (disposing)
                    _parseHostConfig?.Dispose();
            base.Dispose(disposing);
        }

        private void ParseHostConfig(HostsConfig hostConfig, string listener = "")
        {
            if (_hostConfigCache != null)
                foreach (var host in _hostConfigCache.Hosts)
                {
                    foreach (var ipAddress in host.IpAddresses)
                    {
                        var tempHost = host.ToPtrRecords(ipAddress);
                        RemoveCacheItem(tempHost.Item1);
                    }

                    foreach (var domainName in host.DomainNames)
                    {
                        RemoveCacheItem(domainName);
                    }
                }

            _hostConfigCache = (HostsConfig)hostConfig.Clone();

            if (_hostConfigCache != null)
                foreach (var host in _hostConfigCache.Hosts)
                {
                    foreach (var ipAddress in host.IpAddresses)
                    {
                        var tempHost = host.ToPtrRecords(ipAddress);
                        StoreInCache(RecordType.Ptr, tempHost.Item2.Cast<DnsRecordBase>().ToList(), tempHost.Item1);
                    }

                    foreach (var domainName in host.DomainNames)
                    {
                        // TODO: Better group by ARecord and AaaaRecord
                        var tempHost = host.ToAddressRecord(domainName);
                        StoreInCache(tempHost.First().RecordType, tempHost.Cast<DnsRecordBase>().ToList(), domainName);
                    }
                }
        }

        private void RemoveCacheItem(string key)
        {
            var lastChar = key.Substring(key.Length - 1, 1);
            MemoryCache.Remove(lastChar == "."
                ? key
                : $"{key}.");
        }

        private void StoreInCache(RecordType recordType, List<DnsRecordBase> data, string key)
        {
            var cacheoptions = new MemoryCacheEntryOptions();
            cacheoptions.SetPriority(CacheItemPriority.NeverRemove);
            StoreInCache(recordType, data, key, cacheoptions);
        }
    }
}