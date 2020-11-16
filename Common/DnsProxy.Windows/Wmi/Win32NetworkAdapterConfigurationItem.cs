using System.Diagnostics.CodeAnalysis;
using BAG.IT.Core.Wmi.Core;
using JetBrains.Annotations;

namespace BAG.IT.Core.Wmi
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    public interface IWin32NetworkAdapterConfigurationItem : IWmiProviderListItem
    {
#pragma warning disable CA1819 // Properties should not return arrays
        string[] IpAddress { get; }

        string[] IpSubnet { get; }
        string Index { get; }
    }

    [UsedImplicitly]
    [ExcludeFromCodeCoverage]
    internal class Win32NetworkAdapterConfigurationItem : WmiProviderListItem, IWin32NetworkAdapterConfigurationItem
    {
        [WmiName("IPAddress")]
        public string[] IpAddress { get; [UsedImplicitly] private set; }

        [WmiName("IPSubnet")]

        public string[] IpSubnet { get; [UsedImplicitly] private set; }


        [WmiName("Index")]
        public string Index { get; [UsedImplicitly] private set; }

#pragma warning restore CA1819 // Properties should not return arrays
    }
}