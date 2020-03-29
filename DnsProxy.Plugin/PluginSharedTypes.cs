using System;
using System.Text.RegularExpressions;
using DnsProxy.Plugin.Common;
using DnsProxy.Plugin.Configuration;
using DnsProxy.Plugin.DI;
using DnsProxy.Plugin.Models.Dns;
using DnsProxy.Plugin.Models.Rules;
using DnsProxy.Plugin.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DnsProxy.Plugin
{
    public static class PluginSharedTypes
    {
        public static Type[] SharedTypes => new[]
        {
            typeof(IPlugin),
            typeof(IOrder),
            typeof(IDnsProxyConfiguration),
            typeof(IDependencyRegistration),
            typeof(DependencyRegistration),
            typeof(IDnsResolverStrategy),
            typeof(IDnsResolverStrategy<>),
            typeof(IRuleStrategy),
            typeof(IRule),

            // DNS
            typeof(IDnsQuestion),
            typeof(RecordClass),
            typeof(RecordType),
            typeof(IDomainName),
            typeof(IDnsRecordBase),

            // Public Types
            typeof(IServiceCollection),
            typeof(IConfigurationRoot),
            typeof(IConfigurationBuilder),
            typeof(IDisposable),
            typeof(Regex),
        };


    }
}
