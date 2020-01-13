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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Runtime;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Models;
using DnsProxy.Models.Aws;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using DomainName = ARSoft.Tools.Net.DomainName;

namespace DnsProxy.Common.Aws
{
    internal class AwsVpcManager
    {
        private readonly AmazonAPIGatewayConfig _amazonApiGatewayConfig;
        private readonly AmazonEC2Config _amazonEc2Config;
        private readonly AwsContext _awsContext;
        private readonly ILogger<AwsVpcManager> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly int TTL;

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
            _amazonEc2Config.RegionEndpoint = RegionEndpoint.GetBySystemName(_awsContext.AwsSettings?.Region);
            _amazonApiGatewayConfig.RegionEndpoint = _amazonEc2Config.RegionEndpoint;
            TTL = 60 * 60;
        }

        public async Task StartReadingVpcAsync(CancellationToken cancellationToken)
        {
            if (_awsContext?.AwsSettings?.UserAccounts != null) return; 
            try
            {
                foreach (var awsSettingsUserAccount in _awsContext.AwsSettings.UserAccounts)
                {
                    if (awsSettingsUserAccount.DoScan)
                    {
                        await ReadVpcAsync(awsSettingsUserAccount.AwsCredentials, awsSettingsUserAccount, cancellationToken)
                            .ConfigureAwait(false);
                    }

                    foreach (var userRoleExtended in awsSettingsUserAccount.Roles)
                    {
                        if (userRoleExtended.DoScan)
                        {
                            await ReadVpcAsync(userRoleExtended.AwsCredentials, userRoleExtended, cancellationToken)
                                .ConfigureAwait(false);
                        }
                    }
                }
                _logger.LogInformation("AWS import finished!");
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        private async Task ReadVpcAsync(AWSCredentials awsCredentials, IAwsDoScan awsDoScan,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = new List<DnsRecordBase>();
                using (var amazonEc2Client = new AmazonEC2Client(awsCredentials, _amazonEc2Config))
                {
                    var endpoints = await ReadVpcEndpointAsync(amazonEc2Client, awsDoScan, cancellationToken)
                        .ConfigureAwait(false);

                    var readApiGateway = await ReadApiGatewayAsync(awsCredentials, endpoints, cancellationToken)
                        .ConfigureAwait(false);
                    result.AddRange(readApiGateway);

                    var readEndpoints = ReadEndpoints(endpoints);
                    result.AddRange(readEndpoints);
                }

                var groupedResult = (from record in result
                                     group record by record.Name.ToString()
                    into newRecords
                                     orderby newRecords.Key
                                     select newRecords).ToList();
                foreach (var dnsRecordBases in groupedResult)
                {
                    StoreInCache(RecordType.A, dnsRecordBases.ToList(), dnsRecordBases.Key);
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

        private IEnumerable<DnsRecordBase> ReadEndpoints(IEnumerable<Endpoint> endpoints)
        {
            var result = new List<DnsRecordBase>();
            var listEndpoints = endpoints
                .Where(x => !x.VpcEndpoint.ServiceName.Contains(".s3", StringComparison.InvariantCulture)).ToList();
            foreach (var endpoint in listEndpoints)
            {
                var net = endpoint.NetworkInterfaces.First();
                var domainName = CreateDomainName(endpoint.VpcEndpoint.ServiceName);

                result.Add(new ARecord(
                    DomainName.Parse(domainName),
                    TTL,
                    IPAddress.Parse(net.PrivateIpAddress)));
                result.Add(new ARecord(
                    DomainName.Parse(net.PrivateDnsName),
                    TTL,
                    IPAddress.Parse(net.PrivateIpAddress)));
            }

            return result;
        }

        private async Task<List<DnsRecordBase>> ReadApiGatewayAsync(AWSCredentials awsCredentials,
            IEnumerable<Endpoint> endpoints, CancellationToken cancellationToken)
        {
            var result = new List<DnsRecordBase>();
            using (var amazonApiGatewayClient = new AmazonAPIGatewayClient(awsCredentials, _amazonApiGatewayConfig))
            {
                var apiGatewayNetworkInterfaces =
                    endpoints.Where(x =>
                        x.VpcEndpoint.ServiceName.Contains(".execute-api", StringComparison.InvariantCulture)).ToList();
                var apis = await amazonApiGatewayClient.GetRestApisAsync(new GetRestApisRequest(), cancellationToken)
                    .ConfigureAwait(false);
                var orderedApis = apis.Items.Where(x =>
                    x.EndpointConfiguration.Types.Single(x => "PRIVATE".Equals(x, StringComparison.InvariantCulture)) !=
                    null).ToArray();

                foreach (var endpoint in apiGatewayNetworkInterfaces)
                {
                    for (var i = orderedApis.Length - 1; i >= 0; i--)
                    {
                        var item = orderedApis[i];
                        var net = endpoint.NetworkInterfaces.First();
                        var domainName = CreateDomainName(endpoint.VpcEndpoint.ServiceName, item.Id);

                        result.Add(new ARecord(
                            DomainName.Parse(domainName),
                            TTL,
                            IPAddress.Parse(net.PrivateIpAddress)));
                        result.Add(new ARecord(
                            DomainName.Parse(net.PrivateDnsName),
                            TTL,
                            IPAddress.Parse(net.PrivateIpAddress)));
                    }
                }
            }

            return result;
        }

        private string CreateDomainName(string serviceName, string gatewayId)
        {
            return CreateDomainName(serviceName + "." + gatewayId);
        }

        private string CreateDomainName(string serviceName)
        {
            var domainName = string.Empty;
            var arr = serviceName.Split('.');
            for (var i = arr.Length - 1; i >= 0; i--)
            {
                if (string.IsNullOrWhiteSpace(domainName))
                {
                    domainName += arr[i];
                }
                else
                {
                    domainName += "." + arr[i];
                }
            }

            return domainName;
        }

        private async Task<List<Endpoint>> ReadVpcEndpointAsync(AmazonEC2Client amazonEc2Client, IAwsDoScan awsDoScan,
            CancellationToken cancellationToken)
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
                    }).ConfigureAwait(false);
                endpoint.NetworkInterfaces = describeNetworkInterfacesResponse.NetworkInterfaces;
            }

            return result;
        }

        private DescribeVpcsRequest CreateDescribeVpcsRequest(IAwsDoScan awsDoScan)
        {
            if (awsDoScan.ScanVpcIds != null && awsDoScan.ScanVpcIds.Any())
            {
                var vpcIds = string.Join(", ", awsDoScan.ScanVpcIds);
                _logger.LogInformation("Read AWS VPC: [{0}]", vpcIds);
                return new DescribeVpcsRequest
                {
                    VpcIds = awsDoScan.ScanVpcIds
                };
            }

            _logger.LogInformation("Read AWS VPC: [no definitions, read all!]");
            return new DescribeVpcsRequest();
        }

        private void StoreInCache(RecordType recordType, List<DnsRecordBase> data, string key)
        {
            var cacheOptions = new MemoryCacheEntryOptions();
            cacheOptions.SetPriority(CacheItemPriority.NeverRemove);
            StoreInCache(recordType, data, key, cacheOptions);
        }

        private void StoreInCache(RecordType recordType, List<DnsRecordBase> data, string key, MemoryCacheEntryOptions cacheEntryOptions)
        {
            var cacheItem = new CacheItem(recordType, data);
            var lastChar = key.Substring(key.Length - 1, 1);
            _memoryCache.Set(lastChar == "."
                ? key
                : $"{key}.", cacheItem, cacheEntryOptions);
        }
    }
}