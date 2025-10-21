using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// df command - Report file system disk space usage.
    /// </summary>
    [SerialCommand("df")]
    public class DfCommand : ISerialCommand
    {
        public string Name => "df";
        public string Synopsis => "Report file system disk space usage";
        public IReadOnlyList<string> Parameters => new[] { "[-h]" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            bool humanReadable = args.Length > 0 && args[0] == "-h";
            
            if (humanReadable)
            {
                terminal.WriteLine("Filesystem      Size  Used Avail Use% Mounted on");
                terminal.WriteLine("/dev/sda1       450G   45G  405G  10% /");
                terminal.WriteLine("tmpfs            64G  1.2G   63G   2% /tmp");
                terminal.WriteLine("/dev/nvme0n1    2.0T  200G  1.8T  10% /var/log");
            }
            else
            {
                terminal.WriteLine("Filesystem     1K-blocks      Used Available Use% Mounted on");
                terminal.WriteLine("/dev/sda1      471859200  47185920 424673280  10% /");
                terminal.WriteLine("tmpfs           67108864   1258291  65850573   2% /tmp");
                terminal.WriteLine("/dev/nvme0n1  2147483648 209715200 1937768448  10% /var/log");
            }
            
            if (sim.IsUsbMounted())
            {
                terminal.WriteLine($"{sim.GetUsbDeviceName()}       16G  2.1G   14G  14% {sim.GetUsbMountPath()}");
            }
        }
    }
}
