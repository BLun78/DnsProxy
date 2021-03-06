﻿#region Apache License-2.0
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

using DnsProxy.Plugin.Models.Rules;
using System;
using System.Collections.Generic;

namespace DnsProxy.Server.Models
{
#pragma warning disable CA2227 // Collection properties should be read only
    internal class RulesConfig : ICloneable
    {
        public List<Rule> Rules { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
#pragma warning restore CA2227 // Collection properties should be read only
}