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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace DnsProxy.Console.Common
{
    internal static class SerilogExtensions
    {
        // public class dTextFormatter : ITextFormatter
        // {
        //     public void Format(LogEvent logEvent, TextWriter output)
        //     {

        //         var d = 1;
        //     }
        // }

        public static ILogger SetupSerilog(this IConfiguration configuration)
        {
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                //.MinimumLevel.Override("Microsoft", LogEventLevel.Verbose)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithUserName()
                .Enrich.WithAssemblyName()
                .WriteTo.Debug(LogEventLevel.Verbose, outputTemplate: "[{Level:u3}] {Scope} {Message:lj}{NewLine}{Exception}")
                .WriteTo.Console(//formatter:new dTextFormatter(), 
                    LogEventLevel.Verbose,
                    outputTemplate: "[{Level:u3}] {Scope} {Message:lj}{NewLine}{Exception}"//,

                    //theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Literate
                    );

            if (configuration != null)
            {

                loggerConfig = loggerConfig.ReadFrom.Configuration(configuration);
            }

            Log.Logger = loggerConfig.CreateLogger();
#if true
            Serilog.Debugging.SelfLog.Enable(System.Console.Error);
            Serilog.Debugging.SelfLog.Enable(System.Console.WriteLine);
#endif
            return Log.Logger;
        }

        public static IServiceCollection AddSerilog(this IServiceCollection services)
        {
            services.AddLogging(loggingBuilder =>
                loggingBuilder.AddSerilog(Log.Logger, dispose: false));

            return services;
        }
    }
}
