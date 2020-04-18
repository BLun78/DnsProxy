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
using DnsProxy.Dns.Models.Rules;
using DnsProxy.Plugin;
using DnsProxy.Plugin.Configuration;
using DnsProxy.Plugin.Models.Rules;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DnsProxy.Plugin.Common;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Dns
{
    public class DnsPlugin : IPlugin
    {
        public string PluginName => "DnsProxy.Dns";
        public Type DependencyRegistration => typeof(DnsDependencyRegistration);
        public IDnsProxyConfiguration DnsProxyConfiguration => new DnsDnsProxyConfiguration();
        public IRuleFactory RuleFactory => new RuleFactory(this.Rules);
        public IEnumerable<Type> Rules => new List<Type>(){ typeof(DnsRule) };
        
        public void GetHelp(ILogger logger)
        {
        }

        public void GetLicense(ILogger logger)
        {
            logger.LogInformation($"Plugin: {PluginName}");
            logger.LogInformation(LogConsts.SingleLine);
            logger.LogInformation("     Copyright 2019 - 2020 Bjoern Lundstroem - (https://github.com/BLun78/DnsProxy)");
            logger.LogInformation("      ");
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
            logger.LogInformation(LogConsts.SingleLine);
            logger.LogInformation(" Used libraries:");
            logger.LogInformation(LogConsts.SingleLine);
            logger.LogInformation("     ARSoft.Tools.Net - A .net Framework/ .net core DNS Library");
            logger.LogInformation("      ");
            logger.LogInformation("     Copyright 2017 Alexander Reinert - (https://github.com/alexreinert/ARSoft.Tools.Net)");
            logger.LogInformation("      ");
            logger.LogInformation("     Licensed under the Apache License, Version 2.0(the \"License\");");
            logger.LogInformation("     you may not use this file except in compliance with the License.");
            logger.LogInformation("     You may obtain a copy of the License at");
            logger.LogInformation("      ");
            logger.LogInformation("     \thttps://github.com/alexreinert/ARSoft.Tools.Net/blob/master/LICENSE");
            logger.LogInformation("      ");
            logger.LogInformation("     Unless required by applicable law or agreed to in writing, software");
            logger.LogInformation("     distributed under the License is distributed on an \"AS IS\" BASIS,");
            logger.LogInformation("     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.");
            logger.LogInformation("     See the License for the specific language governing permissions and");
            logger.LogInformation("     limitations under the License.");
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
