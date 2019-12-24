using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Dns.Strategies
{
    internal class DnsResolverStrategy : BaseResolverStrategy, IDnsResolverStrategy
    {
        private readonly IDnsResolver DnsClient;

        public DnsResolverStrategy(ILogger<DnsResolverStrategy> logger) : base(logger)
        {
            DnsClient = new RecursiveDnsResolver();
            Order = 2000;
        }

        public async Task<DnsMessage> ResolveAsync(DnsMessage dnsMessage, CancellationToken cancellationToken = default)
        {
            var result = new List<DnsRecordBase>();
            var message = dnsMessage.CreateResponseInstance();

            foreach (DnsQuestion dnsQuestion in dnsMessage.Questions)
            {
                var response = await DnsClient.ResolveAsync<DnsRecordBase>(dnsQuestion.Name, dnsQuestion.RecordType, dnsQuestion.RecordClass)
                    .ConfigureAwait(false);
                result.AddRange(response);
            }
            
            message.AnswerRecords.AddRange(result);
            return message;
        }

    }
}