﻿#region Apache License-2.0
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

using ARSoft.Tools.Net.Dns;
using DnsProxy.Common;
using DnsProxy.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DnsProxy.Dns.Strategies
{
    internal class HostsResolverStrategy : BaseResolverStrategy, IDnsResolverStrategy
    {
        private readonly ILogger<DnsResolverStrategy> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IOptionsMonitor<HostsConfig> _hostConfigOptionsMonitor;
        private HostsConfig _hostConfigCache;

        internal static CancellationToken CacheCancellationToken { get; private set; }

        public HostsResolverStrategy(ILogger<DnsResolverStrategy> logger,
            IMemoryCache memoryCache,
            IOptionsMonitor<HostsConfig> hostConfigOptionsMonitor) : base(logger)
        {
            CacheCancellationToken = new CancellationToken();
            _logger = logger;
            _memoryCache = memoryCache;
            _hostConfigOptionsMonitor = hostConfigOptionsMonitor;
            ParseHostConfig(_hostConfigOptionsMonitor.CurrentValue);
            _hostConfigOptionsMonitor.OnChange(ParseHostConfig);
            this.Order = 0;
        }

        private void ParseHostConfig(HostsConfig hostConfig, string listener = "")
        {
            _logger.LogInformation("listner={listner}", listener);

            if (_hostConfigCache != null)
            {
                foreach (var host in _hostConfigCache.Hosts)
                {
                    foreach (var ipAddress in host.IpAddresses)
                    {
                        _memoryCache.Remove(ipAddress);
                    }
                    foreach (var domainName in host.DomainNames)
                    {
                        _memoryCache.Remove(domainName);
                    }
                }
            }

            _hostConfigCache = (HostsConfig)hostConfig.Clone();

            if (_hostConfigCache != null)
            {
                foreach (var host in _hostConfigCache.Hosts)
                {
                    foreach (var ipAddress in host.IpAddresses)
                    {
                        var tempHost = host.ToPtrRecords(ipAddress);
                        var entry = _memoryCache.CreateEntry(tempHost.Item1);
                        entry.SetAbsoluteExpiration(TimeSpan.MaxValue);
                        entry.SetValue(tempHost.Item2);
                        entry.SetPriority(CacheItemPriority.NeverRemove);
                    }
                    foreach (var domainName in host.DomainNames)
                    {
                        var tempHost = host.ToAddressRecord(domainName);
                        var entry = _memoryCache.CreateEntry(domainName);
                        entry.SetAbsoluteExpiration(TimeSpan.MaxValue);
                        entry.SetValue(tempHost);
                        entry.SetPriority(CacheItemPriority.NeverRemove);
                    }
                }
            }
        }

        public Task<DnsMessage> ResolveAsync(DnsMessage dnsMessage, CancellationToken cancellationToken = default)
        {
            var message = dnsMessage.CreateResponseInstance();

            foreach (DnsQuestion dnsQuestion in dnsMessage.Questions)
            {
                switch (dnsQuestion.RecordType)
                {
                    case RecordType.Ptr:
                    case RecordType.A:
                    case RecordType.Aaaa:
                        var records = _memoryCache.Get<List<DnsRecordBase>>(dnsQuestion.RecordType);
                        if (records != null && records.Any())
                        {
                            message.AnswerRecords.AddRange(records);
                        }
                        break;
                }
            }

            return Task.FromResult(message);
        }

    }
}