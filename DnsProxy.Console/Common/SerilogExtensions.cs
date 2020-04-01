using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.SystemConsole.Themes;

namespace DnsProxy.Console.Common
{
    internal static class SerilogExtensions
    {
        public class dTextFormatter : ITextFormatter
        {
            public void Format(LogEvent logEvent, TextWriter output)
            {

                var d = 1;
            }
        }

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
