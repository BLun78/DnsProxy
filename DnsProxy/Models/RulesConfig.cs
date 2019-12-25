using System;
using System.Collections.Generic;

namespace DnsProxy.Models
{
    internal class RulesConfig : ICloneable
    {
        public List<Rule> Rules { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}