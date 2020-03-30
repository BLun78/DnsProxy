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
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DnsProxy.Plugin
{
    public static class PluginSharedTypes
    {
        private static readonly List<Type> _types;

        static PluginSharedTypes()
        {
            _types = new List<Type>()
            {
                typeof(IDisposable),
                typeof(Regex),
            };
            _types.AddRange(typeof(ILogger<>).Assembly.GetTypes());
            _types.AddRange(typeof(IMemoryCache).Assembly.GetTypes());
            _types.AddRange(typeof(IOptionsMonitor<>).Assembly.GetTypes());
            _types.AddRange(typeof(IServiceCollection).Assembly.GetTypes());
            _types.AddRange(typeof(IConfigurationRoot).Assembly.GetTypes());
        }
        public static Type[] SharedTypes => _types.ToArray();
    }
}
