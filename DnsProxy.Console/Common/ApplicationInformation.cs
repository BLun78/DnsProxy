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
using Serilog;
using System;
using System.Globalization;
using System.IO;

namespace DnsProxy.Console.Common
{
    internal static class ApplicationInformation
    {
        internal const string DefaultTitle = "BLun.de DNS Proxy";

        public static void LogAssemblyInformation()
        {
            var version = typeof(ApplicationInformation).Assembly.GetName().Version;

            Log.Logger.Information(LogConsts.DoubleLine);
            Log.Logger.Information(@"Title: '{title}' Version: '{version}'", DefaultTitle, version);
            var buildTime = GetTimestamp();
            if (buildTime.HasValue)
            {
                Log.Logger.Information(@"Build Time: '{date} {time}'", buildTime.Value.ToLongDateString(), buildTime.Value.ToLongTimeString());
            }
            Log.Logger.Information(LogConsts.DoubleLine);
        }

        public static DateTime? GetTimestamp()
        {
            try
            {
                using (var stream = typeof(ApplicationInformation).Assembly.GetManifestResourceStream("DnsProxy.Console.BuildTimeStamp.txt"))
                using (var reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    if (DateTime.TryParse(result, new CultureInfo("de"), DateTimeStyles.None, out DateTime buildTime))
                    {
                        return buildTime;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, e.Message);
            }
            return null;
        }
    }
}