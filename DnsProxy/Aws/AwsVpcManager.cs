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
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
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

namespace DnsProxy.Aws
{
    internal class AwsVpcManager
    {
        private readonly AmazonAPIGatewayConfig _amazonApiGatewayConfig;
        private readonly AmazonEC2Config _amazonEc2Config;
        private readonly Assembly _assembly;
        private readonly AwsContext _awsContext;
        private readonly ILogger<AwsVpcManager> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly List<string> _proxyBypassList;
        private readonly int _ttl;

        public AwsVpcManager(ILogger<AwsVpcManager> logger,
            IMemoryCache memoryCache,
            AwsContext awsContext,
            AmazonEC2Config amazonEc2Config,
            Assembly assembly,
            AmazonAPIGatewayConfig amazonApiGatewayConfig)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _awsContext = awsContext;
            _amazonEc2Config = amazonEc2Config;
            _assembly = assembly;
            _amazonApiGatewayConfig = amazonApiGatewayConfig;
            _amazonEc2Config.RegionEndpoint = RegionEndpoint.GetBySystemName(_awsContext?.AwsSettings?.Region);
            _amazonApiGatewayConfig.RegionEndpoint = _amazonEc2Config.RegionEndpoint;
            _ttl = 60 * 60;
            _proxyBypassList = new List<string>();
        }

        public async Task StartReadingVpcAsync(CancellationToken cancellationToken)
        {
            if (_awsContext?.AwsSettings?.UserAccounts == null) return;
            _proxyBypassList.Clear();
            try
            {
                foreach (var awsSettingsUserAccount in _awsContext.AwsSettings.UserAccounts)
                {
                    if (awsSettingsUserAccount.DoScan)
                        await ReadVpcAsync(awsSettingsUserAccount.AwsCredentials, awsSettingsUserAccount,
                                cancellationToken)
                            .ConfigureAwait(false);

                    foreach (var userRoleExtended in awsSettingsUserAccount.Roles)
                        if (userRoleExtended.DoScan)
                            await ReadVpcAsync(userRoleExtended.AwsCredentials, userRoleExtended, cancellationToken)
                                .ConfigureAwait(false);
                }

                var path = Path.Combine(Environment.CurrentDirectory, _awsContext.AwsSettings.OutputFileName);
                _logger.LogInformation("AWS write ProxyBypassList in File: [{0}]", path);

                await File.WriteAllTextAsync(path, CreateContentForProxyBypassFile(), Encoding.UTF8, cancellationToken)
                    .ConfigureAwait(false);

                _logger.LogInformation("AWS import finished!");
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        private string CreateContentForProxyBypassFile()
        {
            var fiddler = $"[Fiddler]{Environment.NewLine}{string.Join(Environment.NewLine, _proxyBypassList)}";
            var browser = $"[Browser]{Environment.NewLine}{string.Join(@";", _proxyBypassList)}";
            var fileContent = $"{browser}{Environment.NewLine}{Environment.NewLine}{fiddler}{Environment.NewLine}";
            return fileContent;
        }

        /// <summary>
        ///     Reads the VPC for cacheing DNS-Records to overrides public IPAddresses
        ///     like SQS.[zone].amazonaws.com to 10.10.10.10
        /// </summary>
        /// <param name="awsCredentials">AwsCredintials for read the VPC</param>
        /// <param name="awsScanRules">Rules for Scaning</param>
        /// <param name="cancellationToken">Task Cancellation Toke</param>
        /// <returns>only a Task</returns>
        private async Task ReadVpcAsync(AWSCredentials awsCredentials, IAwsScanRules awsScanRules,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = new List<DnsRecordBase>();
                using (var amazonEc2Client = new AmazonEC2Client(awsCredentials, _amazonEc2Config))
                {
                    var endpoints = await ReadVpcEndpointAsync(amazonEc2Client, awsScanRules, cancellationToken)
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
                    var dnsQuestion = new DnsQuestion(DomainName.Parse(dnsRecordBases.Key), RecordType.A, RecordClass.INet);
                    StoreInCache(dnsQuestion, dnsRecordBases.ToList());
                }
            }
            catch (AmazonEC2Exception aee)
            {
                if (aee.ErrorCode == "UnauthorizedOperation")
                    _logger.LogError(aee, "AWS ErrorCode=[{0}] ==> {1}", aee.ErrorCode, aee.Message);
                else
                    throw;
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, e.Message);
                throw;
            }
        }

        /// <summary>
        /// Read the Endpoint Information and create DNS-Record
        /// </summary>
        /// <param name="endpoints">AWS Endpoints</param>
        /// <returns>List of DNS-Reocrds</returns>
        private IEnumerable<DnsRecordBase> ReadEndpoints(IEnumerable<Endpoint> endpoints)
        {
            var result = new List<DnsRecordBase>();
            var listEndpoints = endpoints
                .Where(x => !x.VpcEndpoint.ServiceName.Contains(".s3", StringComparison.InvariantCulture)).ToList();
            foreach (var endpoint in listEndpoints)
            {
                var net = endpoint.NetworkInterfaces.First();
                var domainName = CreateDomainName(endpoint.VpcEndpoint.ServiceName);

                _proxyBypassList.Add(domainName);
                result.Add(new ARecord(
                    DomainName.Parse(domainName),
                    _ttl,
                    IPAddress.Parse(net.PrivateIpAddress)));
                result.Add(new ARecord(
                    DomainName.Parse(net.PrivateDnsName),
                    _ttl,
                    IPAddress.Parse(net.PrivateIpAddress)));
            }

            return result;
        }

        /// <summary>
        /// Read the Apt-Gateway VpcEndpoint and map all ApiGateway Configurations/Urls to the IPAddress
        /// Create DNs-Records
        /// </summary>
        /// <param name="awsCredentials">AwsCredintials for read the VPC</param>
        /// <param name="endpoints"></param>
        /// <param name="cancellationToken">Task Cancellation Toke</param>
        /// <returns>List of DNS-Reocrds</returns>
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
                    x.EndpointConfiguration.Types.SingleOrDefault(x => "PRIVATE".Equals(x, StringComparison.InvariantCulture)) !=
                    null).ToArray();

                foreach (var endpoint in apiGatewayNetworkInterfaces)
                    for (var i = orderedApis.Length - 1; i >= 0; i--)
                    {
                        var item = orderedApis[i];
                        var net = endpoint.NetworkInterfaces.First();
                        var domainName = CreateApiGatewayDomainName(endpoint.VpcEndpoint.ServiceName, item.Id);

                        _proxyBypassList.Add(domainName);
                        result.Add(new ARecord(
                            DomainName.Parse(domainName),
                            _ttl,
                            IPAddress.Parse(net.PrivateIpAddress)));
                        result.Add(new ARecord(
                            DomainName.Parse(net.PrivateDnsName),
                            _ttl,
                            IPAddress.Parse(net.PrivateIpAddress)));
                    }
            }

            return result;
        }

        /// <summary>
        /// Transform the Servicename + GatewayId to a valid Domainname for DNS
        /// example: from "com.amazonaws.[zone].execute-api.jk38dsk3hd0"  to "jk38dsk3hd0.execute-api.[zone].amazonaws.com"
        /// </summary>
        /// <param name="serviceName">Servicename (example: com.amazonaws.[zone].execute-api)</param>
        /// <param name="gatewayId">ApiGatewayId like "jk38dsk3hd0"</param>
        /// <returns>Domainname (example: jk38dsk3hd0.execute-api.[zone].amazonaws.com)</returns>
        private string CreateApiGatewayDomainName(string serviceName, string gatewayId)
        {
            return CreateDomainName(serviceName + "." + gatewayId);
        }

        /// <summary>
        /// Transform the Servicename to a valid Domainname for DNS
        /// example: from "com.amazonaws.[zone].sqs" to "sqs.[zone].amazonaws.com"
        /// </summary>
        /// <param name="serviceName">Servicename (example: com.amazonaws.[zone].sqs)</param>
        /// <returns>Domainname (example: sqs.[zone].amazonaws.com)</returns>
        private string CreateDomainName(string serviceName)
        {
            var domainName = string.Empty;
            var arr = serviceName.Split('.');
            for (var i = arr.Length - 1; i >= 0; i--)
                if (string.IsNullOrWhiteSpace(domainName))
                    domainName += arr[i];
                else
                    domainName += "." + arr[i];

            return domainName;
        }

        private async Task<List<Endpoint>> ReadVpcEndpointAsync(AmazonEC2Client amazonEc2Client,
            IAwsScanRules awsDoScan,
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

        private DescribeVpcsRequest CreateDescribeVpcsRequest(IAwsScanRules awsDoScan)
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

        protected void StoreInCache(DnsQuestion dnsQuestion, List<DnsRecordBase> data,
            MemoryCacheEntryOptions cacheEntryOptions)
        {
            var key = dnsQuestion.ToString();
            var key2 = new DnsQuestion(dnsQuestion.Name, RecordType.A, dnsQuestion.RecordClass).ToString();

            var cacheItem = new CacheItem(dnsQuestion.RecordType, data);
            _memoryCache.Set(key, cacheItem, cacheEntryOptions);

            if (dnsQuestion.RecordType != RecordType.A)
            {
                _memoryCache.Set(key2, cacheItem, cacheEntryOptions);
            }
        }

        private void StoreInCache(DnsQuestion dnsQuestion, List<DnsRecordBase> data)
        {
            var cacheoptions = new MemoryCacheEntryOptions();
            cacheoptions.SetPriority(CacheItemPriority.NeverRemove);

            StoreInCache(dnsQuestion, data, cacheoptions);
        }
    }
}