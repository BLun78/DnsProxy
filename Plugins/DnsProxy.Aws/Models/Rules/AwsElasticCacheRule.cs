﻿#region Apache License-2.0
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

using DnsProxy.Aws.Strategies;
using DnsProxy.Common.Models.Rules;
using DnsProxy.Plugin.Models.Rules;
using System;

namespace DnsProxy.Aws.Models.Rules
{
    public class AwsElasticCacheRule : RuleBase, IRule, IRuleStrategy
    {
        public AwsElasticCacheRule()
        {
        }

        public AwsElasticCacheRule(Rule rule) : base(rule)
        {
        }

        public override Type GetStrategy()
        {
            return typeof(AwsElasticCacheResolverStrategy);
        }
    }
}