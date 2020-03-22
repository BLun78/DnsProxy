using DnsProxy.Common;
using System;
using DnsProxy.Doh.Models.Rules;

namespace DnsProxy.Doh
{
    public class DohPlugin : IPlugin
    {
        public string PluginName => "DnsProxy.Doh";
        public Type DependencyRegistration => typeof(DohDependencyRegistration);
        public Type DnsProxyConfiguration => typeof(DohDnsProxyConfiguration);
        public Type[] Rules => new[] {typeof(DohRule)};
    }
}
