using System;
using System.Management.Automation;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DnsProxy.PowerShell.Test
{
    [TestClass]
    public class UnitTest1 : TestPowerShellHost
    {
        [TestMethod]
        [DeploymentItem("Test1.ps1")]
        public void TestMethod1()
        {
            try
            {
                var method = MethodBase.GetCurrentMethod();
                var deploymentItem = method.GetCustomAttribute<DeploymentItemAttribute>();
                using (var powerShell = Start(deploymentItem.Path).AddCommand("get-info"))
                {
                    foreach (PSObject result in powerShell.Invoke())
                    {
                        var d = 1 + 1;
                    }
                
                }
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
           
        }
    }
}