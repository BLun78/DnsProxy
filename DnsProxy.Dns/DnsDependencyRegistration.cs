using System;
using System.Collections.Generic;
using System.Text;
using DnsProxy.Common.DI;
using DnsProxy.Dns.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DnsProxy.Dns
{
    public class DnsDependencyRegistration : DependencyRegistration, IDependencyRegistration
    {
        public DnsDependencyRegistration(IConfigurationRoot configuration) : base(configuration)
        {
        }

        public override void Register(IServiceCollection services)
        {

            services.AddSingleton<DnsResolverStrategy>();

            services.Configure<DnsHostConfig>(Configuration.GetSection(nameof(DnsHostConfig)));
        }
    }
}
