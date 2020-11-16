using BAG.IT.Core.Wmi.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;

namespace BAG.IT.Core.Wmi
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    public interface ISccmInventoryActionStatusSwInv : IWmiProvider
    {
        DateTime LastReportDate { get; }
    }

    [UsedImplicitly]
    [ExcludeFromCodeCoverage]
    [WmiSearch("root\\ccm\\invagt", "SELECT * FROM InventoryActionStatus where InventoryActionID='{00000000-0000-0000-0000-000000000002}'")]
    internal class SccmInventoryActionStatusSwInv : WmiProvider, ISccmInventoryActionStatusSwInv
    {

        [WmiName("LastReportDate")]
        public DateTime LastReportDate { get; [UsedImplicitly] private set; }

        public SccmInventoryActionStatusSwInv(ILogger<WmiProvider> logger) : base(logger)
        {
        }
    }
}