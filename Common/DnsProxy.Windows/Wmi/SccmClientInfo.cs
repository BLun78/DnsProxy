using BAG.IT.Core.Wmi.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;

namespace BAG.IT.Core.Wmi
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    public interface ISccmClientInfo : IWmiProvider
    {
        bool InInternet { get; }
        DateTime InternetModeLastUpdateTime { get; }
    }

    [UsedImplicitly]
    [ExcludeFromCodeCoverage]
    [WmiSearch("root\\ccm", "SELECT * FROM ClientInfo")]
    internal class SccmClientInfo : WmiProvider, ISccmClientInfo
    {
        [WmiName("InInternet")]
        public bool InInternet { get; [UsedImplicitly] private set; }
        [WmiName("InternetModeLastUpdateTime")]
        public DateTime InternetModeLastUpdateTime { get; [UsedImplicitly] private set; }

        public SccmClientInfo(ILogger<WmiProvider> logger) : base(logger)
        {
        }
    };
}