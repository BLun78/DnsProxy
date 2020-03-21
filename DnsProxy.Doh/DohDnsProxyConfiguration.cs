using DnsProxy.Common.Configuration;
using System;
using Microsoft.Extensions.Configuration;

namespace DnsProxy.Doh
{
    public class DohDnsProxyConfiguration : IDnsProxyConfiguration
    {
        public IConfigurationBuilder ConfigurationBuilder(IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder = configurationBuilder.AddJsonFile("hosts.json", false, true);
            configurationBuilder = configurationBuilder.AddJsonFile("_hosts.json", false, true);
        }
    }
}
