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

using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using DnsProxy.Aws.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DnsProxy.Aws
{
    internal class AwsMfa
    {
        public Task<string> GetMfaAsync(UserAccountExtended userAccount, CancellationToken cancellationToken)
        {
            if (userAccount == null) throw new ArgumentNullException(nameof(userAccount));
            if (cancellationToken == null) throw new ArgumentNullException(nameof(cancellationToken));

            return Task.Run(() =>
            {
                Console.WriteLine("Login for AwsAccount [" + userAccount.UserName + "]:");
                Console.Write("Enter the MFA code: ");
                string mfaCode = null;
                var mfaExpression = true;
                while (mfaExpression)
                {
                    mfaCode = Console.ReadLine();
                    mfaExpression = !(!string.IsNullOrWhiteSpace(mfaCode)
                                      && mfaCode.Length == 6
                                      && int.TryParse(mfaCode, out var temp2));
                    if (!mfaExpression) break;

                    Console.WriteLine(
                        "==================================================================================");
                    Console.WriteLine("Login for AwsAccount [" + userAccount.UserName + "]:");
                    Console.Write("Retry enter the MFA code [numeric(6)]: ");
                }

                return mfaCode;
            }, cancellationToken);
        }

        private string Role(string targetAccountId, string targetRole)
        {
            return $"arn:aws:iam::{targetAccountId}:role/{targetRole}";
        }

        public async Task CreateAwsCredentialsAsync(UserAccountExtended userAccount, string mfaToken,
            CancellationToken cancellationToken)
        {
            if (userAccount == null) throw new ArgumentNullException(nameof(userAccount));
            if (string.IsNullOrWhiteSpace(mfaToken))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(mfaToken));
            if (cancellationToken == null) throw new ArgumentNullException(nameof(cancellationToken));

            var basicAwsCredentials = new BasicAWSCredentials(userAccount.UserAccessKey, userAccount.UserSecretKey);

            using (var stsClient = new AmazonSecurityTokenServiceClient(basicAwsCredentials))
            {
                var getSessionTokenRequest = new GetSessionTokenRequest
                {
                    DurationSeconds = 3600,
                    SerialNumber = $"arn:aws:iam::{userAccount.UserAccountId}:mfa/{userAccount.UserName}",
                    TokenCode = mfaToken
                };

                var getSessionTokenResponse =
                    await stsClient.GetSessionTokenAsync(getSessionTokenRequest, cancellationToken)
                        .ConfigureAwait(false);
                userAccount.AwsCredentials = getSessionTokenResponse.Credentials;
            }
        }

        public async Task AssumeRoleAsync(UserAccountExtended userAccount, UserRoleExtended userRole,
            CancellationToken cancellationToken)
        {
            if (userAccount == null) throw new ArgumentNullException(nameof(userAccount));
            if (userRole == null) throw new ArgumentNullException(nameof(userRole));
            if (cancellationToken == null) throw new ArgumentNullException(nameof(cancellationToken));

            using (var stsClient = new AmazonSecurityTokenServiceClient(userAccount.AwsCredentials))
            {
                var request = new AssumeRoleRequest
                {
                    RoleArn = Role(userRole.AwsAccountId, userRole.Role),
                    RoleSessionName = userRole.AwsAccountLabel
                };

                var role2 = await stsClient.AssumeRoleAsync(request, cancellationToken)
                    .ConfigureAwait(false);

                var tempAccessKeyId = role2.Credentials.AccessKeyId;
                var tempSessionToken = role2.Credentials.SessionToken;
                var tempSecretAccessKey = role2.Credentials.SecretAccessKey;
                userRole.AwsCredentials =
                    new SessionAWSCredentials(tempAccessKeyId, tempSecretAccessKey, tempSessionToken);
            }
        }
    }
}