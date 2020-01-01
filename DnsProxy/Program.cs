#region Apache License-2.0

// Copyright 2019 Bjoern Lundstroem
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
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using DnsProxy.Common;
using DnsProxy.Dns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DnsProxy
{
    public sealed class Program
    {
        private static ILogger<Program> Logger;
        private static string _title;
        internal static AWSCredentials AwsCredentials;
        internal static ApplicationInformation ApplicationInformation { get; private set; }
        internal static CancellationTokenSource CancellationTokenSource { get; private set; }
        internal static Configuration Configuration { get; private set; }
        internal static DependencyInjector DependencyInjector { get; private set; }
        internal static IServiceProvider ServiceProvider => DependencyInjector.ServiceProvider;

        internal static string Title
        {
            get => _title;
            set
            {
                _title = value;
                if (Environment.UserInteractive) Console.Title = value;
            }
        }

        private static async Task<int> Main(string[] args)
        {
            try
            {
                Setup(args);
                Title = ApplicationInformation.DefaultTitle;
                ApplicationInformation.LogAssemblyInformation();
                Logger.LogInformation("starts up {DefaultTitle}", ApplicationInformation.DefaultTitle);
                using (var dnsServer = ServiceProvider.GetService<DnsServer>())
                {
                    return await WaitForEndAsync().ConfigureAwait(true);
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);
                await Task.Delay(100).ConfigureAwait(true);
                return await Task.FromResult(1).ConfigureAwait(true);
            }
#pragma warning restore CA1031 // Do not catch general exception types
            finally
            {
                Logger.LogInformation("stop {DefaultTitle}", ApplicationInformation.DefaultTitle);
            }
        }

        private static void Setup(string[] args)
        {
            CancellationTokenSource = new CancellationTokenSource();
            Configuration = new Configuration(args);
            DependencyInjector = new DependencyInjector(Configuration.ConfigurationRoot, CancellationTokenSource);

            Logger = DependencyInjector.ServiceProvider.GetService<ILogger<Program>>();
            ApplicationInformation = DependencyInjector.ServiceProvider.GetService<ApplicationInformation>();
        }

        private static Task<int> WaitForEndAsync()
        {
            return Task.Run(() =>
            {
                var exit = false;
                while (!exit)
                {
                    var key = Console.ReadKey(true);
                    switch (key.Modifiers, key.Key)
                    {
                        case (ConsoleModifiers.Control, ConsoleKey.Q):
                        case (ConsoleModifiers.Control, ConsoleKey.X):
                            exit = true;
                            break;
                    }
                }

                CancellationTokenSource.Cancel();
                CancellationTokenSource.Dispose();
                return 0;
            });
        }
    }
}