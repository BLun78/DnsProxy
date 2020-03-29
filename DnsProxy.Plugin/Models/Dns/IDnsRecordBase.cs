using System;
using System.Collections.Generic;
using System.Text;

namespace DnsProxy.Plugin.Models.Dns
{
    public interface IDnsRecordBase
    {
        /// <summary>
        ///   Seconds which a record should be cached at most
        /// </summary>
        int TimeToLive { get; }

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

        /// <summary>
        ///   Returns the textual representation of a record
        /// </summary>
        /// <returns> Textual representation </returns>
        string ToString();
    }
}
