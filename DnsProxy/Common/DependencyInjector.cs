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
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Authentication;
using System.Threading;
using Amazon.APIGateway;
using Amazon.DocDB;
using Amazon.EC2;
using Amazon.ElastiCache;
using Amazon.Runtime;
using DnsProxy.Common.Aws;
using DnsProxy.Dns;
using DnsProxy.Models;
using DnsProxy.Models.Aws;
using DnsProxy.Models.Context;
using DnsProxy.Strategies;
using Makaretu.Dns;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace DnsProxy.Common
{
    internal class DependencyInjector
    {
        private static DnsContextAccessor _dnsContextAccessor;
        public static AwsContext AwsContext;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IConfigurationRoot _configuration;

        public DependencyInjector(IConfigurationRoot configuration, CancellationTokenSource cancellationTokenSource)
        {
            _configuration = configuration;
            _cancellationTokenSource = cancellationTokenSource;
            _dnsContextAccessor = new DnsContextAccessor();
            ServiceProvider = ConfigureDependencyInjector().BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });
        }

        public IServiceProvider ServiceProvider { get; }

        private IServiceCollection ConfigureDependencyInjector()
        {
            var services = new ServiceCollection();

            // Program
            services.AddSingleton(Assembly.GetExecutingAssembly());
            services.AddSingleton<ApplicationInformation>();
            services.AddSingleton<DnsServer>();
            services.AddSingleton<DohClient>();
            services.AddSingleton(_cancellationTokenSource);

            // Dns Context
            services.AddSingleton<IDnsContextAccessor>(_dnsContextAccessor);
            services.AddSingleton<IWriteDnsContextAccessor>(_dnsContextAccessor);
            services.AddTransient<IWriteDnsContext, DnsContext>();

            // Stratgies
            services.AddSingleton<StrategyManager>();
            services.AddTransient<IDnsResolverStrategy, DohResolverStrategy>();
            services.AddTransient<IDnsResolverStrategy, DnsResolverStrategy>();
            services.AddSingleton<IDnsResolverStrategy, InternalNameServerResolverStrategy>();
            services.AddSingleton<IDnsResolverStrategy, HostsResolverStrategy>();
            services.AddTransient<IDnsResolverStrategy, AwsApiGatewayResolverStrategy>();
            services.AddTransient<IDnsResolverStrategy, AwsDocDbResolverStrategy>();
            services.AddTransient<IDnsResolverStrategy, AwsElasticCacheResolverStrategy>();
            services.AddTransient<DohResolverStrategy>();
            services.AddTransient<DnsResolverStrategy>();
            services.AddSingleton<InternalNameServerResolverStrategy>();
            services.AddSingleton<HostsResolverStrategy>();
            services.AddTransient<AwsApiGatewayResolverStrategy>();
            services.AddTransient<AwsDocDbResolverStrategy>();
            services.AddTransient<AwsElasticCacheResolverStrategy>();

            // common
            services.AddSingleton<AwsDocDbResolverStrategy>();
            services.AddSingleton<AwsVpcManager>();
            services.AddSingleton(CreateHttpProxyConfig);
            services.AddSingleton(CreateAmazonConfig<AmazonEC2Config>);
            services.AddSingleton(CreateAmazonConfig<AmazonDocDBConfig>);
            services.AddSingleton(CreateAmazonConfig<AmazonAPIGatewayConfig>);
            services.AddSingleton(CreateAmazonConfig<AmazonElastiCacheConfig>);
            services.AddTransient(CreateAwsContext);

            // .net core frameworks
            services.AddOptions();
            services.Configure<HostsConfig>(_configuration.GetSection(nameof(HostsConfig)));
            services.Configure<DnsHostConfig>(_configuration.GetSection(nameof(DnsHostConfig)));
            services.Configure<RulesConfig>(_configuration.GetSection(nameof(RulesConfig)));
            services.Configure<DnsDefaultServer>(_configuration.GetSection(nameof(DnsDefaultServer)));
            services.Configure<HttpProxyConfig>(_configuration.GetSection(nameof(HttpProxyConfig)));
            services.Configure<InternalNameServerConfig>(_configuration.GetSection(nameof(InternalNameServerConfig)));
            services.Configure<AwsSettings>(_configuration.GetSection(nameof(AwsSettings)));
            services.Configure<CacheConfig>(_configuration.GetSection(nameof(CacheConfig)));

            services.AddLogging(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    //.AddFilter("DnsProxy.Program", LogLevel.Trace)
                    //.AddFilter("DnsProxy.Dns", LogLevel.Trace)
                    //.AddFilter("DnsProxy.Dns.DnsServer", LogLevel.Trace)
                    //.AddFilter("DnsProxy", LogLevel.Trace)
                    .AddConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.Format = ConsoleLoggerFormat.Systemd;
                        options.LogToStandardErrorThreshold = LogLevel.Warning;
                        options.DisableColors = false;
                        options.TimestampFormat = "[dd.MM.yyyy hh:mm:ss]";
                    });
            });
            services.AddMemoryCache();
            // https://github.com/aspnet/Extensions/tree/master/src/HttpClientFactory
            // https://docs.microsoft.com/de-de/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            services.AddHttpClient("httpClient", c => { })
                .ConfigurePrimaryHttpMessageHandler(ConfigureHandler)
                .AddTypedClient((client, provider) =>
                {
                    var dohClient = new DohClient
                    {
                        HttpClient = client
                    };
                    return dohClient;
                });

            return services;
        }

        private HttpMessageHandler ConfigureHandler(IServiceProvider provider)
        {
            var handler = new HttpClientHandler();

            var webProxy = provider.GetService<IWebProxy>();
            if (webProxy != null)
            {
                handler.Proxy = webProxy;
                handler.UseDefaultCredentials = true;
                handler.PreAuthenticate = true;
                handler.UseProxy = true;
            }
            
            handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            handler.SslProtocols = SslProtocols.Tls12; //| SslProtocols.Tls13;

            return handler;
        }

        private IWebProxy CreateHttpProxyConfig(IServiceProvider provider)
        {
            var httpProxyConfig = provider.GetService<IOptions<HttpProxyConfig>>().Value;

            if (string.IsNullOrWhiteSpace(httpProxyConfig.Address))
            {
                return null;
            }

            var proxy = new WebProxy(httpProxyConfig.Address, httpProxyConfig.Port ?? 8080)
            {
                UseDefaultCredentials = false,
                BypassList = httpProxyConfig.BypassAddressesArray
            };

            switch (httpProxyConfig.AuthenticationType)
            {
                case AuthenticationType.None:
                    break;
                case AuthenticationType.Basic:
                    proxy.Credentials =
                        new NetworkCredential(httpProxyConfig.User, httpProxyConfig.Password);
                    break;
                case AuthenticationType.Windows:
                    proxy.Credentials = new NetworkCredential(httpProxyConfig.User,
                        httpProxyConfig.Password, httpProxyConfig.Domain);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(httpProxyConfig.AuthenticationType),
                        httpProxyConfig.AuthenticationType, null);
            }

            return proxy;
        }

        private TConfig CreateAmazonConfig<TConfig>(IServiceProvider provider)
            where TConfig : class, IClientConfig, new()
        {
            IClientConfig config = new TConfig();
            var httpProxyConfig = provider.GetService<IOptions<HttpProxyConfig>>().Value;

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