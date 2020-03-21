using System;
using System.Collections.Generic;
using System.Text;
using DnsProxy.Common;

namespace DnsProxy.Aws
{
    public class AwsPlugin : IPlugin
    {
        public Type DependencyRegistration => typeof(AwsDependencyRegistration);
        public Type DnsProxyConfiguration => typeof(AwsDnsProxyConfiguration);
    }
}
