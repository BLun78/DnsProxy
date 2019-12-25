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

using ARSoft.Tools.Net.Dns;
using Makaretu.Dns.Resolving;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DnsProxy.Dns.Strategies
{
    internal class InternalNameServerResolverStrategy : BaseResolverStrategy, IDnsResolverStrategy
    {
        private readonly IOptionsMonitor<NameServerOptions> _nameServerOptions;
        private NameServer _resolver;

        public InternalNameServerResolverStrategy(ILogger<DnsResolverStrategy> logger,
            IOptionsMonitor<NameServerOptions> nameServerOptions) : base(logger)
        {
            _nameServerOptions = nameServerOptions;
            _nameServerOptions.OnChange(OptionsListener);
            var catalog = new Catalog();

            catalog.IncludeRootHints();
            _resolver = new NameServer { Catalog = catalog };
            Order = 100;
        }

        private void OptionsListener(NameServerOptions nameServerOptions, string arg2)
        {
            throw new NotImplementedException();
        }

        public Task<DnsMessage> ResolveAsync(DnsMessage dnsMessage, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    internal class NameServerOptions
    {

    }
}