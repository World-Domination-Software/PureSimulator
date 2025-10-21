using System.Collections.Generic;
using System.Text;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// pureport command - Manage and display storage port connections.
    /// Shows WWN, IQN, and target connections for host initiators.
    /// </summary>
    /// <remarks>
    /// Referenced in Docs/PuttyLogs/ny2pure04.log in purewes checks
    /// Shows port list with target and initiator information
    /// </remarks>
    [SerialCommand("pureport")]
    public class PurePortCommand : ISerialCommand
    {
        public string Name => "pureport";
        public string Synopsis => "Manage and display storage port connections";
        public IReadOnlyList<string> Parameters => new[] { "list" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            if (args.Length == 0 || args[0] == "list")
            {
                HandleList(sim, terminal);
            }
            else
            {
                terminal.WriteLine($"pureport: unknown subcommand '{args[0]}'");
                terminal.WriteLine("Usage: pureport list");
            }
        }
        
        private void HandleList(Simulation.SimulationState sim, ISerialOutput terminal)
        {
            var hw = sim.GetHardwareModel();
            
            terminal.WriteLine("Port Connections:");
            terminal.WriteLine("");
            terminal.WriteLine($"{"Name",-15} {"Initiator WWN",-25} {"Target Name",-20} {"Status",-10}");
            terminal.WriteLine(new string('-', 75));
            
            // Sample port connections based on FC ports
            var fcPorts = hw.FCPorts;
            int connectedCount = 0;
            
            foreach (var port in fcPorts)
            {
                if (port.Speed != "0.00 b/s" && connectedCount < 8)
                {
                    string wwn = GenerateWWN(connectedCount);
                    string target = $"host-{connectedCount + 1:D3}";
                    terminal.WriteLine($"{port.Name,-15} {wwn,-25} {target,-20} {"connected",-10}");
                    connectedCount++;
                }
            }
            
            if (connectedCount == 0)
            {
                terminal.WriteLine("No port connections found");
            }
        }
        
        private string GenerateWWN(int index)
        {
            // Generate realistic looking WWN addresses
            return $"20:00:00:25:B5:{(0xA0 + index):X2}:{(0x22 + (index % 4)):X2}:{(0x08 + (index / 4)):X2}";
        }
    }
}
