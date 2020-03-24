using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Console.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Console
{
    public sealed class Program2
    {
        private static ILogger<Program2> _logger;
        private static string _title;

        private static CancellationTokenSource CancellationTokenSource { get; set; }
        private static DependencyInjector DependencyInjector { get; set; }
        private static IServiceProvider ServiceProvider => DependencyInjector.ServiceProvider;

        public static async Task<int> Main(string[] args)
        {
            try
            {
                Setup(args);

                //using (var dnsServer = ServiceProvider.GetService<DnsServer>())
                //{
                //    await AwsVpcExtensions.CheckAwsVpc().ConfigureAwait(false);

                //    return await WaitForEndAsync().ConfigureAwait(false);
                //}

                return 0;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                _logger?.LogError(e, e.Message);
                await Task.Delay(100).ConfigureAwait(false);
                return await Task.FromResult(1).ConfigureAwait(false);
            }
#pragma warning restore CA1031 // Do not catch general exception types
            finally
            {
                _logger?.LogInformation("stop {DefaultTitle}", ApplicationInformation.DefaultTitle);
            }
        }

        internal static string Title
        {
            get => _title;
            set
            {
                _title = value;
                if (Environment.UserInteractive) System.Console.Title = value;
            }
        }

        private static void Setup(string[] args)
        {
            CancellationTokenSource = new CancellationTokenSource();
            var d = new PluginManager();

            //Configuration = new Configuration(args);
            //DependencyInjector = new DependencyInjector(
            //    Configuration.ConfigurationRoot,
            //    typeof(Program).Assembly,
            //    CancellationTokenSource);

            //_logger = DependencyInjector.ServiceProvider.GetService<ILogger<Program>>();
            //ApplicationInformation = DependencyInjector.ServiceProvider.GetService<ApplicationInformation>();

            //CreateHeader();
        }

    }
}
