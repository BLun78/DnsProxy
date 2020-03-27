using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using DnsProxy.Plugin;

namespace DnsProxy.Console.Common
{
    internal class PluginManager
    {
        private const string PluginFolder = @".\Plugins";

        public PluginManager()
        {
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), PluginFolder);
                System.Console.WriteLine(path);

                var folder = Directory.GetDirectories(path);
                foreach (var item in folder)
                {
                    System.Console.WriteLine(item);
                    Assembly pluginAssembly = LoadPlugin(item);
                    var plugin = CreateCommands(pluginAssembly);

                    System.Console.WriteLine(plugin?.FirstOrDefault()?.PluginName);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                    if (exFileNotFound != null)
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
                System.Console.WriteLine(errorMessage);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
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
            System.Console.WriteLine($"Loading commands from: {pluginLocation}");
            var pathSplitt = pluginLocation.Split(@"\");
            var assemblyName = new AssemblyName(pathSplitt[pathSplitt.Length - 1]);
            var dllName = assemblyName + ".dll";
            PluginLoadContext loadContext = new PluginLoadContext(Path.Combine(pluginLocation,dllName));
            
            return loadContext.LoadFromAssemblyName(assemblyName);
        }

        private IEnumerable<IPlugin> CreateCommands(Assembly assembly)
        {
            var plugins = new List<IPlugin>();

            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(IPlugin).IsAssignableFrom(type) // || type.
                    )
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
