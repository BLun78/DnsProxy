using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Common;
using DnsProxy.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tmds.Linux;

namespace DnsProxy.Dns.Strategies
{
    internal class HostsResolverStrategy : BaseResolverStrategy, IDnsResolverStrategy
    {
        private readonly ILogger<DnsResolverStrategy> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IOptionsMonitor<HostConfig> _hostConfigOptionsMonitor;
        private HostConfig _hostConfigCache;
        private readonly IDnsResolver DnsClient;
        internal static CancellationToken CacheCancellationToken { get; private set; }

        public HostsResolverStrategy(ILogger<DnsResolverStrategy> logger,
            IMemoryCache memoryCache,
            IOptionsMonitor<HostConfig> hostConfigOptionsMonitor) : base(logger)
        {
            CacheCancellationToken = new CancellationToken();
            _logger = logger;
            _memoryCache = memoryCache;
            _hostConfigOptionsMonitor = hostConfigOptionsMonitor;
            ParseHostConfig(_hostConfigOptionsMonitor.CurrentValue);
            _hostConfigOptionsMonitor.OnChange(ParseHostConfig);
            this.Order = 0;
        }

        private void ParseHostConfig(HostConfig hostConfig, string listener = "")
        {
            _logger.LogInformation("listner={listner}", listener);

            if (_hostConfigCache != null)
            {
                foreach (var host in _hostConfigCache.Hosts)
                {
                    foreach (var ipAddress in host.IPAddresses)
                    {
                        _memoryCache.Remove(ipAddress);
                    }
                    foreach (var domainName in host.DomainNames)
                    {
                        _memoryCache.Remove(domainName);
                    }
                }
            }

            _hostConfigCache = (HostConfig)hostConfig.Clone();

            if (_hostConfigCache != null)
            {
                foreach (var host in _hostConfigCache.Hosts)
                {
                    foreach (var ipAddress in host.IPAddresses)
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

        public async Task<DnsMessage> ResolveAsync(DnsMessage dnsMessage, CancellationToken cancellationToken = default)
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

            return message;
        }

    }
}