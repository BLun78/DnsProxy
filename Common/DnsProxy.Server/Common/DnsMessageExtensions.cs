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

using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Server.Models.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace DnsProxy.Server.Common
{
    internal static class DnsMessageExtensions
    {
        [SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "<Pending>")]
        public static List<AddressRecordBase> ToAddressRecord(this Host host, string domainName)
        {
            return host.IpAddresses.ToAddressRecord(domainName);
        }
        
        public static List<AddressRecordBase> ToAddressRecord(this List<string> ipAddresses, string domainName)
        {
            var result = new List<AddressRecordBase>();
            foreach (var ipAddress in ipAddresses)
            {
                var ip = IPAddress.Parse(ipAddress);
                switch (ip.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        result.Add(new ARecord(DomainName.Parse(domainName), 300, IPAddress.Parse(ipAddress)));
                        break;
                    case AddressFamily.InterNetworkV6:
                        result.Add(new AaaaRecord(DomainName.Parse(domainName), 300, IPAddress.Parse(ipAddress)));
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
            return host.DomainNames.ToPtrRecords(ipAddress);
        }
        
        public static (string, List<PtrRecord>) ToPtrRecords(this List<string> domainNames, string ipAddress)
        {
            var result = new List<PtrRecord>();
            var tempIpAddress = CreatePtrIpAddressName(ipAddress);

            foreach (var domainName in domainNames)
                result.Add(new PtrRecord(DomainName.Parse(tempIpAddress), 300, DomainName.Parse(domainName)));
            return (tempIpAddress, result);
        }

        public static string CreatePtrIpAddressName(this string ipAddress)
        {
            var ip = IPAddress.Parse(ipAddress);
            return CreatePtrIpAddressName(ip);
        }

        public static string CreatePtrIpAddressName(this IPAddress ipAddress)
        {
            string tempIpAddress;
            string reverseIpAddress = string.Empty;
            string[] newIpAddress = ipAddress.ToString().Split('.');
            for (int i = newIpAddress.Length - 1; i >= 0; i--)
            {
                if (string.IsNullOrWhiteSpace(reverseIpAddress))
                {
                    reverseIpAddress = newIpAddress[i];
                }
                else
                {
                    reverseIpAddress += '.' + newIpAddress[i];
                }
            }
            switch (ipAddress.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    tempIpAddress = $"{reverseIpAddress}.in-addr.arpa";
                    break;
                case AddressFamily.InterNetworkV6:
                    tempIpAddress = $"{reverseIpAddress}.ip6.arpa";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ipAddress.AddressFamily), ipAddress.AddressFamily,
                        null);
            }

            return tempIpAddress;
        }
    }
}