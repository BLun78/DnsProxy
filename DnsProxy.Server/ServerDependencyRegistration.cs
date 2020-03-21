using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using DnsProxy.Common.DI;
using DnsProxy.Common.Models;
using DnsProxy.Dns;
using DnsProxy.Models;
using DnsProxy.Server.Models;
using DnsProxy.Server.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DnsProxy.Server
{
    public class ServerDependencyRegistration : DependencyRegistration, IDependencyRegistration
    {
        public ServerDependencyRegistration(IConfigurationRoot configuration) : base(configuration)
        {
        }

        public override void Register(IServiceCollection services)
        {
            // Program
            services.AddSingleton<DnsServer>();

            // Stratgies
            services.AddSingleton<StrategyManager>();

            // common
            services.AddSingleton(CreateHttpProxyConfig);

            // .net core framework
            services.Configure<DnsDefaultServer>(Configuration.GetSection(nameof(DnsDefaultServer)));
            services.Configure<RulesConfig>(Configuration.GetSection(nameof(RulesConfig)));
            services.Configure<HttpProxyConfig>(Configuration.GetSection(nameof(HttpProxyConfig)));
            services.Configure<CacheConfig>(Configuration.GetSection(nameof(CacheConfig)));
        }


        private IWebProxy CreateHttpProxyConfig(IServiceProvider provider)
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
    }
}
