using DnsProxy.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DnsProxy
{
    public sealed class Program
    {
        private static ILogger<Program> Logger;
        internal static ApplicationInformation ApplicationInformation { get; private set; }
        internal static Configuration Configuration { get; private set; }
        internal static DependencyInjector DependencyInjector { get; private set; }
        internal static IServiceProvider ServiceProvider => DependencyInjector.ServiceProvider;

        internal static string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                if (Environment.UserInteractive)
                {
                    Console.Title = value;
                }
            }
        }
        private static string _title;


        static async Task<int> Main(string[] args)
        {
            try
            {
                Setup(args);
                Title = ApplicationInformation.DefaultTitle;
                ApplicationInformation.LogAssemblyInformation();
                Logger.LogInformation("starts up {DefaultTitle}", ApplicationInformation.DefaultTitle);
                var dnsServer = ServiceProvider.GetService<DnsProxy.Dns.DnsServer>();

                dnsServer.StartServer();

                return await WaitForEndAsync().ConfigureAwait(true);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);
                await Task.Delay(100).ConfigureAwait(true);
                return await Task.FromResult(1).ConfigureAwait(true);
            }
#pragma warning restore CA1031 // Do not catch general exception types
            finally
            {
                Logger.LogInformation("stop {DefaultTitle}", ApplicationInformation.DefaultTitle);
                await Task.Delay(100).ConfigureAwait(true);
            }
        }

        private static void Setup(string[] args)
        {
            Program.Configuration = new Configuration(args);
            DependencyInjector = new DependencyInjector(Configuration.ConfigurationRoot);

            Logger = DependencyInjector.ServiceProvider.GetService<ILogger<Program>>();
            ApplicationInformation = DependencyInjector.ServiceProvider.GetService<ApplicationInformation>();
        }

        private static Task<int> WaitForEndAsync()
        {
            return Task.Run(() =>
            {
                var exit = false;
                while (!exit)
                {
                    var key = Console.ReadKey(true);
                    switch (key.Modifiers, key.Key)
                    {
                        case (ConsoleModifiers.Control, ConsoleKey.Q):
                        case (ConsoleModifiers.Control, ConsoleKey.X):
                            exit = true;
                            break;
                    }
                }
                return 0;
            });
        }
    }
}
