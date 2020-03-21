using DnsProxy.Common.DI;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using DnsProxy.Doh.Strategies;
using Makaretu.Dns;
using Microsoft.Extensions.Configuration;

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
