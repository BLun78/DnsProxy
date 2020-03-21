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

namespace DnsProxy.Common.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Strategies
    {
        [EnumMember(Value = nameof(None))] None = 0,
        [EnumMember(Value = nameof(Hosts))] Hosts = 1,

        [EnumMember(Value = nameof(InternalNameServer))]
        InternalNameServer = 2,
        [EnumMember(Value = nameof(Dns))] Dns = 3,
        [EnumMember(Value = nameof(DoH))] DoH = 4,
        [EnumMember(Value = nameof(AwsDocDb))] AwsDocDb = 5,

        [EnumMember(Value = nameof(AwsElasticCache))]
        AwsElasticCache = 6,

        [EnumMember(Value = nameof(AwsApiGateway))]
        AwsApiGateway = 7
    }
}