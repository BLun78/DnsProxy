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
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Aws.Models;
using DnsProxy.Aws.Models.Rules;
using DnsProxy.Common.Models.Context;
using DnsProxy.Common.Strategies;
using Microsoft.Extensions.Caching.Memory;

namespace DnsProxy.Aws.Strategies
{
    [Obsolete]
    internal class AwsApiGatewayResolverStrategy : AwsBaseResolverStrategy<AwsApiGatewayRule, AmazonAPIGatewayClient>,
        IDnsResolverStrategy<AwsApiGatewayRule>
    {
        public AwsApiGatewayResolverStrategy(
            IDnsContextAccessor dnsContextAccessor,
            IMemoryCache memoryCache,
            AwsContext awsContext,
            AmazonAPIGatewayConfig amazonApiGatewayConfig,
            IServiceProvider serviceProvider) : base(dnsContextAccessor, memoryCache, awsContext,
            amazonApiGatewayConfig, serviceProvider)
        {
            StrategyName = "AwsApiGateway";
        }

        public override async Task<List<DnsRecordBase>> AwsResolveAsync(DnsQuestion dnsQuestion,
            List<string> ScanVpcIds, CancellationToken cancellationToken)
        {
            var vpc = await AwsClient.GetRestApisAsync(new GetRestApisRequest(), cancellationToken)
                .ConfigureAwait(false);

            return null;
        }
    }
}