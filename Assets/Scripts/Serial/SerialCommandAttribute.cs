using System;

namespace PureSim.Serial
{
    /// <summary>
    /// Attribute to mark classes as serial terminal commands for automatic discovery.
    /// Commands marked with this attribute will be registered by SerialRegistry.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class SerialCommandAttribute : Attribute
    {
        /// <summary>
        /// Command name as typed by the user
        /// </summary>
        public string Name { get; }
        
        public SerialCommandAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Command name cannot be empty", nameof(name));
            
            Name = name;
        }
    }
}
