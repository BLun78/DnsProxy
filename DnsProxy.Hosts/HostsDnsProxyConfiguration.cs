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
            throw new NotImplementedException();
        }

    }
}
