﻿using System.Threading;
using DnsProxy.Common.DI;
using DnsProxy.Console.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DnsProxy.Console
{
    internal class ConsoleDependencyRegistration : DependencyRegistration, IDependencyRegistration
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public ConsoleDependencyRegistration(IConfigurationRoot configuration, CancellationTokenSource cancellationTokenSource) : base(configuration)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        public override void Register(IServiceCollection services)
        {
            // Program
            services.AddSingleton(this.GetType().Assembly);
            services.AddSingleton<ApplicationInformation>();
            services.AddSingleton(_cancellationTokenSource);
        }
    }
}