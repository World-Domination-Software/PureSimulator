using System;

namespace PureSim.Console
{
    /// <summary>
    /// Attribute to mark classes as console commands for automatic discovery.
    /// Commands marked with this attribute will be registered by ConsoleRegistry.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ConsoleCommandAttribute : Attribute
    {
        /// <summary>
        /// Command name as typed by the user
        /// </summary>
        public string Name { get; }
        
        public ConsoleCommandAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Command name cannot be empty", nameof(name));
            
            Name = name.ToLowerInvariant();
        }
    }
}
