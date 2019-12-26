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
using DnsProxy.Dns;
using DnsProxy.Models;
using DnsProxy.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Common
{
    internal class DependencyInjector
    {
        private readonly IConfigurationRoot _configuration;

        public DependencyInjector(IConfigurationRoot configuration)
        {
            _configuration = configuration;
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

            // Stratgies
            services.AddSingleton<StrategyManager>();
            services.AddTransient<IDnsResolverStrategy, DohResolverStrategy>();
            services.AddTransient<IDnsResolverStrategy, DnsResolverStrategy>();
            services.AddSingleton<IDnsResolverStrategy, InternalNameServerResolverStrategy>();
            services.AddSingleton<IDnsResolverStrategy, MulticastResolverStrategy>();
            services.AddSingleton<IDnsResolverStrategy, HostsResolverStrategy>();
            services.AddTransient<DohResolverStrategy>();
            services.AddTransient<DnsResolverStrategy>();
            services.AddSingleton<InternalNameServerResolverStrategy>();
            services.AddSingleton<MulticastResolverStrategy>();
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
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("Default", LogLevel.Trace)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("DnsProxy.Program", LogLevel.Trace)
                    .AddFilter("DnsProxy.Dns", LogLevel.Trace)
                    .AddFilter("DnsProxy.Dns.DnsServer", LogLevel.Trace)
                    .AddFilter("DnsProxy", LogLevel.Trace)
                    .AddConsole(options => { options.IncludeScopes = true; });
            });
            services.AddMemoryCache();
            // https://github.com/aspnet/Extensions/tree/master/src/HttpClientFactory
            // https://docs.microsoft.com/de-de/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            services.AddHttpClient(); // not top, but it works in all Applications - BL
            return services;
        }
    }
}