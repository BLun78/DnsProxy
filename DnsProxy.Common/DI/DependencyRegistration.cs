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

namespace DnsProxy.Common.DI
{
    public abstract class DependencyRegistration : IDependencyRegistration
    {
        protected IConfigurationRoot Configuration { get; }

        protected DependencyRegistration(IConfigurationRoot configuration)
        {
            Configuration = configuration;
            Order = 100;
        }

        public int Order { get; }

        public abstract void Register(IServiceCollection services);
    }
}