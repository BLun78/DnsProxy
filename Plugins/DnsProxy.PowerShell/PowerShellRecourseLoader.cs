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
using DnsProxy.Common.Cache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DnsProxy.PowerShell
{
    internal class PowerShellRecourseLoader
    {
        public static IServiceProvider ServiceProvider{ get; private set; }
        public static CacheManager CacheManager { get; private set; }
        public static ILogger<PowerShellPlugin> Logger { get; private set; }

        public PowerShellRecourseLoader(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            CacheManager = (CacheManager)serviceProvider.GetService(typeof(CacheManager));
            Logger = (ILogger<PowerShellPlugin>)serviceProvider.GetService(typeof(ILogger<PowerShellPlugin>));
        }
    }
}