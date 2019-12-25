using ARSoft.Tools.Net.Dns;
using Makaretu.Dns;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DnsProxy.Common
{
#pragma warning disable CS0612 // Type or member is obsolete
    internal static class MapperExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "<Pending>")]
        public static DnsRecordBase ToDnsRecord(this ResourceRecord resourceRecord)
        {
            switch (resourceRecord.Type)
            {
                case DnsType.A:
                    var a = ((Makaretu.Dns.ARecord)resourceRecord);
                    return new ARSoft.Tools.Net.Dns.ARecord(resourceRecord.Name.ToDomainName(),
                        resourceRecord.TTL.Seconds,
                        a.Address);
                case DnsType.NS:
                    var ns = ((Makaretu.Dns.NSRecord)resourceRecord);
                    return new ARSoft.Tools.Net.Dns.NsRecord(resourceRecord.Name.ToDomainName(),
                        resourceRecord.TTL.Seconds,
                        ns.Authority.ToDomainName());
                case DnsType.MX:
                    var mx = ((Makaretu.Dns.MXRecord)resourceRecord);
                    return new ARSoft.Tools.Net.Dns.MxRecord(resourceRecord.Name.ToDomainName(),
                        resourceRecord.TTL.Seconds,
                        mx.Preference,
                        mx.Exchange.ToDomainName());
                case DnsType.CNAME:
                    var cname = ((Makaretu.Dns.CNAMERecord)resourceRecord);
                    return new ARSoft.Tools.Net.Dns.CNameRecord(resourceRecord.Name.ToDomainName(),
                        resourceRecord.TTL.Seconds,
                        cname.Target.ToDomainName());
                case DnsType.SOA:
                    var soa = ((Makaretu.Dns.SOARecord)resourceRecord);
                    return new ARSoft.Tools.Net.Dns.SoaRecord(resourceRecord.Name.ToDomainName(),
                        resourceRecord.TTL.Seconds,
                        soa.PrimaryName.ToDomainName(),
                        soa.Mailbox.ToDomainName(),
                        soa.SerialNumber,
                        soa.Refresh.Seconds,
                        soa.Retry.Seconds,
                        soa.Expire.Seconds,
                        soa.Minimum.Seconds);
                case DnsType.PTR:
                    var ptr = ((Makaretu.Dns.PTRRecord)resourceRecord);
                    return new ARSoft.Tools.Net.Dns.PtrRecord(resourceRecord.Name.ToDomainName(),
                        resourceRecord.TTL.Seconds,
                        ptr.DomainName.ToDomainName());
                case DnsType.HINFO:
                    var hinfo = ((Makaretu.Dns.HINFORecord)resourceRecord);
                    return new ARSoft.Tools.Net.Dns.HInfoRecord(resourceRecord.Name.ToDomainName(),
                        resourceRecord.TTL.Seconds,
                        hinfo.Cpu,
                        hinfo.OS);
                case DnsType.TXT:
                    var txt = ((Makaretu.Dns.TXTRecord)resourceRecord);
                    return new ARSoft.Tools.Net.Dns.TxtRecord(resourceRecord.Name.ToDomainName(),
                        resourceRecord.TTL.Seconds,
                        txt.Strings);
                case DnsType.AFSDB:
                    var afsdg = ((Makaretu.Dns.AFSDBRecord)resourceRecord);
                    return new ARSoft.Tools.Net.Dns.AfsdbRecord(resourceRecord.Name.ToDomainName(),
                        resourceRecord.TTL.Seconds,
                        afsdg.Subtype.ToAfsdbRecordAfsSubType(),
                        afsdg.Target.ToDomainName());
                case DnsType.AAAA:
                    var aaaa = ((Makaretu.Dns.AAAARecord)resourceRecord);
                    return new ARSoft.Tools.Net.Dns.AaaaRecord(resourceRecord.Name.ToDomainName(),
                        resourceRecord.TTL.Seconds,
                        aaaa.Address);
                case DnsType.SRV:
                    var srv = ((Makaretu.Dns.SRVRecord)resourceRecord);
                    return new ARSoft.Tools.Net.Dns.SrvRecord(resourceRecord.Name.ToDomainName(),
                        resourceRecord.TTL.Seconds,
                        srv.Priority,
                        srv.Weight,
                        srv.Port,
                        srv.Target.ToDomainName());
                case DnsType.DNAME:
                    var dname = ((Makaretu.Dns.DNAMERecord)resourceRecord);
                    return new ARSoft.Tools.Net.Dns.DNameRecord(resourceRecord.Name.ToDomainName(),
                        resourceRecord.TTL.Seconds,
                        dname.Target.ToDomainName());
                case DnsType.OPT:
                    var opt = ((Makaretu.Dns.OPTRecord)resourceRecord);
                    var optRecord = new ARSoft.Tools.Net.Dns.OptRecord();
                    optRecord.Options.AddRange(opt.Options.ToEDnsOption());
                    optRecord.Version = opt.Version;
                    optRecord.IsDnsSecOk = opt.DO;
                    optRecord.UdpPayloadSize = opt.RequestorPayloadSize;
                    return optRecord;
                case DnsType.DS:
                    var ds = ((Makaretu.Dns.DSRecord)resourceRecord);
                    return new ARSoft.Tools.Net.Dns.DsRecord(resourceRecord.Name.ToDomainName(),
                        ds.Class.ToRecordClass(),
                        resourceRecord.TTL.Seconds,
                        ds.KeyTag,
                        ds.Algorithm.ToDnsSecAlgorithm(),
                        ds.HashAlgorithm.ToDnsSecDigestType(),
                        ds.Digest);
                case DnsType.NSEC:
                    var nsec = ((Makaretu.Dns.NSECRecord)resourceRecord);
                    return new ARSoft.Tools.Net.Dns.NSecRecord(resourceRecord.Name.ToDomainName(),
                        resourceRecord.Class.ToRecordClass(),
                        resourceRecord.TTL.Seconds,
                        nsec.NextOwnerName.ToDomainName(),
                        nsec.Types.ToRecordTypes());
                case DnsType.DNSKEY:
                    var dnskey = ((Makaretu.Dns.DNSKEYRecord)resourceRecord);
                    return new ARSoft.Tools.Net.Dns.DnsKeyRecord(resourceRecord.Name.ToDomainName(),
                        resourceRecord.Class.ToRecordClass(),
                        resourceRecord.TTL.Seconds,
                        dnskey.Flags.ToDnsKeyFlags(),
                        dnskey.Protocol,
                        dnskey.Algorithm.ToDnsSecAlgorithm(),
                        dnskey.PublicKey);
                case DnsType.NSEC3:
                    var nsec3 = ((Makaretu.Dns.NSEC3Record)resourceRecord);
                    return new ARSoft.Tools.Net.Dns.NSec3Record(resourceRecord.Name.ToDomainName(),
                        resourceRecord.Class.ToRecordClass(),
                        resourceRecord.TTL.Seconds,
                        nsec3.HashAlgorithm.ToNSec3HashAlgorithm(),
                        (byte)nsec3.Flags,
                        nsec3.Iterations,
                        nsec3.Salt,
                        nsec3.NextHashedOwnerName,
                        nsec3.Types.ToRecordTypes());
                case DnsType.NSEC3PARAM:
                    var nsec3param = ((Makaretu.Dns.NSEC3PARAMRecord)resourceRecord);
                    return new ARSoft.Tools.Net.Dns.NSec3ParamRecord(resourceRecord.Name.ToDomainName(),
                        resourceRecord.Class.ToRecordClass(),
                        resourceRecord.TTL.Seconds,
                        nsec3param.HashAlgorithm.ToNSec3HashAlgorithm(),
                        nsec3param.Flags,
                        nsec3param.Iterations,
                        nsec3param.Salt);
                case DnsType.TKEY:
                    var tkey = ((Makaretu.Dns.TKEYRecord)resourceRecord);
                    return new ARSoft.Tools.Net.Dns.TKeyRecord(resourceRecord.Name.ToDomainName(),
                        tkey.Algorithm.ToTSigAlgorithm(),
                        tkey.Inception,
                        tkey.Expiration,
                        tkey.Mode.ToTKeyRecordTKeyMode(),
                        tkey.Error.ToReturnCode(),
                        tkey.Key,
                        tkey.OtherData);
                case DnsType.TSIG:
                    var tsig = ((Makaretu.Dns.TSIGRecord)resourceRecord);
                    return new ARSoft.Tools.Net.Dns.TSigRecord(resourceRecord.Name.ToDomainName(),
                       tsig.Algorithm.ToTSigAlgorithm(),
                       tsig.TimeSigned,
                       tsig.Fudge,
                       tsig.OriginalMessageId,
                       tsig.Error.ToReturnCode(),
                       tsig.OtherData,
                       tsig.MAC);
                case DnsType.RP:
                    var rp = ((Makaretu.Dns.RPRecord)resourceRecord);
                    return new ARSoft.Tools.Net.Dns.RpRecord(resourceRecord.Name.ToDomainName(),
                        resourceRecord.TTL.Seconds,
                        rp.Mailbox.ToDomainName(),
                        rp.TextName.ToDomainName());
                case DnsType.AXFR:
                case DnsType.ANY:
                case DnsType.URI:
                case DnsType.MB:
                case DnsType.MG:
                case DnsType.MR:
                case DnsType.MINFO:
                case DnsType.MAILB:
                case DnsType.NULL:
                case DnsType.WKS:
                case DnsType.CAA:
                case DnsType.RRSIG:
                case DnsType.MD:
                case DnsType.MF:
                case DnsType.MAILA:
                    return new ARSoft.Tools.Net.Dns.UnknownRecord(resourceRecord.Name.ToDomainName(),
                        resourceRecord.Type.ToRecordType(),
                        resourceRecord.Class.ToRecordClass(),
                        resourceRecord.TTL.Seconds,
                        resourceRecord.ToByteArray());
                default:
                    throw new ArgumentOutOfRangeException(nameof(resourceRecord.Type), resourceRecord.Type, null);
            }
        }

        public static ReturnCode ToReturnCode(this MessageStatus messageStatus)
        {
            switch (messageStatus)
            {
                case MessageStatus.NoError:
                    return ReturnCode.NoError;
                case MessageStatus.FormatError:
                    return ReturnCode.FormatError;
                case MessageStatus.ServerFailure:
                    return ReturnCode.ServerFailure;
                case MessageStatus.NotImplemented:
                    return ReturnCode.NotImplemented;
                case MessageStatus.Refused:
                    return ReturnCode.Refused;
                case MessageStatus.YXDomain:
                    return ReturnCode.YXDomain;
                case MessageStatus.YXRRSet:
                    return ReturnCode.YXRRSet;
                case MessageStatus.NXRRSet:
                    return ReturnCode.NXRRSet;
                case MessageStatus.NotAuthoritative:
                    return ReturnCode.NotAuthoritive;
                case MessageStatus.NotZone:
                    return ReturnCode.NotZone;
                case MessageStatus.BadVersion:
                    return ReturnCode.BadVersion;
                case MessageStatus.BadKey:
                    return ReturnCode.BadKey;
                case MessageStatus.BadTime:
                    return ReturnCode.BadTime;
                case MessageStatus.BADMODE:
                    return ReturnCode.BadMode;
                case MessageStatus.BADNAME:
                    return ReturnCode.BadName;
                case MessageStatus.BADALG:
                    return ReturnCode.BadAlg;
                case MessageStatus.NameError:
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageStatus), messageStatus, null);
            }
        }

        public static TKeyRecord.TKeyMode ToTKeyRecordTKeyMode(this KeyExchangeMode keyExchangeMode)
        {
            switch (keyExchangeMode)
            {
                case KeyExchangeMode.ServerAssignment:
                    return TKeyRecord.TKeyMode.ServerAssignment;
                case KeyExchangeMode.DiffieHellman:
                    return TKeyRecord.TKeyMode.DiffieHellmanExchange;
                case KeyExchangeMode.GssApi:
                    return TKeyRecord.TKeyMode.GssNegotiation;
                case KeyExchangeMode.ResolverAssignment:
                    return TKeyRecord.TKeyMode.ResolverAssignment;
                case KeyExchangeMode.KeyDeletion:
                    return TKeyRecord.TKeyMode.KeyDeletion;
                default:
                    throw new ArgumentOutOfRangeException(nameof(keyExchangeMode), keyExchangeMode, null);
            }
        }

        public static TSigAlgorithm ToTSigAlgorithm(this DomainName algorithm)
        {
            switch (algorithm.ToString())
            {
                case TSIGRecord.GSSTSIG:
                    return TSigAlgorithm.Unknown;
                case TSIGRecord.HMACMD5:
                    return TSigAlgorithm.Md5;
                case TSIGRecord.HMACSHA1:
                    return TSigAlgorithm.Sha1;
                case TSIGRecord.HMACSHA256:
                    return TSigAlgorithm.Sha256;
                case TSIGRecord.HMACSHA512:
                    return TSigAlgorithm.Sha512;
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm.ToString(), null);
            }
        }

        public static DnsKeyFlags ToDnsKeyFlags(this DNSKEYFlags dnskeyFlags)
        {
            switch (dnskeyFlags)
            {
                case DNSKEYFlags.SecureEntryPoint:
                    return DnsKeyFlags.SecureEntryPoint;
                case DNSKEYFlags.ZoneKey:
                    return DnsKeyFlags.Zone;
                case DNSKEYFlags.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(dnskeyFlags), dnskeyFlags, null);
            }
        }

        public static NSec3HashAlgorithm ToNSec3HashAlgorithm(this DigestType digestType)
        {
            switch (digestType)
            {
                case DigestType.Sha1:
                    return NSec3HashAlgorithm.Sha1;
                case DigestType.Sha256:
                case DigestType.GostR34_11_94:
                case DigestType.Sha384:
                case DigestType.Sha512:
                default:
                    throw new ArgumentOutOfRangeException(nameof(digestType), digestType, null);
            }
        }

        public static IEnumerable<EDnsOptionBase> ToEDnsOption(this List<EdnsOption> ednsOptions)
        {
            foreach (var ednsOption in ednsOptions)
            {
                var unknownOption = (Makaretu.Dns.UnknownEdnsOption)ednsOption;
                yield return new ARSoft.Tools.Net.Dns.UnknownOption(unknownOption.Type.ToEDnsOptionType(), unknownOption.Data);
            }
        }

        public static ARSoft.Tools.Net.Dns.EDnsOptionType ToEDnsOptionType(this Makaretu.Dns.EdnsOptionType ednsOptionType)
        {
            switch (ednsOptionType)
            {
                case EdnsOptionType.NSID:
                    return EDnsOptionType.NsId;
                case EdnsOptionType.DAU:
                    return EDnsOptionType.DnssecAlgorithmUnderstood;
                case EdnsOptionType.DHU:
                    return EDnsOptionType.DsHashUnderstood;
                case EdnsOptionType.N3U:
                    return EDnsOptionType.Nsec3HashUnderstood;
                case EdnsOptionType.ClientSubnet:
                    return EDnsOptionType.ClientSubnet;
                case EdnsOptionType.Expire:
                    return EDnsOptionType.Expire;
                case EdnsOptionType.Cookie:
                    return EDnsOptionType.Cookie;
                case EdnsOptionType.Keepalive:
                case EdnsOptionType.Padding:
                case EdnsOptionType.Chain:
                case EdnsOptionType.KeyTag:
                case EdnsOptionType.ExperimentalMin:
                case EdnsOptionType.ExperimentalMax:
                case EdnsOptionType.FutureExpansion:
                default:
                    throw new ArgumentOutOfRangeException(nameof(ednsOptionType), ednsOptionType, null);
            }
        }

        public static AfsdbRecord.AfsSubType ToAfsdbRecordAfsSubType(this ushort afsSubType)
        {
            switch (afsSubType)
            {
                case 1:
                    return AfsdbRecord.AfsSubType.Afs;
                case 2:
                    return AfsdbRecord.AfsSubType.Dce;
                default:
                    throw new ArgumentOutOfRangeException(nameof(afsSubType), afsSubType, null);
            }
        }

        public static DnsSecDigestType ToDnsSecDigestType(this DigestType digestType)
        {
            switch (digestType)
            {
                case DigestType.Sha1:
                    return DnsSecDigestType.Sha1;
                case DigestType.Sha256:
                    return DnsSecDigestType.Sha256;
                case DigestType.GostR34_11_94:
                    return DnsSecDigestType.EccGost;
                case DigestType.Sha384:
                    return DnsSecDigestType.Sha384;
                case DigestType.Sha512:
                default:
                    throw new ArgumentOutOfRangeException(nameof(digestType), digestType, null);
            }
        }

        public static DnsSecAlgorithm ToDnsSecAlgorithm(this SecurityAlgorithm securityAlgorithm)
        {
            switch (securityAlgorithm)
            {
                case SecurityAlgorithm.RSAMD5:
                    return DnsSecAlgorithm.RsaMd5;
                case SecurityAlgorithm.DH:
                    return DnsSecAlgorithm.DiffieHellman;
                case SecurityAlgorithm.DSA:
                    return DnsSecAlgorithm.Dsa;
                case SecurityAlgorithm.RSASHA1:
                    return DnsSecAlgorithm.RsaSha1;
                case SecurityAlgorithm.DSANSEC3SHA1:
                    return DnsSecAlgorithm.DsaNsec3Sha1;
                case SecurityAlgorithm.RSASHA1NSEC3SHA1:
                    return DnsSecAlgorithm.RsaSha1Nsec3Sha1;
                case SecurityAlgorithm.RSASHA256:
                    return DnsSecAlgorithm.RsaSha256;
                case SecurityAlgorithm.RSASHA512:
                    return DnsSecAlgorithm.RsaSha512;
                case SecurityAlgorithm.ECCGOST:
                    return DnsSecAlgorithm.EccGost;
                case SecurityAlgorithm.ECDSAP256SHA256:
                    return DnsSecAlgorithm.EcDsaP256Sha256;
                case SecurityAlgorithm.ECDSAP384SHA384:
                    return DnsSecAlgorithm.EcDsaP384Sha384;
                case SecurityAlgorithm.INDIRECT:
                    return DnsSecAlgorithm.Indirect;
                case SecurityAlgorithm.PRIVATEDNS:
                    return DnsSecAlgorithm.PrivateDns;
                case SecurityAlgorithm.PRIVATEOID:
                    return DnsSecAlgorithm.PrivateOid;
                case SecurityAlgorithm.ED25519:
                case SecurityAlgorithm.ED448:
                case SecurityAlgorithm.DELETE:
                default:
                    throw new ArgumentOutOfRangeException(nameof(securityAlgorithm), securityAlgorithm, null);
            }
        }

        public static ARSoft.Tools.Net.DomainName ToDomainName(this Makaretu.Dns.DomainName domainName)
        {
            return ARSoft.Tools.Net.DomainName.Parse(domainName.ToString());
        }

        public static Makaretu.Dns.DomainName ToDomainName(this ARSoft.Tools.Net.DomainName domainName)
        {
            return new DomainName(domainName.ToString());
        }

        public static RecordClass ToRecordClass(this DnsClass dnsClass)
        {
            switch (dnsClass)
            {
                case DnsClass.IN:
                    return RecordClass.INet;
                case DnsClass.CH:
                    return RecordClass.Chaos;
                case DnsClass.HS:
                    return RecordClass.Hesiod;
                case DnsClass.None:
                    return RecordClass.None;
                case DnsClass.ANY:
                    return RecordClass.Any;
                case DnsClass.CS:
                default:
                    throw new ArgumentOutOfRangeException(nameof(dnsClass), dnsClass, null);
            }
        }

        public static DnsClass ToDnsClass(this RecordClass recordClass)
        {
            switch (recordClass)
            {
                case RecordClass.INet:
                    return DnsClass.IN;
                case RecordClass.Chaos:
                    return DnsClass.CH;
                case RecordClass.Hesiod:
                    return DnsClass.HS;
                case RecordClass.None:
                    return DnsClass.None;
                case RecordClass.Any:
                    return DnsClass.ANY;
                case RecordClass.Invalid:
                default:
                    throw new ArgumentOutOfRangeException(nameof(recordClass), recordClass, null);
            }
        }

        public static List<RecordType> ToRecordTypes(this IEnumerable<DnsType> dnsTypes)
        {
            return dnsTypes.Select(dnsType => dnsType.ToRecordType()).ToList();
        }

        public static RecordType ToRecordType(this DnsType dnsType)
        {
            switch (dnsType)
            {
                case DnsType.A:
                    return RecordType.A;
                case DnsType.NS:
                    return RecordType.Ns;
                case DnsType.MD:
                    return RecordType.Md;
                case DnsType.MF:
                    return RecordType.Mf;
                case DnsType.CNAME:
                    return RecordType.CName;
                case DnsType.SOA:
                    return RecordType.Soa;
                case DnsType.MB:
                    return RecordType.Mb;
                case DnsType.MG:
                    return RecordType.Mg;
                case DnsType.MR:
                    return RecordType.Mr;
                case DnsType.NULL:
                    return RecordType.Null;
                case DnsType.WKS:
                    return RecordType.Wks;
                case DnsType.PTR:
                    return RecordType.Ptr;
                case DnsType.HINFO:
                    return RecordType.HInfo;
                case DnsType.MINFO:
                    return RecordType.MInfo;
                case DnsType.MX:
                    return RecordType.Mx;
                case DnsType.TXT:
                    return RecordType.Txt;
                case DnsType.RP:
                    return RecordType.Rp;
                case DnsType.AFSDB:
                    return RecordType.Afsdb;
                case DnsType.AAAA:
                    return RecordType.Aaaa;
                case DnsType.SRV:
                    return RecordType.Srv;
                case DnsType.DNAME:
                    return RecordType.DName;
                case DnsType.OPT:
                    return RecordType.Opt;
                case DnsType.DS:
                    return RecordType.Ds;
                case DnsType.RRSIG:
                    return RecordType.RrSig;
                case DnsType.NSEC:
                    return RecordType.NSec;
                case DnsType.DNSKEY:
                    return RecordType.DnsKey;
                case DnsType.NSEC3:
                    return RecordType.NSec3;
                case DnsType.NSEC3PARAM:
                    return RecordType.NSec3Param;
                case DnsType.TKEY:
                    return RecordType.TKey;
                case DnsType.TSIG:
                    return RecordType.TSig;
                case DnsType.AXFR:
                    return RecordType.Axfr;
                case DnsType.MAILB:
                    return RecordType.MailB;
                case DnsType.MAILA:
                    return RecordType.MailA;
                case DnsType.ANY:
                    return RecordType.Any;
                case DnsType.URI:
                    return RecordType.Uri;
                case DnsType.CAA:
                    return RecordType.CAA;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dnsType), dnsType, null);
            }
        }

        public static DnsType ToDnsType(this RecordType recordType)
        {
            switch (recordType)
            {
                case RecordType.A:
                    return DnsType.A;
                case RecordType.Ns:
                    return DnsType.NS;
                case RecordType.Md:
                    return DnsType.MD;
                case RecordType.Mf:
                    return DnsType.MF;
                case RecordType.CName:
                    return DnsType.CNAME;
                case RecordType.Soa:
                    return DnsType.SOA;
                case RecordType.Mb:
                    return DnsType.MB;
                case RecordType.Mg:
                    return DnsType.MG;
                case RecordType.Mr:
                    return DnsType.MR;
                case RecordType.Null:
                    return DnsType.NULL;
                case RecordType.Wks:
                    return DnsType.WKS;
                case RecordType.Ptr:
                    return DnsType.PTR;
                case RecordType.HInfo:
                    return DnsType.HINFO;
                case RecordType.MInfo:
                    return DnsType.MINFO;
                case RecordType.Mx:
                    return DnsType.MX;
                case RecordType.Txt:
                    return DnsType.TXT;
                case RecordType.Rp:
                    return DnsType.RP;
                case RecordType.Afsdb:
                    return DnsType.AFSDB;
                case RecordType.Aaaa:
                    return DnsType.AAAA;
                case RecordType.Srv:
                    return DnsType.SRV;
                case RecordType.Opt:
                    return DnsType.OPT;
                case RecordType.DName:
                    return DnsType.DNAME;
                case RecordType.Ds:
                    return DnsType.DS;
                case RecordType.RrSig:
                    return DnsType.RRSIG;
                case RecordType.NSec:
                    return DnsType.NSEC;
                case RecordType.DnsKey:
                    return DnsType.DNSKEY;
                case RecordType.NSec3:
                    return DnsType.NSEC3;
                case RecordType.NSec3Param:
                    return DnsType.NSEC3PARAM;
                case RecordType.TKey:
                    return DnsType.TKEY;
                case RecordType.TSig:
                    return DnsType.TSIG;
                case RecordType.Axfr:
                    return DnsType.AXFR;
                case RecordType.MailB:
                    return DnsType.MAILB;
                case RecordType.MailA:
                    return DnsType.MAILA;
                case RecordType.Any:
                    return DnsType.ANY;
                case RecordType.Uri:
                    return DnsType.URI;
                case RecordType.CAA:
                    return DnsType.CAA;

                case RecordType.Dhcid:
                case RecordType.X25:
                case RecordType.Isdn:
                case RecordType.Rt:
                case RecordType.Nsap:
                case RecordType.NsapPtr:
                case RecordType.Sig:
                case RecordType.Key:
                case RecordType.Px:
                case RecordType.GPos:
                case RecordType.Loc:
                case RecordType.Invalid:
                case RecordType.Eid:
                case RecordType.NimLoc:
                case RecordType.AtmA:
                case RecordType.Naptr:
                case RecordType.Kx:
                case RecordType.Cert:
                case RecordType.Sink:
                case RecordType.Apl:
                case RecordType.SshFp:
                case RecordType.IpSecKey:
                case RecordType.Tlsa:
                case RecordType.Hip:
                case RecordType.NInfo:
                case RecordType.RKey:
                case RecordType.TALink:
                case RecordType.CDs:
                case RecordType.CDnsKey:
                case RecordType.OpenPGPKey:
                case RecordType.CSync:
                case RecordType.Nxt:
                case RecordType.A6:
                case RecordType.Spf:
                case RecordType.UInfo:
                case RecordType.UId:
                case RecordType.Gid:
                case RecordType.Unspec:
                case RecordType.NId:
                case RecordType.L32:
                case RecordType.L64:
                case RecordType.LP:
                case RecordType.Eui48:
                case RecordType.Eui64:
                case RecordType.Ixfr:
                case RecordType.Ta:
                case RecordType.Dlv:
                default:
                    throw new ArgumentOutOfRangeException(nameof(recordType), recordType, null);
            }
        }
    }

#pragma warning restore CS0618 // Type or member is obsolete
}
