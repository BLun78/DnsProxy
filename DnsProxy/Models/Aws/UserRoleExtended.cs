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

using Amazon.Runtime;

namespace DnsProxy.Models.Aws
{
    internal class UserRoleExtended : UserRole, IAwsDoScan
    {
        public UserRoleExtended(UserRole userRole)
        {
            AwsAccountId = userRole.AwsAccountId;
            AwsAccountLabel = userRole.AwsAccountLabel;
            Role = userRole.Role;
            DoScan = userRole.DoScan;
            ScanVpcIds = userRole.ScanVpcIds;
        }

        public AWSCredentials AwsCredentials { get; set; }
    }
}