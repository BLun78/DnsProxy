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
using System.Net;
using DnsProxy.Common.Models.Rules;
using DnsProxy.Dns.Strategies;
using Rule = DnsProxy.Common.Models.Rules.Rule;

namespace DnsProxy.Dns.Models.Rules
{
    internal class DnsRule : RuleBase, IRule, IRuleStrategy
    {
        public DnsRule()
        {
        }

        public DnsRule(Rule rule) : base(rule)
        {
            CompressionMutation = rule.CompressionMutation;
            NameServerIpAddresses = GetNameServerIpAddresses(rule.NameServer);
        }

        public List<IPAddress> NameServerIpAddresses { get; set; }
        public bool CompressionMutation { get; set; }

        public override Type GetStrategy()
        {
            return typeof(DnsResolverStrategy);
        }
    }
}