using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.EC2;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Models.Context;
using DnsProxy.Models.Rules;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Strategies
{
    internal class AwsVpcResolverStrategy : BaseResolverStrategy<AwsRule>, IDnsResolverStrategy<AwsRule>
    {
        public AwsVpcResolverStrategy(ILogger<BaseResolverStrategy<AwsRule>> logger,
            IDnsContextAccessor dnsContextAccessor,
            IMemoryCache memoryCache) : base(logger, dnsContextAccessor, memoryCache)
        {
            var client = new AmazonEC2Client();
            //client.Config.ProxyCredentials = null;
            //client.Config.ProxyHost = null;
            //client.Config.GetWebProxy()

            // var vpcend = client.DescribeVpcEndpointsAsync();
        }

        public override Task<List<DnsRecordBase>> ResolveAsync(DnsQuestion dnsQuestion,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Models.Strategies GetStrategy()
        {
            return Models.Strategies.Aws;
        }
    }
}