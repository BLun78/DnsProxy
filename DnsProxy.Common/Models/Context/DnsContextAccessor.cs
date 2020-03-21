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

using System.Threading;

namespace DnsProxy.Common.Models.Context
{
    internal class DnsContextAccessor : IDnsContextAccessor, IWriteDnsContextAccessor
    {
        private static readonly AsyncLocal<WriteDnsContextHolder> _writeDnsContextCurrent =
            new AsyncLocal<WriteDnsContextHolder>();

        public IDnsCtx DnsContext
        {
            get => WriteDnsContext;
            set => WriteDnsContext = value as IWriteDnsContext;
        }

        public IWriteDnsContext WriteDnsContext
        {
            get => _writeDnsContextCurrent.Value?.Context;
            set
            {
                var holder = _writeDnsContextCurrent.Value;
                if (holder != null)
                {
                    // Clear current DnsContext trapped in the AsyncLocals, as its done.
                    holder.Context = null;
                }

                if (value != null)
                {
                    // Use an object indirection to hold the DnsContext in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    _writeDnsContextCurrent.Value = new WriteDnsContextHolder { Context = value };
                }
            }
        }

        private class WriteDnsContextHolder
        {
            public IWriteDnsContext Context;
        }
    }
}