﻿#region Apache License-2.0
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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using DnsProxy.Plugin.DI;

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
            services.AddSerilog();
            //services.AddLogging(builder =>
            //{
            //    builder
            //        .SetMinimumLevel(LogLevel.Debug)
            //        .AddFilter("Microsoft", LogLevel.Warning)
            //        .AddFilter("System", LogLevel.Warning)
            //        //.AddFilter("DnsProxy.Program", LogLevel.Trace)
            //        //.AddFilter("DnsProxy.Dns", LogLevel.Trace)
            //        //.AddFilter("DnsProxy.Dns.DnsServer", LogLevel.Trace)
            //        //.AddFilter("DnsProxy", LogLevel.Trace)
            //        //.AddConsole(options =>
            //        //{
            //        //    options.IncludeScopes = true;
            //        //    options.Format = ConsoleLoggerFormat.Systemd;
            //        //    options.LogToStandardErrorThreshold = LogLevel.Warning;
            //        //    options.DisableColors = false;
            //        //    options.TimestampFormat = "[dd.MM.yyyy hh:mm:ss]";
            //        //})
            //        ;
            //});
            services.AddMemoryCache();

            return services;
        }
    }
}