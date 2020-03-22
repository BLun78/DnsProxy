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

using DnsProxy.Aws.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DnsProxy.Aws
{
    public static class AwsVpcExtensions
    {

        private static readonly IOptionsMonitor<AwsSettings> _awsSettingsOptionsMonitor;
        private static readonly IDisposable _awsSettingsOptionsMonitorListener;

#pragma warning disable CA1810 // Initialize reference type static fields inline
        static AwsVpcExtensions()
#pragma warning restore CA1810 // Initialize reference type static fields inline
        {
            _awsSettingsOptionsMonitor = ServiceProvider.GetService<IOptionsMonitor<AwsSettings>>();
            _awsSettingsOptionsMonitorListener = _awsSettingsOptionsMonitor.OnChange(settings => _requestNewMfa = true);
        }

        public static async Task CheckAwsVpc(IServiceProvider serviceProvider, ILogger logger, CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                var awsSettings = serviceProvider.GetService<IOptions<AwsSettings>>();
                if (awsSettings?.Value != null
                    && !string.IsNullOrWhiteSpace(awsSettings.Value.Region)
                    && awsSettings.Value.UserAccounts != null
                    && awsSettings.Value.UserAccounts.Any())
                {
                    await CheckForAwsMfaAsync().ConfigureAwait(false);
                    var aws = serviceProvider.GetService<AwsVpcManager>();
                    await aws.StartReadingVpcAsync(cancellationTokenSource.Token).ConfigureAwait(false);
                }
                else
                {
                    logger.LogInformation("No AWS config found!");
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }
        }

        public static async Task CheckForAwsMfaAsync(CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                var awsSettings = _awsSettingsOptionsMonitor.CurrentValue;
                if (!awsSettings.UserAccounts.Any()) return;

                var awsContext = new AwsContext(awsSettings);
                var mfa = new AwsMfa();

                foreach (var userAccount in awsContext.AwsSettings.UserAccounts)
                {
                    var mfsToken = await mfa.GetMfaAsync(userAccount, cancellationTokenSource.Token)
                        .ConfigureAwait(false);

                    await mfa.CreateAwsCredentialsAsync(userAccount, mfsToken, cancellationTokenSource.Token)
                        .ConfigureAwait(false);

                    foreach (var role in userAccount.Roles.Where(x => x.DoScan == true))
                        await mfa.AssumeRoleAsync(userAccount, role, cancellationTokenSource.Token)
                            .ConfigureAwait(false);
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
