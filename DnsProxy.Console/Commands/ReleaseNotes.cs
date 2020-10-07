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
using DnsProxy.Plugin.Common;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Console.Commands
{
    internal class ReleaseNotes
    {
        private readonly ILogger<Program> _logger;

        public ReleaseNotes(ILogger<Program> logger)
        {
            _logger = logger;
        }

        public void WriteReleaseNotes()
        {
            var buildTime = ApplicationInformation.GetTimestamp();
            _logger.LogInformation(LogConsts.DoubleLine);
            if (buildTime.HasValue)
            {
                _logger.LogInformation(@"Release Notes of {data} {time}", buildTime.Value.ToShortDateString(), buildTime.Value.ToLongTimeString());
            }
            else
            {
                _logger.LogInformation(@"Release Notes");
            }

#if DEBUG
            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation("2.0.5.0");
            _logger.LogInformation("    - nuget/lib update");
            _logger.LogInformation("    - plugin: powershell plugin only works for debug flag");
#endif

            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation("2.0.4.0");
            _logger.LogInformation("    - fix: WebProxyConfig would now used by AWS Plugin and DOH Plugin.");
            _logger.LogInformation("    - nuget/lib update");
            _logger.LogInformation("    - fix: hot key order");

            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation("2.0.3.0");
            _logger.LogInformation("    - add error handling for request");
            _logger.LogInformation("    - add more logs for errors with configs");
            _logger.LogInformation("    - add release notes");

            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation("2.0.2.0");
            _logger.LogInformation("    - config renaming of attributes");

            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation("2.0.1.0");
            _logger.LogInformation("    - fix: plugin engine problem for macos");

            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation("2.0.0.0");
            _logger.LogInformation("    - add plugin engine");
            _logger.LogInformation("    - add plugin - DNS");
            _logger.LogInformation("    - add plugin - DNS over HTTP");
            _logger.LogInformation("    - add plugin - Read AWS VPC");
            _logger.LogInformation("    - nuget/lib update");

            _logger.LogInformation(LogConsts.DoubleLine);
        }
    }
}
