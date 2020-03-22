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

using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Amazon.Runtime;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Aws.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DomainName = ARSoft.Tools.Net.DomainName;

namespace DnsProxy.Aws.Adapter
{
    internal class AwsApiGatewayAdapter : AwsAdapterBase
    {
        private AmazonAPIGatewayConfig AmazonApiGatewayConfig => AmazonClientConfig as AmazonAPIGatewayConfig;

        public AwsApiGatewayAdapter(
            ILogger<AwsAdapterBase> logger,
            AwsContext awsContext,
            AmazonAPIGatewayConfig amazonClientConfig)
            : base(logger, awsContext, amazonClientConfig)
        {
        }

        /// <summary>
        /// Read the Apt-Gateway VpcEndpoint and map all ApiGateway Configurations/Urls to the IPAddress
        /// Create DNs-Records
        /// </summary>
        public override async Task<AwsAdapterResult> GetAdapterResultAsync(
            AWSCredentials awsCredentials,
            IEnumerable<Endpoint> endpoints,
            CancellationToken cancellationToken)
        {
            using (var amazonApiGatewayClient = new AmazonAPIGatewayClient(awsCredentials, AmazonApiGatewayConfig))
            {
                var result = new AwsAdapterResult();
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

                        result.ProxyBypassList.Add(domainName);
                        result.DnsRecords.Add(new ARecord(
                            ARSoft.Tools.Net.DomainName.Parse(domainName),
                            Ttl,
                            IPAddress.Parse(net.PrivateIpAddress)));
                        result.DnsRecords.Add(new ARecord(
                            DomainName.Parse(net.PrivateDnsName),
                            Ttl,
                            IPAddress.Parse(net.PrivateIpAddress)));
                    }
                return result;
            }
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
    }
}
