﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Org.BouncyCastle.Bcpg;

namespace DnsProxy.Models
{
    internal class HttpProxyConfig
    {
        public string Uri { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public string BypassAddresses { get; set; }
        [JsonIgnore]
        public string[] BypassAddressesArray => string.IsNullOrWhiteSpace(BypassAddresses)
            ? Array.Empty<string>()
            : BypassAddresses.Split(';');
        public AuthenticationType AuthenticationType { get; set; }
    }
}