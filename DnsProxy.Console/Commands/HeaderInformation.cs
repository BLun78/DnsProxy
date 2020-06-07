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

using System;
using System.Collections.Generic;
using System.Text;
using DnsProxy.Console.Common;
using DnsProxy.Plugin.Common;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Console.Commands
{
    internal class HeaderInformation
    {
        private readonly ILogger<Program> _logger;

        public HeaderInformation(ILogger<Program> logger)
        {
            _logger = logger;
        }

        public void WriteHeader(PluginManager pluginManager)
        {
            if (pluginManager == null) throw new ArgumentNullException(nameof(pluginManager));

            ApplicationInformation.LogAssemblyInformation();
            _logger.LogInformation("Copyright 2019 - 2020 Bjoern Lundstroem - (https://github.com/BLun78/DnsProxy)");
            _logger.LogInformation("      ");
            _logger.LogInformation("Licensed under the Apache License, Version 2.0(the \"License\");");
            _logger.LogInformation("you may not use this file except in compliance with the License.");
            _logger.LogInformation("You may obtain a copy of the License at");
            _logger.LogInformation("      ");
            _logger.LogInformation("    \thttp://www.apache.org/licenses/LICENSE-2.0");
            _logger.LogInformation("      ");
            _logger.LogInformation("Unless required by applicable law or agreed to in writing, software");
            _logger.LogInformation("distributed under the License is distributed on an \"AS IS\" BASIS,");
            _logger.LogInformation("WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.");
            _logger.LogInformation("See the License for the specific language governing permissions and");
            _logger.LogInformation("limitations under the License.");
            _logger.LogInformation(LogConsts.DoubleLine);
            _logger.LogInformation("    \t[strg]+[x] or [strg]+[q] = exit Application");
            _logger.LogInformation("    \t[strg]+[h] = show this help / information");
            _logger.LogInformation("    \t[strg]+[n] = show the release notes");
            pluginManager.Plugin.ForEach(x => x.GetHelp(_logger));
            _logger.LogInformation("    \t[strg]+[l] = show the license information");
            _logger.LogInformation(LogConsts.DoubleLine);
            _logger.LogInformation("Description:");
            _logger.LogInformation("    \tA DNS-Proxy with routing for DNS-Request for development with hybrid clouds!");
            _logger.LogInformation("    \tconfig.json, rules.json and hosts,json are used for configure.");
            _logger.LogInformation(LogConsts.DoubleLine);
            _logger.LogInformation("starts up " + ApplicationInformation.DefaultTitle + " ...");
            _logger.LogInformation(LogConsts.DoubleLine);
        }
    }
}
