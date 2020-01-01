using System.Collections.Generic;
using System.Linq;
using Amazon.Runtime;

namespace DnsProxy.Models.Aws
{
    internal class AwsContext
    {
        public AwsContext(AwsSettings awsSettings)
        {
            AwsSettings = new AwsSettingsExtended(awsSettings);
        }

        public AwsSettingsExtended AwsSettings { get; set; }
    }

    internal class AwsSettingsExtended : AwsSettings
    {
        public AwsSettingsExtended(AwsSettings awsSettings)
        {
            UserAccounts = awsSettings.UserAccounts.Select(x => new UserAccountExtended(x)).ToList();
        }

        public new List<UserAccountExtended> UserAccounts { get; set; }
    }

    internal class UserAccountExtended : UserAccount
    {
        public UserAccountExtended(UserAccount userAccount)
        {
            Roles = userAccount.Roles.Select(x => new UserRoleExtended(x)).ToList();
            UserAccountId = userAccount.UserAccountId;
            UserName = userAccount.UserName;
            UserAccessKey = userAccount.UserAccessKey;
            UserSecretKey = userAccount.UserSecretKey;
        }

        public new List<UserRoleExtended> Roles { get; set; }
        public AWSCredentials AwsCredentials { get; set; }
    }

    internal class UserRoleExtended : UserRole
    {
        public UserRoleExtended(UserRole userRole)
        {
            AwsAccountId = userRole.AwsAccountId;
            AwsAccountLabel = userRole.AwsAccountLabel;
            Role = userRole.Role;
        }

        public AWSCredentials AwsCredentials { get; set; }
    }

    internal class UserAccount
    {
        public string UserAccountId { get; set; }
        public string UserName { get; set; }
        public string UserAccessKey { get; set; }
        public string UserSecretKey { get; set; }
        public List<UserRole> Roles { get; set; }
    }
}