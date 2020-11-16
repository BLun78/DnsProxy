using BAG.IT.Core.Wmi.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;

namespace BAG.IT.Core.Wmi
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    public interface ISccmInventoryActionStatusHeartbeatInv : IWmiProvider
    {
        DateTime LastReportDate { get; }
    }

    [UsedImplicitly]
    [ExcludeFromCodeCoverage]
    [WmiSearch("root\\ccm\\invagt", "SELECT * FROM InventoryActionStatus where InventoryActionID='{00000000-0000-0000-0000-000000000003}'")]
    internal class SccmInventoryActionStatusHeartbeatInv : WmiProvider, ISccmInventoryActionStatusHeartbeatInv
    {

        [WmiName("LastReportDate")]
        public DateTime LastReportDate { get; [UsedImplicitly] private set; }


        public SccmInventoryActionStatusHeartbeatInv(ILogger<WmiProvider> logger) : base(logger)
        {
        }
    }
}