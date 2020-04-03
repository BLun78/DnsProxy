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

using DnsProxy.Aws.Models.Rules;
using DnsProxy.Common.Models.Rules;
using DnsProxy.Plugin;
using DnsProxy.Plugin.Configuration;
using DnsProxy.Plugin.Models.Rules;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DnsProxy.Aws.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DnsProxy.Aws
{
    public class AwsPlugin : IPlugin
    {
        private IServiceProvider _serviceProvider;
        private ILogger<AwsPlugin> _logger;
        private AwsVpcManager _awsVpcManager;
        private CancellationTokenSource _cancellationTokenSource;
        private IOptionsMonitor<AwsSettings> _awsSettingsOptionsMonitor;
        private AwsContext _awsContext;

        public string PluginName => "DnsProxy.Aws";
        public Type DependencyRegistration => typeof(AwsDependencyRegistration);
        public IDnsProxyConfiguration DnsProxyConfiguration => new AwsDnsProxyConfiguration();
        public IRuleFactory RuleFactory => new RuleFactory(this.Rules);
        public Type[] Rules => new[] { typeof(AwsApiGatewayRule), typeof(AwsDocDbRule), typeof(AwsElasticCacheRule) };

        public void GetHelp(ILogger logger)
        {
            logger?.LogInformation("\t[strg]+[r] = reload AWS-VPC's with new mfa");
        }

        public async Task CheckKeyAsync(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Modifiers, keyInfo.Key)
            {
                case (ConsoleModifiers.Control, ConsoleKey.R):
                    if (_awsSettingsOptionsMonitor?.CurrentValue != null
                        && !string.IsNullOrWhiteSpace(_awsSettingsOptionsMonitor.CurrentValue.Region)
                        && _awsSettingsOptionsMonitor.CurrentValue.UserAccounts != null
                        && _awsSettingsOptionsMonitor.CurrentValue.UserAccounts.Any())
                    {
                        await CheckForAwsMfaAsync(_cancellationTokenSource).ConfigureAwait(false);
                        await _awsVpcManager.StartReadingVpcAsync(_awsContext, _cancellationTokenSource.Token).ConfigureAwait(false);
                    }
                    else
                    {
                        _logger.LogInformation("No AWS config found!");
                    }
                    break;
            }
        }

        public async Task CheckForAwsMfaAsync(CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                var awsSettings = _awsSettingsOptionsMonitor.CurrentValue;
                if (!awsSettings.UserAccounts.Any()) return;

                var mfa = new AwsMfa();

                foreach (var userAccount in _awsContext.AwsSettings.UserAccounts)
                {
                    var mfsToken = await mfa.GetMfaAsync(userAccount, cancellationTokenSource.Token)
                        .ConfigureAwait(false);

                    await mfa.CreateAwsCredentialsAsync(userAccount, mfsToken, cancellationTokenSource.Token)
                        .ConfigureAwait(false);

                    foreach (var role in userAccount.Roles.Where(x => x.DoScan == true))
                        await mfa.AssumeRoleAsync(userAccount, role, cancellationTokenSource.Token)
                            .ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        public void InitialPlugin(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            try
            {
                _awsContext = (AwsContext)serviceProvider.GetService(typeof(AwsContext));
                
                _awsSettingsOptionsMonitor = (IOptionsMonitor<AwsSettings>)serviceProvider.GetService(typeof(IOptionsMonitor<AwsSettings>));
                _logger = (ILogger<AwsPlugin>)_serviceProvider.GetService(typeof(ILogger<AwsPlugin>));
                _awsVpcManager = (AwsVpcManager)_serviceProvider.GetService(typeof(AwsVpcManager));
                _cancellationTokenSource = (CancellationTokenSource)_serviceProvider.GetService(typeof(CancellationTokenSource));
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

        }
    }
}
