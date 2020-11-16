using System.Diagnostics.CodeAnalysis;
using BAG.IT.Core.Wmi.Core;
using JetBrains.Annotations;
// ReSharper disable InconsistentNaming
namespace BAG.IT.Core.Wmi
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
#pragma warning disable CA1707 // Identifiers should not contain underscores
    public interface ICCM_InstalledComponent : IWmiProviderListItem
    {
        string Name { get; }
        string DisplayName { get; }
        string DisplayNameResourceFile { get; }
        string DisplayNameResourceID { get; }
        string Version { get; }

    }

    [UsedImplicitly]
    [ExcludeFromCodeCoverage]
    internal class CCM_InstalledComponent : WmiProviderListItem, ICCM_InstalledComponent
#pragma warning restore CA1707 // Identifiers should not contain underscores
    {
        [WmiName("Name")]
        public string Name { get; [UsedImplicitly] private set; }

        [WmiName("DisplayName")]
        public string DisplayName { get; [UsedImplicitly] private set; }

        [WmiName("DisplayNameResourceFile")]
        public string DisplayNameResourceFile { get; [UsedImplicitly] private set; }

        [WmiName("DisplayNameResourceID")]

        public string DisplayNameResourceID { get; [UsedImplicitly] private set; }

        [WmiName("Version")]
        public string Version { get; [UsedImplicitly] private set; }
    };
#pragma warning restore CA1707 // Identifiers should not contain underscores
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
}