using System.Diagnostics.CodeAnalysis;
using BAG.IT.Core.Wmi.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace BAG.IT.Core.Wmi
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    // ReSharper disable InconsistentNaming
    public interface IMsftdaConnectionStatus : IWmiProvider
    {
        string Status { get; }
        string Substatus { get; }

    }

    [UsedImplicitly]
    [WmiSearch("root\\StandardCimv2", "SELECT * FROM MSFT_DAConnectionStatus")]
    [ExcludeFromCodeCoverage]
    internal class MsftdaConnectionStatus : WmiProvider, IMsftdaConnectionStatus
    {
        [WmiName("Status")]
        public string Status { get; [UsedImplicitly] private set; }

        [WmiName("Substatus")]
        public string Substatus { get; [UsedImplicitly] private set; }

        public MsftdaConnectionStatus(ILogger<WmiProvider> logger) : base(logger)
        {
        }
    }
}