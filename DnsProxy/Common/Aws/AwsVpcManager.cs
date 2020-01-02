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
using System.Linq;
using System.Threading.Tasks;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Runtime;
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
        private static AmazonEC2Client amazonEc2Client;

        public AwsVpcManager(ILogger<AwsVpcManager> logger,
            IMemoryCache memoryCache,
            AwsContext awsContext,
            AmazonEC2Config amazonEc2Config)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _awsContext = awsContext;
            _amazonEc2Config = amazonEc2Config;
        }

        public async Task StartReadingVpcAsync()
        {
            foreach (var awsSettingsUserAccount in _awsContext.AwsSettings.UserAccounts)
            {
                await ReadVpc(_amazonEc2Config, awsSettingsUserAccount.AwsCredentials, awsSettingsUserAccount).ConfigureAwait(true);
                foreach (var userRoleExtended in awsSettingsUserAccount.Roles)
                {
                    await ReadVpc(_amazonEc2Config, userRoleExtended.AwsCredentials, userRoleExtended).ConfigureAwait(true);
                }
            }
            amazonEc2Client?.Dispose();
        }

        private async Task ReadVpc(AmazonEC2Config amazonEc2Config, AWSCredentials awsCredentials, IAwsDoScan awsDoScan)
        {
            try
            {
                if (!awsDoScan.DoScan)
                {
                    return;
                }
                amazonEc2Client?.Dispose();
                amazonEc2Client = new AmazonEC2Client(awsCredentials, amazonEc2Config);

                var vpc = await amazonEc2Client.DescribeVpcsAsync(new DescribeVpcsRequest()).ConfigureAwait(true);
                
                // get endpoints
                var vpcend = await amazonEc2Client.DescribeVpcEndpointsAsync(new DescribeVpcEndpointsRequest())
                    .ConfigureAwait(true);
                var sqs = vpcend.VpcEndpoints.Where(x => x.ServiceName.Contains(".sqs")).ToList();
                var secretsmanager = vpcend.VpcEndpoints.Where(x => x.ServiceName.Contains(".secretsmanager")).ToList();
                var executeApi =
                    vpcend.VpcEndpoints.Where(x => x.ServiceName.Contains(".execute-api")).ToList(); // Api-Gateway

                var net = await amazonEc2Client.DescribeNetworkInterfacesAsync(new DescribeNetworkInterfacesRequest()
                {
                    NetworkInterfaceIds = sqs.SelectMany(x => x.NetworkInterfaceIds).ToList()
                }).ConfigureAwait(false);

                // TODO: Read 
                // get Network Interfaces

                // get Network Interface Ip Adresses
                // (Get-EC2NetworkInterface -NetworkInterfaceId $sqsNI).PrivateIpAddress

                // get api gateway
                // get-AGRestAPIList
                // $executeapiNIIP + " " + $entry.Id + ".execute-api.eu-central-1.amazonaws.com" 
                var b = 3;
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
    }
}