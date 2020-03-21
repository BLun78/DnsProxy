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

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DnsProxy.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    internal enum AuthenticationType
    {
        [EnumMember(Value = nameof(None))] None = 0,
        [EnumMember(Value = nameof(Basic))] Basic = 1,
        [EnumMember(Value = nameof(WindowsDomain))] WindowsDomain = 2,
        [EnumMember(Value = nameof(WindowsUser))] WindowsUser = 3
    }
}