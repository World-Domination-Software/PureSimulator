using System.Collections.Generic;

namespace PureSim.Console.Commands
{
    /// <summary>
    /// Clear command - clears the console output.
    /// </summary>
    [ConsoleCommand("clear")]
    public class ClearCommand : IConsoleCommand
    {
        public string Name => "clear";
        public string Synopsis => "Clear the console output";
        public IReadOnlyList<string> Parameters => new string[0];
        
        public void Execute(Simulation.SimulationState sim, string[] args, IConsoleOutput output)
        {
            output.Clear();
            output.WriteLine("=== PureSim Developer Console ===");
            output.WriteLine("Type 'help' for available commands");
        }
    }
}
