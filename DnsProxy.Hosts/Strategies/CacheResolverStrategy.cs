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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Common.Models;
using DnsProxy.Common.Models.Context;
using DnsProxy.Common.Strategies;
using DnsProxy.Hosts.Common;
using DnsProxy.Hosts.Models;
using DnsProxy.Hosts.Models.Rules;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DnsProxy.Hosts.Strategies
{
    internal class CacheResolverStrategy : BaseResolverStrategy<HostsRule>, IDnsResolverStrategy<HostsRule>
    {
        private readonly IOptionsMonitor<HostsConfig> _hostConfigOptionsMonitor;
        private readonly IDisposable _parseHostConfig;
        private HostsConfig _hostConfigCache;

        public CacheResolverStrategy(
            IMemoryCache memoryCache,
            IOptionsMonitor<HostsConfig> hostConfigOptionsMonitor,
            IOptionsMonitor<CacheConfig> cacheConfigOptionsMonitor,
            IDnsContextAccessor dnsContextAccessor)
            : base(dnsContextAccessor, memoryCache, cacheConfigOptionsMonitor)
        {
            CacheCancellationToken = new CancellationToken();
            _hostConfigOptionsMonitor = hostConfigOptionsMonitor;
            ParseHostConfig(_hostConfigOptionsMonitor.CurrentValue);
            _parseHostConfig = _hostConfigOptionsMonitor.OnChange(ParseHostConfig);

            StrategyName = "Cache";
            NeedsQueryTimeout = true;
            IsCache = true;
        }

        internal static CancellationToken CacheCancellationToken { get; private set; }

        public override Task<List<DnsRecordBase>> ResolveAsync(DnsQuestion dnsQuestion, CancellationToken cancellationToken)
        {
            var logger = DnsContextAccessor.DnsContext.Logger;
            using (logger.BeginScope("CACHE =>"))
            {
                var stopwatch = new Stopwatch();
                LogDnsQuestion(dnsQuestion, stopwatch);
                var result = new List<DnsRecordBase>();
                var key = dnsQuestion.ToString();

                var cacheItem = MemoryCache.Get<CacheItem>(key);
                if (cacheItem != null && cacheItem.DnsRecordBases.Any())
                {
                    if (cacheItem.RecordType == dnsQuestion.RecordType)
                    {
                        result.AddRange(cacheItem.DnsRecordBases);
                    }
                }

                stopwatch.Stop();
                if (result.Any())
                {
                    LogDnsQuestionAndResult(dnsQuestion, result, stopwatch);
                }

                return Task.FromResult(result);
            }
        }

        public override bool MatchPattern(DnsQuestion dnsQuestion)
        {
            return true;
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
                        var question = new DnsQuestion(DomainName.Parse(tempHost.Item1), RecordType.Ptr, RecordClass.INet);
                        RemoveCacheItem(question);
                    }

                    foreach (var domainName in host.DomainNames)
                    {
                        var tempHost = host.ToAddressRecord(domainName);
                        var question = new DnsQuestion(DomainName.Parse(domainName), tempHost.First().RecordType, RecordClass.INet);
                        RemoveCacheItem(question);
                    }
                }

            _hostConfigCache = (HostsConfig)hostConfig.Clone();

            if (_hostConfigCache != null)
                foreach (var host in _hostConfigCache.Hosts)
                {
                    foreach (var ipAddress in host.IpAddresses)
                    {
                        var tempHost = host.ToPtrRecords(ipAddress);
                        var question = new DnsQuestion(DomainName.Parse(tempHost.Item1), RecordType.Ptr, RecordClass.INet);
                        StoreInCache(question, tempHost.Item2.Cast<DnsRecordBase>().ToList());
                    }

                    foreach (var domainName in host.DomainNames)
                    {
                        var tempHost = host.ToAddressRecord(domainName);
                        var question = new DnsQuestion(DomainName.Parse(domainName), tempHost.First().RecordType, RecordClass.INet);
                        StoreInCache(question, tempHost.Cast<DnsRecordBase>().ToList());
                    }
                }
        }

        private void RemoveCacheItem(DnsQuestion dnsQuestion)
        {
            var key = dnsQuestion.ToString();
            var lastChar = key.Substring(key.Length - 1, 1);
            MemoryCache.Remove(lastChar == "."
                ? key
                : $"{key}.");
        }

        private void StoreInCache(DnsQuestion dnsQuestion, List<DnsRecordBase> data)
        {
            var cacheoptions = new MemoryCacheEntryOptions();
            cacheoptions.SetPriority(CacheItemPriority.NeverRemove);

            StoreInCache(dnsQuestion, data, cacheoptions);
        }
    }
}