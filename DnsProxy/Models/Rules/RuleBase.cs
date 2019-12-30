using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace DnsProxy.Models.Rules
{
    internal abstract class RuleBase : IRule, IRuleStrategy
    {
        protected RuleBase()
        {
            _lockObject = new object();
        }

        protected RuleBase(IRule rule) : this()
        {
            Strategy = rule.Strategy;
            IsEnabled = rule.IsEnabled;
            DomainName = rule.DomainName;
            DomainNamePattern = rule.DomainNamePattern;
            QueryTimeout = rule.QueryTimeout;
        }
        
        public Strategies Strategy { get; set; }
        public bool IsEnabled { get; set; }
        public string DomainName { get; set; }
        public string DomainNamePattern { get; set; }
       
        /// <summary>
        ///     Query timeout in milliseconds
        /// </summary>
        public int QueryTimeout { get; set; }

        public abstract Type GetStraegy();

        public Regex GetDomainNameRegex()
        {
            if (_regex == null)
            {
                lock (_lockObject)
                {
                    if (_regex == null)
                    {
                        string pattern;
                        if (!string.IsNullOrWhiteSpace(DomainNamePattern))
                        {
                            pattern = DomainNamePattern;

                        }
                        else if (!string.IsNullOrWhiteSpace(DomainName))
                        {
                            pattern = $"^{DomainName.Replace(".", @"\.", StringComparison.InvariantCulture)}$";
                        }
                        else
                        {
                            throw new NotSupportedException($"On Attribute {nameof(DomainName)} or {nameof(DomainNamePattern)} must be set!");
                        }
                        _regex = new Regex(pattern);
                    }
                }
            }
            return _regex;
        }
        private Regex _regex;
        private readonly object _lockObject;

        public static List<IPAddress> GetNameServerIpAddresses(List<string> nameServerIpAdresses)
        {
            return new List<IPAddress>(nameServerIpAdresses.Select(IPAddress.Parse));
        }

        public static List<Uri> GetNameServerUri(List<string> nameServerUri)
        {
            return new List<Uri>(nameServerUri.Select(x => new Uri(x)));
        }
    }
}