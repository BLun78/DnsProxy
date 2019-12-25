using ARSoft.Tools.Net.Dns;
using DnsProxy.Common;
using Makaretu.Dns;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace DnsProxy.Dns.Strategies
{
    internal class MulticastResolverStrategy : BaseResolverStrategy, IDnsResolverStrategy
    {

        public MulticastResolverStrategy(ILogger<DnsResolverStrategy> logger) : base(logger)
        {
            Order = 5000;
        }

        public async Task<DnsMessage> ResolveAsync(DnsMessage dnsMessage, CancellationToken cancellationToken = default)
        {
            var message = dnsMessage.CreateResponseInstance();

            foreach (DnsQuestion dnsQuestion in dnsMessage.Questions)
            {
                var query = new Message();
                query.Questions.Add(new Question
                {
                    Name = dnsQuestion.Name.ToString(),
                    Type = DnsType.ANY
                });
                var cancellation = new CancellationTokenSource(3000);

                using (var mdns = new MulticastService())
                {
                    mdns.Start();
                    var response = await mdns.ResolveAsync(query, cancellation.Token).ConfigureAwait(false);

                    foreach (ResourceRecord answer in response.Answers)
                    {
                        message.AnswerRecords.Add(answer.ToDnsRecord());
                    }
                }
            }

            return message;
        }
    }
}