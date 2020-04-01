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
using Amazon.DocDB;
using Amazon.EC2;
using Amazon.ElastiCache;
using Amazon.Runtime;
using DnsProxy.Aws.Adapter;
using DnsProxy.Aws.Models;
using DnsProxy.Aws.Strategies;
using DnsProxy.Plugin.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;

namespace DnsProxy.Aws
{
    public class AwsDependencyRegistration : DependencyRegistration, IDependencyRegistration
    {

        internal static AwsContext AwsContext;

        public AwsDependencyRegistration(IConfigurationRoot configuration) : base(configuration)
        {
        }

        public override IServiceCollection Register(IServiceCollection services)
        {
            services.Configure<AwsSettings>(Configuration.GetSection(nameof(AwsSettings)));

            services.AddSingleton<AwsVpcManager>();

            services.AddSingleton(CreateAmazonConfig<AmazonEC2Config>);
            services.AddSingleton(CreateAmazonConfig<AmazonDocDBConfig>);
            services.AddSingleton(CreateAmazonConfig<AmazonAPIGatewayConfig>);
            services.AddSingleton(CreateAmazonConfig<AmazonElastiCacheConfig>);
            services.AddTransient(CreateAwsContext);

            services.AddTransient<AwsApiGatewayResolverStrategy>();
            services.AddTransient<AwsDocDbResolverStrategy>();
            services.AddTransient<AwsElasticCacheResolverStrategy>();

            services.AddSingleton<AwsVpcReader>();
            services.AddSingleton<AwsAdapterBase, AwsApiGatewayAdapter>();
            services.AddSingleton<AwsAdapterBase, AwsVpcEndpointAdapter>();

            return services;
        }



        private TConfig CreateAmazonConfig<TConfig>(IServiceProvider provider)
            where TConfig : class, IClientConfig, new()
        {
            IClientConfig config = new TConfig();

            var webProxy = provider.GetService<IWebProxy>();
            if (webProxy != null)
            {
                ((ClientConfig)config)?.SetWebProxy(webProxy);
            }

            return config as TConfig;
        }

        private AwsContext CreateAwsContext(IServiceProvider provider)
        {
            return AwsContext;
        }
    }
}
