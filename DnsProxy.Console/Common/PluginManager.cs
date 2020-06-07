#region Apache License-2.0
// Copyright 2020 Bjoern Lundstroem
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
#endregion

using ARSoft.Tools.Net;
using DnsProxy.Common;
using DnsProxy.Plugin;
using DnsProxy.Plugin.Common;
using DnsProxy.Plugin.Configuration;
using DnsProxy.Plugin.DI;
using DnsProxy.Plugin.Models.Rules;
using McMaster.NETCore.Plugins;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DnsProxy.Console.Common
{
    internal class PluginManager : IDisposable
    {
        private readonly ILogger _logger;
        private const string PluginFolder = @"Plugins";

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
            _logger.Information("[PluginManager] Load Plugins >>");
            InitialPluginManager();
        }

        public void RegisterDependencyRegistration(IConfigurationRoot configurationRoot)
        {
            DependencyRegistration.AddRange(Plugin.Where(x => x.DependencyRegistration != null)
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

                //var path = Path.Combine(Directory.GetCurrentDirectory(), PluginFolder);
                var path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                                            ?? AppDomain.CurrentDomain.BaseDirectory,
                                               PluginFolder);
                _logger.Information("[PluginManager] Plugins now load from path: [{path}]", path);

                var folder = Directory.GetDirectories(path);
                foreach (var item in folder)
                {
                    try
                    {
                        _logger.Information(LogConsts.SingleLine);
                        var pathSplit = GetSplitPath(item);

                        _logger.Information("[PluginManager] Load Plugin Folder: {folder}", pathSplit[^1]);
                        Assembly pluginAssembly = LoadPlugin(item);

                        var plugins = CreateCommands(pluginAssembly).ToList();
                        Plugin.AddRange(plugins);

                        _logger.Information("[PluginManager] Loaded Plugin: {pluginName}", plugins?.FirstOrDefault()?.PluginName);
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine(e);
                    }
                }

                Configurations.AddRange(Plugin.Where(x => x.DnsProxyConfiguration != null).Select(x => (IDnsProxyConfiguration)x.DnsProxyConfiguration));
                RuleFactories.AddRange(Plugin.Where(x => x.RuleFactory != null).Select(x => x.RuleFactory));

                _logger.Information(LogConsts.SingleLine);
                _logger.Information("[PluginManager] Plugins loaded >> Program starts");
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

        private static string[] GetSplitPath(string relativePath)
        {
            return relativePath.Split(Path.DirectorySeparatorChar);
        }

        private Assembly LoadPlugin(string relativePath)
        {
            _logger.Information("[PluginManager] Loading plugin from: [{pluginLocation}]", relativePath);

            var pathSplit = GetSplitPath(relativePath);
            var assemblyName = new AssemblyName(pathSplit[^1]);
            var pluginDll = $"{relativePath}{Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)}{assemblyName}.dll";

            var sharedTypes = new List<Type>() { typeof(DomainName) };
            sharedTypes.AddRange(PluginSharedTypes.SharedTypes);
            sharedTypes.AddRange(typeof(PluginSharedTypes).Assembly.GetTypes());
            sharedTypes.AddRange(typeof(CommonDnsProxyConfiguration).Assembly.GetTypes());
            sharedTypes.AddRange(typeof(DomainName).Assembly.GetTypes());
            sharedTypes.AddRange(typeof(DnsProxy.Server.ServerDependencyRegistration).Assembly.GetTypes());
            sharedTypes.AddRange(typeof(Serilog.ILogger).Assembly.GetTypes());
            sharedTypes.AddRange(typeof(Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme).Assembly.GetTypes());
            sharedTypes.AddRange(typeof(System.Console).Assembly.GetTypes());
            sharedTypes.AddRange(typeof(Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider).Assembly.GetTypes());
            sharedTypes.AddRange(typeof(Microsoft.Extensions.Logging.LoggerFactory).Assembly.GetTypes());
            sharedTypes.AddRange(typeof(Microsoft.Extensions.Primitives.IChangeToken).Assembly.GetTypes());

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
                var availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
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
