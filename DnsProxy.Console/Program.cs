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

using DnsProxy.Common;
using DnsProxy.Console.Common;
using DnsProxy.Plugin.Configuration;
using DnsProxy.Plugin.DI;
using DnsProxy.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DnsProxy.Console
{
    public static class Program
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
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("logging.json", false, true);

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
                    Setup();

                    using (var dnsServer = ServiceProvider.GetService<DnsServer>())
                    {
                        return await WaitForEndAsync().ConfigureAwait(false);
                    }

                    return 0;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            // Last Global Exception Handler!!!
            catch (System.Net.Sockets.SocketException socex)
            {
                if (socex.ErrorCode == 10048)
                {
                    Log.Fatal(socex, "The Port for the DNS-Proxy-Server is in use. Stop the application that use the same Port or change the ListenerPort!");
                }
                else
                {
                    Log.Fatal(socex, "Host terminated unexpectedly {DefaultTitle}", ApplicationInformation.DefaultTitle);
                }

                await Task.Delay(100).ConfigureAwait(false);
                return await Task.FromResult(1).ConfigureAwait(false);
            }
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Log.Fatal(ex, "Host terminated unexpectedly {DefaultTitle}", ApplicationInformation.DefaultTitle);

                await Task.Delay(100).ConfigureAwait(false);
                return await Task.FromResult(1).ConfigureAwait(false);
            }
            finally
            {
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

        private static void Setup()
        {
            Configuration.SetupSerilog();
            PluginManager.RegisterDependencyRegistration(Configuration);
            CancellationTokenSource = new CancellationTokenSource();

            var dependencyRegistrations = new List<IDependencyRegistration>();
            dependencyRegistrations.AddRange(PluginManager.DependencyRegistration);
            dependencyRegistrations.Add(new ConsoleDependencyRegistration(Configuration, CancellationTokenSource));
            dependencyRegistrations.Add(new CommonDependencyRegistration(Configuration));
            dependencyRegistrations.Add(new ServerDependencyRegistration(Configuration, PluginManager.RuleFactories));
            DependencyInjector = new DependencyInjector(Configuration, dependencyRegistrations);

            ApplicationInformation = ServiceProvider.GetService<ApplicationInformation>();

            CreateHeader();
        }

        private static void CreateHeader()
        {
            Title = ApplicationInformation.DefaultTitle;
            Log.Information("========================================================================================");
            ApplicationInformation.LogAssemblyInformation();
            Log.Information("========================================================================================");
            Log.Information("Copyright 2019 - 2020 Bjoern Lundstroem - (https://github.com/BLun78)");
            Log.Information("");
            Log.Information("Licensed under the Apache License, Version 2.0(the \"License\");");
            Log.Information("you may not use this file except in compliance with the License.");
            Log.Information("You may obtain a copy of the License at");
            Log.Information("");
            Log.Information("\thttp://www.apache.org/licenses/LICENSE-2.0");
            Log.Information("");
            Log.Information("Unless required by applicable law or agreed to in writing, software");
            Log.Information("distributed under the License is distributed on an \"AS IS\" BASIS,");
            Log.Information("WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.");
            Log.Information("See the License for the specific language governing permissions and");
            Log.Information("limitations under the License.");
            Log.Information("========================================================================================");
            Log.Information("\t[strg]+[x] or [strg]+[q] = exit Application");
            Log.Information("\t[strg]+[r] = reload AWS-VPC's with new mfa");
            Log.Information("\t[strg]+[h] = show this help / information");
            Log.Information("========================================================================================");
            Log.Information("Description:");
            Log.Information("\tA DNS-Proxy with routing for DNS-Request for development with hybrid clouds!");
            Log.Information("\tconfig.json, rules.json and hosts,json are used for configure.");
            Log.Information("========================================================================================");
            Log.Information("starts up " + ApplicationInformation.DefaultTitle + " ...");
            Log.Information("==================================================================================");
        }
    }
}
