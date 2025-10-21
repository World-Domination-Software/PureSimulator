using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// Umount command - unmounts a filesystem.
    /// </summary>
    [SerialCommand("umount")]
    [SerialCommand("unmount")]
    public class UmountCommand : ISerialCommand
    {
        public string Name => "umount";
        public string Synopsis => "Unmount a filesystem";
        public IReadOnlyList<string> Parameters => new[] { "<mountpoint|device>" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            if (args.Length < 1)
            {
                terminal.WriteLine("Usage: umount <mountpoint|device>");
                return;
            }
            
            string target = args[0];
            
            // Check if USB is mounted
            if (!sim.IsUsbMounted())
            {
                terminal.WriteLine($"umount: {target}: not mounted");
                return;
            }
            
            // Check if target matches USB mount
            if (target == sim.GetUsbMountPath() || target == sim.GetUsbDeviceName() || 
                target == "/dev/sdb1" || target == "/dev/sdb")
            {
                sim.SetUsbMounted(false);
                // Successful unmount produces no output in Linux
            }
            else
            {
                terminal.WriteLine($"umount: {target}: not mounted");
            }
        }
    }
}
