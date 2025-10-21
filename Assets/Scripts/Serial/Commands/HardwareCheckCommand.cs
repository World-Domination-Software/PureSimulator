using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// hardware_check.py - System hardware verification script.
    /// Shows CPU, RAM, FC targets, iSCSI ports, storage summary.
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs/putty2025-03-03.log L194-218
    /// </remarks>
    [SerialCommand("hardware_check.py")]
    public class HardwareCheckCommand : ISerialCommand
    {
        public string Name => "hardware_check.py";
        public string Synopsis => "System hardware verification script";
        public IReadOnlyList<string> Parameters => new string[0];
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            var hardware = sim.GetHardwareModel();
            
            // Output format from actual log
            // Source: Docs/PuttyLogs/putty2025-03-03.log L195-218
            terminal.WriteLine("");
            terminal.WriteLine("==== CPU ====");
            terminal.WriteLine("model name: Intel(R) Xeon(R) Gold 6230 CPU @ 2.10GHz        x 80");
            terminal.WriteLine("");
            terminal.WriteLine("==== RAM ====");
            terminal.WriteLine("MemTotal:       394562164 kB");
            terminal.WriteLine("");
            terminal.WriteLine("==== FC TARGETS ====");
            
            int fcCount = hardware.FCPorts.Count;
            terminal.WriteLine($"Detected {fcCount} targets.");
            terminal.WriteLine($"Found {fcCount} FC adapter ports.");
            terminal.WriteLine("");
            terminal.WriteLine("==== iSCSI TARGETS ====");
            
            // Count iSCSI-capable ethernet ports
            int iscsiCount = 0;
            foreach (var port in hardware.EthernetPorts)
            {
                if (port.Services.Contains("iscsi"))
                    iscsiCount++;
            }
            terminal.WriteLine($"Detected {iscsiCount} iSCSI capable ports");
            terminal.WriteLine("");
            terminal.WriteLine("==== NON-TRANSPARENT BRIDGE ====");
            terminal.WriteLine("Found NTB:");
            terminal.WriteLine("17:00.0 Bridge [0680]: Intel Corporation Sky Lake-E Non-Transparent Bridge Registers [8086:201c] (rev 07)");
            terminal.WriteLine("d7:00.0 Bridge [0680]: Intel Corporation Sky Lake-E Non-Transparent Bridge Registers [8086:201c] (rev 07)");
            terminal.WriteLine("Bar 2 Size: 256G");
            terminal.WriteLine("");
            terminal.WriteLine("==== INFINIBAND ADAPTERS ====");
            terminal.WriteLine("Skipping Platinum family and Oxygen family check for Infiniband adapters.");
            terminal.WriteLine("But we did find InfiniBand adapters: mlx5_3 mlx5_1 mlx5_2 mlx5_0");
            terminal.WriteLine("");
            terminal.WriteLine("==== STORAGE ====");
            
            // Count installed drives
            int driveCount = 0;
            foreach (var drive in hardware.Drives)
            {
                if (drive.Status != "not_installed")
                    driveCount++;
            }
            
            terminal.WriteLine($"summary: enclosures: 1, drives: {driveCount}, drive models: 2");
            terminal.WriteLine("enclosure: 1A112-CT0, drives {'Micron_5300_MTFDDAV240TDU': 1}, revs {'D3MU001': 1}, paths 1");
        }
    }
}
