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
using System.Net;
using System.Net.Sockets;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Models;

namespace DnsProxy.Common
{
    internal static class DnsMessageExtensions
    {
        [SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "<Pending>")]
        public static List<AddressRecordBase> ToAddressRecord(this Host host, string domainName)
        {
            var result = new List<AddressRecordBase>();
            foreach (var ipAddress in host.IpAddresses)
            {
                var ip = IPAddress.Parse(ipAddress);
                switch (ip.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        result.Add(new ARecord(DomainName.Parse(domainName), 30, IPAddress.Parse(ipAddress)));
                        break;
                    case AddressFamily.InterNetworkV6:
                        result.Add(new AaaaRecord(DomainName.Parse(domainName), 30, IPAddress.Parse(ipAddress)));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(ip.AddressFamily), ip.AddressFamily, null);
                }
            }

            return result;
        }

        [SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "<Pending>")]
        public static (string, List<PtrRecord>) ToPtrRecords(this Host host, string ipAddress)
        {
            var result = new List<PtrRecord>();
            var ip = IPAddress.Parse(ipAddress);
            string tempIpAddress;
            switch (ip.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    tempIpAddress = $"{ipAddress}.in-addr.arpa";
                    break;
                case AddressFamily.InterNetworkV6:
                    tempIpAddress = $"{ipAddress}.ip6.arpa";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ip.AddressFamily), ip.AddressFamily, null);
            }

            foreach (var domainName in host.DomainNames)
                result.Add(new PtrRecord(DomainName.Parse(tempIpAddress), 30, DomainName.Parse(domainName)));
            return (tempIpAddress, result);
        }
    }
}