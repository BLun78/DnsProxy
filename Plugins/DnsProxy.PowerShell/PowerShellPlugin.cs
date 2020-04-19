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
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using DnsProxy.Common.Models.Rules;
using DnsProxy.Plugin;
using DnsProxy.Plugin.Configuration;
using DnsProxy.Plugin.Models.Rules;
using DnsProxy.PowerShell.Commands;
using DnsProxy.PowerShell.Commands.Cache;
using Microsoft.Extensions.Logging;

namespace DnsProxy.PowerShell
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/powershell/scripting/developer/windows-powershell?view=powershell-7
    /// </summary>
    public class PowerShellPlugin : IPlugin
    {
        public string PluginName => "DnsProxy.PowerShell";
        public Type DependencyRegistration { get; }
        public IDnsProxyConfiguration DnsProxyConfiguration { get; }
        public IRuleFactory RuleFactory => new RuleFactory(this.Rules);
        public IEnumerable<Type> Rules => new List<Type>() { };

        public void GetHelp(ILogger logger)
        {
        }

        public void GetLicense(ILogger logger)
        {
        }

        public Task CheckKeyAsync(ConsoleKeyInfo keyInfo)
        {
            return Task.CompletedTask;
        }

        public void InitialPlugin(IServiceProvider serviceProvider)
        {
            PowerShellRecourseLoader = new PowerShellRecourseLoader(serviceProvider);
            SessionStateCmdletEntry cmdletEntry = new SessionStateCmdletEntry("get-proc", typeof(AddDnsCacheItem), null);

        }

        internal PowerShellRecourseLoader PowerShellRecourseLoader { get; private set; }
    }
}
