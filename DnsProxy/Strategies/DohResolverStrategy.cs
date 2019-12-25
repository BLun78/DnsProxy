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

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Common;
using Makaretu.Dns;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Strategies
{
    internal class DohResolverStrategy : BaseResolverStrategy, IDnsResolverStrategy
    {
        private readonly DohClient _dohClient;
        private readonly IMemoryCache _memoryCache;

        public DohResolverStrategy(
            ILogger<DnsResolverStrategy> logger,
            IMemoryCache memoryCache,
            IHttpClientFactory httpClientFactory) : base(logger)
        {
            _memoryCache = memoryCache;
            _dohClient = new DohClient();
            _dohClient.HttpClient = httpClientFactory.CreateClient(nameof(_dohClient));
            _dohClient.ServerUrl = "https://cloudflare-dns.com/dns-query";
            Order = 1000;
        }

        public override async Task<DnsMessage> ResolveAsync(DnsMessage dnsMessage,
            CancellationToken cancellationToken = default)
        {
            var resultMessage = dnsMessage.CreateResponseInstance();
            var requestMessage = new Message();

            foreach (var dnsQuestion in dnsMessage.Questions)
            {
                var question = new Question
                {
                    Name = dnsQuestion.Name.ToDomainName(),
                    Type = dnsQuestion.RecordType.ToDnsType(),
                    Class = dnsQuestion.RecordClass.ToDnsClass()
                };

                requestMessage.Questions.Add(question);
            }

            var responseMessage = await _dohClient.QueryAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            foreach (var answer in responseMessage.Answers)
            {
                var resultAnswer = answer.ToDnsRecord();
                resultMessage.AnswerRecords.Add(resultAnswer);
            }

            return resultMessage;
        }

        public override Models.Strategies GetStrategy()
        {
            return Models.Strategies.DoH;
        }
    }
}