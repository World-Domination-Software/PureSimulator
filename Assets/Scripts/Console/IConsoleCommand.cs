using System;
using System.Collections.Generic;

namespace PureSim.Console
{
    /// <summary>
    /// Interface for console commands that control the simulator (not operational commands).
    /// All console commands manipulate SimulationState, WorkflowEngine, or simulator diagnostics.
    /// </summary>
    public interface IConsoleCommand
    {
        /// <summary>
        /// Command name as typed by the user (e.g., "jump", "help", "inject")
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
        /// Execute the console command
        /// </summary>
        /// <param name="sim">Current simulation state</param>
        /// <param name="args">Command arguments (not including command name)</param>
        /// <param name="output">Output interface for writing results</param>
        void Execute(Simulation.SimulationState sim, string[] args, IConsoleOutput output);
    }
}
