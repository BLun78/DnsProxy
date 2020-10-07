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
using DnsProxy.Plugin.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DnsProxy.Plugin.DI
{
    public abstract class DependencyRegistration : IDependencyRegistration
    {
        protected IConfigurationRoot Configuration { get; }

        protected DependencyRegistration(IConfigurationRoot configuration)
        {
            Configuration = configuration;
            Order = 100;
        }
        
        protected IWebProxy CreateHttpProxyConfig(IServiceProvider provider)
        {
            var httpProxyConfig = provider.GetService<IOptions<HttpProxyConfig>>().Value;

            if (string.IsNullOrWhiteSpace(httpProxyConfig.Address))
            {
                return null;
            }

            var proxy = new WebProxy(httpProxyConfig.Address, httpProxyConfig.Port ?? 8080)
            {
                BypassList = httpProxyConfig.BypassAddressesArray,
                BypassProxyOnLocal = true
            };

            switch (httpProxyConfig.AuthenticationType)
            {
                case AuthenticationType.None:
                    proxy.UseDefaultCredentials = false;
                    break;
                case AuthenticationType.WindowsUser:
                    proxy.UseDefaultCredentials = true;
                    break;
                case AuthenticationType.Basic:
                    proxy.UseDefaultCredentials = false;
                    proxy.Credentials = new NetworkCredential(httpProxyConfig.User, httpProxyConfig.Password);
                    break;
                case AuthenticationType.WindowsDomain:
                    proxy.UseDefaultCredentials = false;
                    proxy.Credentials = new NetworkCredential(httpProxyConfig.User, httpProxyConfig.Password, httpProxyConfig.Domain);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(httpProxyConfig.AuthenticationType),
                        httpProxyConfig.AuthenticationType, null);
            }

            return proxy;
        }

        public int Order { get; }

        public virtual IServiceCollection Register(IServiceCollection services)
        {
            services.Configure<HttpProxyConfig>(Configuration.GetSection(nameof(HttpProxyConfig)));
            services.AddSingleton<IWebProxy>(CreateHttpProxyConfig);
            return services;
        }
    }
}