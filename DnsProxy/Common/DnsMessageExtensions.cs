using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace DnsProxy.Common
{
    internal static class DnsMessageExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "<Pending>")]
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "<Pending>")]
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
            {
                result.Add(new PtrRecord(DomainName.Parse(tempIpAddress), 30, DomainName.Parse(domainName)));
            }
            return (tempIpAddress, result);
        }
    }
}
