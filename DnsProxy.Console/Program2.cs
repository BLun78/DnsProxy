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
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DnsProxy.Console.Common;
using DnsProxy.Console.Common.Plugin;
using DnsProxy.Plugin;
using DnsProxy.Plugin.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DnsProxy.Console
{
    public sealed class Program2
    {
        private static ILogger<Program2> _logger;
        private static string _title;

        private static CancellationTokenSource CancellationTokenSource { get; set; }
        private static DependencyInjector DependencyInjector { get; set; }
        private static IServiceProvider ServiceProvider => DependencyInjector.ServiceProvider;

        public static IConfigurationRoot Configuration
        {
            get
            {
                IConfigurationBuilder builder = new ConfigurationBuilder();
                builder = builder.SetBasePath(Directory.GetCurrentDirectory());

                if (PluginManager != null)
                {
                    foreach (IDnsProxyConfiguration dnsProxyConfiguration in PluginManager.Configurations)
                    {
                        builder = dnsProxyConfiguration.ConfigurationBuilder(builder);
                    }
                }

                builder = builder.AddEnvironmentVariables();
                return builder.Build();
            }
        }

        private static PluginManager PluginManager { get; set; }

        public static async Task<int> Main(string[] args)
        {
            //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            SerilogExtensions.SetupSerilog(null);
            try
            {
                PluginManager = new PluginManager(Log.Logger);
                var containerOptions = Configuration.SetupSerilog();
                PluginManager.RegisterDependencyRegistration(Configuration);

#if true
                Serilog.Debugging.SelfLog.Enable(System.Console.Error);
                Serilog.Debugging.SelfLog.Enable(System.Console.WriteLine);
#endif

                Setup(args);

                //using (var dnsServer = ServiceProvider.GetService<DnsServer>())
                //{
                //    await AwsVpcExtensions.CheckAwsVpc().ConfigureAwait(false);

                //    return await WaitForEndAsync().ConfigureAwait(false);
                //}

                return 0;
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
                Log.CloseAndFlush();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            System.Console.WriteLine("Resolving...");
            return typeof(Program2).Assembly;
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


            //Configuration = new Configuration(args);
            //DependencyInjector = new DependencyInjector(
            //    Configuration.ConfigurationRoot,
            //    typeof(Program).Assembly,
            //    CancellationTokenSource);

            //_logger = DependencyInjector.ServiceProvider.GetService<ILogger<Program>>();
            //ApplicationInformation = DependencyInjector.ServiceProvider.GetService<ApplicationInformation>();

            //CreateHeader();
        }

    }
}
