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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DnsProxy.Common;
using DnsProxy.Common.Aws;
using DnsProxy.Dns;
using DnsProxy.Models.Aws;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DnsProxy.Console
{
    public sealed class Program
    {
        private static volatile bool RequestNewMfa;
        private static ILogger<Program> Logger;
        private static string _title;

        private static IOptionsMonitor<AwsSettings> AwsSettingsOptionsMonitor;
        private static IDisposable AwsSettingsOptionsMonitorListener;
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
                if (Environment.UserInteractive) System.Console.Title = value;
            }
        }

        private static async Task<int> Main(string[] args)
        {
            try
            {
                Setup(args);

                using (var dnsServer = ServiceProvider.GetService<DnsServer>())
                {
                    await CheckAwsVpc().ConfigureAwait(false);

                    return await WaitForEndAsync().ConfigureAwait(false);
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);
                await Task.Delay(100).ConfigureAwait(false);
                return await Task.FromResult(1).ConfigureAwait(false);
            }
#pragma warning restore CA1031 // Do not catch general exception types
            finally
            {
                Logger.LogInformation("stop {DefaultTitle}", ApplicationInformation.DefaultTitle);
            }
        }

        private static void CreateHeader()
        {
            Title = ApplicationInformation.DefaultTitle;
            System.Console.WriteLine("==================================================================================");
            ApplicationInformation.LogAssemblyInformation();
            System.Console.WriteLine("==================================================================================");
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
            System.Console.WriteLine("==================================================================================");
            var color = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ConsoleColor.DarkYellow;
            System.Console.WriteLine("\t[strg]+[x] or [strg]+[q] = exit Application");
            System.Console.WriteLine("\t[strg]+[r] = reload AWS-VPC's with new mfa");
            System.Console.WriteLine("\t[strg]+[h] = show this help / information");
            System.Console.ForegroundColor = color;
            System.Console.WriteLine("==================================================================================");
            System.Console.WriteLine("Description:");
            System.Console.WriteLine("\tA DNS-Proxy with routing for DNS-Request for development with hybrid clouds!");
            System.Console.WriteLine("\tconfig.json, rules.json and hosts,json are used for configure.");
            System.Console.WriteLine("==================================================================================");
            System.Console.WriteLine("starts up " + ApplicationInformation.DefaultTitle + " ...");
            System.Console.WriteLine("==================================================================================");
        }

        private static async Task CheckAwsVpc()
        {
            try
            {
                var awsSettings = ServiceProvider.GetService<IOptions<AwsSettings>>();
                if (awsSettings?.Value != null
                    && !string.IsNullOrWhiteSpace(awsSettings.Value.Region)
                    && awsSettings.Value.UserAccounts != null
                    && awsSettings.Value.UserAccounts.Any())
                {
                    await CheckForAwsMfaAsync().ConfigureAwait(false);
                    var aws = ServiceProvider.GetService<AwsVpcManager>();
                    await aws.StartReadingVpcAsync(CancellationTokenSource.Token).ConfigureAwait(false);
                }
                else
                {
                    Logger.LogInformation("No AWS config found!");
                }

            }
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);
            }
        }

        private static void Setup(string[] args)
        {
            CancellationTokenSource = new CancellationTokenSource();
            Configuration = new Configuration(args);
            DependencyInjector = new DependencyInjector(Configuration.ConfigurationRoot, CancellationTokenSource);

            Logger = DependencyInjector.ServiceProvider.GetService<ILogger<Program>>();
            ApplicationInformation = DependencyInjector.ServiceProvider.GetService<ApplicationInformation>();
            AwsSettingsOptionsMonitor = ServiceProvider.GetService<IOptionsMonitor<AwsSettings>>();
            AwsSettingsOptionsMonitorListener = AwsSettingsOptionsMonitor.OnChange(settings => RequestNewMfa = true);

            CreateHeader();
        }

        private static async Task<int> WaitForEndAsync()
        {
            return await Task.Run(async () =>
            {
                var exit = false;
                while (!exit)
                {
                    if (RequestNewMfa)
                    {
                        await CheckAwsVpc().ConfigureAwait(false);
                        RequestNewMfa = false;
                    }

                    var key = System.Console.ReadKey(true);
                    switch (key.Modifiers, key.Key)
                    {
                        case (ConsoleModifiers.Control, ConsoleKey.H):
                            CreateHeader();
                            break;
                        case (ConsoleModifiers.Control, ConsoleKey.R):
                            RequestNewMfa = true;
                            break;
                        case (ConsoleModifiers.Control, ConsoleKey.Q):
                        case (ConsoleModifiers.Control, ConsoleKey.X):
                            exit = true;
                            break;
                    }
                }

                CancellationTokenSource?.Cancel();
                CancellationTokenSource?.Dispose();
                AwsSettingsOptionsMonitorListener?.Dispose();
                return 0;
            }).ConfigureAwait(false);
        }

        private static async Task CheckForAwsMfaAsync()
        {
            try
            {
                var awsSettings = AwsSettingsOptionsMonitor.CurrentValue;
                if (!awsSettings.UserAccounts.Any()) return;

                var awsContext = new AwsContext(awsSettings);
                var mfa = new AwsMfa();

                foreach (var userAccount in awsContext.AwsSettings.UserAccounts)
                {
                    var mfsToken = await mfa.GetMfaAsync(userAccount, CancellationTokenSource.Token)
                        .ConfigureAwait(false);

                    await mfa.CreateAwsCredentialsAsync(userAccount, mfsToken, CancellationTokenSource.Token)
                        .ConfigureAwait(false);

                    foreach (var role in userAccount.Roles)
                    {
                        await mfa.AssumeRoleAsync(userAccount, role, CancellationTokenSource.Token)
                            .ConfigureAwait(false);
                    }
                }

                DependencyInjector.AwsContext = awsContext;
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
        }
    }
}