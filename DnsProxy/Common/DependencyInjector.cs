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
        private readonly IServiceCollection _services;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IConfiguration _configuration;

        public DependencyInjector(IConfigurationRoot configuration, CancellationTokenSource cancellationTokenSource)
        {
            _services = new ServiceCollection();
            _configuration = configuration;
            _cancellationTokenSource = cancellationTokenSource;
            _dnsContextAccessor = new DnsContextAccessor();
            ServiceProvider = ConfigureDependencyInjector().BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });
        }

        public DependencyInjector(IConfiguration configuration, IServiceCollection services, CancellationTokenSource cancellationTokenSource)
        {
            _services = services;
            _configuration = configuration;
            _cancellationTokenSource = cancellationTokenSource;
            _dnsContextAccessor = new DnsContextAccessor();
        }

        public IServiceProvider ServiceProvider { get; }

        public IServiceCollection ConfigureDependencyInjector(Func<ILoggingBuilder, ILoggingBuilder> loggingBuilder = null)
        {
            // Program
            _services.AddSingleton(Assembly.GetExecutingAssembly());
            _services.AddSingleton<ApplicationInformation>();
            _services.AddSingleton<DnsServer>();
            _services.AddSingleton<DohClient>();
            _services.AddSingleton(_cancellationTokenSource);

            // Dns Context
            _services.AddSingleton<IDnsContextAccessor>(_dnsContextAccessor);
            _services.AddSingleton<IWriteDnsContextAccessor>(_dnsContextAccessor);
            _services.AddTransient<IWriteDnsContext, DnsContext>();

            // Stratgies
            _services.AddSingleton<StrategyManager>();
            _services.AddTransient<IDnsResolverStrategy, DohResolverStrategy>();
            _services.AddTransient<IDnsResolverStrategy, DnsResolverStrategy>();
            _services.AddSingleton<IDnsResolverStrategy, InternalNameServerResolverStrategy>();
            _services.AddSingleton<IDnsResolverStrategy, HostsResolverStrategy>();
            _services.AddTransient<IDnsResolverStrategy, AwsApiGatewayResolverStrategy>();
            _services.AddTransient<IDnsResolverStrategy, AwsDocDbResolverStrategy>();
            _services.AddTransient<IDnsResolverStrategy, AwsElasticCacheResolverStrategy>();
            _services.AddTransient<DohResolverStrategy>();
            _services.AddTransient<DnsResolverStrategy>();
            _services.AddSingleton<InternalNameServerResolverStrategy>();
            _services.AddSingleton<HostsResolverStrategy>();
            _services.AddTransient<AwsApiGatewayResolverStrategy>();
            _services.AddTransient<AwsDocDbResolverStrategy>();
            _services.AddTransient<AwsElasticCacheResolverStrategy>();

            // common
            _services.AddSingleton<AwsDocDbResolverStrategy>();
            _services.AddSingleton<AwsVpcManager>();
            _services.AddSingleton(CreateHttpProxyConfig);
            _services.AddSingleton(CreateAmazonConfig<AmazonEC2Config>);
            _services.AddSingleton(CreateAmazonConfig<AmazonDocDBConfig>);
            _services.AddSingleton(CreateAmazonConfig<AmazonAPIGatewayConfig>);
            _services.AddSingleton(CreateAmazonConfig<AmazonElastiCacheConfig>);
            _services.AddTransient(CreateAwsContext);

            // .net core frameworks
            _services.AddOptions();
            _services.Configure<HostsConfig>(_configuration.GetSection(nameof(HostsConfig)));
            _services.Configure<DnsHostConfig>(_configuration.GetSection(nameof(DnsHostConfig)));
            _services.Configure<RulesConfig>(_configuration.GetSection(nameof(RulesConfig)));
            _services.Configure<DnsDefaultServer>(_configuration.GetSection(nameof(DnsDefaultServer)));
            _services.Configure<HttpProxyConfig>(_configuration.GetSection(nameof(HttpProxyConfig)));
            _services.Configure<InternalNameServerConfig>(_configuration.GetSection(nameof(InternalNameServerConfig)));
            _services.Configure<AwsSettings>(_configuration.GetSection(nameof(AwsSettings)));

            AddLogging(loggingBuilder);

            _services.AddMemoryCache();
            // https://github.com/aspnet/Extensions/tree/master/src/HttpClientFactory
            // https://docs.microsoft.com/de-de/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            _services.AddHttpClient("httpClient", c => { })
                .ConfigurePrimaryHttpMessageHandler(ConfigureHandler)
                .AddTypedClient((client, provider) =>
                {
                    var dohClient = new DohClient
                    {
                        HttpClient = client
                    };
                    return dohClient;
                });

            return _services;
        }

        private void AddLogging(Func<ILoggingBuilder, ILoggingBuilder> loggingBuilder = null)
        {
            _services.AddLogging(builder =>
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
                        options.Format = ConsoleLoggerFormat.Default;
                        options.LogToStandardErrorThreshold = LogLevel.Warning;
                        options.DisableColors = false;
                        options.TimestampFormat = "[dd.MM.yyyy hh:mm:ss]";
                    });
                loggingBuilder?.Invoke(builder);
            });
        }

        private HttpMessageHandler ConfigureHandler(IServiceProvider provider)
        {
            var httpProxyConfig = provider.GetService<IOptions<HttpProxyConfig>>().Value;

            var handler = new HttpClientHandler();
            if (string.IsNullOrWhiteSpace(httpProxyConfig.Uri))
            {
                var webProxy = provider.GetService<IWebProxy>();
                if (webProxy != null)
                {
                    handler.Proxy = webProxy;
                    handler.UseDefaultCredentials = true;
                }
            }

            handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            handler.SslProtocols = SslProtocols.Tls12; //| SslProtocols.Tls13;

            return handler;
        }

        private IWebProxy CreateHttpProxyConfig(IServiceProvider provider)
        {
            var httpProxyConfig = provider.GetService<IOptions<HttpProxyConfig>>().Value;

            if (string.IsNullOrWhiteSpace(httpProxyConfig.Uri))
            {
                return null;
            }

            var proxy = new WebProxy
            {
                Address = new Uri(httpProxyConfig.Uri),
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
            IClientConfig config = new TConfig() as ClientConfig;
            var httpProxyConfig = provider.GetService<IOptions<HttpProxyConfig>>().Value;

            if (!string.IsNullOrWhiteSpace(httpProxyConfig.Uri))
            {
                var webProxy = provider.GetService<IWebProxy>();
                if (webProxy != null)
                {
                    ((ClientConfig)config)?.SetWebProxy(webProxy);
                }
            }

            return config as TConfig;
        }

        private AwsContext CreateAwsContext(IServiceProvider provider)
        {
            return AwsContext;
        }
    }
}