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

using DnsProxy.Console.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using DnsProxy.Plugin;
using DnsProxy.Plugin.DI;

namespace DnsProxy.Console
{
    internal class ConsoleDependencyRegistration : DependencyRegistration, IDependencyRegistration
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public ConsoleDependencyRegistration(IConfigurationRoot configuration, CancellationTokenSource cancellationTokenSource) : base(configuration)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        public override void Register(IServiceCollection services)
        {
            // Program
            services.AddSingleton(this.GetType().Assembly);
            services.AddSingleton<ApplicationInformation>();
            services.AddSingleton(_cancellationTokenSource);
        }
    }
}
