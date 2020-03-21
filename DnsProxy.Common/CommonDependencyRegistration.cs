using System;
using System.Collections.Generic;
using System.Text;
using DnsProxy.Common.DI;
using DnsProxy.Models.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DnsProxy
{
    public class CommonDependencyRegistration : DependencyRegistration, IDependencyRegistration
    {
        private readonly DnsContextAccessor _dnsContextAccessor;

        public CommonDependencyRegistration(IConfigurationRoot configuration) : base(configuration)
        {
            _dnsContextAccessor = new DnsContextAccessor();
        }

        public override void Register(IServiceCollection services)
        {
            // Dns Context
            services.AddSingleton<IDnsContextAccessor>(_dnsContextAccessor);
            services.AddSingleton<IWriteDnsContextAccessor>(_dnsContextAccessor);
            services.AddTransient<IWriteDnsContext, DnsContext>();

            // .net core frameworks
            services.AddOptions();
        }
    }
}
