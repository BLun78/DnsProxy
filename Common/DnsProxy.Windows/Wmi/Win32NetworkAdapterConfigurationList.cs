using BAG.IT.Core.Wmi.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;

namespace BAG.IT.Core.Wmi
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    // ReSharper disable InconsistentNaming
    public interface IWin32NetworkAdapterConfigurationList : IWmiProviderList<IWin32NetworkAdapterConfigurationItem>
    {
    }

    [UsedImplicitly]
    [ExcludeFromCodeCoverage]
    [WmiSearch("root\\cimv2", "Select * from Win32_NetworkAdapterConfiguration where IPEnabled = TRUE")]
    internal class Win32NetworkAdapterConfigurationList : WmiProviderList<IWin32NetworkAdapterConfigurationItem>, IWin32NetworkAdapterConfigurationList
    {
        public Win32NetworkAdapterConfigurationList(IServiceProvider serviceProvider,
            ILogger<WmiProviderList<IWin32NetworkAdapterConfigurationItem>> logger)
            : base(serviceProvider, logger)
        {
        }
    }
}