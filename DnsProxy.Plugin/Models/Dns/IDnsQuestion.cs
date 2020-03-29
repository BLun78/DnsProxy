using System;
using System.Collections.Generic;
using System.Text;

namespace DnsProxy.Plugin.Models.Dns
{
    public interface IDnsQuestion
    {
        /// <summary>
        ///   Domain name
        /// </summary>
        IDomainName Name { get; }

        /// <summary>
        ///   Type of the record
        /// </summary>
        RecordType RecordType { get; }

        /// <summary>
        ///   Class of the record
        /// </summary>
        RecordClass RecordClass { get; }

    }
}
