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

using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Reflection;

namespace DnsProxy.Console.Common
{
    internal class ApplicationInformation
    {
        internal const string DefaultTitle = "BLun.de DNS Proxy";
        private readonly Assembly _assembly;
        private readonly ILogger<ApplicationInformation> _logger;

        public ApplicationInformation(Assembly assembly, ILogger<ApplicationInformation> logger)
        {
            _assembly = assembly;
            _logger = logger;
        }

        public void LogAssemblyInformation()
        {
            var version = _assembly.GetName().Version;
            var buildTime = new DateTime(2020, 4, 4);
            var title = DefaultTitle;
            //_logger.LogTrace(@"Title: '{title}' Version: '{version}' Builddate: '{buildTime}'", title, version,
            //    buildTime);
            _logger.LogInformation(@"Title: '{0}' Version: '{1}' Builddate: '{2}'", title, version,
                buildTime.ToLongDateString());
        }
    }
}