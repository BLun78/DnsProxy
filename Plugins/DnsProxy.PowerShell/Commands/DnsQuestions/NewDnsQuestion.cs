using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Net;

namespace DnsProxy.PowerShell.Commands.DnsQuestions
{
    [Cmdlet(VerbsCommon.New, "DnsQuestionARecord")]
    internal class DnsQuestionARecord : DnsCmdlet
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
            
        }
    }
}