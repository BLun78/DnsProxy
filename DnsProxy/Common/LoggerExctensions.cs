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

//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Text;
//using ARSoft.Tools.Net.Dns;
//using ARecord = ARSoft.Tools.Net.Dns.ARecord;

//namespace DnsProxy.Common
//{
//    internal static class LoggerExctensions
//    {

//        public static List<string> ToLoggerString(this DnsRecordBase dnsRecordBase)
//        {
//            switch (dnsRecordBase.RecordType)
//            {
//                case RecordType.A:
//                    var a = dnsRecordBase as ARecord;
//                    return new List<string>() { a?.Address?.ToString() };
//                case RecordType.Ns:
//                    var ns = dnsRecordBase as NsRecord;
//                    return new List<string>() { ns?.NameServer.ToString() };
//                case RecordType.Md:
//                case RecordType.Mf:
//                case RecordType.CName:
//                    var cname = dnsRecordBase as CNameRecord;
//                    return new List<string>() { };
//                case RecordType.Soa:
//                    var soa = dnsRecordBase as SoaRecord;
//                    return new List<string>() { };
//                case RecordType.Mb:

//                case RecordType.Mg:

//                case RecordType.Mr:

//                case RecordType.Null:

//                case RecordType.Wks:
//                    var wks = dnsRecordBase as WksRecord;
//                    return new List<string>() { };
//                case RecordType.Ptr:
//                    var ptr = dnsRecordBase as PtrRecord;
//                    return new List<string>() { };
//                case RecordType.HInfo:
//                    var hinfo = dnsRecordBase as HInfoRecord;
//                    return new List<string>() { };
//                case RecordType.MInfo:

//                case RecordType.Mx:
//                    var mx = dnsRecordBase as MxRecord;
//                    return new List<string>() { };
//                case RecordType.Txt:
//                    var txt = dnsRecordBase as TxtRecord;
//                    return new List<string>() { };
//                case RecordType.Rp:
//                    var rp = dnsRecordBase as RpRecord;
//                    return new List<string>() { };
//                case RecordType.Afsdb:
//                    var afsdb = dnsRecordBase as AfsdbRecord;
//                    return new List<string>() { };
//                case RecordType.Aaaa:
//                    var aaaa = dnsRecordBase as AaaaRecord;
//                    return new List<string>() { };
//                case RecordType.Srv:
//                    var srv = dnsRecordBase as SrvRecord;
//                    return new List<string>() { };
//                case RecordType.Opt:
//                    var opt = dnsRecordBase as OptRecord;
//                    return new List<string>() { };
//                case RecordType.DName:
//                    var dname = dnsRecordBase as DNameRecord;
//                    return new List<string>() { };
//                case RecordType.Ds:
//                    var ds = dnsRecordBase as DsRecord;
//                    return new List<string>() { };
//                case RecordType.RrSig:
//                    var rrsig = dnsRecordBase as RrSigRecord;
//                    return new List<string>() { };
//                case RecordType.NSec:
//                    var nsec = dnsRecordBase as NSecRecord;
//                    return new List<string>() { };
//                case RecordType.DnsKey:
//                    var dnskey = dnsRecordBase as DnsKeyRecord;
//                    return new List<string>() { };
//                case RecordType.NSec3:
//                    var nsec3 = dnsRecordBase as NSec3Record;
//                    return new List<string>() { };
//                case RecordType.NSec3Param:
//                    var nsec3param = dnsRecordBase as NSec3ParamRecord;
//                    return new List<string>() { };
//                case RecordType.TKey:
//                    var tkey = dnsRecordBase as TKeyRecord;
//                    return new List<string>() { };
//                case RecordType.TSig:
//                    var tsig = dnsRecordBase as TSigRecord;
//                    return new List<string>() { };
//                case RecordType.Axfr:

//                case RecordType.MailB:

//                case RecordType.MailA:
//                case RecordType.Any:

//                case RecordType.Uri:
//                    var uri = dnsRecordBase as UriRecord;
//                    return new List<string>() { };
//                case RecordType.CAA:

//                case RecordType.Dhcid:
//                    var dhcid = dnsRecordBase as DhcidRecord;
//                    return new List<string>() { };
//                case RecordType.X25:
//                    var x25 = dnsRecordBase as X25Record;
//                    return new List<string>() { };
//                case RecordType.Isdn:
//                    var isdn = dnsRecordBase as IsdnRecord;
//                    return new List<string>() { };
//                case RecordType.Rt:
//                    var rt = dnsRecordBase as RtRecord;
//                    return new List<string>() { };
//                case RecordType.Nsap:
//                    var nsap = dnsRecordBase as NsapRecord;
//                    return new List<string>() { };
//                case RecordType.NsapPtr:

//                case RecordType.Sig:
//                    var sig = dnsRecordBase as DhcidRecord;
//                    return new List<string>() { };
//                case RecordType.Key:
//                    var key = dnsRecordBase as KeyRecord;
//                    return new List<string>() { };
//                case RecordType.Px:
//                    var px = dnsRecordBase as PxRecord;
//                    return new List<string>() { };
//                case RecordType.GPos:
//                    var gpos = dnsRecordBase as GPosRecord;
//                    return new List<string>() { };
//                case RecordType.Loc:
//                    var loc = dnsRecordBase as LocRecord;
//                    return new List<string>() { };
//                case RecordType.Invalid:

//                case RecordType.Eid:

//                case RecordType.NimLoc:

//                case RecordType.AtmA:

//                case RecordType.Naptr:
//                    var rnaptr = dnsRecordBase as NaptrRecord;
//                    return new List<string>() { };
//                case RecordType.Kx:
//                    var kx = dnsRecordBase as KxRecord;
//                    return new List<string>() { };
//                case RecordType.Cert:
//                    var cert = dnsRecordBase as CertRecord;
//                    return new List<string>() { };
//                case RecordType.Sink:

//                case RecordType.Apl:
//                    var apl = dnsRecordBase as AplRecord;
//                    return new List<string>() { };
//                case RecordType.SshFp:
//                    var sshfp = dnsRecordBase as SshFpRecord;
//                    return new List<string>() { };
//                case RecordType.IpSecKey:
//                    var ipseckey = dnsRecordBase as IpSecKeyRecord;
//                    return new List<string>() { };
//                case RecordType.Tlsa:
//                    var tlsa = dnsRecordBase as TlsaRecord;
//                    return new List<string>() { };
//                case RecordType.Hip:
//                case RecordType.NInfo:
//                case RecordType.RKey:
//                case RecordType.TALink:
//                case RecordType.CDs:
//                case RecordType.CDnsKey:
//                case RecordType.OpenPGPKey:
//                case RecordType.CSync:
//                case RecordType.Nxt:
//                case RecordType.A6:
//                case RecordType.Spf:
//                case RecordType.UInfo:
//                case RecordType.UId:
//                case RecordType.Gid:
//                case RecordType.Unspec:
//                case RecordType.NId:
//                case RecordType.L32:
//                case RecordType.L64:
//                case RecordType.LP:
//                case RecordType.Eui48:
//                case RecordType.Eui64:
//                case RecordType.Ixfr:
//                case RecordType.Ta:
//                case RecordType.Dlv:
//                default:
//                    throw new ArgumentOutOfRangeException(nameof(dnsRecordBase.RecordType), dnsRecordBase.RecordType, null);
//            }
//        }
//    }
//}

