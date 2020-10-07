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

using DnsProxy.Common.Models;
using DnsProxy.Plugin.DI;
using DnsProxy.Plugin.Models.Rules;
using DnsProxy.Server.Models;
using DnsProxy.Server.Models.Models;
using DnsProxy.Server.Models.Rules;
using DnsProxy.Server.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;

namespace DnsProxy.Server
{
    public class ServerDependencyRegistration : DependencyRegistration, IDependencyRegistration
    {
        private readonly List<IRuleFactory> _ruleFactories;

        public ServerDependencyRegistration(IConfigurationRoot configuration, List<IRuleFactory> ruleFactories) : base(configuration)
        {
            _ruleFactories = ruleFactories;
        }

        public override IServiceCollection Register(IServiceCollection services)
        {
            // Program
            services.AddSingleton<DnsServer>();

            // Stratgies
            services.AddSingleton<StrategyManager>();
            services.AddSingleton<IRuleFactories>(new RuleFactories(_ruleFactories));
            
            // .net core framework
            services.Configure<DnsDefaultServer>(Configuration.GetSection(nameof(DnsDefaultServer)));
            services.Configure<RulesConfig>(Configuration.GetSection(nameof(RulesConfig)));
            services.Configure<HttpProxyConfig>(Configuration.GetSection(nameof(HttpProxyConfig)));
            services.Configure<CacheConfig>(Configuration.GetSection(nameof(CacheConfig)));
            services.Configure<DnsHostConfig>(Configuration.GetSection(nameof(DnsHostConfig)));

            services.Configure<HostsConfig>(Configuration.GetSection(nameof(HostsConfig)));
            services.AddSingleton<CacheResolverStrategy>();

            return services;
        }
    }
}
