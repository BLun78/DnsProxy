using DnsProxy.Common.DI;
using System;
using System.Collections.Generic;
using System.Text;
using DnsProxy.Hosts.Models;
using DnsProxy.Hosts.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DnsProxy.Hosts
{
    public class HostsDependencyRegistration : DependencyRegistration, IDependencyRegistration
    {
        public HostsDependencyRegistration(IConfigurationRoot configuration) : base(configuration)
        {
        }

        public override void Register(IServiceCollection services)
        {

            services.Configure<HostsConfig>(Configuration.GetSection(nameof(HostsConfig)));
            services.AddSingleton<CacheResolverStrategy>();
        }
    }
}
