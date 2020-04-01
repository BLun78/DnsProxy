using System;
using System.Collections.Generic;
using System.Text;

namespace DnsProxy.Dns.Models
{
    public sealed class DnsServerException : Exception
    {
        public DnsServerException(string message) : base(message)
        {
        }

        public DnsServerException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }
}
