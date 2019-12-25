using System;
using System.Collections.Generic;

namespace DnsProxy.Models
{
    public class HostsConfig : ICloneable
    {
#pragma warning disable CA2227 // Collection properties should be read only
        public List<Host> Hosts { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
