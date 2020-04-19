#region Apache License-2.0
// Copyright 2020 Bjoern Lundstroem
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
#endregion

using System.Collections.Generic;
using System.Management.Automation;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Common.Cache;
using Microsoft.Extensions.Caching.Memory;

namespace DnsProxy.PowerShell.Commands.Cache
{
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