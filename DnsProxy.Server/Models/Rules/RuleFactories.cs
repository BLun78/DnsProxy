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
using DnsProxy.Plugin.Models.Rules;

namespace DnsProxy.Server.Models.Rules
{
    public class RuleFactories : IRuleFactories
    {
        public RuleFactories(List<IRuleFactory> ruleFactories)
        {
            Factories = ruleFactories;
        }
        public List<IRuleFactory> Factories { get; }
    }
}