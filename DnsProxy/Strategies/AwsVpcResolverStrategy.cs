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
        private readonly AmazonEC2Client _amazonEc2Client;

        public AwsVpcResolverStrategy(ILogger<BaseResolverStrategy<AwsRule>> logger,
            IDnsContextAccessor dnsContextAccessor,
            IMemoryCache memoryCache,
            AmazonEC2Config amazonEc2Config) : base(logger, dnsContextAccessor, memoryCache)
        { 
            _amazonEc2Client = new AmazonEC2Client(amazonEc2Config);
            
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
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
                if (disposing)
                    _amazonEc2Client?.Dispose();
            base.Dispose(disposing);
        }
    }
}