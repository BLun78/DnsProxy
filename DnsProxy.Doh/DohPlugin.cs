#region Apache License-2.0
// Copyright 2020 Bjoern Lundstroem
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
#endregion

using DnsProxy.Doh.Models.Rules;
using System;
using DnsProxy.Common.Models.Rules;
using DnsProxy.Plugin;
using DnsProxy.Plugin.Configuration;
using DnsProxy.Plugin.Models.Rules;

namespace DnsProxy.Doh
{
    public class DohPlugin : IPlugin
    {
        public string PluginName => "DnsProxy.Doh";
        public Type DependencyRegistration => typeof(DohDependencyRegistration);
        public IDnsProxyConfiguration DnsProxyConfiguration => new DohDnsProxyConfiguration();
        public IRuleFactory RuleFactory => new RuleFactory(this.Rules);
        public Type[] Rules => new[] { typeof(DohRule) };
    }
}
