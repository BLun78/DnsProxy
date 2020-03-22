using System;
using System.Collections.Generic;
using System.Text;
using DnsProxy.Common.Configuration;
using Microsoft.Extensions.Configuration;

namespace DnsProxy.Hosts
{
    public class HostsDnsProxyConfiguration : IDnsProxyConfiguration
    {
        public IConfigurationBuilder ConfigurationBuilder(IConfigurationBuilder configurationBuilder)
        {

            configurationBuilder = configurationBuilder.AddJsonFile("hosts.json", false, true);
            configurationBuilder = configurationBuilder.AddJsonFile("_hosts.json", false, true);
            return configurationBuilder;
        }

    }
}
