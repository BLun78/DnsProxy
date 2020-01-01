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
using DnsProxy.Models.Context;
using DnsProxy.Models.Rules;
using Makaretu.Dns.Resolving;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DnsProxy.Strategies
{
    internal class InternalNameServerResolverStrategy : BaseResolverStrategy<InternalNameServerRule>,
        IDnsResolverStrategy<InternalNameServerRule>
    {
        private readonly IOptionsMonitor<NameServerOptions> _nameServerOptions;
        private readonly IDisposable _nameServerOptionsListener;
        private NameServer _resolver;

        public InternalNameServerResolverStrategy(
            ILogger<InternalNameServerResolverStrategy> logger,
            IOptionsMonitor<NameServerOptions> nameServerOptions,
            IDnsContextAccessor dnsContextAccessor,
            IMemoryCache memoryCache) : base(logger, dnsContextAccessor, memoryCache)
        {
            _nameServerOptions = nameServerOptions;
            _nameServerOptionsListener = _nameServerOptions.OnChange(NameServerOptionsListener);

            var catalog = new Catalog();
            catalog.IncludeRootHints();
            _resolver = new NameServer {Catalog = catalog};
            Order = 100;
        }

        public override Task<List<DnsRecordBase>> ResolveAsync(DnsQuestion dnsQuestion,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Models.Strategies GetStrategy()
        {
            return Models.Strategies.InternalNameServer;
        }

        public override void OnRuleChanged()
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
                if (disposing)
                    _nameServerOptionsListener?.Dispose();
            base.Dispose(disposing);
        }

        private void NameServerOptionsListener(NameServerOptions nameServerOptions, string arg2)
        {
            throw new NotImplementedException();
        }
    }

    internal class NameServerOptions
    {
    }
}