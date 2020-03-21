using DnsProxy.Common;
using System;

namespace DnsProxy.Doh
{
    public class DohPlugin : IPlugin
    {
        public Type DependencyRegistration => typeof(DohDependencyRegistration);
        public Type DnsProxyConfiguration => typeof(DohDnsProxyConfiguration);
    }
}
