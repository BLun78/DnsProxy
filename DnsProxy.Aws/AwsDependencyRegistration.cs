using System;
using System.Collections.Generic;
using System.Text;
using DnsProxy.Common.DI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;


namespace DnsProxy.Aws
{
    public class AwsDependencyRegistration : DependencyRegistration, IDependencyRegistration
    {
        public AwsDependencyRegistration(IConfigurationRoot configuration) : base(configuration)
        {
        }

        public override void Register(IServiceCollection serviceCollection)
        {
            throw new NotImplementedException();
        }
    }
}
