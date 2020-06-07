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
            var buildTime = GetTimestamp();
            Log.Logger.Information(LogConsts.DoubleLine);
            Log.Logger.Information(@"Title: '{title}' Version: '{version}'", DefaultTitle, version);
            Log.Logger.Information(@"Build Time: '{date} {time}'", buildTime.ToLongDateString(), buildTime.ToLongTimeString());
            Log.Logger.Information(LogConsts.DoubleLine);
        }

        public static DateTime GetTimestamp()
        {
            using (var stream = typeof(ApplicationInformation).Assembly.GetManifestResourceStream("DnsProxy.Console.BuildTimeStamp.txt"))
            using (var reader = new StreamReader(stream))
            {
                var result = reader.ReadToEnd();
                return DateTime.Parse(result, new DateTimeFormatInfo());
            }
        }
    }
}