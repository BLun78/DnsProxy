using System.Diagnostics.CodeAnalysis;
using BAG.IT.Core.Wmi.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming
namespace BAG.IT.Core.Wmi
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    public interface ISccmClient : IWmiProvider
    {
        string ClientId { get; }
        string ClientIdChangeDate { get; }
        string PreviousClientId { get; }
    }

    [UsedImplicitly]
    [ExcludeFromCodeCoverage]
    [WmiSearch("root\\ccm", "SELECT * FROM CCM_Client")]
    internal class SccmClient : WmiProvider, ISccmClient
    {
        [WmiName("ClientID")]
        public string ClientId { get; private set; }

        [WmiName("ClientIdChangeDate")]
        public string ClientIdChangeDate { get; private set; }

        [WmiName("PreviousClientId")]
        public string PreviousClientId { get; private set; }


        public SccmClient(ILogger<WmiProvider> logger) : base(logger)
        {
        }
    }
}