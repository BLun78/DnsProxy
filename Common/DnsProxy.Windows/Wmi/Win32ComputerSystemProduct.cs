using System.Diagnostics.CodeAnalysis;
using BAG.IT.Core.Wmi.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

// ReSharper disable InconsistentNaming
namespace BAG.IT.Core.Wmi
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    public interface IWin32ComputerSystemProduct : IWmiProvider
    {
        string Caption { get; }
        string Description { get; }
        string IdentifyingNumber { get; }
        string Name { get; }
        string SkuNumber { get; }
        string Uuid { get; }
        string Vendor { get; }
        string Version { get; }
    }

    [UsedImplicitly]
    [ExcludeFromCodeCoverage]
    [WmiSearch("root\\CIMV2", "SELECT * FROM Win32_ComputerSystemProduct")]
    internal class Win32ComputerSystemProduct : WmiProvider, IWin32ComputerSystemProduct
    {


        [WmiName("Caption")]
        public string Caption { get; [UsedImplicitly] private set; }


        [WmiName("Description")]
        public string Description { get; [UsedImplicitly] private set; }

        [WmiName("IdentifyingNumber")]
        public string IdentifyingNumber { get; [UsedImplicitly] private set; }

        [WmiName("Name")]
        public string Name { get; [UsedImplicitly] private set; }

        [WmiName("SKUNumber")]
        public string SkuNumber { get; [UsedImplicitly] private set; }

        [WmiName("UUID")]
        public string Uuid { get; [UsedImplicitly] private set; }

        [WmiName("Vendor")]
        public string Vendor { get; [UsedImplicitly] private set; }

        [WmiName("Version")]
        public string Version { get; [UsedImplicitly] private set; }

        public Win32ComputerSystemProduct(ILogger<WmiProvider> logger) : base(logger)
        {
        }
    }
}