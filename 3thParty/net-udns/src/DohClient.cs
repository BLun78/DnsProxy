﻿using Common.Logging;
using Nito.AsyncEx;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Makaretu.Dns
{
    /// <summary>
    ///     Client to a DNS server over HTTPS.
    /// </summary>
    /// <remarks>
    ///     DNS over HTTPS (DoH) is an experimental protocol for performing remote
    ///     Domain Name System (DNS) resolution via the HTTPS protocol. The goal
    ///     is to increase user privacy and security by preventing eavesdropping and
    ///     manipulation of DNS data by man-in-the-middle attacks.
    ///     <para>
    ///         The <b>DohClient</b> uses the HTTP POST method to hide as much
    ///         information as is possible.  Also, it tends to generate smaller
    ///         requests.
    ///     </para>
    /// </remarks>
    /// <seealso href="https://en.wikipedia.org/wiki/DNS_over_HTTPS" />
    public class DohClient : DnsClientBase
    {
        /// <summary>
        ///     The MIME type for a DNS message encoded in UPD wire format.
        /// </summary>
        /// <remarks>
        ///     Previous drafts defined this as "application/dns-udpwireformat".
        /// </remarks>
        public const string DnsWireFormat = "application/dns-message";

        /// <summary>
        ///     The MIME type for a DNS message encoded in JSON.
        /// </summary>
        public const string DnsJsonFormat = "application/dns-json";

        private static readonly ILog log = LogManager.GetLogger(typeof(DohClient));
        private readonly AsyncLock _dnsServerLock = new AsyncLock();
        private readonly object _httpClientLock = new object();

        private HttpClient _httpClient;

        /// <summary>
        ///     Time to wait for a DNS response.
        /// </summary>
        /// <value>
        ///     The default is 4 seconds.
        /// </value>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(4);

        /// <summary>
        ///     The DNS server to communication with.
        /// </summary>
        /// <value>
        ///     Defaults to "https://cloudflare-dns.com/dns-query".
        /// </value>
        public string ServerUrl { get; set; } = "https://cloudflare-dns.com/dns-query";

        /// <summary>
        ///     The client that sends HTTP requests and receives HTTP responses.
        /// </summary>
        /// <remarks>
        ///     It is best practice to use only one <see cref="HttpClient" /> in an
        ///     application.
        /// </remarks>
        public HttpClient HttpClient
        {
            get
            {
                if (_httpClient == null)
                    lock (_httpClientLock)
                    {
                        _httpClient = new HttpClient();
                    }

                return _httpClient;
            }
            set => _httpClient = value;
        }

        /// <summary>
        ///     Send a DNS query with the specified message.
        /// </summary>
        /// <param name="request">
        ///     A <see cref="Message" /> containing a <see cref="Question" />.
        /// </param>
        /// <param name="cancel">
        ///     Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException" /> is raised.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task's value
        ///     contains the response <see cref="Message" />.
        /// </returns>
        /// <exception cref="IOException">
        ///     When the DNS server returns error status or no response.
        /// </exception>
        public override async Task<Message> QueryAsync(
            Message request,
            CancellationToken cancel = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (log.IsDebugEnabled)
            {
                var names = request.Questions
                    .Select(q => q.Name + " " + q.Type)
                    .Aggregate((current, next) => current + ", " + next);
                log.Debug($"query #{request.Id.ToString(CultureInfo.InvariantCulture)} for '{names}'");
            }

            // Cancel the request when either the timeout is reached or the
            // task is cancelled by the caller.
            HttpResponseMessage httpResponse;
            using (var cts1 = new CancellationTokenSource(Timeout))
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancel, cts1.Token))
            {
                using (var ms = new MemoryStream())
                {
                    request.Write(ms);
                    ms.Position = 0;
                    using (var content = new StreamContent(ms))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue(DnsWireFormat);
                        // Only one writer at a time.
                        using (await _dnsServerLock.LockAsync())
                        {
                            httpResponse = await HttpClient.PostAsync(ServerUrl, content, cts.Token)
                                .ConfigureAwait(false);
                        }
                    }
                }

                // Check the HTTP response.
                httpResponse.EnsureSuccessStatusCode();
                var contentType = httpResponse.Content.Headers.ContentType.MediaType;
                if (DnsWireFormat != contentType)
                    throw new HttpRequestException($"Expected content-type '{DnsWireFormat}' not '{contentType}'.");

                // Check the DNS response.
                var body = await httpResponse.Content.ReadAsStreamAsync()
                    .ConfigureAwait(false);
                var dnsResponse = (Message)new Message().Read(body);
                if (ThrowResponseError)
                    if (dnsResponse.Status != MessageStatus.NoError)
                    {
                        log.Warn($"DNS error '{dnsResponse.Status.ToString("G")}'.");
                        throw new IOException($"DNS error '{dnsResponse.Status.ToString("G")}'.");
                    }

                if (log.IsDebugEnabled)
                    log.Debug($"Got response #{dnsResponse.Id}");
                if (log.IsTraceEnabled)
                    log.Trace(dnsResponse);
                return dnsResponse;
            }
        }

        /// <summary>
        /// Dispose the class
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            _httpClient?.Dispose();
            base.Dispose(disposing);
        }
    }
}