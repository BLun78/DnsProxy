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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Common;
using DnsProxy.Models;
using DnsProxy.Models.Context;
using DnsProxy.Models.Rules;
using Makaretu.Dns;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DnsProxy.Strategies
{
    internal class DohResolverStrategy : BaseResolverStrategy<DohRule>, IDnsResolverStrategy<DohRule>
    {
        private static DohClient _dohClient;

        public DohResolverStrategy(
            ILogger<DohResolverStrategy> logger,
            IMemoryCache memoryCache,
            HttpClient httpClient,
            IDnsContextAccessor dnsContextAccessor,
            IOptionsMonitor<CacheConfig> cacheConfigOptionsMonitor)
            : base(logger, dnsContextAccessor, memoryCache, cacheConfigOptionsMonitor)
        {
            _dohClient = new DohClient
            {
                HttpClient = httpClient,
                ThrowResponseError = false
            };
            Order = 1000;
        }

        public override async Task<List<DnsRecordBase>> ResolveAsync(DnsQuestion dnsQuestion,
            CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            LogDnsQuestion(dnsQuestion, stopwatch);
            var result = new List<DnsRecordBase>();

            // https://github.com/curl/curl/wiki/DNS-over-HTTPS
            foreach (var nameServerUri in Rule.NameServerUri)
            {
                _dohClient.ServerUrl = nameServerUri?.AbsoluteUri;
                _dohClient.Timeout = TimeSpan.FromSeconds(value: Rule.QueryTimeout / 1000);

                var question = new Question
                {
                    Name = dnsQuestion.Name.ToDomainName(),
                    Type = dnsQuestion.RecordType.ToDnsType(),
                    Class = dnsQuestion.RecordClass.ToDnsClass()
                };

                try
                {
                    var responseMessage =
                        await _dohClient.SecureQueryAsync(question.Name, question.Type, cancellationToken).ConfigureAwait(false);

                    foreach (var answer in responseMessage.Answers)
                    {
                        var resultAnswer = answer.ToDnsRecord();
                        result.Add(resultAnswer);
                    }
                }
                catch (IOException ioe)
                {
                    HandelIoException(ioe, nameServerUri);
                }
                catch (OperationCanceledException operationCanceledException)
                {
                    LogDnsCanncelQuestion(dnsQuestion, operationCanceledException, stopwatch);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "DoH [{0}]: unexpected error [{1}]", nameServerUri, e.Message);
                }

                if (result.Any())
                {
                    break;
                }
            }

            if (result.Any())
            {
                var ttl = result.First().TimeToLive;
                if (ttl <= CacheConfigOptionsMonitor.CurrentValue.MinimalTimeToLiveInSeconds)
                {
                    ttl = CacheConfigOptionsMonitor.CurrentValue.MinimalTimeToLiveInSeconds;
                }
                StoreInCache(dnsQuestion.RecordType, result, dnsQuestion.Name.ToString(),
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(ttl)));
            }

            LogDnsQuestionAndResult(dnsQuestion, result, stopwatch);
            return result;
        }

        public override Models.Strategies GetStrategy()
        {
            return Models.Strategies.DoH;
        }
        
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _dohClient?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
            }
            base.Dispose(disposing);
        }

        private void HandelIoException(IOException ioe, Uri nameServerUri)
        {
            var message = ioe.Message.Split("'");
            if (message.Length == 3)
            {
                if (Enum.TryParse(message[1], out MessageStatus messageStatus))
                {
                    switch (messageStatus)
                    {
                        case MessageStatus.NoError:
                            break;
                        case MessageStatus.FormatError:
                            break;
                        case MessageStatus.ServerFailure:
                            break;
                        case MessageStatus.NameError:
                            break;
                        case MessageStatus.NotImplemented:
                            break;
                        case MessageStatus.Refused:
                            break;
                        case MessageStatus.YXDomain:
                            break;
                        case MessageStatus.YXRRSet:
                            break;
                        case MessageStatus.NXRRSet:
                            break;
                        case MessageStatus.NotAuthoritative:
                            break;
                        case MessageStatus.NotZone:
                            break;
                        case MessageStatus.BadVersion:
                            break;
                        case MessageStatus.BadKey:
                            break;
                        case MessageStatus.BadTime:
                            break;
                        case MessageStatus.BADMODE:
                            break;
                        case MessageStatus.BADNAME:
                            break;
                        case MessageStatus.BADALG:
                            break;
                        default:
                            Logger.LogWarning(ioe, "DoH [{0}]: unexpectet IO-error [{1}]", nameServerUri, ioe.Message);
                            break;
                    }
                }
                else
                {
                    Logger.LogWarning(ioe, "DoH [{0}]: unexpectet IO-error [{1}]", nameServerUri, ioe.Message);
                }
            }
            else
            {
                Logger.LogWarning(ioe, "DoH [{0}]: unexpectet IO-error [{1}]", nameServerUri, ioe.Message);
            }
        }
    }
}