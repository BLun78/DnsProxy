using System;
using System.Diagnostics;
using System.Management.Automation;

namespace DnsProxy.PowerShell.Test
{
    internal class TestHost : IDisposable
    {
        protected System.Management.Automation.PowerShell PowerShell { get; set; }

        public TestHost(string script)
        {
            PowerShell = System.Management.Automation.PowerShell.Create().AddScript(script);
        }
        
        public void Dispose()
        {
            PowerShell.Dispose();
        }
    }
}