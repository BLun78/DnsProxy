using System;
using System.Net;
using Amazon.APIGateway;
using Amazon.DocDB;
using Amazon.EC2;
using Amazon.ElastiCache;
using Amazon.Runtime;
using DnsProxy.Aws.Adapter;
using DnsProxy.Aws.Models;
using DnsProxy.Aws.Strategies;
using DnsProxy.Common.DI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace DnsProxy.Aws
{
    public class AwsDependencyRegistration : DependencyRegistration, IDependencyRegistration
    {

        internal static AwsContext AwsContext;

        public AwsDependencyRegistration(IConfigurationRoot configuration) : base(configuration)
        {
        }

        public override void Register(IServiceCollection services)
        {
            services.Configure<AwsSettings>(Configuration.GetSection(nameof(AwsSettings)));

            services.AddSingleton<AwsVpcManager>();

            services.AddSingleton(CreateAmazonConfig<AmazonEC2Config>);
            services.AddSingleton(CreateAmazonConfig<AmazonDocDBConfig>);
            services.AddSingleton(CreateAmazonConfig<AmazonAPIGatewayConfig>);
            services.AddSingleton(CreateAmazonConfig<AmazonElastiCacheConfig>);
            services.AddTransient(CreateAwsContext);

            services.AddSingleton<AwsApiGatewayResolverStrategy>();
            services.AddSingleton<AwsDocDbResolverStrategy>();
            services.AddSingleton<AwsElasticCacheResolverStrategy>(); 
            services.AddSingleton<AwsDocDbResolverStrategy>();

            services.AddSingleton<AwsApiGatewayAdapter>();
            services.AddSingleton<AwsVpcEndpointAdapter>();
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
