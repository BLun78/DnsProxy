using System;
using System.Collections.Generic;
using System.Text;
using DnsProxy.Common;
using DnsProxy.Hosts.Models.Rules;

namespace DnsProxy.Hosts
{
    public class HostsPlugin : IPlugin
    {
        public string PluginName => "DnsProxy.Hosts";
        public Type DependencyRegistration => typeof(HostsDependencyRegistration);
        public Type DnsProxyConfiguration => typeof(HostsDnsProxyConfiguration);
        public Type[] Rules => new[] {typeof(HostsRule)};
    }
}
