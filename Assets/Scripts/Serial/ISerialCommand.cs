using System.Collections.Generic;

namespace PureSim.Serial
{
    /// <summary>
    /// Interface for serial terminal commands that execute operational commands.
    /// These commands simulate a real serial connection to Purity (Ubuntu-based OS).
    /// All outputs must mirror real logs/PDFs exactly.
    /// </summary>
    public interface ISerialCommand
    {
        /// <summary>
        /// Command name as typed by the user (e.g., "lsblk", "mount", "ls")
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Brief one-line description of what the command does
        /// </summary>
        string Synopsis { get; }
        
        /// <summary>
        /// List of parameter descriptions for help text
        /// </summary>
        IReadOnlyList<string> Parameters { get; }
        
        /// <summary>
        /// Execute the serial command
        /// </summary>
        /// <param name="sim">Current simulation state</param>
        /// <param name="args">Command arguments (not including command name)</param>
        /// <param name="terminal">Terminal output interface for writing results</param>
        void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal);
    }
}
