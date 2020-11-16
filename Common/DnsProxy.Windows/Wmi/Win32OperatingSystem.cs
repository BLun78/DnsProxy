using BAG.IT.Core.Wmi.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable InconsistentNaming
namespace BAG.IT.Core.Wmi
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    public interface IWin32OperatingSystem : IWmiProvider
    {
        string BootDevice { get; }
        string BuildNumber { get; }
        string BuildType { get; }
        string CountryCode { get; }
        string Caption { get; }
        string Locale { get; }
        string Name { get; }
        string OsArchitecture { get; }
        string OsLanguage { get; }
        string OsType { get; }
        string OsProductSuite { get; }
        string SerialNumber { get; }
        string Status { get; }
        string SystemDevice { get; }
        string SystemDirectory { get; }
        string SystemDrive { get; }
        string WindowsDirectory { get; }
        string Version { get; }
        IDictionary<string, object> Data { get; }

    }

    [ExcludeFromCodeCoverage]
    [WmiSearch("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem")]
    internal class Win32OperatingSystem : WmiProvider, IWin32OperatingSystem
    {
        [WmiName("BootDevice")]
        public string BootDevice { get; [UsedImplicitly] private set; }

        [WmiName("BuildNumber")]
        public string BuildNumber { get; [UsedImplicitly] private set; }

        [WmiName("BuildType")]
        public string BuildType { get; [UsedImplicitly]  private set; }

        [WmiName("CountryCode")]
        public string CountryCode { get; [UsedImplicitly] private set; }

        [WmiName("Caption")]
        public string Caption { get; [UsedImplicitly] private set; }

        [WmiName("Locale")]
        public string Locale { get; [UsedImplicitly] private set; }


        [WmiName("Name")]
        public string Name { get; [UsedImplicitly] private set; }

        [WmiName("OSArchitecture")]
        public string OsArchitecture { get; [UsedImplicitly] private set; }

        [WmiName("OSLanguage")]
        public string OsLanguage { get; [UsedImplicitly] private set; }

        [WmiName("OSType")]
        public string OsType { get; [UsedImplicitly] private set; }

        [WmiName("OSProductSuite")]
        public string OsProductSuite { get; [UsedImplicitly] private set; }

        [WmiName("SerialNumber")]
        public string SerialNumber { get; [UsedImplicitly] private set; }

        [WmiName("Status")]
        public string Status { get; [UsedImplicitly] private set; }

        [WmiName("SystemDevice")]
        public string SystemDevice { get; [UsedImplicitly] private set; }

        [WmiName("SystemDirectory")]
        public string SystemDirectory { get; [UsedImplicitly]  private set; }

        [WmiName("SystemDrive")]
        public string SystemDrive { get; [UsedImplicitly]  private set; }

        [WmiName("WindowsDirectory")]
        public string WindowsDirectory { get; [UsedImplicitly] private set; }

        [WmiName("Version")]
        public string Version { get; [UsedImplicitly] private set; }

        public Win32OperatingSystem(ILogger<WmiProvider> logger) : base(logger)
        {
        }
    }
}