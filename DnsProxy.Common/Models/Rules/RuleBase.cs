#region Apache License-2.0
// Copyright 2020 Bjoern Lundstroem
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace DnsProxy.Common.Models.Rules
{
    public abstract class RuleBase : IRule, IRuleStrategy
    {
        private readonly object _lockObject;
        private Regex _regex;

        protected RuleBase()
        {
            _lockObject = new object();
        }

        protected RuleBase(IRule rule) : this()
        {
            StrategyName = rule.StrategyName;
            IsEnabled = rule.IsEnabled;
            DomainName = rule.DomainName;
            DomainNamePattern = rule.DomainNamePattern;
            QueryTimeout = rule.QueryTimeout;
            IsCache = rule.IsCache;
            Order = rule.Order;
        }

        public bool IsCache { get; set; }
        public string StrategyName { get; set; }
        public bool IsEnabled { get; set; }
        public string DomainName { get; set; }
        public string DomainNamePattern { get; set; }

        /// <summary>
        ///     Query timeout in milliseconds
        /// </summary>
        public int QueryTimeout { get; set; }

        public abstract Type GetStrategy();

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
                            throw new NotSupportedException(
                                $"On Attribute {nameof(DomainName)} or {nameof(DomainNamePattern)} must be set!");
                        }

                        _regex = new Regex(pattern);
                    }
                }
            }

            return _regex;
        }

        public static List<IPAddress> GetNameServerIpAddresses(List<string> nameServerIpAdresses)
        {
            return new List<IPAddress>(nameServerIpAdresses.Select(IPAddress.Parse));
        }

        public static List<Uri> GetNameServerUri(List<string> nameServerUri)
        {
            return new List<Uri>(nameServerUri.Select(x => new Uri(x)));
        }

        public int Order { get; set; }
    }
}