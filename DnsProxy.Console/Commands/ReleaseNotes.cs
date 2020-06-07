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
            _logger.LogInformation(LogConsts.DoubleLine);
            _logger.LogInformation("Release Notes:");

            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation("2.0.3.0");
            _logger.LogInformation("    - add more logs for errors with configs");
            _logger.LogInformation("    - add release notes");

            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation("2.0.2.0");
            _logger.LogInformation("    - config renaming of attributes");

            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation("2.0.1.0");
            _logger.LogInformation("    - Fix: plugin engine problem for macos");

            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation("2.0.0.0");
            _logger.LogInformation("    - Add plugin engine");

            _logger.LogInformation(LogConsts.DoubleLine);
        }
    }
}
