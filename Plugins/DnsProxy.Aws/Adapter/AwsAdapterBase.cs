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

using Amazon.Runtime;
using DnsProxy.Aws.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DnsProxy.Aws.Adapter
{
    internal abstract class AwsAdapterBase : AwsBase
    {
        protected int Ttl;

        protected AwsAdapterBase(
            ILogger<AwsAdapterBase> logger,
            AwsContext awsContext) : this(logger, awsContext, default(ClientConfig))
        { }

        protected AwsAdapterBase(
            ILogger<AwsAdapterBase> logger,
            AwsContext awsContext,
            ClientConfig amazonClientConfig
            ) : base(logger, awsContext, amazonClientConfig)
        {
            Ttl = 60 * 60;
        }

        public abstract Task<AwsAdapterResult> GetAdapterResultAsync(AWSCredentials awsCredentials, IEnumerable<Endpoint> endpoints, CancellationToken cancellationToken);

        /// <summary>
        /// Transform the Servicename to a valid Domainname for DNS
        /// example: from "com.amazonaws.[zone].sqs" to "sqs.[zone].amazonaws.com"
        /// </summary>
        /// <param name="serviceName">Servicename (example: com.amazonaws.[zone].sqs)</param>
        /// <returns>Domainname (example: sqs.[zone].amazonaws.com)</returns>
        protected static string CreateDomainName(string serviceName)
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
    }
}