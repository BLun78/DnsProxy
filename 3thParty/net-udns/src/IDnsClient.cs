﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Makaretu.Dns
{
    /// <summary>
    ///     Client interface to a DNS server.
    /// </summary>
    /// <seealso cref="DnsClientBase" />
    public interface IDnsClient : IDisposable, IResolver
    {
        /// <summary>
        ///     Determines if an exception is thrown when a response contains
        ///     an error.
        /// </summary>
        /// <value>
        ///     Defaults to <b>true</b>.
        /// </value>
        /// <remarks>
        ///     When set, an exception is thrown if the response's <see cref="Message.Status" />
        ///     is not <see cref="MessageStatus.NoError" /> or is considered invalid for the
        ///     request.
        /// </remarks>
        bool ThrowResponseError { get; set; }

        /// <summary>
        ///     Get the next ID for a query message.
        /// </summary>
        /// <returns>
        ///     The next <see cref="Message.Id" />
        /// </returns>
        /// <remarks>
        ///     This is thread safe.
        /// </remarks>
        ushort NextQueryId();

        /// <summary>
        ///     Get the IP addresses for the specified name.
        /// </summary>
        /// <param name="name">
        ///     A domain name to resolve.
        /// </param>
        /// <param name="cancel">
        ///     Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException" /> is raised.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task's value
        ///     contains the <see cref="IPAddress" /> sequence for the <paramref name="name" />.
        /// </returns>
        Task<IEnumerable<IPAddress>> ResolveAsync(
            DomainName name,
            CancellationToken cancel = default);

        /// <summary>
        ///     Send a DNS query with the specified name and resource record type.
        /// </summary>
        /// <param name="name">
        ///     A domain name.
        /// </param>
        /// <param name="rtype">
        ///     A resource record type.
        /// </param>
        /// <param name="cancel">
        ///     Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException" /> is raised.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task's value
        ///     contains the response <see cref="Message" />.
        /// </returns>
        /// <remarks>
        ///     Creates a query <see cref="Message" /> and then calls <see cref="QueryAsync(Message, CancellationToken)" />.
        /// </remarks>
        Task<Message> QueryAsync(
            DomainName name,
            DnsType rtype,
            CancellationToken cancel = default);

        /// <summary>
        ///     Send a DNSSEC query with the specified name and resource record type.
        /// </summary>
        /// <param name="name">
        ///     A domain name.
        /// </param>
        /// <param name="rtype">
        ///     A resource record type.
        /// </param>
        /// <param name="cancel">
        ///     Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException" /> is raised.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task's value
        ///     contains the response <see cref="Message" />.
        /// </returns>
        /// <remarks>
        ///     Creates a secure query <see cref="Message" /> and then calls <see cref="QueryAsync(Message, CancellationToken)" />.
        /// </remarks>
        /// <seealso cref="Message.UseDnsSecurity" />
        Task<Message> SecureQueryAsync(
            DomainName name,
            DnsType rtype,
            CancellationToken cancel = default);

        /// <summary>
        ///     Reverse query for an IP address.
        /// </summary>
        /// <param name="address">
        ///     An IP address with an <see cref="IPAddress.AddressFamily" /> of
        ///     <see cref="AddressFamily.InterNetwork" /> or
        ///     <see cref="AddressFamily.InterNetworkV6" />.
        /// </param>
        /// <param name="cancel">
        ///     Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException" /> is raised.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task's value
        ///     is the domain name of <paramref name="address" />.
        /// </returns>
        /// <remarks>
        ///     Performs a reverse lookup with a <see cref="DnsType.PTR" />.  The
        ///     response contains the name(s) of the <paramref name="address" />.
        /// </remarks>
        Task<DomainName> ResolveAsync(
            IPAddress address,
            CancellationToken cancel = default);

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
        /// <remarks>
        ///     The <paramref name="request" /> is sent with UDP.  If no response is
        ///     received (or is truncated) in <see cref="Timeout" />, then it is resent via TCP.
        ///     <para>
        ///         Some home routers have issues with IPv6, so IPv4 servers are tried first.
        ///     </para>
        /// </remarks>
        Task<Message> QueryAsync(
            Message request,
            CancellationToken cancel = default);
    }
}