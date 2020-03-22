using System;
using System.Collections.Generic;
using System.Text;
using DnsProxy.Aws.Models.Rules;
using DnsProxy.Common;

namespace DnsProxy.Aws
{
    public class AwsPlugin : IPlugin
    {
        public string PluginName => " DnsProxy.Aws";
        public Type DependencyRegistration => typeof(AwsDependencyRegistration);
        public Type DnsProxyConfiguration => typeof(AwsDnsProxyConfiguration);
        public Type[] Rules => new[] { typeof(AwsApiGatewayRule), typeof(AwsDocDbRule), typeof(AwsElasticCacheRule) };
    }
}
