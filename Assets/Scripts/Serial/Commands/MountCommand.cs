using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// Mount command - mounts a device to a mount point.
    /// Implements success case and multiple error paths mirroring real Purity behavior.
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs/putty2025-02-18.log L650-653 (success with warning)
    /// Source: Docs/PuttyLogs/putty2025-02-18.log L2229 (clean mount)
    /// Source: Docs/PuttyLogs/putty2025-02-18.log L4046 (clean mount)
    /// </remarks>
    [SerialCommand("mount")]
    public class MountCommand : ISerialCommand
    {
        public string Name => "mount";
        public string Synopsis => "Mount a filesystem";
        public IReadOnlyList<string> Parameters => new[] { "<device>", "<mountpoint>" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            if (args.Length < 2)
            {
                terminal.WriteLine("Usage: mount <device> <mountpoint>");
                return;
            }
            
            string device = args[0];
            string mountpoint = args[1];
            
            // Error case 1: USB not inserted
            if (!sim.IsUsbInserted() && device.Contains("sdb"))
            {
                // Source: Custom error (inferred from system behavior)
                terminal.WriteLine($"mount: special device {device} does not exist");
                return;
            }
            
            // Error case 2: Already mounted
            if (sim.IsUsbMounted() && device == sim.GetUsbDeviceName())
            {
                // Source: Standard Linux mount error
                terminal.WriteLine($"mount: {device} is already mounted or {mountpoint} busy");
                terminal.WriteLine($"       {device} is already mounted on {sim.GetUsbMountPath()}");
                return;
            }
            
            // Error case 3: Not a block device (wrong device path)
            if (!device.StartsWith("/dev/sd"))
            {
                // Source: Standard Linux mount error
                terminal.WriteLine($"mount: {device} is not a block device");
                return;
            }
            
            // Error case 4: Device doesn't exist (wrong device)
            if (device != "/dev/sda1" && device != sim.GetUsbDeviceName())
            {
                terminal.WriteLine($"mount: special device {device} does not exist");
                return;
            }
            
            // Validate device exists
            if (device == "/dev/sdb1" || device == "/dev/sdb")
            {
                // Ensure USB is actually inserted
                if (!sim.IsUsbInserted())
                {
                    terminal.WriteLine($"mount: special device {device} does not exist");
                    return;
                }
            }
            
            // Success case
            // Source: Docs/PuttyLogs/putty2025-02-18.log L650-653
            // Sometimes shows a warning about unclean filesystem
            bool showWarning = (System.Environment.TickCount % 3 == 0); // Random-ish
            
            if (showWarning)
            {
                terminal.WriteLine("The disk contains an unclean file system (0, 0).");
                terminal.WriteLine("The file system wasn't safely closed on Windows. Fixing.");
            }
            
            // Mark as mounted in simulation state
            if (device == sim.GetUsbDeviceName() || device == "/dev/sdb1" || device == "/dev/sdb")
            {
                sim.SetUsbMounted(true);
            }
            
            // Note: Successful mount in Linux typically produces no output
            // Source: Docs/PuttyLogs/putty2025-02-18.log L2229 (no output after mount)
        }
    }
}
