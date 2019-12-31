using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;

namespace DnsProxy.Common.Aws
{
    public class AwsCredentials
    {
        private Credentials MfaAwsSessionCredentials;

        private Task<string> GetMfaAsync(string awsAccount, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                Console.WriteLine("Login for AwsAccount: [" + awsAccount + "]:");
                Console.Write("Enter the MFA code: ");
                var mfaCode = Console.ReadLine();
                return mfaCode;
            }, cancellationToken);
        }

        private string Role(string targetAccountId, string targetRole)
        {
            return $"arn:aws:iam::{targetAccountId}:role/{targetRole}";
        }

        public async Task<AWSCredentials> CheckMfa(string awsAccount, string AccountId, string UserName,
            string accessKey, string secretKey, string targetAccountId, string targetRole, CancellationToken cancellationToken)
        {
            var mfaTOTP = await GetMfaAsync(awsAccount, cancellationToken).ConfigureAwait(false);
            try
            {
                AWSCredentials basicCredentials = new BasicAWSCredentials(accessKey, secretKey);

                var stsClient = new AmazonSecurityTokenServiceClient(basicCredentials);

                var getSessionTokenRequest = new GetSessionTokenRequest
                {
                    DurationSeconds = 3600,
                    SerialNumber = $"arn:aws:iam::{AccountId}:mfa/{UserName}",
                    TokenCode = mfaTOTP
                };

                var getSessionTokenResponse =
                    await stsClient.GetSessionTokenAsync(getSessionTokenRequest, cancellationToken).ConfigureAwait(false);
                MfaAwsSessionCredentials = getSessionTokenResponse.Credentials;


                stsClient = new AmazonSecurityTokenServiceClient(getSessionTokenResponse.Credentials);

                var role2 = await stsClient.AssumeRoleAsync(new AssumeRoleRequest
                {
                    RoleArn = Role(targetAccountId, targetRole),
                    RoleSessionName = "ReadEC2InstancesForRDP"
                }, cancellationToken).ConfigureAwait(false);

                var tempAccessKeyId = role2.Credentials.AccessKeyId;
                var tempSessionToken = role2.Credentials.SessionToken;
                var tempSecretAccessKey = role2.Credentials.SecretAccessKey;
                return new SessionAWSCredentials(tempAccessKeyId,
                    tempSecretAccessKey, tempSessionToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}