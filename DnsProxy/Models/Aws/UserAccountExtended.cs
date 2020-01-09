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
using System.Linq;
using Amazon.Runtime;

namespace DnsProxy.Models.Aws
{
    internal class UserAccountExtended : UserAccount, IAwsDoScan
    {
        public UserAccountExtended(UserAccount userAccount)
        {
            Roles = userAccount.Roles.Select(x => new UserRoleExtended(x)).ToList();
            UserAccountId = userAccount.UserAccountId;
            UserName = userAccount.UserName;
            UserAccessKey = userAccount.UserAccessKey;
            UserSecretKey = userAccount.UserSecretKey;
            DoScan = userAccount.DoScan;
            ScanVpcIds = userAccount.ScanVpcIds;
        }

        public new List<UserRoleExtended> Roles { get; set; }
        public AWSCredentials AwsCredentials { get; set; }
    }
}