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

using DnsProxy.Common.Models.Rules;
using DnsProxy.Doh.Models.Rules;
using DnsProxy.Plugin;
using DnsProxy.Plugin.Configuration;
using DnsProxy.Plugin.Models.Rules;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Doh
{
    public class DohPlugin : IPlugin
    {
        public string PluginName => "DnsProxy.Doh";
        public Type DependencyRegistration => typeof(DohDependencyRegistration);
        public IDnsProxyConfiguration DnsProxyConfiguration => new DohDnsProxyConfiguration();
        public IRuleFactory RuleFactory => new RuleFactory(this.Rules);
        public Type[] Rules => new[] { typeof(DohRule) };
        public void GetHelp(ILogger logger)
        {
        }

        public void GetLicense(ILogger logger)
        {
            logger.LogInformation($"Plugin: {PluginName}");
            logger.LogInformation("----------------------------------------------------------------------------------------");
            logger.LogInformation("     Copyright 2019 - 2020 Bjoern Lundstroem - (https://github.com/BLun78/DnsProxy)");
            logger.LogInformation("     ");
            logger.LogInformation("     Licensed under the Apache License, Version 2.0(the \"License\");");
            logger.LogInformation("     you may not use this file except in compliance with the License.");
            logger.LogInformation("     You may obtain a copy of the License at");
            logger.LogInformation("      ");
            logger.LogInformation("     \thttp://www.apache.org/licenses/LICENSE-2.0");
            logger.LogInformation("      ");
            logger.LogInformation("     Unless required by applicable law or agreed to in writing, software");
            logger.LogInformation("     distributed under the License is distributed on an \"AS IS\" BASIS,");
            logger.LogInformation("     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.");
            logger.LogInformation("     See the License for the specific language governing permissions and");
            logger.LogInformation("     limitations under the License.");
            logger.LogInformation("----------------------------------------------------------------------------------------");
            logger.LogInformation(" Used libraries:");
            logger.LogInformation("----------------------------------------------------------------------------------------");
            logger.LogInformation("     Makaretu.Dns.Unicast  - Clients for unicast DNS servers");
            logger.LogInformation("      ");
            logger.LogInformation("     MIT License");
            logger.LogInformation("      ");
            logger.LogInformation("     Copyright(c) 2018 Richard Schneider");
            logger.LogInformation("      ");
            logger.LogInformation("     Permission is hereby granted, free of charge, to any person obtaining a copy");
            logger.LogInformation("     of this software and associated documentation files(the \"Software\"), to deal");
            logger.LogInformation("     in the Software without restriction, including without limitation the rights");
            logger.LogInformation("     to use, copy, modify, merge, publish, distribute, sublicense, and/ or sell");
            logger.LogInformation("     copies of the Software, and to permit persons to whom the Software is");
            logger.LogInformation("     furnished to do so, subject to the following conditions:");
            logger.LogInformation("      ");
            logger.LogInformation("     The above copyright notice and this permission notice shall be included in all");
            logger.LogInformation("     copies or substantial portions of the Software.");
            logger.LogInformation("      ");
            logger.LogInformation("     THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR");
            logger.LogInformation("     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,");
            logger.LogInformation("     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE");
            logger.LogInformation("     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER");
            logger.LogInformation("     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,");
            logger.LogInformation("     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE");
            logger.LogInformation("     SOFTWARE.");

        }

        public Task CheckKeyAsync(ConsoleKeyInfo keyInfo)
        {
            return Task.CompletedTask;
        }

        public void InitialPlugin(IServiceProvider serviceProvider)
        {
        }

        public void CheckKey(ConsoleKeyInfo keyInfo)
        {
        }
    }
}
