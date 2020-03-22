﻿#region Apache License-2.0
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
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace DnsProxy.Common.Models.Rules
{
    public class Rule : IRule
    {
        public int Order { get; set; }
        public List<string> NameServer { get; set; }
        public string IpAddress { get; set; }
        public bool CompressionMutation { get; set; }

        /// <summary>
        ///     Query timeout in milliseconds
        /// </summary>
        public int QueryTimeout { get; set; }

        public bool IsCache { get; set; }
        public string StrategyName { get; set; }
        public bool IsEnabled { get; set; }

        public string DomainName { get; set; }
        public string DomainNamePattern { get; set; }

        public Type GetStrategy()
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
            switch (StrategyName)
            {
                // TODO
                //case Common.Models.Strategies.Hosts:
                //    return new HostsRule(this);
                //case Common.Models.Strategies.Dns:
                //    return new DnsRule(this);
                //case Common.Models.Strategies.DoH:
                //    return new DohRule(this);
                //case Common.Models.Strategies.AwsDocDb:
                //    return new AwsDocDbRule(this);
                //case Common.Models.Strategies.AwsElasticCache:
                //    return new AwsElasticCacheRule(this);
                //case Common.Models.Strategies.AwsApiGateway:
                //    return new AwsApiGatewayRule(this);
                default:
                    throw new ArgumentOutOfRangeException(nameof(StrategyName), StrategyName, null);
            }
        }
    }
}