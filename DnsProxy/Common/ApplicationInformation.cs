using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace DnsProxy.Common
{
    internal class ApplicationInformation
    {
        internal const string DefaultTitle = "BLun.de DNS Proxy";
        private readonly Assembly _assembly;
        private readonly ILogger<ApplicationInformation> _logger;

        public ApplicationInformation(Assembly assembly, ILogger<ApplicationInformation> logger)
        {
            _assembly = assembly;
            _logger = logger;
        }

        public void LogAssemblyInformation()
        {
            var version = _assembly.GetName().Version;
            var buildTime = new DateTime(1900, 1, 1);
            var title = DefaultTitle;
            _logger.LogInformation(@"Title: '{title}' Version: '{version}' Builddate: '{buildTime}'", title, version,
                buildTime);
        }
    }
}
