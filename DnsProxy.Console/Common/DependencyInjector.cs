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

using DnsProxy.Console.Commands;
using DnsProxy.Plugin.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;

namespace DnsProxy.Console.Common
{
    internal class DependencyInjector : DependencyRegistration, IDependencyRegistration
    {
        private readonly List<IDependencyRegistration> _dependencyRegistration;

        public DependencyInjector(IConfigurationRoot configuration, List<IDependencyRegistration> dependencyRegistration) : base(configuration)
        {
            _dependencyRegistration = dependencyRegistration;
            ServiceProvider = ConfigureDependencyInjector().BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = false,
                ValidateScopes = false
            });
        }

        public IServiceProvider ServiceProvider { get; }

        private IServiceCollection ConfigureDependencyInjector()
        {
            IServiceCollection services = new ServiceCollection();

            foreach (var item in _dependencyRegistration)
            {
                services = item.Register(services);
            }

            return Register(services);
        }

        public override IServiceCollection Register(IServiceCollection services)
        {
            //services.AddSerilog();
            services.AddLogging(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("DnsProxy.Program", LogLevel.Trace)
                    .AddFilter("DnsProxy.Dns", LogLevel.Trace)
                    .AddFilter("DnsProxy.Doh", LogLevel.Trace)
                    .AddFilter("DnsProxy.Aws", LogLevel.Trace)
                    .AddFilter("DnsProxy.Plugin", LogLevel.Trace)
                    .AddFilter("DnsProxy.Server", LogLevel.Trace)
                    .AddFilter("DnsProxy.Console", LogLevel.Trace)
                    .AddFilter("DnsProxy.Common", LogLevel.Trace)
                    .AddFilter("ARSoft.Tools.Net", LogLevel.Trace)
                    .AddConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.Format = ConsoleLoggerFormat.Systemd;
                        options.LogToStandardErrorThreshold = LogLevel.Warning;
                        options.DisableColors = false;
                        options.TimestampFormat = "[hh:mm:ss]";
                    })
                    ;
            });
            services.AddMemoryCache();

            services.AddSingleton<ReleaseNotes>();
            services.AddSingleton<LicenseInformation>();
            services.AddSingleton<HeaderInformation>();

            return services;
        }
    }
}