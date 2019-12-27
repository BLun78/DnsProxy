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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Models.Rules;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Strategies
{
    internal class DnsResolverStrategy : BaseResolverStrategy<DnsRule>, IDnsResolverStrategy<DnsRule>
    {
        private readonly List<DnsClient> DnsClient;

        public DnsResolverStrategy(ILogger<DnsResolverStrategy> logger) : base(logger)
        {
            Order = 2000;
            DnsClient = new List<DnsClient>();
        }

        public override async Task<DnsMessage> ResolveAsync(DnsMessage dnsMessage, CancellationToken cancellationToken)
        {
            var options = new DnsQueryOptions();
            options = null;

            foreach (var dnsQuestion in dnsMessage.Questions)
            {
                foreach (var dnsClient in DnsClient)
                {
                    var response = await dnsClient.ResolveAsync(dnsQuestion.Name, dnsQuestion.RecordType,
                            dnsQuestion.RecordClass, options, CreateCancellationToken(cancellationToken))
                        .ConfigureAwait(false);
                    dnsMessage.AnswerRecords.AddRange(response.AnswerRecords);
                }
            }

            DisposeCancellationToken();
            return dnsMessage;
        }

        public override Models.Strategies GetStrategy()
        {
            return Models.Strategies.Dns;
        }

        public override void OnRuleChanged()
        {
            DnsClient.Clear();
            foreach (var ipAddress in Rule.NameServerIpAddresses)
            {
                var dns = new DnsClient(ipAddress, Rule.QueryTimeout);
                DnsClient.Add(dns);
            }
        }
    }
}