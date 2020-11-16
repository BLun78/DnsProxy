using System.Diagnostics.CodeAnalysis;
using BAG.IT.Core.Wmi.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

// ReSharper disable InconsistentNaming
namespace BAG.IT.Core.Wmi
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    public interface ISmsClient : IWmiProvider
    {
        string ClientVersion { get; }
        string AllowLocalAdminOverride { get; }
        string EnableAutoAssignment { get; }
        string ClientType { get; }
    }

    [UsedImplicitly]
    [ExcludeFromCodeCoverage]
    [WmiSearch("root\\ccm", "SELECT * FROM SMS_Client")]
    internal class SmsClient : WmiProvider, ISmsClient
    {
        [WmiName("ClientVersion")]
        public string ClientVersion { get; [UsedImplicitly] private set; }

        [WmiName("AllowLocalAdminOverride")]
        public string AllowLocalAdminOverride { get; [UsedImplicitly] private set; }

        [WmiName("EnableAutoAssignment")]
        public string EnableAutoAssignment { get; [UsedImplicitly] private set; }

        [WmiName("ClientType")]
        public string ClientType { get; [UsedImplicitly] private set; }




        //    this.txtMP.Text = this.GWMI("CurrentManagementPoint", "Select * from SMS_Authority", "root\\ccm");
        //        this.txtGUID.Text = this.GWMI("ClientID", "Select * from CCM_Client", "root\\ccm");
        //        this.txtClientVersion.Text = this.GWMI("ClientVersion", "Select * from SMS_Client", "root\\ccm");
        //        this.txtSitecode.Text = this.GWMI("Name", "Select * from SMS_Authority", "root\\ccm").Substring(4);
        public SmsClient(ILogger<WmiProvider> logger) : base(logger)
        {
        }
    }
}