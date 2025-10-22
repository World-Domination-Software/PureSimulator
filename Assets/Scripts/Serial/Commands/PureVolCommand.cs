using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// purevol command - Manage and display volumes.
    /// Lists volumes and their connections to hosts.
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs/putty2025-03-03.log L428
    /// Shows volume list with connection filtering
    /// </remarks>
    [SerialCommand("purevol")]
    public class PureVolCommand : ISerialCommand
    {
        public string Name => "purevol";
        public string Synopsis => "Manage and display volumes";
        public IReadOnlyList<string> Parameters => new[] { "list", "[--connect]" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            bool showConnections = false;
            
            // Parse arguments
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "list")
                {
                    continue;
                }
                else if (args[i] == "--connect")
                {
                    showConnections = true;
                }
            }
            
            HandleList(showConnections, terminal);
        }
        
        private void HandleList(bool showConnections, ISerialOutput terminal)
        {
            if (showConnections)
            {
                terminal.WriteLine("Connected Volumes:");
                terminal.WriteLine("");
                terminal.WriteLine($"{"Name",-25} {"Size",-12} {"Host",-25} {"LUN",-5}");
                terminal.WriteLine(new string('-', 70));
                
                // Sample connected volumes
                var volumes = new[]
                {
                    ("vol-001", "1.00T", "host-001-ll", 0),
                    ("vol-002", "2.00T", "host-002-ll", 0),
                    ("vol-003", "500.00G", "host-004-hl", 0),
                    ("vol-004", "1.50T", "host-005-ll", 0),
                    ("protocol-endpoint-001", "4.00G", "*", 255),
                    ("protocol-endpoint-002", "4.00G", "*", 255)
                };
                
                foreach (var (name, size, host, lun) in volumes)
                {
                    terminal.WriteLine($"{name,-25} {size,-12} {host,-25} {lun,-5}");
                }
            }
            else
            {
                terminal.WriteLine("Volumes:");
                terminal.WriteLine("");
                terminal.WriteLine($"{"Name",-25} {"Size",-12} {"Created",-25} {"Source",-15}");
                terminal.WriteLine(new string('-', 80));
                
                // Sample volumes
                var volumes = new[]
                {
                    ("vol-001", "1.00T", "2024-01-15 10:30:00", "-"),
                    ("vol-002", "2.00T", "2024-01-20 14:45:00", "-"),
                    ("vol-003", "500.00G", "2024-02-01 09:15:00", "-"),
                    ("vol-004", "1.50T", "2024-02-10 16:20:00", "-"),
                    ("protocol-endpoint-001", "4.00G", "2024-01-10 08:00:00", "system"),
                    ("protocol-endpoint-002", "4.00G", "2024-01-10 08:00:00", "system")
                };
                
                foreach (var (name, size, created, source) in volumes)
                {
                    terminal.WriteLine($"{name,-25} {size,-12} {created,-25} {source,-15}");
                }
            }
        }
    }
}
