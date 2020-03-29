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

using Amazon.DocDB;
using Amazon.DocDB.Model;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Aws.Models;
using DnsProxy.Aws.Models.Rules;
using DnsProxy.Common.Models.Context;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DnsProxy.Plugin.Models.Dns;
using DnsProxy.Plugin.Strategies;

namespace DnsProxy.Aws.Strategies
{
    [Obsolete]
    internal class AwsDocDbResolverStrategy : AwsBaseResolverStrategy<AwsDocDbRule, AmazonDocDBClient>,
        IDnsResolverStrategy<AwsDocDbRule>
    {
        public AwsDocDbResolverStrategy(
            IDnsContextAccessor dnsContextAccessor,
            IMemoryCache memoryCache,
            AwsContext awsContext,
            AmazonDocDBConfig amazonDocDbConfig,
            IServiceProvider serviceProvider) : base(dnsContextAccessor, memoryCache, awsContext,
            amazonDocDbConfig, serviceProvider)
        {
            StrategyName = "AwsDocDb";
        }

        public override async Task<List<IDnsRecordBase>> AwsResolveAsync(IDnsQuestion dnsQuestion,
            List<string> ScanVpcIds, CancellationToken cancellationToken)
        {
            var logger = DnsContextAccessor.DnsContext.Logger;
            using (logger.BeginScope($"{StrategyName} =>"))
            {
                var clusters = await AwsClient
                    .DescribeDBClustersAsync(new DescribeDBClustersRequest(), cancellationToken)
                    .ConfigureAwait(false);


                return null;
            }
        }
    }
}