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
using ARSoft.Tools.Net.Dns;
using DnsProxy.Common.Models;
using DnsProxy.Common.Models.Context;
using DnsProxy.Common.Strategies;
using DnsProxy.Dns.Models.Rules;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DnsProxy.Dns.Strategies
{
    internal class DnsResolverStrategy : BaseResolverStrategy<DnsRule>, IDnsResolverStrategy<DnsRule>
    {
        public DnsResolverStrategy(
            IDnsContextAccessor dnsContextAccessor,
            IMemoryCache memoryCache,
            IOptionsMonitor<CacheConfig> cacheConfigOptionsMonitor)
            : base(dnsContextAccessor, memoryCache, cacheConfigOptionsMonitor)
        {
            Order = 2000;
        }

        public override async Task<List<DnsRecordBase>> ResolveAsync(DnsQuestion dnsQuestion,
            CancellationToken cancellationToken)
        {
            var logger = DnsContextAccessor.DnsContext.Logger;
            using (logger.BeginScope("DNS =>"))
            {
                var stopwatch = new Stopwatch();
                LogDnsQuestion(dnsQuestion, stopwatch);
                var result = new List<DnsRecordBase>();

                try
                {
                    var dnsClient = new DnsClient(Rule.NameServerIpAddresses, Rule.QueryTimeout);

                    var response = await dnsClient.ResolveAsync(dnsQuestion.Name, dnsQuestion.RecordType,
                            dnsQuestion.RecordClass, null, cancellationToken)
                        .ConfigureAwait(false);

                    result.AddRange(response?.AnswerRecords ?? new List<DnsRecordBase>());
                }
                catch (OperationCanceledException operationCanceledException)
                {
                    LogDnsCanncelQuestion(dnsQuestion, operationCanceledException, stopwatch);
                }
                catch (Exception e)
                {
                    DnsContextAccessor.DnsContext.Logger.LogError(e, e.Message);
                    throw;
                }

                if (result.Any())
                {
                    var ttl = result.First().TimeToLive;
                    if (ttl <= CacheConfigOptionsMonitor.CurrentValue.MinimalTimeToLiveInSeconds)
                    {
                        ttl = CacheConfigOptionsMonitor.CurrentValue.MinimalTimeToLiveInSeconds;
                    }

                    StoreInCache(dnsQuestion, result, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(ttl)));
                }

                LogDnsQuestionAndResult(dnsQuestion, result, stopwatch);
                return result;
            }
        }

        public override Common.Models.Strategies GetStrategy()
        {
            return Common.Models.Strategies.Dns;
        }
    }
}