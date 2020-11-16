using System.Diagnostics.CodeAnalysis;
using BAG.IT.Core.Wmi.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

// ReSharper disable InconsistentNaming
namespace BAG.IT.Core.Wmi
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    public interface IWin32Bios : IWmiProvider
    {
        string Manufacturer { get; }
        string SerialNumber { get; }
        string ReleaseDate { get; }
        string Status { get; }
        string Name { get; }
        string Version { get; }
    }

    [ExcludeFromCodeCoverage]
    [WmiSearch("root\\CIMV2", "SELECT * FROM Win32_BIOS")]
    internal class Win32Bios : WmiProvider, IWin32Bios
    {
        [WmiName("Manufacturer")]
        public string Manufacturer { get; [UsedImplicitly] private set; }

        [WmiName("SerialNumber")]
        public string SerialNumber { get; [UsedImplicitly] private set; }

        [WmiName("ReleaseDate")]
        public string ReleaseDate { get; [UsedImplicitly] private set; }

        [WmiName("Status")]
        public string Status { get; [UsedImplicitly] private set; }

        [WmiName("Name")]
        public string Name { get; [UsedImplicitly] private set; }



        [WmiName("Version")]
        public string Version { get; [UsedImplicitly] private set; }

        public Win32Bios(ILogger<WmiProvider> logger) : base(logger)
        {
        }
    }
}