using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using DnsProxy.Server.Common;
using ARSoft.Tools.Net.Dns;

namespace DnsProxy.PowerShell.Commands.DnsQuestions
{
    [Cmdlet(VerbsCommon.New, "DnsQuestionARecord")] 
    internal class NewDnsQuestion : DnsCmdlet
    {
        [Parameter(Position = 0)]
        [ValidateNotNullOrEmpty]
        public string DomainName { get; set; }
        
        [Parameter(Position = 0)]
        [ValidateNotNullOrEmpty]
        [ValidateCount(1, Int32.MaxValue)]
        public List<string> IpAddress{ get; set; }

        protected override void ProcessRecord()
        {
            var tempHost = IpAddress.ToAddressRecord(DomainName);
            var question = new DnsQuestion(ARSoft.Tools.Net.DomainName.Parse(DomainName), tempHost.First().RecordType, RecordClass.INet);
            WriteObject(question, false);
        }
    }
}