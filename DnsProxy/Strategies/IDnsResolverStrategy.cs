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
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Common;
using DnsProxy.Models.Rules;

namespace DnsProxy.Strategies
{
    internal interface IDnsResolverStrategy : IDisposable, IOrder
    {
        IRule Rule { get; }
        Task<List<DnsRecordBase>> ResolveAsync(DnsQuestion dnsQuestion, CancellationToken cancellationToken);
        Models.Strategies GetStrategy();
        void OnRuleChanged();
        void SetRule(IRule rule);
        bool MatchPattern(DnsQuestion dnsQuestion);
    }

    internal interface IDnsResolverStrategy<TRule> : IDnsResolverStrategy
        where TRule : IRule
    {
        TRule Rule { get; }
        void SetRule(TRule rule);
    }
}