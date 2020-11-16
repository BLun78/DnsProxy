using System.Diagnostics.CodeAnalysis;
using BAG.IT.Core.Wmi.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

// ReSharper disable InconsistentNaming
namespace BAG.IT.Core.Wmi
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    public interface ISmsAuthority : IWmiProvider
    {
        string Name { get; }
        string CurrentManagementPoint { get; }
    }

    [UsedImplicitly]
    [ExcludeFromCodeCoverage]
    [WmiSearch("root\\ccm", "SELECT * FROM SMS_Authority")]
    internal class SmsAuthority : WmiProvider, ISmsAuthority
    {
        [WmiName("Name")]
        public string Name { get; [UsedImplicitly] private set; }

        [WmiName("CurrentManagementPoint")]
        public string CurrentManagementPoint { get; [UsedImplicitly] private set; }


        public SmsAuthority(ILogger<WmiProvider> logger) : base(logger)
        {
        }
    }
}



