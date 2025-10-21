using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// Lsblk command - lists block devices (simulated via ls /dev/sd*).
    /// Mirrors device tree output from real Purity systems.
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs/putty2025-02-18.log L648-650
    /// Source: Docs/PuttyLogs/putty2025-03-03.log (ls /dev/sdb*1*1 pattern)
    /// </remarks>
    [SerialCommand("ls")]
    [SerialCommand("lsblk")]
    public class LsblkCommand : ISerialCommand
    {
        public string Name => "ls";
        public string Synopsis => "List directory contents or block devices";
        public IReadOnlyList<string> Parameters => new[] { "[path]" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            // Handle ls /dev/sd* pattern
            if (args.Length > 0 && args[0].StartsWith("/dev/sd"))
            {
                ListBlockDevices(sim, args[0], terminal);
            }
            else if (args.Length == 0 || args[0] == "/dev")
            {
                // Generic device listing
                terminal.WriteLine("ls: command requires path argument");
            }
            else
            {
                // Other paths not implemented in this stub
                terminal.WriteLine($"ls: cannot access '{args[0]}': No such file or directory");
            }
        }
        
        private void ListBlockDevices(Simulation.SimulationState sim, string pattern, ISerialOutput terminal)
        {
            // Source: Docs/PuttyLogs/putty2025-02-18.log L648-650
            // Expected output when USB is inserted:
            // /dev/sda1  /dev/sdb1
            
            if (!sim.IsUsbInserted())
            {
                // Error case: USB not inserted
                // When no USB is present, only sda1 exists
                if (pattern.Contains("sdb"))
                {
                    terminal.WriteLine($"ls: cannot access '{pattern}': No such file or directory");
                }
                else
                {
                    terminal.WriteLine("/dev/sda1");
                }
                return;
            }
            
            // Happy path: USB is inserted
            if (pattern == "/dev/sd*" || pattern == "/dev/sd*1" || pattern.Contains("*"))
            {
                // Source: Docs/PuttyLogs/putty2025-02-18.log L648
                terminal.WriteLine("/dev/sda1  /dev/sdb1");
            }
            else if (pattern == "/dev/sdb1" || pattern == "/dev/sdb")
            {
                // Specific USB device check
                terminal.WriteLine(sim.GetUsbDeviceName());
            }
            else if (pattern == "/dev/sda1" || pattern == "/dev/sda")
            {
                // Internal device
                terminal.WriteLine("/dev/sda1");
            }
            else
            {
                // Wrong device path error
                terminal.WriteLine($"ls: cannot access '{pattern}': No such file or directory");
            }
        }
    }
}
