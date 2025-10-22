using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// puredb command - Database and preference management.
    /// Used for setting controller preferences and checking NPIV status.
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs/putty2025-03-03.log L1122-1196
    /// Shows 'puredb prefer CT1' and 'puredb npiv status' usage
    /// </remarks>
    [SerialCommand("puredb")]
    public class PureDbCommand : ISerialCommand
    {
        public string Name => "puredb";
        public string Synopsis => "Database and preference management";
        public IReadOnlyList<string> Parameters => new[] { "prefer", "npiv", "status" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            if (args.Length == 0)
            {
                terminal.WriteLine("Usage: puredb <subcommand> [options]");
                terminal.WriteLine("  prefer <CT0|CT1>    Set preferred controller");
                terminal.WriteLine("  npiv status         Show NPIV status");
                return;
            }
            
            var subcommand = args[0];
            
            switch (subcommand)
            {
                case "prefer":
                    HandlePrefer(args, terminal);
                    break;
                case "npiv":
                    if (args.Length > 1 && args[1] == "status")
                    {
                        HandleNpivStatus(terminal);
                    }
                    else
                    {
                        terminal.WriteLine("Usage: puredb npiv status");
                    }
                    break;
                default:
                    terminal.WriteLine($"puredb: unknown subcommand '{subcommand}'");
                    break;
            }
        }
        
        private void HandlePrefer(string[] args, ISerialOutput terminal)
        {
            if (args.Length < 2)
            {
                terminal.WriteLine("Usage: puredb prefer <CT0|CT1>");
                return;
            }
            
            var controller = args[1].ToUpper();
            
            if (controller != "CT0" && controller != "CT1")
            {
                terminal.WriteLine($"Error: Invalid controller '{args[1]}'");
                terminal.WriteLine("Valid controllers: CT0, CT1");
                return;
            }
            
            // Source: Docs/PuttyLogs/putty2025-03-03.log L1125, L1134, L1194
            // Sometimes shows success, sometimes shows error
            // Simulate occasional error for realism
            if (System.DateTime.Now.Second % 3 == 0)
            {
                terminal.WriteLine($"Error on {controller}: Not able to satisfy XML-RPC at this time. Please try again.");
            }
            else
            {
                terminal.WriteLine($"Preferred controller set to {controller}");
            }
        }
        
        private void HandleNpivStatus(ISerialOutput terminal)
        {
            // Source: Docs/PuttyLogs/putty2025-03-03.log L1122
            terminal.WriteLine("NPIV Status: Enabled");
        }
    }
}
