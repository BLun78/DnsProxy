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
using DnsProxy.Console;
using DnsProxy.Console.Commands;
using DnsProxy.Console.Common;
using DnsProxy.Plugin;
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
using LicenseInformation = DnsProxy.Console.Commands.LicenseInformation;

// ReSharper disable once CheckNamespace
namespace DnsProxy
{
    public class Program
    {
        private static string _title;
        private static string[] _args;
        private static Microsoft.Extensions.Logging.ILogger _logger;
        private static LicenseInformation _licenseInformation;
        private static ReleaseNotes _releaseNotes;
        private static HeaderInformation _headerInformation;

        private static CancellationTokenSource CancellationTokenSource { get; set; }
        private static DependencyInjector DependencyInjector { get; set; }
        private static IServiceProvider ServiceProvider => DependencyInjector.ServiceProvider;
        private static PluginManager PluginManager { get; set; }

        private static IConfigurationRoot Configuration
        {
            get
            {
                IConfigurationBuilder builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory());

                builder = new ServerDnsProxyConfiguration().ConfigurationBuilder(builder);
                builder = new CommonDnsProxyConfiguration().ConfigurationBuilder(builder);

                if (PluginManager != null)
                {
                    foreach (IDnsProxyConfiguration dnsProxyConfiguration
                        in PluginManager.Configurations)
                    {
                        builder = dnsProxyConfiguration.ConfigurationBuilder(builder);
                    }
                }

                builder = builder.AddEnvironmentVariables();
                builder = builder.AddCommandLine(_args);
                return builder.Build();
            }
        }

        public static async Task<int> Main(string[] args)
        {
            _args = args;
            Title = ApplicationInformation.DefaultTitle;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            SerilogExtensions.SetupSerilog(null);
            ApplicationInformation.LogAssemblyInformation();
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

        private static Task<int> WaitForEndAsync()
        {
            return Task.Run(async () =>
           {
               var exit = false;
               while (!exit)
               {
                   var key = System.Console.ReadKey(true);

                   switch (key.Modifiers, key.Key)
                   {
                       case (ConsoleModifiers.Control, ConsoleKey.N):
                           CreateReleaseNotes();
                           break;
                       case (ConsoleModifiers.Control, ConsoleKey.H):
                           CreateHeader();
                           break;
                       case (ConsoleModifiers.Control, ConsoleKey.L):
                           CreateLicenseInformation();
                           break;
                       case (ConsoleModifiers.Control, ConsoleKey.Q):
                       case (ConsoleModifiers.Control, ConsoleKey.X):
                           exit = true;
                           break;
                   }

                   foreach (IPlugin plugin in PluginManager.Plugin)
                   {
                       await plugin.CheckKeyAsync(key).ConfigureAwait(false);
                   }
                   await Task.Delay(250).ConfigureAwait(true);
               }

               return 0;
           });
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

            PluginManager.Plugin.ForEach(x => x.InitialPlugin(ServiceProvider));

            _logger = ServiceProvider.GetService<Microsoft.Extensions.Logging.ILogger<Program>>();
            _licenseInformation = ServiceProvider.GetService<LicenseInformation>();
            _releaseNotes = ServiceProvider.GetService<ReleaseNotes>();
            _headerInformation = ServiceProvider.GetService<HeaderInformation>();

            CreateHeader();
        }

        private static void CreateLicenseInformation()
        {
            _licenseInformation.CreateLicenseInformation(PluginManager);
        }

        private static void CreateHeader()
        {
            _headerInformation.WriteHeader(PluginManager);
        }

        private static void CreateReleaseNotes()
        {
            _releaseNotes.WriteReleaseNotes();
        }
    }
}
