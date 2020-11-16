
using BAG.IT.Core.Wmi.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;

namespace BAG.IT.Core.Wmi
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    // ReSharper disable InconsistentNaming
    public interface INetworkAdapterList : IWmiProviderList<INetworkAdapterItem>
    {
    }

    [UsedImplicitly]
    [ExcludeFromCodeCoverage]
    [WmiSearch("root\\cimv2", "Select * from Win32_NetworkAdapter WHERE PhysicalAdapter = TRUE AND NOT PNPDeviceID LIKE 'ROOT\\\\%' AND ConfigManagerErrorCode = 0")]
    internal class NetworkAdapterList : WmiProviderList<INetworkAdapterItem>, INetworkAdapterList
    {
        public NetworkAdapterList(IServiceProvider serviceProvider,
            ILogger<WmiProviderList<INetworkAdapterItem>> logger)
            : base(serviceProvider, logger)
        {
        }
    }
}