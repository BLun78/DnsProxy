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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Runtime;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Models.Aws;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Common.Aws
{
    internal class AwsVpcManager
    {
        private readonly ILogger<AwsVpcManager> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly AwsContext _awsContext;
        private readonly AmazonEC2Config _amazonEc2Config;
        private readonly AmazonAPIGatewayConfig _amazonApiGatewayConfig;

        public AwsVpcManager(ILogger<AwsVpcManager> logger,
            IMemoryCache memoryCache,
            AwsContext awsContext,
            AmazonEC2Config amazonEc2Config,
            AmazonAPIGatewayConfig amazonApiGatewayConfig)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _awsContext = awsContext;
            _amazonEc2Config = amazonEc2Config;
            _amazonApiGatewayConfig = amazonApiGatewayConfig;
        }

        public async Task StartReadingVpcAsync(CancellationToken cancellationToken)
        {
            foreach (var awsSettingsUserAccount in _awsContext.AwsSettings.UserAccounts)
            {
                if (awsSettingsUserAccount.DoScan)
                {
                    await ReadVpc(awsSettingsUserAccount.AwsCredentials, awsSettingsUserAccount, cancellationToken).ConfigureAwait(true);
                }
                foreach (var userRoleExtended in awsSettingsUserAccount.Roles)
                {
                    if (userRoleExtended.DoScan)
                    {
                        await ReadVpc(userRoleExtended.AwsCredentials, userRoleExtended, cancellationToken).ConfigureAwait(true);
                    }
                }
            }
        }

        private async Task ReadVpc(AWSCredentials awsCredentials, IAwsDoScan awsDoScan, CancellationToken cancellationToken)
        {
            try
            {
                using (var amazonEc2Client = new AmazonEC2Client(awsCredentials, _amazonEc2Config))
                {
                    var vpcendpoints = await ReadVpcEndpoint(amazonEc2Client, awsDoScan, cancellationToken).ConfigureAwait(true);
                    var r = await ReadApiGateway(awsCredentials, vpcendpoints, awsDoScan, cancellationToken).ConfigureAwait(true);
                }
            }
            catch (AmazonEC2Exception aee)
            {
                if (aee.ErrorCode == "UnauthorizedOperation")
                {
                    _logger.LogError(aee, "AWS ErrorCode=[{0}] ==> {1}", aee.ErrorCode, aee.Message);
                }
                else
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, e.Message);
                throw;
            }
        }

        //private async Task<> ReadddVpc(AmazonEC2Config amazonEc2Config, AWSCredentials awsCredentials, IAwsDoScan awsDoScan, CancellationToken cancellationToken)
        //{

        //    var amazonEc2Client = new AmazonEC2Client(awsCredentials, amazonEc2Config);

        //    var vpc = await amazonEc2Client.DescribeVpcsAsync(CreateDescribeVpcsRequest(awsDoScan), cancellationToken).ConfigureAwait(true);

        //    // get endpoints
        //    var vpcend = await amazonEc2Client.DescribeVpcEndpointsAsync(new DescribeVpcEndpointsRequest(), CancellationToken.None)
        //        .ConfigureAwait(true);
        //    var sqs = vpcend.VpcEndpoints.Where(x => x.ServiceName.Contains(".sqs")).ToList();
        //    var secretsmanager = vpcend.VpcEndpoints.Where(x => x.ServiceName.Contains(".secretsmanager")).ToList();
        //    var executeApi =
        //        vpcend.VpcEndpoints.Where(x => x.ServiceName.Contains(".execute-api")).ToList(); // Api-Gateway

        //    var net = await amazonEc2Client.DescribeNetworkInterfacesAsync(new DescribeNetworkInterfacesRequest()
        //    {
        //        NetworkInterfaceIds = sqs.SelectMany(x => x.NetworkInterfaceIds).ToList()
        //    }).ConfigureAwait(false);

        //    // TODO: Read 
        //    // get Network Interfaces

        //    // get Network Interface Ip Adresses
        //    // (Get-EC2NetworkInterface -NetworkInterfaceId $sqsNI).PrivateIpAddress

        //    // get api gateway
        //    // get-AGRestAPIList
        //    // $executeapiNIIP + " " + $entry.Id + ".execute-api.eu-central-1.amazonaws.com" 

        //}

        private async Task<List<DnsRecordBase>> ReadApiGateway(AWSCredentials awsCredentials, List<VpcEndpoint> vpcEndpoints, IAwsDoScan awsDoScan, CancellationToken cancellationToken)
        {
            using (var amazonApiGatewayClient = new AmazonAPIGatewayClient(awsCredentials, _amazonApiGatewayConfig))
            {
                var apis = await amazonApiGatewayClient.GetRestApisAsync(new GetRestApisRequest()
                {

                }, cancellationToken)
                    .ConfigureAwait(true);

                var ordertApis = apis.Items.Where(x => x.EndpointConfiguration.Types.Single(x => "PRIVATE".Equals(x, StringComparison.InvariantCulture)) != null).ToList();
            }

            return null;
        }

        private async Task<List<VpcEndpoint>> ReadVpcEndpoint(AmazonEC2Client amazonEc2Client, IAwsDoScan awsDoScan, CancellationToken cancellationToken)
        {
            var vpc = await amazonEc2Client.DescribeVpcsAsync(CreateDescribeVpcsRequest(awsDoScan), cancellationToken).ConfigureAwait(true);
            var vpcIds = vpc.Vpcs.Select(x => x.VpcId).ToList();
            var vpcEndpointList = await amazonEc2Client.DescribeVpcEndpointsAsync(new DescribeVpcEndpointsRequest(), cancellationToken)
                .ConfigureAwait(true);
            return vpcEndpointList.VpcEndpoints.Where(x => vpcIds.Contains(x.VpcId)).ToList();
        }

        private DescribeVpcsRequest CreateDescribeVpcsRequest(IAwsDoScan awsDoScan)
        {
            if (awsDoScan.ScanVpcIds != null && awsDoScan.ScanVpcIds.Any())
            {
                var vpcIds = string.Join(", ", awsDoScan.ScanVpcIds);
                _logger.LogInformation("Read AWS VPC: [{0}]", vpcIds);
                return new DescribeVpcsRequest()
                {
                    VpcIds = awsDoScan.ScanVpcIds
                };
            }
            _logger.LogInformation("Read AWS VPC: [no definitions, read all!]");
            return new DescribeVpcsRequest();
        }

    }
}