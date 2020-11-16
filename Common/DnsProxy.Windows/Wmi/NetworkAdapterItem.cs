using System.Diagnostics.CodeAnalysis;
using BAG.IT.Core.Wmi.Core;
using JetBrains.Annotations;

namespace BAG.IT.Core.Wmi
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    // ReSharper disable InconsistentNaming
    public interface INetworkAdapterItem : IWmiProviderListItem
    {
        string NetConnectionStatus { get; }
        string Name { get; }
        string MacAddress { get; }
        string Index { get; }
        string NetConnectionStatusDisplayText { get; }
        bool IsConnected { get; }
    }

    [ExcludeFromCodeCoverage]
    [UsedImplicitly]
    internal class NetworkAdapterItem : WmiProviderListItem, INetworkAdapterItem
    {
        [WmiName("NetConnectionStatus")]
        public string NetConnectionStatus { get; [UsedImplicitly] private set; }

        [WmiName("Name")]
        public string Name { get; [UsedImplicitly] private set; }

        [WmiName("MacAddress")]
        public string MacAddress { get; [UsedImplicitly] private set; }

        [WmiName("Index")]
        public string Index { get; [UsedImplicitly] private set; }

        public string NetConnectionStatusDisplayText
        {
            get
            {
                return NetConnectionStatus switch
                {
                    "0" => "Disconnected",
                    "1" => "Connecting",
                    "2" => "Connected",
                    "3" => "Disconnecting",
                    "4" => "Hardware Not Present",
                    "5" => "Hardware Disabled",
                    "6" => "Hardware Malfunction",
                    "7" => "Media Disconnected",
                    "8" => "Authenticating",
                    "9" => "Authentication Succeeded",
                    "10" => "Authentication Failed",
                    "11" => "Invalid Address",
                    "12" => "Credentials Required",
                    _ => "Other"
                };
            }
        }

        public bool IsConnected => NetConnectionStatus == "2";
    }
}