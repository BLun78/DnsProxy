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
using DnsProxy.Models.Context;
using DnsProxy.Models.Rules;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Strategies
{
    internal class DnsResolverStrategy : BaseResolverStrategy<DnsRule>, IDnsResolverStrategy<DnsRule>
    {
       
        public DnsResolverStrategy(
            ILogger<DnsResolverStrategy> logger,
            IDnsContextAccessor dnsContextAccessor,
            IMemoryCache memoryCache) : base(logger, dnsContextAccessor, memoryCache)
        {
            Order = 2000;
        }

        public override async Task<List<DnsRecordBase>> ResolveAsync(DnsQuestion dnsQuestion,
            CancellationToken cancellationToken)
        {
            LogDnsQuestion(dnsQuestion);
            var result = new List<DnsRecordBase>();
            var dnsClient = new DnsClient(Rule.NameServerIpAddresses, Rule.QueryTimeout);

            var response = await dnsClient.ResolveAsync(dnsQuestion.Name, dnsQuestion.RecordType,
                        dnsQuestion.RecordClass, null, cancellationToken)
                    .ConfigureAwait(false);

            result.AddRange(response.AnswerRecords);

            if (result.Any())
            {
                StoreInCache(result, dnsQuestion.Name.ToString(),
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(new TimeSpan(0, 0, result.First().TimeToLive)));
            }

            LogDnsQuestionAndResult(dnsQuestion, result);
            return result;
        }

        public override Models.Strategies GetStrategy()
        {
            return Models.Strategies.Dns;
        }
    }
}