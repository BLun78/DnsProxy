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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Common;
using DnsProxy.Models.Context;
using DnsProxy.Models.Rules;
using Makaretu.Dns;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Strategies
{
    internal class DohResolverStrategy : BaseResolverStrategy<DohRule>, IDnsResolverStrategy<DohRule>
    {
        private readonly IServiceProvider _serviceProvider;
        private DohClient _dohClient;

        public DohResolverStrategy(
            ILogger<DohResolverStrategy> logger,
            IMemoryCache memoryCache,
            IServiceProvider serviceProvider,
            IDnsContextAccessor dnsContextAccessor)
            : base(logger, dnsContextAccessor, memoryCache)
        {
            _serviceProvider = serviceProvider;
            Order = 1000;
        }

        public override async Task<List<DnsRecordBase>> ResolveAsync(DnsQuestion dnsQuestion,
            CancellationToken cancellationToken)
        {
            LogDnsQuestion(dnsQuestion);
            var result = new List<DnsRecordBase>();

            // https://github.com/curl/curl/wiki/DNS-over-HTTPS
            foreach (var nameServerUri in Rule.NameServerUri)
            {
                _dohClient?.Dispose();
                _dohClient = _serviceProvider.GetService<DohClient>();
                _dohClient.ServerUrl = nameServerUri?.AbsoluteUri;

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
                    LogDnsCanncelQuestion(dnsQuestion, operationCanceledException);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "DoH [{0}]: unexpectet error [{1}]", nameServerUri, e.Message);
                }

                if (result.Any())
                {
                    break;
                }
            }

            if (result.Any())
            {
                var ttl = result.First().TimeToLive;
                if (ttl <= 0)
                {
                    ttl = 10;
                }
                StoreInCache(result, dnsQuestion.Name.ToString(),
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(new TimeSpan(0, 0, ttl)));
            }

            LogDnsQuestionAndResult(dnsQuestion, result);
            return result;
        }

        public override Models.Strategies GetStrategy()
        {
            return Models.Strategies.DoH;
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