using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// purenetwork command - Lists and manages network interfaces and ports.
    /// Subcommands: list, eth list, fc list
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs/putty2025-02-22-2.txt showing purenetwork list output
    /// Source: Docs/Purenetwork.pdf
    /// </remarks>
    [SerialCommand("purenetwork")]
    public class PureNetworkCommand : ISerialCommand
    {
        public string Name => "purenetwork";
        public string Synopsis => "List and manage network interfaces";
        public IReadOnlyList<string> Parameters => new[] { "list", "eth", "fc" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            if (args.Length == 0)
            {
                terminal.WriteLine("Usage: purenetwork <subcommand>");
                terminal.WriteLine("  list       List all network interfaces");
                terminal.WriteLine("  eth list   List Ethernet interfaces");
                terminal.WriteLine("  fc list    List Fibre Channel interfaces");
                return;
            }
            
            var subcommand = args[0];
            
            switch (subcommand)
            {
                case "list":
                    HandleList(sim, terminal);
                    break;
                case "eth":
                    if (args.Length > 1 && args[1] == "list")
                        HandleEthList(sim, terminal);
                    else
                        terminal.WriteLine("Usage: purenetwork eth list");
                    break;
                case "fc":
                    if (args.Length > 1 && args[1] == "list")
                        HandleFcList(sim, terminal);
                    else
                        terminal.WriteLine("Usage: purenetwork fc list");
                    break;
                default:
                    terminal.WriteLine($"purenetwork: unknown subcommand '{subcommand}'");
                    break;
            }
        }
        
        /// <summary>
        /// Handle 'purenetwork list' command - list all network interfaces.
        /// Source: Docs/PuttyLogs/putty2025-03-03.log showing purenetwork list with FC and ETH
        /// </summary>
        private void HandleList(Simulation.SimulationState sim, ISerialOutput terminal)
        {
            var hardware = sim.GetHardwareModel();
            
            // Header
            terminal.WriteLine("Name      Enabled  Speed       Services");
            
            var sb = new StringBuilder();
            
            // List FC ports first (uppercase names)
            foreach (var port in hardware.FCPorts.OrderBy(p => p.Name))
            {
                var enabled = port.Speed != "0.00 b/s" ? "True" : "False";
                sb.AppendFormat("{0,-9} {1,-8} {2,-11} {3}\n",
                    port.Name, enabled, port.Speed, "scsi-fc");
            }
            
            // Then list ethernet ports (lowercase names)
            foreach (var port in hardware.EthernetPorts.OrderBy(p => p.Name))
            {
                var name = port.Name.ToLower();
                sb.AppendFormat("{0,-9} {1,-8} {2,-11} {3}\n",
                    name, port.Enabled ? "True" : "False", port.Speed, port.Services);
            }
            
            terminal.WriteLine(sb.ToString().TrimEnd());
        }
        
        /// <summary>
        /// Handle 'purenetwork eth list' command - list Ethernet interfaces only.
        /// </summary>
        private void HandleEthList(Simulation.SimulationState sim, ISerialOutput terminal)
        {
            // Same as list for now since we only have ethernet ports in the model
            HandleList(sim, terminal);
        }
        
        /// <summary>
        /// Handle 'purenetwork fc list' command - list Fibre Channel interfaces.
        /// </summary>
        private void HandleFcList(Simulation.SimulationState sim, ISerialOutput terminal)
        {
            var hardware = sim.GetHardwareModel();
            
            if (hardware.FCPorts.Count == 0)
            {
                terminal.WriteLine("No Fibre Channel ports configured");
                return;
            }
            
            // Header
            terminal.WriteLine("Name     Status  Slot  WWN");
            
            var sb = new StringBuilder();
            
            // List all FC ports
            foreach (var port in hardware.FCPorts.OrderBy(p => p.Name))
            {
                var name = port.Name.ToLower().Replace(".", ".");  // ct1.fc0 format
                sb.AppendFormat("{0,-8} {1,-7} {2,-5} {3}\n",
                    name, port.Status, port.Slot, "20:00:00:25:b5:00:00:00");  // Placeholder WWN
            }
            
            terminal.WriteLine(sb.ToString().TrimEnd());
        }
    }
}
