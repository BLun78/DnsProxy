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

using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Runtime;
using DnsProxy.Aws.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DnsProxy.Aws.Adapter
{
    internal class AwsVpcReader : AwsBase
    {
        private AmazonEC2Config AmazonEc2Config => AmazonClientConfig as AmazonEC2Config;

        public AwsVpcReader(
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
                var vpc = await amazonEc2Client.DescribeVpcsAsync(CreateDescribeVpcsRequest(awsDoScan),
                        cancellationToken)
                    .ConfigureAwait(false);
                var vpcIds = vpc.Vpcs.Select(x => x.VpcId).ToList();
                var vpcEndpointList = await amazonEc2Client
                    .DescribeVpcEndpointsAsync(new DescribeVpcEndpointsRequest(), cancellationToken)
                    .ConfigureAwait(false);
                var result = vpcEndpointList.VpcEndpoints
                    .Where(x => vpcIds.Contains(x.VpcId))
                    .Select(x => new Endpoint(x))
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
