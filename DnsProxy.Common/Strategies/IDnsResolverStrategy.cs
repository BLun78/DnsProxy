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

using ARSoft.Tools.Net.Dns;
using DnsProxy.Common.Models.Rules;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DnsProxy.Plugin;

namespace DnsProxy.Common.Strategies
{
    public interface IDnsResolverStrategy : IDisposable, IOrder
    {
        string StrategyName { get; }
        bool NeedsQueryTimeout { get; }
        IRule Rule { get; }
        Task<List<DnsRecordBase>> ResolveAsync(DnsQuestion dnsQuestion, CancellationToken cancellationToken);
        void SetRule(IRule rule);
        bool MatchPattern(DnsQuestion dnsQuestion);
    }

    public interface IDnsResolverStrategy<TRule> : IDnsResolverStrategy
        where TRule : IRule
    {
        new TRule Rule { get; }
        void SetRule(TRule rule);
    }
}