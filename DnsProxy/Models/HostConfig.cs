using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;

namespace DnsProxy.Models
{
    public class HostConfig: ICloneable
    {
        public List<Host> Hosts { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
