#region Apache License-2.0

// Copyright 2019 Bjoern Lundstroem
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
using System.Reflection;
using System.Threading;
using DnsProxy.Dns;
using DnsProxy.Models;
using DnsProxy.Models.Context;
using DnsProxy.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace DnsProxy.Common
{
    internal class DependencyInjector
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IConfigurationRoot _configuration;
        private static DnsContextAccessor _dnsContextAccessor;

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
            services.AddTransient<DohResolverStrategy>();
            services.AddTransient<DnsResolverStrategy>();
            services.AddSingleton<InternalNameServerResolverStrategy>();
            services.AddSingleton<HostsResolverStrategy>();

            // .net core frameworks
            services.AddOptions();
            services.Configure<HostsConfig>(_configuration.GetSection(nameof(HostsConfig)));
            services.Configure<DnsHostConfig>(_configuration.GetSection(nameof(DnsHostConfig)));
            services.Configure<RulesConfig>(_configuration.GetSection(nameof(RulesConfig)));
            services.Configure<DnsDefaultServer>(_configuration.GetSection(nameof(DnsDefaultServer)));
            services.Configure<HttpProxyConfig>(_configuration.GetSection(nameof(HttpProxyConfig)));
            services.Configure<InternalNameServerConfig>(_configuration.GetSection(nameof(InternalNameServerConfig)));

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
                        options.Format = ConsoleLoggerFormat.Default;
                        options.LogToStandardErrorThreshold = LogLevel.Warning;
                        options.DisableColors = false;
                        options.TimestampFormat = "[dd.MM.yyyy hh:mm:ss]";
                    });
            });
            services.AddMemoryCache();
            // https://github.com/aspnet/Extensions/tree/master/src/HttpClientFactory
            // https://docs.microsoft.com/de-de/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            services.AddHttpClient(); // not top, but it works in all Applications - BL
            return services;
        }
    }
}