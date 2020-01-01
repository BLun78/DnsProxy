using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.EC2;
using Amazon.EC2.Model;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Models.Aws;
using DnsProxy.Models.Context;
using DnsProxy.Models.Rules;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Strategies
{
    internal class AwsEc2ResolverStrategy : BaseResolverStrategy<AwsRule>, IDnsResolverStrategy<AwsRule>
    {
        public AwsEc2ResolverStrategy(ILogger<BaseResolverStrategy<AwsRule>> logger,
            IDnsContextAccessor dnsContextAccessor,
            IMemoryCache memoryCache,
            AwsContext awsContext,
            AmazonEC2Config amazonEc2Config) : base(logger, dnsContextAccessor, memoryCache)
        {
            ReadEc2Async(awsContext, amazonEc2Config);
        }

        private async Task ReadEc2Async(AwsContext awsContext, AmazonEC2Config amazonEc2Config)
        {
            AmazonEC2Client amazonEc2Client = default;
            foreach (var awsSettingsUserAccount in awsContext.AwsSettings.UserAccounts)
            {
                foreach (var userRoleExtended in awsSettingsUserAccount.Roles)
                {
                    amazonEc2Client?.Dispose();
                    amazonEc2Client = new AmazonEC2Client(userRoleExtended.AwsCredentials, amazonEc2Config);
                    var vpcend = await amazonEc2Client.DescribeVpcEndpointsAsync(new DescribeVpcEndpointsRequest()).ConfigureAwait(true);
                    var sqs = vpcend.VpcEndpoints.Where(x => x.ServiceName.Contains(".sqs")).ToList();
                    var secretsmanager = vpcend.VpcEndpoints.Where(x => x.ServiceName.Contains(".secretsmanager")).ToList();

                    var b = 2;
                }
            }
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

        public override bool MatchPattern(DnsQuestion dnsQuestion)
        {
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
                if (disposing)
                     
            base.Dispose(disposing);
        }
    }
}