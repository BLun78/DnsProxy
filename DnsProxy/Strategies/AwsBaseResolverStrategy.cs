﻿#region Apache License-2.0

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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Models.Aws;
using DnsProxy.Models.Context;
using DnsProxy.Models.Rules;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Strategies
{
    internal abstract class AwsBaseResolverStrategy<TRule, TClient> : BaseResolverStrategy<TRule>
        where TRule : IRule
        where TClient : AmazonServiceClient, IAmazonService, IDisposable, new()
    {
        protected readonly AwsContext AwsContext;
        protected readonly ClientConfig AwsClientConfig;
        protected TClient AwsClient;

        protected AwsBaseResolverStrategy(ILogger<AwsBaseResolverStrategy<TRule, TClient>> logger,
            IDnsContextAccessor dnsContextAccessor,
            IMemoryCache memoryCache,
            AwsContext awsContext,
            ClientConfig awsClientConfig)
            : base(logger, dnsContextAccessor, memoryCache)
        {
            AwsContext = awsContext;
            AwsClientConfig = awsClientConfig;
        }

        public override async Task<List<DnsRecordBase>> ResolveAsync(DnsQuestion dnsQuestion, CancellationToken cancellationToken)
        {
            var result = new List<DnsRecordBase>();
            AwsClient?.Dispose();
            foreach (var awsSettingsUserAccount in AwsContext.AwsSettings.UserAccounts)
            {
                await DoScan(dnsQuestion, cancellationToken, awsSettingsUserAccount, result).ConfigureAwait(false);
                foreach (var userRoleExtended in awsSettingsUserAccount.Roles)
                {
                    await DoScan(dnsQuestion, cancellationToken, userRoleExtended, result).ConfigureAwait(false);
                }
            }
            AwsClient?.Dispose();
            return result;
        }

        private async Task DoScan(DnsQuestion dnsQuestion, CancellationToken cancellationToken, IAwsDoScan awsDoScan, List<DnsRecordBase> result)
        {
            if (awsDoScan.DoScan)
            {
                AwsClient?.Dispose();
                AwsClient = (TClient)Activator.CreateInstance(typeof(TClient), awsDoScan.AwsCredentials, AwsClientConfig);
                var userAccountResult = await AwsResolveAsync(dnsQuestion, awsDoScan.ScanVpcIds, cancellationToken)
                    .ConfigureAwait(true);
                result.AddRange(userAccountResult);
            }
        }

        public abstract Task<List<DnsRecordBase>> AwsResolveAsync(DnsQuestion dnsQuestion, List<string> ScanVpcIds, CancellationToken cancellationToken);

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    AwsClient?.Dispose();
                    base.Dispose(disposing);
                }
            }
        }
    }
}