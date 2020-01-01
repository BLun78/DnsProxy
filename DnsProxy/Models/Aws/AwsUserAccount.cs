using System.Collections.Generic;

namespace DnsProxy.Models.Aws
{
    public class AwsUserAccount
    {
        public string UserAccountId { get; set; }
        public string UserName { get; set; }
        public string UserAccessKey { get; set; }
        public string UserSecretKey { get; set; }
        public List<UserRole> Roles { get; set; }
    }
}