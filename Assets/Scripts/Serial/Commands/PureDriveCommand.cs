using System.Collections.Generic;
using System.Text;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// puredrive command - Lists and manages drives (SSDs and NVRAM).
    /// Shows drive name, type, status, capacity.
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs/putty2025-03-03.log showing puredrive list output
    /// Source: Docs/puredrive.pdf
    /// </remarks>
    [SerialCommand("puredrive")]
    public class PureDriveCommand : ISerialCommand
    {
        public string Name => "puredrive";
        public string Synopsis => "List and manage drives";
        public IReadOnlyList<string> Parameters => new[] { "list" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            if (args.Length == 0 || args[0] != "list")
            {
                terminal.WriteLine("Usage: puredrive list");
                return;
            }
            
            var hardware = sim.GetHardwareModel();
            
            // Output header
            // Source: Docs/PuttyLogs/putty2025-03-03.log showing puredrive list
            terminal.WriteLine("Name       Type   Status   Capacity  Details");
            
            var sb = new StringBuilder();
            
            // List all drives (SSDs and NVRAM)
            foreach (var drive in hardware.Drives)
            {
                // Only show installed drives (skip not_installed)
                if (drive.Status != "not_installed")
                {
                    sb.AppendFormat("{0,-10} {1,-6} {2,-8} {3,-9} {4}\n",
                        drive.Name, drive.Type, drive.Status, drive.Capacity, drive.Details);
                }
            }
            
            terminal.WriteLine(sb.ToString().TrimEnd());
        }
    }
}
