using System.Collections.Generic;

namespace PureSim.Console.Commands
{
    /// <summary>
    /// ClearFault command - clears a specific fault or all faults from the simulation.
    /// </summary>
    [ConsoleCommand("clearfault")]
    public class ClearFaultCommand : IConsoleCommand
    {
        public string Name => "clearfault";
        public string Synopsis => "Clear a specific fault from the simulation";
        public IReadOnlyList<string> Parameters => new[] { "<fault_id>|all" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, IConsoleOutput output)
        {
            if (args.Length == 0)
            {
                output.WriteError("Usage: clearfault <fault_id>|all");
                output.WriteLine("Use 'faults' to see active faults");
                return;
            }
            
            string faultId = args[0];
            
            if (faultId.ToLowerInvariant() == "all")
            {
                var faults = sim.GetActiveFaults();
                int count = faults.Count;
                
                foreach (var fault in faults)
                {
                    sim.ClearFault(fault.Id);
                }
                
                output.WriteSuccess($"Cleared {count} fault(s)");
                return;
            }
            
            if (!sim.HasFault(faultId))
            {
                output.WriteWarning($"Fault '{faultId}' is not active");
                return;
            }
            
            sim.ClearFault(faultId);
            output.WriteSuccess($"Cleared fault: {faultId}");
        }
    }
}
