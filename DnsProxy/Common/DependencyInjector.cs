using DnsProxy.Dns;
using DnsProxy.Dns.Strategies;
using DnsProxy.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace DnsProxy.Common
{
    internal class DependencyInjector
    {
        private readonly IConfigurationRoot _configuration;
        public IServiceProvider ServiceProvider { get; private set; }

        public DependencyInjector(IConfigurationRoot configuration)
        {
            _configuration = configuration;
            ServiceProvider = ConfigureDependencyInjector().BuildServiceProvider(new ServiceProviderOptions()
            {
                ValidateOnBuild = true,
                ValidateScopes = true,
            });
        }

        private IServiceCollection ConfigureDependencyInjector()
        {
            var services = new ServiceCollection();

            services.AddSingleton<Assembly>(Assembly.GetExecutingAssembly());
            services.AddSingleton<ApplicationInformation>();
            services.AddSingleton<DnsProxy.Dns.DnsServer>();

            services.AddTransient<IDnsResolverStrategy, DohResolverStrategy>();
            services.AddTransient<IDnsResolverStrategy, DnsResolverStrategy>();
            services.AddSingleton<IDnsResolverStrategy, InternalNameServerResolverStrategy>();
            services.AddSingleton<IDnsResolverStrategy, MulticastResolverStrategy>();
            services.AddSingleton<IDnsResolverStrategy, HostsResolverStrategy>();
            services.AddTransient<DohResolverStrategy>();
            services.AddTransient<DnsResolverStrategy>();
            services.AddSingleton<InternalNameServerResolverStrategy>();
            services.AddSingleton<MulticastResolverStrategy>();
            services.AddSingleton<HostsResolverStrategy>();

            services.AddOptions();
            services.Configure<HostsConfig>(_configuration.GetSection("HostsConfig"));

            services.AddLogging(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("Default", LogLevel.Trace)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("DnsProxy.Program", LogLevel.Trace)
                    .AddConsole(options => { options.IncludeScopes = true; });
            });
            services.AddMemoryCache();
            // https://github.com/aspnet/Extensions/tree/master/src/HttpClientFactory
            // https://docs.microsoft.com/de-de/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            services.AddHttpClient(); // not top, but it works in all Applications - BL
            return services;
        }
    }
}
