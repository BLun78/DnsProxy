#region Apache License-2.0

// Copyright 2019 Bjoern Lundstroem
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
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using DnsProxy.Models.Rules;

namespace DnsProxy.Models
{
    internal class Rule : IRule
    {
        public List<string> NameServer { get; set; }
        public string IpAddress { get; set; }
        public bool CompressionMutation { get; set; }

        /// <summary>
        ///     Query timeout in milliseconds
        /// </summary>
        public int QueryTimeout { get; set; }

        public Strategies Strategy { get; set; }
        public bool IsEnabled { get; set; }

        public string DomainName { get; set; }
        public string DomainNamePattern { get; set; }

        public Type GetStraegy()
        {
            throw new NotImplementedException();
        }

        public Regex GetDomainNameRegex()
        {
            throw new NotImplementedException();
        }

        [SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "<Pending>")]
        public IRule GetInternalRule()
        {
            switch (Strategy)
            {
                case Strategies.Hosts:
                    return new HostsRule(this);
                case Strategies.InternalNameServer:
                    return new InternalNameServerRule(this);
                case Strategies.Dns:
                    return new DnsRule(this);
                case Strategies.DoH:
                    return new DohRule(this);
                case Strategies.Aws:
                    return new AwsRule(this);
                default:
                    throw new ArgumentOutOfRangeException(nameof(Strategy), Strategy, null);
            }
        }
    }
}