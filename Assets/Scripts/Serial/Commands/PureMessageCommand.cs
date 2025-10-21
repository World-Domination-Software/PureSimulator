using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// puremessage command - List and manage system messages/alerts.
    /// Common usage: puremessage list --open, puremessage list --open --hidden
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs showing puremessage list --open usage
    /// </remarks>
    [SerialCommand("puremessage")]
    public class PureMessageCommand : ISerialCommand
    {
        public string Name => "puremessage";
        public string Synopsis => "List and manage system messages";
        public IReadOnlyList<string> Parameters => new[] { "list", "[--open]", "[--hidden]" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            if (args.Length == 0 || args[0] != "list")
            {
                terminal.WriteLine("Usage: puremessage list [--open] [--hidden]");
                return;
            }
            
            // Check for filters
            bool openOnly = false;
            bool includeHidden = false;
            
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "--open")
                    openOnly = true;
                else if (args[i] == "--hidden")
                    includeHidden = true;
            }
            
            // Header
            terminal.WriteLine("ID  Severity  Code  Message");
            
            // In a healthy system, there are typically no open messages
            // Show empty list or sample messages based on faults
            var faults = sim.GetActiveFaults();
            
            if (faults.Count == 0)
            {
                // No messages in healthy system
                return;
            }
            
            // Show messages for active faults
            int id = 1;
            foreach (var fault in faults)
            {
                terminal.WriteLine($"{id}   warning   {fault.Id}  {fault.Description}");
                id++;
            }
        }
    }
}
