using System.Diagnostics.CodeAnalysis;
using BAG.IT.Core.Wmi.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

// ReSharper disable InconsistentNaming
namespace BAG.IT.Core.Wmi
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    public interface IWin32ComputerSystem : IWmiProvider, IWmiProviderListItem
    {
        string ComputerName { get; }
        string PCSystemType { get; }
        string UserName { get; }
        string TotalPhysicalMemory { get; }
        string SystemType { get; }
        string Manufacturer { get; }
        string Model { get; }
        string NumberOfLogicalProcessors { get; }
        string NumberOfProcessors { get; }
        string SystemFamily { get; }
        string SystemSKUNumber { get; }

    }

    [UsedImplicitly]
    [ExcludeFromCodeCoverage]
    [WmiSearch("root\\CIMV2", "SELECT * FROM Win32_ComputerSystem")]
    internal class Win32ComputerSystem : WmiProvider, IWin32ComputerSystem
    {
        [WmiName("Name")]
        public string ComputerName { get; [UsedImplicitly] private set; }

        [WmiName("PCSystemType")]
        public string PCSystemType { get; [UsedImplicitly] private set; }

        [WmiName("UserName")]
        public string UserName { get; [UsedImplicitly] private set; }

        [WmiName("TotalPhysicalMemory")]
        public string TotalPhysicalMemory { get; [UsedImplicitly] private set; }

        [WmiName("SystemType")]
        public string SystemType { get; [UsedImplicitly] private set; }

        [WmiName("Manufacturer")]
        public string Manufacturer { get; [UsedImplicitly] private set; }

        [WmiName("Model")]
        public string Model { get; [UsedImplicitly] private set; }

        [WmiName("NumberOfLogicalProcessors")]
        public string NumberOfLogicalProcessors { get; [UsedImplicitly] private set; }

        [WmiName("NumberOfProcessors")]
        public string NumberOfProcessors { get; [UsedImplicitly] private set; }

        [WmiName("SystemFamily")]
        public string SystemFamily { get; [UsedImplicitly] private set; }

        [WmiName("SystemSKUNumber")]
        public string SystemSKUNumber { get; [UsedImplicitly] private set; }


        public Win32ComputerSystem(ILogger<WmiProvider> logger) : base(logger)
        {
        }
    }
}