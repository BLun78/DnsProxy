using System;
using System.Collections.Generic;
using System.Text;
using DnsProxy.Common;

namespace DnsProxy.Hosts
{
    public class HostsPlugin : IPlugin
    {
        public Type DependencyRegistration => typeof(HostsDependencyRegistration);
        public Type DnsProxyConfiguration => typeof(HostsDnsProxyConfiguration);
    }
}
