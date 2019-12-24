using System;
using Microsoft.Extensions.Configuration;
using System.IO;
using DnsProxy.Models;

namespace DnsProxy.Common
{
    internal class Configuration
    {
        internal static IConfigurationRoot ConfigurationRoot => new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("config.json", optional: false, reloadOnChange: true)
            .AddJsonFile("rules.json", optional: false, reloadOnChange: true)
            .AddJsonFile("hosts.json", optional: false, reloadOnChange: true)
            .AddJsonFile("nameserver.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
