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

using Amazon.EC2;
using Amazon.Runtime;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Aws.Adapter;
using DnsProxy.Aws.Models;
using DnsProxy.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DnsProxy.Common.Cache;
using DomainName = ARSoft.Tools.Net.DomainName;

namespace DnsProxy.Aws
{
    internal class AwsVpcManager
    {
        private readonly ILogger<AwsVpcManager> _logger;
        private readonly AwsVpcReader _awsVpcReader;
        private readonly CacheManager _cacheManager;
        private readonly IEnumerable<AwsAdapterBase> _awsAdapter;
        private readonly List<string> _proxyBypassList;

        public AwsVpcManager(
            ILogger<AwsVpcManager> logger,
            AwsVpcReader awsVpcReader,
            CacheManager cacheManager,
            IEnumerable<AwsAdapterBase> awsAdapter)
        {
            _logger = logger;
            _awsVpcReader = awsVpcReader;
            _cacheManager = cacheManager;
            _awsAdapter = awsAdapter;
            _proxyBypassList = new List<string>();
        }

        public async Task StartReadingVpcAsync(AwsContext awsContext, CancellationToken cancellationToken)
        {
            if (awsContext?.AwsSettings?.UserAccounts == null) return;
            _proxyBypassList.Clear();
            try
            {
                foreach (var awsSettingsUserAccount in awsContext.AwsSettings.UserAccounts)
                {
                    if (awsSettingsUserAccount.DoScan)
                        await ReadVpcAsync(awsSettingsUserAccount.AwsCredentials, awsSettingsUserAccount,
                                cancellationToken)
                            .ConfigureAwait(false);

                    foreach (var userRoleExtended in awsSettingsUserAccount.Roles)
                        if (userRoleExtended.DoScan)
                            await ReadVpcAsync(userRoleExtended.AwsCredentials, userRoleExtended, cancellationToken)
                                .ConfigureAwait(false);
                }

                var path = Path.Combine(Environment.CurrentDirectory, awsContext.AwsSettings.OutputFileName);
                _logger.LogInformation("AWS write ProxyBypassList in File: [{0}]", path);

                await File.WriteAllTextAsync(path, CreateContentForProxyBypassFile(), Encoding.UTF8, cancellationToken)
                    .ConfigureAwait(false);

                _logger.LogInformation("AWS import finished!");
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        private string CreateContentForProxyBypassFile()
        {
            var fiddler = $"[Fiddler]{Environment.NewLine}{string.Join(Environment.NewLine, _proxyBypassList)}";
            var browser = $"[Browser]{Environment.NewLine}{string.Join(@";", _proxyBypassList)}";
            var fileContent = $"{browser}{Environment.NewLine}{Environment.NewLine}{fiddler}{Environment.NewLine}";
            return fileContent;
        }

        /// <summary>
        ///     Reads the VPC for cacheing DNS-Records to overrides public IPAddresses
        ///     like SQS.[zone].amazonaws.com to 10.10.10.10
        /// </summary>
        /// <param name="awsCredentials">AwsCredintials for read the VPC</param>
        /// <param name="awsScanRules">Rules for Scaning</param>
        /// <param name="cancellationToken">Task Cancellation Toke</param>
        /// <returns>only a Task</returns>
        private async Task ReadVpcAsync(AWSCredentials awsCredentials, IAwsScanRules awsScanRules,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = new List<DnsRecordBase>();

                var endpoints = await _awsVpcReader.ReadEndpoints(awsCredentials, awsScanRules, cancellationToken)
                    .ConfigureAwait(false);

                foreach (AwsAdapterBase adapter in _awsAdapter)
                {
                    var adapterResult = await adapter
                        .GetAdapterResultAsync(awsCredentials, endpoints, cancellationToken)
                        .ConfigureAwait(false);
                    result.AddRange(adapterResult.DnsRecords);
                    _proxyBypassList.AddRange(adapterResult.ProxyBypassList);
                }

                var groupedResult = (from record in result
                    group record by record.Name.ToString()
                    into newRecords
                    orderby newRecords.Key
                    select newRecords).ToList();
                foreach (var dnsRecordBases in groupedResult)
                {
                    var dnsQuestion = new DnsQuestion(DomainName.Parse(dnsRecordBases.Key), RecordType.A,
                        RecordClass.INet);
                    StoreInCache(dnsQuestion, dnsRecordBases.ToList());
                }
            }
            catch (AmazonEC2Exception aee)
            {
                if (aee.ErrorCode == "UnauthorizedOperation")
                    _logger.LogError(aee, "AWS ErrorCode=[{0}] ==> {1}", aee.ErrorCode, aee.Message);
                else
                    throw;
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, e.Message);
                throw;
            }
        }

        //private void StoreInCache(DnsQuestion dnsQuestion, List<DnsRecordBase> data,
        //    MemoryCacheEntryOptions cacheEntryOptions)
        //{
        //    _cacheManager.StoreInCache(dnsQuestion, data, cacheEntryOptions);
        //}

        private void StoreInCache(DnsQuestion dnsQuestion, List<DnsRecordBase> data)
        {
            _cacheManager.StoreInCache(dnsQuestion, data);
        }
    }
}