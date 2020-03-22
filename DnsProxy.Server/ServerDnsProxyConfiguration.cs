using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DnsProxy.Common.Configuration;
using Microsoft.Extensions.Configuration;

namespace DnsProxy.Server
{
    public class ServerDnsProxyConfiguration : IDnsProxyConfiguration
    {
        public IConfigurationBuilder ConfigurationBuilder(IConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("config.json", false, true)
                    .AddJsonFile("rules.json", false, true)
                    .AddJsonFile("hosts.json", false, true)
                    .AddJsonFile("_config.json", true, true)
                    .AddJsonFile("_rules.json", true, true)
                    .AddJsonFile("_hosts.json", false, true);
        }
    }
}
