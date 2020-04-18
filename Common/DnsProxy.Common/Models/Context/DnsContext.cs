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
using DnsProxy.Plugin.Models.Rules;
using DnsProxy.Plugin.Strategies;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DnsProxy.Common.Models.Context
{
    internal class DnsContext : IWriteDnsContext, IDnsCtx, IDisposable
    {
        private ILogger<IDnsCtx> _logger;
        private IDisposable _loggerScope;

        public void Dispose()
        {
            DefaultDnsStrategy?.Dispose();
            DefaultDnsStrategy = null;
            DnsResolverStrategies?.ForEach(x => x?.Dispose());
            DnsResolverStrategies?.Clear();
            DnsResolverStrategies = null;
            _loggerScope?.Dispose();
            Rules = null;
            GC.SuppressFinalize(this);
        }

        public List<IRule> Rules { get; set; }
        public DnsMessage Request { get; set; }
        public DnsMessage Response { get; set; }
        public IDnsResolverStrategy DefaultDnsStrategy { get; set; }
        public IDnsResolverStrategy CacheResolverStrategy { get; set; }
        public List<IDnsResolverStrategy> DnsResolverStrategies { get; set; }
        public CancellationToken RootCancellationToken { get; set; }
        public string IpEndPoint { get; set; }

        public ILogger<IDnsCtx> Logger
        {
            get => _logger;
            set
            {
                _logger = value;

                if (!string.IsNullOrWhiteSpace(Request?.TransactionID.ToString()))
                {
                    _loggerScope = _logger.BeginScope(Request?.TransactionID.ToString());
                }
            }
        }
    }
}