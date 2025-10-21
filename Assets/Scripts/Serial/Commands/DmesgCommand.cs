using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// dmesg command - Print kernel ring buffer messages.
    /// </summary>
    [SerialCommand("dmesg")]
    public class DmesgCommand : ISerialCommand
    {
        public string Name => "dmesg";
        public string Synopsis => "Print kernel ring buffer messages";
        public IReadOnlyList<string> Parameters => new[] { "[-T]" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            bool showTimestamp = args.Length > 0 && args[0] == "-T";
            
            // Show sample kernel messages
            string[] messages = new[]
            {
                "[    0.000000] Linux version 5.4.0-pure (pure@build)",
                "[    0.000000] Command line: BOOT_IMAGE=/boot/vmlinuz root=UUID=abc123",
                "[    0.523142] PCI: Using configuration type 1 for base access",
                "[    1.234567] Intel(R) Ethernet Controller detected",
                "[    2.456789] scsi host0: Adaptec AIC94xx SAS/SATA driver",
                "[    3.678901] NVMe device detected: /dev/nvme0n1",
                "[    4.890123] Pure Storage array controller initialized"
            };
            
            if (sim.IsUsbInserted())
            {
                messages = new[]
                {
                    "[    0.000000] Linux version 5.4.0-pure (pure@build)",
                    "[    0.000000] Command line: BOOT_IMAGE=/boot/vmlinuz root=UUID=abc123",
                    "[    0.523142] PCI: Using configuration type 1 for base access",
                    "[    1.234567] Intel(R) Ethernet Controller detected",
                    "[    2.456789] scsi host0: Adaptec AIC94xx SAS/SATA driver",
                    "[    3.678901] NVMe device detected: /dev/nvme0n1",
                    "[    4.890123] Pure Storage array controller initialized",
                    "[  123.456789] usb 1-1: new high-speed USB device number 2 using xhci_hcd",
                    "[  123.567890] usb-storage 1-1:1.0: USB Mass Storage device detected",
                    "[  123.678901] scsi 1:0:0:0: Direct-Access     SanDisk  USB Flash Drive  1.00 PQ: 0 ANSI: 6",
                    "[  123.789012] sd 1:0:0:0: [sdb] 31457280 512-byte logical blocks: (16.1 GB/15.0 GiB)",
                    "[  123.890123] sd 1:0:0:0: [sdb] Write Protect is off",
                    "[  123.901234] sd 1:0:0:0: [sdb] Mode Sense: 43 00 00 00",
                    "[  124.012345]  sdb: sdb1"
                };
            }
            
            foreach (var msg in messages)
            {
                terminal.WriteLine(msg);
            }
        }
    }
}
