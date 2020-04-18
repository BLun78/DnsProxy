using System;
using DnsProxy.Common.Cache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DnsProxy.PowerShell
{
    internal class PowerShellRecourseLoader
    {
        public static IServiceProvider ServiceProvider{ get; private set; }
        public static CacheManager CacheManager { get; private set; }
        public static ILogger<PowerShellPlugin> Logger { get; private set; }

        public PowerShellRecourseLoader(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            CacheManager = (CacheManager)serviceProvider.GetService(typeof(CacheManager));
            Logger = (ILogger<PowerShellPlugin>)serviceProvider.GetService(typeof(ILogger<PowerShellPlugin>));
        }
    }
}