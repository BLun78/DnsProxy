using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace DnsProxy.Console.Common
{
    internal static class SerilogExtensions
    {
        public static ILogger SetupSerilog(this IConfiguration configuration)
        {
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Verbose)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithUserName()
                .Enrich.WithAssemblyName()
                .WriteTo.Console();

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
                loggingBuilder.AddSerilog(dispose: true));

            return services;
        }
    }
}
