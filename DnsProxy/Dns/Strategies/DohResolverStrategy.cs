using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Common;
using Makaretu.Dns;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Dns.Strategies
{
    internal class DohResolverStrategy : BaseResolverStrategy, IDnsResolverStrategy
    {
        private readonly IMemoryCache _memoryCache;
        private readonly DohClient _dohClient;

        public DohResolverStrategy(
            ILogger<DnsResolverStrategy> logger,
            IMemoryCache memoryCache,
            IHttpClientFactory httpClientFactory) : base(logger)
        {
            _memoryCache = memoryCache;
            _dohClient = new DohClient();
            _dohClient.HttpClient = httpClientFactory.CreateClient(nameof(_dohClient));
            _dohClient.ServerUrl = "https://cloudflare-dns.com/dns-query";
            Order = 1000;
        }

        public async Task<DnsMessage> ResolveAsync(DnsMessage dnsMessage, CancellationToken cancellationToken = default)
        {
            var resultMessage = dnsMessage.CreateResponseInstance();
            var requestMessage = new Message();

            foreach (DnsQuestion dnsQuestion in dnsMessage.Questions)
            {
                var question = new Question()
                {
                    Name = dnsQuestion.Name.ToDomainName(),
                    Type = dnsQuestion.RecordType.ToDnsType(),
                    Class = dnsQuestion.RecordClass.ToDnsClass()
                };

                requestMessage.Questions.Add(question);
            }

            var responseMessage = await _dohClient.QueryAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            foreach (ResourceRecord answer in responseMessage.Answers)
            {
                var resultAnswer = answer.ToDnsRecord();
                resultMessage.AnswerRecords.Add(resultAnswer);
            }

            return resultMessage;
        }

    }
}
