using System.Collections.Generic;
using System.Text;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// purearray command - Lists and manages array and controller information.
    /// Common subcommands: list --controller, phonehome --send-today, remoteassist --connect
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs/putty2025-03-03.log L27-29
    /// Source: Docs/PuttyLogs/putty2025-02-22-2.txt
    /// Source: Docs/purearray.pdf
    /// </remarks>
    [SerialCommand("purearray")]
    public class PureArrayCommand : ISerialCommand
    {
        public string Name => "purearray";
        public string Synopsis => "List and manage array and controller information";
        public IReadOnlyList<string> Parameters => new[] { "list", "[--controller]", "phonehome", "remoteassist" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            if (args.Length == 0)
            {
                terminal.WriteLine("Usage: purearray <subcommand> [options]");
                terminal.WriteLine("  list [--controller]     List controllers");
                terminal.WriteLine("  phonehome --send-today  Send phonehome data");
                terminal.WriteLine("  remoteassist --connect  Connect remote assist");
                return;
            }
            
            var subcommand = args[0];
            
            switch (subcommand)
            {
                case "list":
                    HandleList(sim, args, terminal);
                    break;
                case "phonehome":
                    HandlePhonehome(args, terminal);
                    break;
                case "remoteassist":
                    HandleRemoteAssist(args, terminal);
                    break;
                default:
                    terminal.WriteLine($"purearray: unknown subcommand '{subcommand}'");
                    break;
            }
        }
        
        /// <summary>
        /// Handle 'purearray list --controller' command.
        /// Source: Docs/PuttyLogs/putty2025-03-03.log L27-29
        /// </summary>
        private void HandleList(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            bool showController = args.Length > 1 && args[1] == "--controller";
            
            if (showController)
            {
                var hardware = sim.GetHardwareModel();
                
                // Header
                terminal.WriteLine("Name  Type              Mode       Model     Version  Status  Internal Details");
                
                // List controllers
                var sb = new StringBuilder();
                foreach (var controller in hardware.Controllers)
                {
                    // Format: CT0   array_controller  secondary  FA-X70R3  6.5.8    ready
                    sb.AppendFormat("{0,-5} {1,-17} {2,-10} {3,-9} {4,-8} {5,-7} \n",
                        controller.Name, controller.Type, controller.Mode, 
                        controller.Model, controller.Version, controller.Status);
                }
                
                terminal.WriteLine(sb.ToString().TrimEnd());
            }
            else
            {
                // Basic array info (when no --controller flag)
                terminal.WriteLine("Name  Version  Model");
                terminal.WriteLine("array 6.5.8    FA-X70R3");
            }
        }
        
        /// <summary>
        /// Handle 'purearray phonehome --send-today' command.
        /// Source: Docs/PuttyLogs/putty2025-03-03.log L30-31
        /// </summary>
        private void HandlePhonehome(string[] args, ISerialOutput terminal)
        {
            if (args.Length < 2 || args[1] != "--send-today")
            {
                terminal.WriteLine("Usage: purearray phonehome --send-today");
                return;
            }
            
            // Simulate phonehome already running or completed
            // Source: Docs/PuttyLogs/putty2025-03-03.log L30-31
            terminal.WriteLine("Status  Action");
            terminal.WriteLine("-       -");
        }
        
        /// <summary>
        /// Handle 'purearray remoteassist --connect' command.
        /// </summary>
        private void HandleRemoteAssist(string[] args, ISerialOutput terminal)
        {
            if (args.Length < 2 || args[1] != "--connect")
            {
                terminal.WriteLine("Usage: purearray remoteassist --connect");
                return;
            }
            
            terminal.WriteLine("Remote Assist connection initiated");
        }
    }
}
