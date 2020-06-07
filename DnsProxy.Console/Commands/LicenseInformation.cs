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

using DnsProxy.Console.Common;
using DnsProxy.Plugin;
using DnsProxy.Plugin.Common;
using Microsoft.Extensions.Logging;
using System;

namespace DnsProxy.Console.Commands
{
    internal class LicenseInformation
    {
        private readonly ILogger<Program> _logger;

        public LicenseInformation(ILogger<Program> logger)
        {
            _logger = logger;
        }

        public void CreateLicenseInformation(PluginManager pluginManager)
        {
            if (pluginManager == null) throw new ArgumentNullException(nameof(pluginManager));

            _logger.LogInformation(LogConsts.DoubleLine);
            ApplicationInformation.LogAssemblyInformation();
            _logger.LogInformation(LogConsts.DoubleLine);
            _logger.LogInformation("Copyright 2019 - 2020 Bjoern Lundstroem - (https://github.com/BLun78/DnsProxy)");
            _logger.LogInformation("      ");
            _logger.LogInformation("Licensed under the Apache License, Version 2.0(the \"License\");");
            _logger.LogInformation("you may not use this file except in compliance with the License.");
            _logger.LogInformation("You may obtain a copy of the License at");
            _logger.LogInformation("      ");
            _logger.LogInformation("\thttp://www.apache.org/licenses/LICENSE-2.0");
            _logger.LogInformation("      ");
            _logger.LogInformation("Unless required by applicable law or agreed to in writing, software");
            _logger.LogInformation("distributed under the License is distributed on an \"AS IS\" BASIS,");
            _logger.LogInformation("WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.");
            _logger.LogInformation("See the License for the specific language governing permissions and");
            _logger.LogInformation("limitations under the License.");
            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation(" Used framework and technology:");
            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation("     .NET Core SDK");
            _logger.LogInformation("      ");
            _logger.LogInformation("     The MIT License (MIT)");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Copyright © .NET Foundation and Contributors - (https://github.com/dotnet/core)");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Permission is hereby granted, free of charge, to any person obtaining a copy");
            _logger.LogInformation("     of this software and associated documentation files(the \"Software\"), to deal");
            _logger.LogInformation("     in the Software without restriction, including without limitation the rights");
            _logger.LogInformation("     to use, copy, modify, merge, publish, distribute, sublicense, and/ or sell");
            _logger.LogInformation("     copies of the Software, and to permit persons to whom the Software is");
            _logger.LogInformation("     furnished to do so, subject to the following conditions:");
            _logger.LogInformation("      ");
            _logger.LogInformation("     The above copyright notice and this permission notice shall be included in all");
            _logger.LogInformation("     copies or substantial portions of the Software.");
            _logger.LogInformation("      ");
            _logger.LogInformation("     THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR");
            _logger.LogInformation("     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,");
            _logger.LogInformation("     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE");
            _logger.LogInformation("     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER");
            _logger.LogInformation("     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,");
            _logger.LogInformation("     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE");
            _logger.LogInformation("     SOFTWARE.");
            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation("     Microsoft.Extensions.xxxxxx");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Copyright © .NET Foundation and Contributors -  (https://github.com/dotnet/extensions/tree/release/3.1)");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Licensed under the Apache License, Version 2.0(the \"License\");");
            _logger.LogInformation("     you may not use this file except in compliance with the License.");
            _logger.LogInformation("     You may obtain a copy of the License at");
            _logger.LogInformation("      ");
            _logger.LogInformation("     \thttps://github.com/dotnet/extensions/blob/release/3.1/LICENSE.txt");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Unless required by applicable law or agreed to in writing, software");
            _logger.LogInformation("     distributed under the License is distributed on an \"AS IS\" BASIS,");
            _logger.LogInformation("     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.");
            _logger.LogInformation("     See the License for the specific language governing permissions and");
            _logger.LogInformation("     limitations under the License.");
            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation(" Used libraries:");
            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation("     McMaster.NETCore.Plugins - .NET Core library for dynamically loading code");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Copyright 2019 - 2020 Nate McMaster - (https://github.com/natemcmaster/DotNetCorePlugins)");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Licensed under the Apache License, Version 2.0(the \"License\");");
            _logger.LogInformation("     you may not use this file except in compliance with the License.");
            _logger.LogInformation("     You may obtain a copy of the License at");
            _logger.LogInformation("      ");
            _logger.LogInformation("     \thttps://github.com/natemcmaster/DotNetCorePlugins/blob/master/LICENSE.txt");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Unless required by applicable law or agreed to in writing, software");
            _logger.LogInformation("     distributed under the License is distributed on an \"AS IS\" BASIS,");
            _logger.LogInformation("     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.");
            _logger.LogInformation("     See the License for the specific language governing permissions and");
            _logger.LogInformation("     limitations under the License.");
            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation("     Serilog - simple .NET logging with fully-structured events and more plugins");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Copyright © and maintained by its contributors. - (https://github.com/serilog/serilog/graphs/contributors)");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Licensed under the Apache License, Version 2.0(the \"License\");");
            _logger.LogInformation("     you may not use this file except in compliance with the License.");
            _logger.LogInformation("     You may obtain a copy of the License at");
            _logger.LogInformation("      ");
            _logger.LogInformation("     \thttps://github.com/serilog/serilog/blob/dev/LICENSE");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Unless required by applicable law or agreed to in writing, software");
            _logger.LogInformation("     distributed under the License is distributed on an \"AS IS\" BASIS,");
            _logger.LogInformation("     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.");
            _logger.LogInformation("     See the License for the specific language governing permissions and");
            _logger.LogInformation("     limitations under the License.");
            _logger.LogInformation(LogConsts.DoubleLine);
            DnsProxy.Server.LicenseInformation.GetLicense(_logger);
            _logger.LogInformation(LogConsts.DoubleLine);
            foreach (IPlugin plugin in pluginManager.Plugin)
            {
                plugin.GetLicense(_logger);
                _logger.LogInformation(LogConsts.DoubleLine);
            }
        }
    }
}
