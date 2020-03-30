using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ARSoft.Tools.Net;
using DnsProxy.Common;
using DnsProxy.Plugin;
using DnsProxy.Plugin.Configuration;
using DnsProxy.Plugin.DI;
using DnsProxy.Plugin.Models.Rules;
using DnsProxy.Server;
using McMaster.NETCore.Plugins;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DnsProxy.Console.Common
{
    internal class PluginManager : IDisposable
    {
        private readonly ILogger _logger;
        private const string PluginFolder = @".\Plugins";

        public List<IPlugin> Plugin { get; }
        public List<IDnsProxyConfiguration> Configurations { get; }
        public List<DependencyRegistration> DependencyRegistration { get; }
        public List<IRuleFactory> RuleFactories { get; }
        private List<PluginLoader> PluginLoaders { get; }

        public PluginManager(ILogger logger)
        {
            Plugin = new List<IPlugin>();
            Configurations = new List<IDnsProxyConfiguration>();
            DependencyRegistration = new List<DependencyRegistration>();
            PluginLoaders = new List<PluginLoader>();
            RuleFactories = new List<IRuleFactory>();
            _logger = logger;
            InitialPluginManager();
        }

        public void RegisterDependencyRegistration(IConfigurationRoot configurationRoot)
        {
            DependencyRegistration.AddRange(Plugin
                .Select(x => (DependencyRegistration)Activator.CreateInstance(x.DependencyRegistration, configurationRoot)));
        }

        private void InitialPluginManager()
        {
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    asm.GetTypes();
                }

                var path = Path.Combine(Directory.GetCurrentDirectory(), PluginFolder);
                _logger.Information("Pluginpath: {path}", path);

                var folder = Directory.GetDirectories(path);
                foreach (var item in folder)
                {
                    _logger.Information(item);
                    Assembly pluginAssembly = LoadPlugin(item);

                    var plugins = CreateCommands(pluginAssembly).ToList();
                    Plugin.AddRange(plugins);

                    _logger.Information("Loaded Plugin: {pluginName}", plugins?.FirstOrDefault()?.PluginName);

                    Configurations.AddRange(Plugin.Select(x => (IDnsProxyConfiguration)x.DnsProxyConfiguration));
                    RuleFactories.AddRange(Plugin.Select(x => x.RuleFactory));
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                var sb = new StringBuilder();
                foreach (var exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    if (exSub is FileNotFoundException exFileNotFound)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }

                string errorMessage = sb.ToString();
                //Display or log the error based on your application.
                _logger.Error(ex, errorMessage);
            }
            catch (Exception e)
            {
                _logger.Fatal(e, e.Message);
                throw;
            }
        }

        private Assembly LoadPlugin(string relativePath)
        {
            // Navigate up to the solution root
            string root = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(
                            Path.GetDirectoryName(
                                Path.GetDirectoryName(typeof(Program2).Assembly.Location)))))));

            string pluginLocation = Path.GetFullPath(Path.Combine(root, relativePath.Replace('\\', Path.DirectorySeparatorChar)));

            _logger.Information("Loading commands from: {pluginLocation}", pluginLocation);

            var pathSplit = pluginLocation.Split(@"\");
            var assemblyName = new AssemblyName(pathSplit[pathSplit.Length - 1]);
            var pluginDll = $"{pluginLocation}\\{assemblyName}.dll";

            List<Type> sharedTypes = new List<Type>() { typeof(DomainName) };
            sharedTypes.AddRange(PluginSharedTypes.SharedTypes);
            sharedTypes.AddRange(typeof(PluginSharedTypes).Assembly.GetTypes());
            sharedTypes.AddRange(typeof(CommonDnsProxyConfiguration).Assembly.GetTypes());
            sharedTypes.AddRange(typeof(DomainName).Assembly.GetTypes());
            sharedTypes.AddRange(typeof(ServerDnsProxyConfiguration).Assembly.GetTypes());
            sharedTypes.AddRange(typeof(Log).Assembly.GetTypes());

            var loader = PluginLoader.CreateFromAssemblyFile(
                pluginDll,
                sharedTypes: sharedTypes.ToArray());

            PluginLoaders.Add(loader);

            return loader.LoadDefaultAssembly();

        }

        private IEnumerable<IPlugin> CreateCommands(Assembly assembly)
        {
            var plugins = new List<IPlugin>();

            foreach (Type type in assembly
                .GetTypes()
                .Where(x => typeof(IPlugin).IsAssignableFrom(x) && !x.IsAbstract))
            {
                if (Activator.CreateInstance(type) is IPlugin result)
                {
                    plugins.Add(result);
                }
            }

            if (plugins.Count == 0)
            {
                string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
                throw new ApplicationException(
                    $"Can't find any type which implements IPlugin in {assembly} from {assembly.Location}.\n" +
                    $"Available types: {availableTypes}");
            }

            return plugins;
        }

        public void Dispose()
        {
            PluginLoaders.ForEach(x => x.Dispose());
            PluginLoaders.Clear();
            Configurations.Clear();
            DependencyRegistration.Clear();
            Plugin.Clear();
        }
    }
}
