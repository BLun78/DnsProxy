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

using Microsoft.Extensions.Configuration;
using System.IO;
using DnsProxy.Plugin;
using DnsProxy.Plugin.Configuration;

namespace DnsProxy.Server
{
    public class ServerDnsProxyConfiguration : IDnsProxyConfiguration
    {
        public IConfigurationBuilder ConfigurationBuilder(IConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("config.json", false, true)
                    .AddJsonFile("rules.json", false, true)
                    .AddJsonFile("hosts.json", false, true)
                    .AddJsonFile("_config.json", true, true)
                    .AddJsonFile("_rules.json", true, true)
                    .AddJsonFile("_hosts.json", false, true);
        }
    }
}
