using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// pureinstall command - Install Purity software package to alternate partition.
    /// Takes a .ppkg file and installs it to the non-active partition.
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs/putty2025-03-03.log L1201-1226
    /// Shows installation process with progress dots and warnings about firmware updates
    /// </remarks>
    [SerialCommand("pureinstall")]
    public class PureInstallCommand : ISerialCommand
    {
        public string Name => "pureinstall";
        public string Synopsis => "Install Purity software package to alternate partition";
        public IReadOnlyList<string> Parameters => new[] { "<package.ppkg>" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            if (args.Length == 0)
            {
                terminal.WriteLine("Usage: pureinstall <package.ppkg>");
                terminal.WriteLine("  Install Purity software package to alternate partition");
                return;
            }
            
            var packageFile = args[0];
            
            // Validate package file extension
            if (!packageFile.EndsWith(".ppkg"))
            {
                terminal.WriteLine($"Error: Invalid package file '{packageFile}'");
                terminal.WriteLine("Package files must have .ppkg extension");
                return;
            }
            
            // Simulate installation process
            // Source: Docs/PuttyLogs/putty2025-03-03.log L1201-1226
            terminal.WriteLine($"Installing Purity on alternate partition labeled second.");
            terminal.WriteLine("Erasing Purity software image from alternate partition second to prepare for installation.");
            terminal.WriteLine("WARNING: Do not interrupt this process!!");
            terminal.WriteLine("Unpacking new Purity software.");
            
            // Simulate progress dots (abbreviated)
            terminal.Write(".");
            for (int i = 0; i < 50; i++)
            {
                terminal.Write(".");
            }
            terminal.WriteLine("");
            
            terminal.WriteLine("Verifying package...");
            terminal.WriteLine("");
            terminal.WriteLine("Finalizing installation. This may take several minutes.");
            terminal.WriteLine("Executing /altroot/opt/purextras/bin/finish-install.sh");
            terminal.WriteLine("Executing /altroot/opt/purextras/bin/finish-install.azure-propagate.sh");
            terminal.WriteLine("Executing /altroot/opt/purextras/bin/finish-install.cbs.sh");
            terminal.WriteLine("Executing /altroot/opt/purextras/bin/finish-install.cdu-pureinstall-lib.sh");
            terminal.WriteLine("Executing /altroot/opt/purextras/bin/finish-install.fix-dpkg.sh");
            terminal.WriteLine("Purity installed.");
            terminal.WriteLine("Installation complete. The new Purity version will load at next reboot.");
            terminal.WriteLine("");
            terminal.WriteLine("Important!");
            terminal.WriteLine("The first boot of a new Purity version may take longer if the new version includes controller firmware updates.");
            terminal.WriteLine("DO NOT REBOOT THE CONTROLLER DURING THE FIRMWARE UPDATE.");
            terminal.WriteLine("");
            terminal.WriteLine("Refer to http://community.purestorage.com for more information about the Purity upgrade process and firmware updates.");
        }
    }
}
