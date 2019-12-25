using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace DnsProxy.Models.Rules
{
    internal abstract class RuleBase : IRule
    {
        private readonly IRule _rule;

        protected RuleBase(IRule rule)
        {
            _rule = rule;
        }

        public Strategies Strategy => _rule.Strategy;
        public bool IsEnabled => _rule.IsEnabled;
        public string DomainName => _rule.DomainName;
        public string DomainNamePattern => _rule.DomainNamePattern;

        public Regex GetDomainNameRegex()
        {
            return new Regex(DomainNamePattern);
        }

        public static List<IPAddress> GetNameServerIpAddresses(List<string> nameServerIpAdresses)
        {
            return new List<IPAddress>(nameServerIpAdresses.Select(IPAddress.Parse));
        }
    }
}