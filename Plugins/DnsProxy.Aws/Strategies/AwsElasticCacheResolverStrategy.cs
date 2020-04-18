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

using Amazon.ElastiCache;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Aws.Models;
using DnsProxy.Aws.Models.Rules;
using DnsProxy.Common.Models.Context;
using DnsProxy.Plugin.Strategies;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DnsProxy.Common.Cache;

namespace DnsProxy.Aws.Strategies
{
    [Obsolete]
    internal class AwsElasticCacheResolverStrategy :
        AwsBaseResolverStrategy<AwsElasticCacheRule, AmazonElastiCacheClient>, IDnsResolverStrategy<AwsElasticCacheRule>
    {
        public AwsElasticCacheResolverStrategy(
            IDnsContextAccessor dnsContextAccessor,
            CacheManager cacheManager,
            AwsContext awsContext,
            AmazonElastiCacheConfig amazonElastiCacheConfig,
            IServiceProvider serviceProvider) 
            : base(dnsContextAccessor, cacheManager, awsContext,
                amazonElastiCacheConfig, serviceProvider)
        {
            StrategyName = "AwsElasticCache";
        }

        public override Task<List<DnsRecordBase>> AwsResolveAsync(DnsQuestion dnsQuestion, List<string> ScanVpcIds,
            CancellationToken cancellationToken)
        {
            var logger = DnsContextAccessor.DnsContext.Logger;
            using (logger.BeginScope($"{StrategyName} =>"))
            {
                throw new NotImplementedException();
            }
        }
    }
}