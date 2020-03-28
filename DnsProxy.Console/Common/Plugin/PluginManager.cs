using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DnsProxy.Plugin;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DnsProxy.Console.Common.Plugin
{
    internal class PluginManager
    {
        private readonly ILogger _logger;
        private const string PluginFolder = @".\Plugins";

        public List<IPlugin> Plugin { get; }
        public List<IDnsProxyConfiguration> Configurations { get; }
        public List<DependencyRegistration> DependencyRegistration { get; }

        public PluginManager(ILogger logger)
        {
            Plugin = new List<IPlugin>();
            Configurations = new List<IDnsProxyConfiguration>();
            DependencyRegistration = new List<DependencyRegistration>();
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

                    //foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                    //{
                    //    asm.GetTypes();
                    //}

                    Plugin.AddRange(CreateCommands(pluginAssembly));

                    _logger.Information("Loaded Plugin: {pluginName}", Plugin?.FirstOrDefault()?.PluginName);

                    Configurations.AddRange(Plugin.Select(x => x.DnsProxyConfiguration));
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
            var dllName = assemblyName + ".dll";
            var loadContext = new PluginLoadContext(Path.Combine(pluginLocation, dllName));

            return loadContext.LoadFromAssemblyName(assemblyName);
        }

        private IEnumerable<IPlugin> CreateCommands(Assembly assembly)
        {
            var plugins = new List<IPlugin>();

            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(IPlugin).IsAssignableFrom(type))
                {
                    if (Activator.CreateInstance(type) is IPlugin result)
                    {
                        plugins.Add(result);
                    }
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
    }
}
