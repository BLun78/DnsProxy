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
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DnsProxy.Strategies
{
    internal class StrategyManager
    {
        private readonly ILogger<StrategyManager> _logger;
        private readonly IOptionsMonitor<RulesConfig> _rulesConfigOptionsMonitor;

        public StrategyManager(ILogger<StrategyManager> logger,
            IOptionsMonitor<RulesConfig> rulesConfigOptionsMonitor)
        {
            _logger = logger;
            _rulesConfigOptionsMonitor = rulesConfigOptionsMonitor;
        }

        public Task<DnsMessage> ResolveAsync(DnsMessage dnsMessage, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}