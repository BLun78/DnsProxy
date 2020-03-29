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
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DnsProxy.Common;
using DnsProxy.Console.Common;
using DnsProxy.Plugin.Configuration;
using DnsProxy.Plugin.DI;
using DnsProxy.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DnsProxy.Console
{
    public static class Program2
    {
        private static string _title;
        private static string[] _args;

        private static CancellationTokenSource CancellationTokenSource { get; set; }
        private static DependencyInjector DependencyInjector { get; set; }
        private static ApplicationInformation ApplicationInformation { get; set; }
        private static IServiceProvider ServiceProvider => DependencyInjector.ServiceProvider;

        public static IConfigurationRoot Configuration
        {
            get
            {
                IConfigurationBuilder builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory());

                builder = new ServerDnsProxyConfiguration().ConfigurationBuilder(builder);
                builder = new CommonDnsProxyConfiguration().ConfigurationBuilder(builder);

                if (PluginManager != null)
                {
                    foreach (IDnsProxyConfiguration dnsProxyConfiguration in PluginManager.Configurations)
                    {
                        builder = dnsProxyConfiguration.ConfigurationBuilder(builder);
                    }
                }

                builder = builder.AddEnvironmentVariables();
                builder = builder.AddCommandLine(_args);
                return builder.Build();
            }
        }

        private static PluginManager PluginManager { get; set; }

        public static async Task<int> Main(string[] args)
        {
            _args = args;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            SerilogExtensions.SetupSerilog(null);
            try
            {
                using (PluginManager = new PluginManager(Log.Logger))
                {
                    Configuration.SetupSerilog();
                    PluginManager.RegisterDependencyRegistration(Configuration);

#if true
                    Serilog.Debugging.SelfLog.Enable(System.Console.Error);
                    Serilog.Debugging.SelfLog.Enable(System.Console.WriteLine);
#endif

                    Setup(args);

                    using (var dnsServer = ServiceProvider.GetService<DnsServer>())
                    {
                        return await WaitForEndAsync().ConfigureAwait(false);
                    }

                    return 0;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            // Last Global Exception Handler!!!
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Log.Fatal(ex, "Host terminated unexpectedly {DefaultTitle}", ApplicationInformation.DefaultTitle);

                await Task.Delay(100).ConfigureAwait(false);
                return await Task.FromResult(1).ConfigureAwait(false);
            }
            finally
            {
                CurrentDomain_ProcessExit(null, null);
            }
        }

        private static async Task<int> WaitForEndAsync()
        {
            return await Task.Run(async () =>
            {
                var exit = false;
                while (!exit)
                {

                    var key = System.Console.ReadKey(true);
                    switch (key.Modifiers, key.Key)
                    {
                        case (ConsoleModifiers.Control, ConsoleKey.H):
                            CreateHeader();
                            break;
                        case (ConsoleModifiers.Control, ConsoleKey.R):
                            break;
                        case (ConsoleModifiers.Control, ConsoleKey.Q):
                        case (ConsoleModifiers.Control, ConsoleKey.X):
                            exit = true;
                            break;
                    }
                }

                CancellationTokenSource?.Cancel();
                CancellationTokenSource?.Dispose();
                return 0;
            }).ConfigureAwait(false);
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            CancellationTokenSource?.Cancel();
            PluginManager?.Dispose();
            CancellationTokenSource?.Dispose();
            Log.CloseAndFlush();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        internal static string Title
        {
            get => _title;
            set
            {
                _title = value;
                if (Environment.UserInteractive) System.Console.Title = value;
            }
        }

        private static void Setup(string[] args)
        {
            CancellationTokenSource = new CancellationTokenSource();

            var dependencyRegistrations = new List<IDependencyRegistration>();
            dependencyRegistrations.AddRange(PluginManager.DependencyRegistration);
            dependencyRegistrations.Add(new ConsoleDependencyRegistration(Configuration, CancellationTokenSource));
            dependencyRegistrations.Add(new CommonDependencyRegistration(Configuration));
            dependencyRegistrations.Add(new ServerDependencyRegistration(Configuration));
            DependencyInjector = new DependencyInjector(Configuration, dependencyRegistrations);

            ApplicationInformation = DependencyInjector.ServiceProvider.GetService<ApplicationInformation>();

            CreateHeader();
        }

        private static void CreateHeader()
        {
            Title = ApplicationInformation.DefaultTitle;
            System.Console.WriteLine(
                "========================================================================================");
            ApplicationInformation.LogAssemblyInformation();
            System.Console.WriteLine(
                "========================================================================================");
            System.Console.WriteLine("Copyright 2019 - 2020 Bjoern Lundstroem - (https://github.com/BLun78)");
            System.Console.WriteLine("");
            System.Console.WriteLine("Licensed under the Apache License, Version 2.0(the \"License\");");
            System.Console.WriteLine("you may not use this file except in compliance with the License.");
            System.Console.WriteLine("You may obtain a copy of the License at");
            System.Console.WriteLine("");
            System.Console.WriteLine("\thttp://www.apache.org/licenses/LICENSE-2.0");
            System.Console.WriteLine("");
            System.Console.WriteLine("Unless required by applicable law or agreed to in writing, software");
            System.Console.WriteLine("distributed under the License is distributed on an \"AS IS\" BASIS,");
            System.Console.WriteLine("WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.");
            System.Console.WriteLine("See the License for the specific language governing permissions and");
            System.Console.WriteLine("limitations under the License.");
            System.Console.WriteLine(
                "========================================================================================");
            var color = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ConsoleColor.DarkYellow;
            System.Console.WriteLine("\t[strg]+[x] or [strg]+[q] = exit Application");
            System.Console.WriteLine("\t[strg]+[r] = reload AWS-VPC's with new mfa");
            System.Console.WriteLine("\t[strg]+[h] = show this help / information");
            System.Console.ForegroundColor = color;
            System.Console.WriteLine(
                "========================================================================================");
            System.Console.WriteLine("Description:");
            System.Console.WriteLine("\tA DNS-Proxy with routing for DNS-Request for development with hybrid clouds!");
            System.Console.WriteLine("\tconfig.json, rules.json and hosts,json are used for configure.");
            System.Console.WriteLine(
                "========================================================================================");
            System.Console.WriteLine("starts up " + ApplicationInformation.DefaultTitle + " ...");
            System.Console.WriteLine(
                "==================================================================================");
        }
    }
}
