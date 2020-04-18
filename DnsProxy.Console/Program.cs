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
using DnsProxy.Plugin;
using DnsProxy.Plugin.Common;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace DnsProxy
{
    public class Program
    {
        private static string _title;
        private static string[] _args;
        private static Microsoft.Extensions.Logging.ILogger _logger;

        private static CancellationTokenSource CancellationTokenSource { get; set; }
        private static DependencyInjector DependencyInjector { get; set; }
        private static IServiceProvider ServiceProvider => DependencyInjector.ServiceProvider;

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

        private static PluginManager PluginManager { get; set; }

        public static async Task<int> Main(string[] args)
        {
            _args = args;
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

            CreateHeader();
        }

        private static void CreateLicenseInformation()
        {
            _logger.LogInformation(LogConsts.DoubleLine);
            ApplicationInformation.LogAssemblyInformation();
            _logger.LogInformation(LogConsts.DoubleLine);
            _logger.LogInformation("Copyright 2019 - 2020 Bjoern Lundstroem - (https://github.com/BLun78/DnsProxy)");
            _logger.LogInformation("      ");
            _logger.LogInformation("Licensed under the Apache License, Version 2.0(the \"License\");");
            _logger.LogInformation("you may not use this file except in compliance with the License.");
            _logger.LogInformation("You may obtain a copy of the License at");
            _logger.LogInformation("      ");
            _logger.LogInformation("\thttp://www.apache.org/licenses/LICENSE-2.0");
            _logger.LogInformation("      ");
            _logger.LogInformation("Unless required by applicable law or agreed to in writing, software");
            _logger.LogInformation("distributed under the License is distributed on an \"AS IS\" BASIS,");
            _logger.LogInformation("WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.");
            _logger.LogInformation("See the License for the specific language governing permissions and");
            _logger.LogInformation("limitations under the License.");
            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation(" Used framework and technology:");
            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation("     .NET Core SDK");
            _logger.LogInformation("      ");
            _logger.LogInformation("     The MIT License (MIT)");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Copyright © .NET Foundation and Contributors - (https://github.com/dotnet/core)");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Permission is hereby granted, free of charge, to any person obtaining a copy");
            _logger.LogInformation("     of this software and associated documentation files(the \"Software\"), to deal");
            _logger.LogInformation("     in the Software without restriction, including without limitation the rights");
            _logger.LogInformation("     to use, copy, modify, merge, publish, distribute, sublicense, and/ or sell");
            _logger.LogInformation("     copies of the Software, and to permit persons to whom the Software is");
            _logger.LogInformation("     furnished to do so, subject to the following conditions:");
            _logger.LogInformation("      ");
            _logger.LogInformation("     The above copyright notice and this permission notice shall be included in all");
            _logger.LogInformation("     copies or substantial portions of the Software.");
            _logger.LogInformation("      ");
            _logger.LogInformation("     THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR");
            _logger.LogInformation("     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,");
            _logger.LogInformation("     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE");
            _logger.LogInformation("     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER");
            _logger.LogInformation("     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,");
            _logger.LogInformation("     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE");
            _logger.LogInformation("     SOFTWARE.");
            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation("     Microsoft.Extensions.xxxxxx");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Copyright © .NET Foundation and Contributors -  (https://github.com/dotnet/extensions/tree/release/3.1)");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Licensed under the Apache License, Version 2.0(the \"License\");");
            _logger.LogInformation("     you may not use this file except in compliance with the License.");
            _logger.LogInformation("     You may obtain a copy of the License at");
            _logger.LogInformation("      ");
            _logger.LogInformation("     \thttps://github.com/dotnet/extensions/blob/release/3.1/LICENSE.txt");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Unless required by applicable law or agreed to in writing, software");
            _logger.LogInformation("     distributed under the License is distributed on an \"AS IS\" BASIS,");
            _logger.LogInformation("     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.");
            _logger.LogInformation("     See the License for the specific language governing permissions and");
            _logger.LogInformation("     limitations under the License.");
            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation(" Used libraries:");
            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation("     McMaster.NETCore.Plugins - .NET Core library for dynamically loading code");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Copyright 2019 - 2020 Nate McMaster - (https://github.com/natemcmaster/DotNetCorePlugins)");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Licensed under the Apache License, Version 2.0(the \"License\");");
            _logger.LogInformation("     you may not use this file except in compliance with the License.");
            _logger.LogInformation("     You may obtain a copy of the License at");
            _logger.LogInformation("      ");
            _logger.LogInformation("     \thttps://github.com/natemcmaster/DotNetCorePlugins/blob/master/LICENSE.txt");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Unless required by applicable law or agreed to in writing, software");
            _logger.LogInformation("     distributed under the License is distributed on an \"AS IS\" BASIS,");
            _logger.LogInformation("     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.");
            _logger.LogInformation("     See the License for the specific language governing permissions and");
            _logger.LogInformation("     limitations under the License.");
            _logger.LogInformation(LogConsts.SingleLine);
            _logger.LogInformation("     Serilog - simple .NET logging with fully-structured events and more plugins");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Copyright © and maintained by its contributors. - (https://github.com/serilog/serilog/graphs/contributors)");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Licensed under the Apache License, Version 2.0(the \"License\");");
            _logger.LogInformation("     you may not use this file except in compliance with the License.");
            _logger.LogInformation("     You may obtain a copy of the License at");
            _logger.LogInformation("      ");
            _logger.LogInformation("     \thttps://github.com/serilog/serilog/blob/dev/LICENSE");
            _logger.LogInformation("      ");
            _logger.LogInformation("     Unless required by applicable law or agreed to in writing, software");
            _logger.LogInformation("     distributed under the License is distributed on an \"AS IS\" BASIS,");
            _logger.LogInformation("     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.");
            _logger.LogInformation("     See the License for the specific language governing permissions and");
            _logger.LogInformation("     limitations under the License.");
            _logger.LogInformation(LogConsts.DoubleLine);
            LicenseInformation.GetLicense(_logger);
            _logger.LogInformation(LogConsts.DoubleLine);
            foreach (IPlugin plugin in PluginManager.Plugin)
            {
                plugin.GetLicense(_logger);
                _logger.LogInformation(LogConsts.DoubleLine);
            }
        }

        private static void CreateHeader()
        {
            Title = ApplicationInformation.DefaultTitle;
            ApplicationInformation.LogAssemblyInformation();
            _logger.LogInformation("Copyright 2019 - 2020 Bjoern Lundstroem - (https://github.com/BLun78/DnsProxy)");
            _logger.LogInformation("      ");
            _logger.LogInformation("Licensed under the Apache License, Version 2.0(the \"License\");");
            _logger.LogInformation("you may not use this file except in compliance with the License.");
            _logger.LogInformation("You may obtain a copy of the License at");
            _logger.LogInformation("      ");
            _logger.LogInformation("    \thttp://www.apache.org/licenses/LICENSE-2.0");
            _logger.LogInformation("      ");
            _logger.LogInformation("Unless required by applicable law or agreed to in writing, software");
            _logger.LogInformation("distributed under the License is distributed on an \"AS IS\" BASIS,");
            _logger.LogInformation("WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.");
            _logger.LogInformation("See the License for the specific language governing permissions and");
            _logger.LogInformation("limitations under the License.");
            _logger.LogInformation(LogConsts.DoubleLine);
            _logger.LogInformation("    \t[strg]+[x] or [strg]+[q] = exit Application");
            _logger.LogInformation("    \t[strg]+[h] = show this help / information");
            PluginManager.Plugin.ForEach(x => x.GetHelp(_logger));
            _logger.LogInformation("    \t[strg]+[l] = show this licens information");
            _logger.LogInformation(LogConsts.DoubleLine);
            _logger.LogInformation("Description:");
            _logger.LogInformation("    \tA DNS-Proxy with routing for DNS-Request for development with hybrid clouds!");
            _logger.LogInformation("    \tconfig.json, rules.json and hosts,json are used for configure.");
            _logger.LogInformation(LogConsts.DoubleLine);
            _logger.LogInformation("starts up " + ApplicationInformation.DefaultTitle + " ...");
            _logger.LogInformation(LogConsts.DoubleLine);
        }
    }
}
