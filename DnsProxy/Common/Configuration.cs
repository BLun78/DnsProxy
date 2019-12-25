using Microsoft.Extensions.Configuration;
using System.IO;

namespace DnsProxy.Common
{
    internal class Configuration
    {
        private readonly string[] _args;

        public Configuration(string[] args)
        {
            _args = args;
        }

        internal IConfigurationRoot ConfigurationRoot => new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("config.json", optional: false, reloadOnChange: true)
            .AddJsonFile("rules.json", optional: false, reloadOnChange: true)
            .AddJsonFile("hosts.json", optional: false, reloadOnChange: true)
            .AddJsonFile("nameserver.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(_args)
            .Build();
    }
}
