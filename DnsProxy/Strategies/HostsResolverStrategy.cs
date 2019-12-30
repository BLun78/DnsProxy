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
        private readonly IMemoryCache _memoryCache;
        private readonly IDisposable _parseHostConfig;
        private HostsConfig _hostConfigCache;

        public HostsResolverStrategy(
            ILogger<HostsResolverStrategy> logger,
            IMemoryCache memoryCache,
            IOptionsMonitor<HostsConfig> hostConfigOptionsMonitor,
            IDnsContextAccessor dnsContextAccessor) : base(logger, dnsContextAccessor)
        {
            CacheCancellationToken = new CancellationToken();
            _memoryCache = memoryCache;
            _hostConfigOptionsMonitor = hostConfigOptionsMonitor;
            ParseHostConfig(_hostConfigOptionsMonitor.CurrentValue);
            _parseHostConfig = _hostConfigOptionsMonitor.OnChange(ParseHostConfig);
            Order = 0;
        }

        internal static CancellationToken CacheCancellationToken { get; private set; }

        public override Task<List<DnsRecordBase>> ResolveAsync(DnsQuestion dnsQuestion, CancellationToken cancellationToken)
        {
            var result = new List<DnsRecordBase>();
            
                switch (dnsQuestion.RecordType)
                {
                    case RecordType.Ptr:
                        var recordsPtr = _memoryCache.Get<List<DnsRecordBase>>(dnsQuestion.Name.ToString());
                        if (recordsPtr != null && recordsPtr.Any()) result.AddRange(recordsPtr);
                        break;
                    case RecordType.A:
                    case RecordType.Aaaa:
                        var records = _memoryCache.Get<List<AddressRecordBase>>(dnsQuestion.Name.ToString());
                        if (records != null && records.Any()) result.AddRange(records);
                        break;
                }
                return Task.FromResult(result);
        }

        public override Models.Strategies GetStrategy()
        {
            return Models.Strategies.Hosts;
        }

        public override void OnRuleChanged()
        {
            throw new NotImplementedException();
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
            Logger.LogInformation("listner={listner}", listener);

            if (_hostConfigCache != null)
                foreach (var host in _hostConfigCache.Hosts)
                {
                    foreach (var ipAddress in host.IpAddresses) _memoryCache.Remove(ipAddress);
                    foreach (var domainName in host.DomainNames) _memoryCache.Remove(domainName);
                }

            _hostConfigCache = (HostsConfig) hostConfig.Clone();

            if (_hostConfigCache != null)
                foreach (var host in _hostConfigCache.Hosts)
                {
                    foreach (var ipAddress in host.IpAddresses)
                    {
                        var tempHost = host.ToPtrRecords(ipAddress);
                        var cacheoptions = new MemoryCacheEntryOptions();
                        cacheoptions.SetPriority(CacheItemPriority.NeverRemove);
                        _memoryCache.Set(ipAddress, tempHost, cacheoptions);
                    }

                    foreach (var domainName in host.DomainNames)
                    {
                        var tempHost = host.ToAddressRecord(domainName);
                        var cacheoptions = new MemoryCacheEntryOptions();
                        cacheoptions.SetPriority(CacheItemPriority.NeverRemove);
                        _memoryCache.Set(domainName, tempHost, cacheoptions);
                    }
                }
        }
    }
}