using System;
using System.Diagnostics;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DnsProxy.PowerShell.Test
{
    public abstract class TestPowerShellHost
    {
        public System.Management.Automation.PowerShell Start(string script)
        {
            return System.Management.Automation.PowerShell.Create().AddScript(script);
        }
    }
}