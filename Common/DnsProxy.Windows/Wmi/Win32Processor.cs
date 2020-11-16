using BAG.IT.Core.Wmi.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable InconsistentNaming
namespace BAG.IT.Core.Wmi
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    public interface IWin32Processor : IWmiProvider
    {
        string Caption { get; }
        string CurrentClockSpeed { get; }
        string Description { get; }
        string Name { get; }
        string L2CacheSize { get; }
        string L3CacheSize { get; }
        string MaxClockSpeed { get; }
        string NumberOfCores { get; }
        string ProcessorId { get; }
        string ThreadCount { get; }
        string NumberOfLogicalProcessors { get; }
        IDictionary<string, object> Data { get; }

    }

    [ExcludeFromCodeCoverage]
    [WmiSearch("root\\CIMV2", "SELECT * FROM Win32_Processor")]
    internal class Win32Processor : WmiProvider, IWin32Processor
    {


        [WmiName("Caption")]
        public string Caption { get; [UsedImplicitly] private set; }


        [WmiName("CurrentClockSpeed")]
        public string CurrentClockSpeed { get; [UsedImplicitly] private set; }

        [WmiName("Description")]
        public string Description { get; [UsedImplicitly] private set; }

        [WmiName("Name")]
        public string Name { get; [UsedImplicitly] private set; }

        [WmiName("L2CacheSize")]
        public string L2CacheSize { get; [UsedImplicitly] private set; }

        [WmiName("L3CacheSize")]
        public string L3CacheSize { get; [UsedImplicitly] private set; }

        [WmiName("MaxClockSpeed")]
        public string MaxClockSpeed { get; [UsedImplicitly] private set; }

        [WmiName("NumberOfCores")]
        public string NumberOfCores { get; [UsedImplicitly] private set; }


        [WmiName("ProcessorId")]
        public string ProcessorId { get; [UsedImplicitly] private set; }


        [WmiName("ThreadCount")]
        public string ThreadCount { get; [UsedImplicitly]  private set; }


        [WmiName("NumberOfLogicalProcessors")]
        public string NumberOfLogicalProcessors { get; [UsedImplicitly] private set; }


        public Win32Processor(ILogger<WmiProvider> logger) : base(logger)
        {
        }
    }
}