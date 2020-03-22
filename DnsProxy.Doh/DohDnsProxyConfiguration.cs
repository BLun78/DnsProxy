using DnsProxy.Common.Configuration;
using System;
using Microsoft.Extensions.Configuration;

namespace DnsProxy.Doh
{
    public class DohDnsProxyConfiguration : IDnsProxyConfiguration
    {
        public IConfigurationBuilder ConfigurationBuilder(IConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder;
        }
    }
}
