using System;
using System.Collections.Generic;
using System.Text;
using DnsProxy.Common.Configuration;
using Microsoft.Extensions.Configuration;

namespace DnsProxy.Aws
{
    public class AwsDnsProxyConfiguration : IDnsProxyConfiguration
    {
        public IConfigurationBuilder ConfigurationBuilder(IConfigurationBuilder configurationBuilder)
        {
            throw new NotImplementedException();
        }
    }
}
