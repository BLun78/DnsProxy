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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Common;
using DnsProxy.Models.Rules;
using Makaretu.Dns;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Strategies
{
    internal class MulticastResolverStrategy : BaseResolverStrategy<MulticastRule>, IDnsResolverStrategy<MulticastRule>
    {
        public MulticastResolverStrategy(ILogger<MulticastResolverStrategy> logger) : base(logger)
        {
            Order = 5000;
        }

        public override async Task<List<DnsRecordBase>> ResolveAsync(DnsQuestion dnsQuestion, CancellationToken cancellationToken)
        {
            var result = new List<DnsRecordBase>();

            var query = new Message();
            query.Questions.Add(new Question
            {
                Name = dnsQuestion.Name.ToString(),
                Type = DnsType.ANY
            });

            using (var mdns = new MulticastService())
            {
                mdns.Start();
                var response = await mdns.ResolveAsync(query, cancellationToken).ConfigureAwait(false);

                foreach (var answer in response.Answers) result.Add(answer.ToDnsRecord());
            }


            return result;
        }

        public override Models.Strategies GetStrategy()
        {
            return Models.Strategies.Multicast;
        }

        public override void OnRuleChanged()
        {
            throw new NotImplementedException();
        }
    }
}