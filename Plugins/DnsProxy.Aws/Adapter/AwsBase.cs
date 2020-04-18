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

using Amazon;
using Amazon.Runtime;
using DnsProxy.Aws.Models;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Aws.Adapter
{
    internal abstract class AwsBase
    {
        protected ILogger<AwsAdapterBase> Logger { get; }
        protected AwsContext AwsContext { get; }
        protected ClientConfig AmazonClientConfig { get; }


        protected AwsBase(
            ILogger<AwsAdapterBase> logger,
            AwsContext awsContext) : this(logger, awsContext, default(ClientConfig))
        { }

        protected AwsBase(
            ILogger<AwsAdapterBase> logger,
            AwsContext awsContext,
            ClientConfig amazonClientConfig
        )
        {
            Logger = logger;
            AwsContext = awsContext;
            AmazonClientConfig = amazonClientConfig;
            if (AmazonClientConfig != null)
            {
                AmazonClientConfig.RegionEndpoint = RegionEndpoint.GetBySystemName(awsContext?.AwsSettings?.Region);
            }
        }
    }
}