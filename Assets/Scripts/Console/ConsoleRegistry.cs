using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PureSim.Console
{
    /// <summary>
    /// Registry for console commands using reflection-based discovery.
    /// Finds all classes marked with [ConsoleCommand] attribute and registers them.
    /// </summary>
    public class ConsoleRegistry
    {
        private readonly Dictionary<string, IConsoleCommand> commands = new Dictionary<string, IConsoleCommand>();
        
        public ConsoleRegistry()
        {
            DiscoverCommands();
        }
        
        private void DiscoverCommands()
        {
            commands.Clear();
            
            try
            {
                // Find all types with ConsoleCommandAttribute
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var types = assembly.GetTypes()
                            .Where(t => t.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).Length > 0)
                            .Where(t => typeof(IConsoleCommand).IsAssignableFrom(t))
                            .Where(t => !t.IsAbstract && !t.IsInterface);
                        
                        foreach (var type in types)
                        {
                            try
                            {
                                var instance = Activator.CreateInstance(type) as IConsoleCommand;
                                if (instance != null)
                                {
                                    var attributes = type.GetCustomAttributes(typeof(ConsoleCommandAttribute), false)
                                        as ConsoleCommandAttribute[];
                                    
                                    foreach (var attr in attributes)
                                    {
                                        string cmdName = attr.Name.ToLowerInvariant();
                                        if (!commands.ContainsKey(cmdName))
                                        {
                                            commands[cmdName] = instance;
                                            Debug.Log($"Registered console command: {cmdName}");
                                        }
                                        else
                                        {
                                            Debug.LogWarning($"Duplicate console command registration: {cmdName}");
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"Failed to instantiate command {type.Name}: {e.Message}");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // Skip assemblies that can't be reflected
                        Debug.LogWarning($"Could not reflect assembly {assembly.FullName}: {e.Message}");
                    }
                }
                
                Debug.Log($"Console command discovery complete. Registered {commands.Count} commands.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Command discovery failed: {e.Message}");
            }
        }
        
        public bool TryGetCommand(string commandName, out IConsoleCommand command)
        {
            return commands.TryGetValue(commandName.ToLowerInvariant(), out command);
        }
        
        public IReadOnlyList<string> GetAllCommandNames()
        {
            return commands.Keys.OrderBy(k => k).ToList();
        }
        
        public IReadOnlyList<IConsoleCommand> GetAllCommands()
        {
            return commands.Values.ToList();
        }
        
        public void Reload()
        {
            DiscoverCommands();
        }
    }
}
