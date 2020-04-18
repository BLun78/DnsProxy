using System;
using System.Collections.Generic;
using System.Management.Automation;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Common.Cache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DnsProxy.PowerShell.Commands
{
    internal abstract class DnsCmdlet : Cmdlet
    {
        protected ILogger Logger => PowerShellRecourseLoader.Logger;
        protected IServiceProvider ServiceProvider => PowerShellRecourseLoader.ServiceProvider;

        protected DnsCmdlet() : base()
        {
        }
    }

    [Cmdlet(VerbsCommon.Remove, "DnsCacheItem")]
    internal class RemoveDnsCacheItem2 : DnsCmdlet
    {
        protected CacheManager CacheManager => PowerShellRecourseLoader.CacheManager;


        [Parameter(Position = 0)]
        [ValidateNotNullOrEmpty]
        public DnsQuestion DnsQuestion { get; set; }

        protected override void ProcessRecord()
        {
            CacheManager.RemoveCacheItem(DnsQuestion);
        }
    }

    [Cmdlet(VerbsCommon.Add, "DnsCacheItem")]
    internal class AddDnsCacheItem : DnsCmdlet
    {
        protected CacheManager CacheManager => PowerShellRecourseLoader.CacheManager;

        public AddDnsCacheItem()
        {
            DnsRecords = new List<DnsRecordBase>();
        }

        [Parameter(Position = 0)]
        [ValidateNotNullOrEmpty]
        public DnsQuestion DnsQuestion { get; set; }

        [Parameter(Position = 1)]
        [ValidateNotNullOrEmpty]
        public List<DnsRecordBase> DnsRecords { get; set; }

        [Parameter(Position = 2)] 
        public MemoryCacheEntryOptions MemoryCacheEntryOptions { get; set; }

        protected override void ProcessRecord()
        {
            if (MemoryCacheEntryOptions == null)
            {
                CacheManager.StoreInCache(DnsQuestion, DnsRecords);
            }
            else
            {
                CacheManager.StoreInCache(DnsQuestion, DnsRecords, MemoryCacheEntryOptions);
            }
        }
    }
}