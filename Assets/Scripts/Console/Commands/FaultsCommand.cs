using System.Collections.Generic;

namespace PureSim.Console.Commands
{
    /// <summary>
    /// Faults command - lists all active faults in the simulation.
    /// </summary>
    [ConsoleCommand("faults")]
    public class FaultsCommand : IConsoleCommand
    {
        public string Name => "faults";
        public string Synopsis => "List all active faults";
        public IReadOnlyList<string> Parameters => new string[0];
        
        public void Execute(Simulation.SimulationState sim, string[] args, IConsoleOutput output)
        {
            var faults = sim.GetActiveFaults();
            
            if (faults.Count == 0)
            {
                output.WriteSuccess("No active faults");
                output.WriteLine("Use 'inject <fault_id> [description]' to inject a fault");
                return;
            }
            
            output.WriteLine($"Active Faults ({faults.Count}):");
            output.WriteLine("");
            
            foreach (var fault in faults)
            {
                output.WriteLine($"  • {fault.Id}");
                if (!string.IsNullOrEmpty(fault.Description))
                {
                    output.WriteLine($"    {fault.Description}");
                }
            }
            
            output.WriteLine("");
            output.WriteLine("Use 'clearfault <fault_id>' to clear a specific fault");
        }
    }
}
