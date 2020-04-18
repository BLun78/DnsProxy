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
using ARSoft.Tools.Net.Dns;
using DnsProxy.Common.Models;
using Microsoft.Extensions.Caching.Memory;

namespace DnsProxy.Common.Cache
{
    public class CacheManager
    {
        private readonly IMemoryCache _memoryCache;

        public CacheManager(
            IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public TElement Get<TElement>(string key)
        {
            return _memoryCache.Get<TElement>(key);
        }

        public void RemoveCacheItem(DnsQuestion dnsQuestion)
        {
            var key = dnsQuestion.ToString();
            var lastChar = key.Substring(key.Length - 1, 1);
            _memoryCache.Remove(lastChar == "."
                ? key
                : $"{key}.");
        }

        public void StoreInCache(DnsQuestion dnsQuestion, List<DnsRecordBase> data)
        {
            var cacheoptions = new MemoryCacheEntryOptions();
            cacheoptions.SetPriority(CacheItemPriority.NeverRemove);

            StoreInCache(dnsQuestion, data, cacheoptions);
        }

        public void StoreInCache(DnsQuestion dnsQuestionInput, List<DnsRecordBase> data,
            MemoryCacheEntryOptions cacheEntryOptions)
        {
            var dnsQuestion = dnsQuestionInput as DnsQuestion;
            var key = dnsQuestion.ToString();
            var key2 = new DnsQuestion(dnsQuestion.Name, RecordType.A, dnsQuestion.RecordClass).ToString();

            var cacheItem = new CacheItem(dnsQuestion.RecordType, data);
            _memoryCache.Set(key, cacheItem, cacheEntryOptions);

            if (dnsQuestion.RecordType != RecordType.A)
            {
                _memoryCache.Set(key2, cacheItem, cacheEntryOptions);
            }
        }
    }
}