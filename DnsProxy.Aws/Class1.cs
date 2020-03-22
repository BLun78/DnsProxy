using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DnsProxy.Aws.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DnsProxy.Aws
{
    public static class AwsVpcExtensions
    {

        private static IOptionsMonitor<AwsSettings> _awsSettingsOptionsMonitor;

        static AwsVpcExtensions()
        {
            _awsSettingsOptionsMonitor = ServiceProvider.GetService<IOptionsMonitor<AwsSettings>>();
            _awsSettingsOptionsMonitorListener = _awsSettingsOptionsMonitor.OnChange(settings => _requestNewMfa = true);
        }

        public static async Task CheckAwsVpc(IServiceProvider serviceProvider, ILogger logger)
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
                    logger.LogInformation("No AWS config found!");
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }
        }

        public static async Task CheckForAwsMfaAsync()
        {
            try
            {
                var awsSettings = _awsSettingsOptionsMonitor.CurrentValue;
                if (!awsSettings.UserAccounts.Any()) return;

                var awsContext = new AwsContext(awsSettings);
                var mfa = new AwsMfa();

                foreach (var userAccount in awsContext.AwsSettings.UserAccounts)
                {
                    var mfsToken = await mfa.GetMfaAsync(userAccount, CancellationTokenSource.Token)
                        .ConfigureAwait(false);

                    await mfa.CreateAwsCredentialsAsync(userAccount, mfsToken, CancellationTokenSource.Token)
                        .ConfigureAwait(false);

                    foreach (var role in userAccount.Roles.Where(x => x.DoScan == true))
                        await mfa.AssumeRoleAsync(userAccount, role, CancellationTokenSource.Token)
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
