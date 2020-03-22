using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Aws.Models;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DnsProxy.Aws.Adapter
{
    internal class AwsVpcEndpointAdapter : AwsAdapterBase
    {
        public AwsVpcEndpointAdapter(
            ILogger<AwsAdapterBase> logger, 
            AwsContext awsContext) : base(logger, awsContext)
        {
        }

        /// <summary>
        /// Read the Endpoint Information and create DNS-Record
        /// </summary>
        public override Task<AwsAdapterResult> GetAdapterResultAsync(AWSCredentials awsCredentials, IEnumerable<Endpoint> endpoints, CancellationToken cancellationToken)
        {
            var result = new AwsAdapterResult();
            // TODO: Change from S3 to Endpoint Type filter
            var listEndpoints = endpoints
                .Where(x => !x.VpcEndpoint.ServiceName.Contains(".s3", StringComparison.InvariantCulture)).ToList();
            foreach (var endpoint in listEndpoints)
            {
                var net = endpoint.NetworkInterfaces.First();
                var domainName = CreateDomainName(endpoint.VpcEndpoint.ServiceName);

                result.ProxyBypassList.Add(domainName);
                result.DnsRecords.Add(new ARecord(
                    ARSoft.Tools.Net.DomainName.Parse(domainName),
                    Ttl,
                    IPAddress.Parse(net.PrivateIpAddress)));
                result.DnsRecords.Add(new ARecord(
                    ARSoft.Tools.Net.DomainName.Parse(net.PrivateDnsName),
                    Ttl,
                    IPAddress.Parse(net.PrivateIpAddress)));
            }

            return Task.FromResult(result);
        }
    }
}
