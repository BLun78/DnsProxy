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
using DnsProxy.Common.Strategies;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using DnsProxy.Plugin.Models.Rules;
using DnsProxy.Plugin.Strategies;

namespace DnsProxy.Common.Models.Context
{
    public interface IWriteDnsContext : IDnsCtx, IDisposable
    {
        new ILogger<IDnsCtx> Logger { get; set; }
        new List<IRule> Rules { get; set; }
        new DnsMessage Request { get; set; }
        new DnsMessage Response { get; set; }
        new IDnsResolverStrategy DefaultDnsStrategy { get; set; }
        new IDnsResolverStrategy CacheResolverStrategy { get; set; }
        new List<IDnsResolverStrategy> DnsResolverStrategies { get; set; }
        new CancellationToken RootCancellationToken { get; set; }
        new string IpEndPoint { get; set; }
    }
}