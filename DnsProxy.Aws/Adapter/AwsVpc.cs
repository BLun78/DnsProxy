using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Runtime;
using DnsProxy.Aws.Models;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Aws.Adapter
{
    internal class AwsVpc : AwsBase
    {
        private AmazonEC2Config AmazonEc2Config => AmazonClientConfig as AmazonEC2Config;

        public AwsVpc(
            ILogger<AwsAdapterBase> logger,
            AwsContext awsContext,
            AmazonEC2Config amazonEc2Config)
            : base(logger, awsContext, amazonEc2Config)
        {
        }

        public async Task<List<Endpoint>> ReadEndpoints(
            AWSCredentials awsCredentials,
            IAwsScanRules awsDoScan,
            CancellationToken cancellationToken)
        {
            using (var amazonEc2Client = new AmazonEC2Client(awsCredentials, AmazonEc2Config))
            {
                var vpc = await amazonEc2Client.DescribeVpcsAsync(CreateDescribeVpcsRequest(awsDoScan), cancellationToken)
                    .ConfigureAwait(false);
                var vpcIds = vpc.Vpcs.Select(x => x.VpcId).ToList();
                var vpcEndpointList = await amazonEc2Client
                    .DescribeVpcEndpointsAsync(new DescribeVpcEndpointsRequest(), cancellationToken)
                    .ConfigureAwait(false);
                var result = vpcEndpointList.VpcEndpoints.Where(x => vpcIds.Contains(x.VpcId)).Select(x => new Endpoint(x))
                    .ToList();
                foreach (var endpoint in result)
                {
                    var describeNetworkInterfacesResponse = await amazonEc2Client.DescribeNetworkInterfacesAsync(
                        new DescribeNetworkInterfacesRequest
                        {
                            NetworkInterfaceIds = endpoint.VpcEndpoint.NetworkInterfaceIds
                        }, 
                        cancellationToken).ConfigureAwait(false);
                    endpoint.NetworkInterfaces = describeNetworkInterfacesResponse.NetworkInterfaces;
                }

                return result;
            }
        }
        
        private DescribeVpcsRequest CreateDescribeVpcsRequest(IAwsScanRules awsDoScan)
        {
            if (awsDoScan.ScanVpcIds != null && awsDoScan.ScanVpcIds.Any())
            {
                var vpcIds = string.Join(", ", awsDoScan.ScanVpcIds);
                Logger.LogInformation("Read AWS VPC: [{0}]", vpcIds);
                return new DescribeVpcsRequest
                {
                    VpcIds = awsDoScan.ScanVpcIds
                };
            }

            Logger.LogInformation("Read AWS VPC: [no definitions, read all!]");
            return new DescribeVpcsRequest();
        }
    }
}
