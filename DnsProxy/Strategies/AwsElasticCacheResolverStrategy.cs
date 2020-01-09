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
using System.Threading;
using System.Threading.Tasks;
using Amazon.ElastiCache;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Models.Aws;
using DnsProxy.Models.Context;
using DnsProxy.Models.Rules;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Strategies
{
    [Obsolete]
    internal class AwsElasticCacheResolverStrategy :
        AwsBaseResolverStrategy<AwsElasticCacheRule, AmazonElastiCacheClient>, IDnsResolverStrategy<AwsElasticCacheRule>
    {
        public AwsElasticCacheResolverStrategy(ILogger<AwsElasticCacheResolverStrategy> logger,
            IDnsContextAccessor dnsContextAccessor,
            IMemoryCache memoryCache,
            AwsContext awsContext,
            AmazonElastiCacheConfig amazonElastiCacheConfig,
            IServiceProvider serviceProvider) : base(logger, dnsContextAccessor, memoryCache, awsContext,
            amazonElastiCacheConfig, serviceProvider)
        {
        }

        public override Models.Strategies GetStrategy()
        {
            return Models.Strategies.AwsElasticCache;
        }

        public override Task<List<DnsRecordBase>> AwsResolveAsync(DnsQuestion dnsQuestion, List<string> ScanVpcIds,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}