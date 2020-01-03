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
using System.Threading;
using System.Threading.Tasks;
using DnsProxy.Common;
using DnsProxy.Common.Aws;
using DnsProxy.Dns;
using DnsProxy.Models;
using DnsProxy.Models.Aws;
using DnsProxy.Strategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DnsProxy
{
    public sealed class Program
    {
        private static volatile bool RequestNewMfa;
        private static ILogger<Program> Logger;
        private static string _title;
        internal static ApplicationInformation ApplicationInformation { get; private set; }

        private static IOptionsMonitor<AwsSettings> AwsSettingsOptionsMonitor;
        private static IDisposable AwsSettingsOptionsMonitorListner;

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
                    await CheckForAwsMfaAsync().ConfigureAwait(true);
                    var aws = ServiceProvider.GetService<AwsVpcManager>();
                    await aws.StartReadingVpcAsync(CancellationTokenSource.Token).ConfigureAwait(true);

                    return await WaitForEndAsync(dnsServer).ConfigureAwait(true);
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
            AwsSettingsOptionsMonitor = ServiceProvider.GetService<IOptionsMonitor<AwsSettings>>();
            AwsSettingsOptionsMonitorListner = AwsSettingsOptionsMonitor.OnChange(settings => RequestNewMfa = true);
        }

        private static async Task<int> WaitForEndAsync(DnsServer dnsServer)
        {
            return await Task.Run(async () =>
            {
                var exit = false;
                while (!exit)
                {
                    if (RequestNewMfa)
                    {
                        await CheckForAwsMfaAsync().ConfigureAwait(true);
                        RequestNewMfa = false;
                    }

                    var key = Console.ReadKey(true);
                    switch (key.Modifiers, key.Key)
                    {
                        case (ConsoleModifiers.Control, ConsoleKey.Q):
                        case (ConsoleModifiers.Control, ConsoleKey.X):
                            exit = true;
                            break;
                    }
                }

                CancellationTokenSource?.Cancel();
                CancellationTokenSource?.Dispose();
                AwsSettingsOptionsMonitorListner?.Dispose();
                return 0;
            }).ConfigureAwait(true);
        }

        private static async Task CheckForAwsMfaAsync()
        {
            try
            {
                var awsSettings = AwsSettingsOptionsMonitor.CurrentValue;
                var rules = ServiceProvider.GetService<IOptionsMonitor<RulesConfig>>().CurrentValue;
                //var aws = rules.Rules.FirstOrDefault(x => x.IsEnabled == false && x.Strategy == Models.Strategies.Aws);

                //if (aws == null) return;

                var awsContext = new AwsContext(awsSettings);
                var mfa = new AwsMfa();

                foreach (var userAccount in awsContext.AwsSettings.UserAccounts)
                {
                    var mfsToken = await mfa.GetMfaAsync(userAccount, CancellationTokenSource.Token)
                        .ConfigureAwait(true);

                    await mfa.CreateAwsCredentialsAsync(userAccount, mfsToken, CancellationTokenSource.Token).ConfigureAwait(true);

                    foreach (var role in userAccount.Roles)
                    {
                        await mfa.AssumeRoleAsync(userAccount, role, CancellationTokenSource.Token).ConfigureAwait(true);
                    }
                }

                DependencyInjector.AwsContext = awsContext;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}