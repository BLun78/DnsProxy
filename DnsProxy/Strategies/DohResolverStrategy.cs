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
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Common;
using DnsProxy.Models;
using DnsProxy.Models.Context;
using DnsProxy.Models.Rules;
using Makaretu.Dns;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DnsProxy.Strategies
{
    internal class DohResolverStrategy : BaseResolverStrategy<DohRule>, IDnsResolverStrategy<DohRule>
    {
        private readonly IServiceProvider _serviceProvider;
        private DohClient _dohClient;

        public DohResolverStrategy(
            ILogger<DohResolverStrategy> logger,
            IMemoryCache memoryCache,
            IServiceProvider serviceProvider,
            IDnsContextAccessor dnsContextAccessor)
            : base(logger, dnsContextAccessor, memoryCache)
        {
            _serviceProvider = serviceProvider;
            Order = 1000;
        }

        public override async Task<List<DnsRecordBase>> ResolveAsync(DnsQuestion dnsQuestion,
            CancellationToken cancellationToken)
        {
            LogDnsQuestion(dnsQuestion);
            var result = new List<DnsRecordBase>();
            var requestMessage = new Message();

            _dohClient?.Dispose();
            _dohClient = _serviceProvider.GetService<DohClient>();


            _dohClient.ServerUrl = Rule.NameServerUri.First()?.AbsoluteUri;

            var question = new Question
            {
                Name = dnsQuestion.Name.ToDomainName(),
                Type = dnsQuestion.RecordType.ToDnsType(),
                Class = dnsQuestion.RecordClass.ToDnsClass()
            };

            requestMessage.Questions.Add(question);

            var responseMessage = await _dohClient.QueryAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            foreach (var answer in responseMessage.Answers)
            {
                var resultAnswer = answer.ToDnsRecord();
                result.Add(resultAnswer);
            }

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
            return Models.Strategies.DoH;
        }
    }
}