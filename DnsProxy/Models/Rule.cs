using System.Text.RegularExpressions;

namespace DnsProxy.Models
{
    internal class Rule
    {
        public string DomainName { get; set; }
        public string DomainNamePattern { get; set; }
        public string NameServer { get; set; }
        public string IpAddress { get; set; }

        public Strategies Strategy { get; set; }
        public bool IsEnabled { get; set; }
        public bool CompressionMutation { get; set; }

        /// <summary>
        /// Query timeout in seconds
        /// </summary>
        public int QueryTimeout { get; set; }

        public Regex GetDomainNameRegex()
        {
            return new Regex(DomainNamePattern);
        }
    }
}
