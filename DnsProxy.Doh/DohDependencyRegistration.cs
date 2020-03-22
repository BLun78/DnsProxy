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

using DnsProxy.Common.DI;
using DnsProxy.Doh.Strategies;
using Makaretu.Dns;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;

namespace DnsProxy.Doh
{
    public class DohDependencyRegistration : DependencyRegistration, IDependencyRegistration
    {
        public DohDependencyRegistration(IConfigurationRoot configuration) : base(configuration)
        {
        }

        public override void Register(IServiceCollection services)
        {
            //services.AddSingleton<DohResolverStrategy>(); // take a look for services.AddHttpClient

            services.AddSingleton<DohClient>();

            // https://github.com/aspnet/Extensions/tree/master/src/HttpClientFactory
            // https://docs.microsoft.com/de-de/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            //services.AddHttpClient("httpClient", c => { })
            //    .ConfigurePrimaryHttpMessageHandler(ConfigureHandler)
            //    .AddTypedClient((client, provider) =>
            //    {
            //        var dohClient = new DohClient
            //        {
            //            HttpClient = client,
            //            ThrowResponseError = false
            //    };
            //        return dohClient;
            //    });
            services.AddHttpClient<DohResolverStrategy>();
        }

        private HttpMessageHandler ConfigureHandler(IServiceProvider provider)
        {
            var handler = new HttpClientHandler();

            var webProxy = provider.GetService<IWebProxy>();
            if (webProxy != null)
            {
                handler.Proxy = webProxy;
                handler.UseDefaultCredentials = true;
                handler.PreAuthenticate = true;
                handler.UseProxy = true;
            }

            handler.MaxConnectionsPerServer = 10000;
            handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

            return handler;
        }

    }
}
