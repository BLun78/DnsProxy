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

using DnsProxy.Plugin.Models.Rules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DnsProxy.Common.Models.Rules
{
    public class RuleFactory : IRuleFactory
    {
        private readonly IEnumerable<Type> _rules;

        public RuleFactory(IEnumerable<Type> rules)
        {
            _rules = rules;
        }

        public IRule Create(string ruleName, IRule rule)
        {
            foreach (var ruleType in _rules.Where(x => typeof(IRule).IsAssignableFrom(x)))
            {
                if ($"{ruleName}Rule".Equals(ruleType.Name, StringComparison.InvariantCultureIgnoreCase)
                    || ruleType.Name.Equals(ruleName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return (IRule)Activator.CreateInstance(ruleType, (Rule)rule);
                }
            }
            return default(IRule);
        }
    }
}
