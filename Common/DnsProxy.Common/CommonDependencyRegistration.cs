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

using DnsProxy.Common.Cache;
using DnsProxy.Common.Models.Context;
using DnsProxy.Plugin.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DnsProxy.Common
{
    public class CommonDependencyRegistration : Plugin.DI.DependencyRegistration, IDependencyRegistration
    {
        private readonly DnsContextAccessor _dnsContextAccessor;

        public CommonDependencyRegistration(IConfigurationRoot configuration) : base(configuration)
        {
            _dnsContextAccessor = new DnsContextAccessor();
        }

        public override IServiceCollection Register(IServiceCollection services)
        {
            services.AddSingleton<CacheManager>();
            // Dns Context
            services.AddSingleton<IDnsContextAccessor>(_dnsContextAccessor);
            services.AddSingleton<IWriteDnsContextAccessor>(_dnsContextAccessor);
            services.AddTransient<IWriteDnsContext, DnsContext>();

            // .net core frameworks
            services.AddOptions();
            
            return services;
        }
    }
}
