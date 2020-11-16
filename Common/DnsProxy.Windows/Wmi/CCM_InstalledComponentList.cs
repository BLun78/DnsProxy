using BAG.IT.Core.Wmi.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;

namespace BAG.IT.Core.Wmi
{
#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    // ReSharper disable InconsistentNaming
    public interface ICCM_InstalledComponentList : IWmiProviderList<ICCM_InstalledComponent>
    {
    }

    [WmiSearch("root\\ccm", "Select * from CCM_InstalledComponent")]
    [UsedImplicitly]
    [ExcludeFromCodeCoverage]
    internal class CCM_InstalledComponentList : WmiProviderList<ICCM_InstalledComponent>, ICCM_InstalledComponentList
    {
        public CCM_InstalledComponentList(IServiceProvider serviceProvider,
            ILogger<WmiProviderList<ICCM_InstalledComponent>> logger)
            : base(serviceProvider, logger)
        {
        }
    }
#pragma warning restore CA1707 // Identifiers should not contain underscores
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
}