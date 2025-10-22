using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// pureversion command - Display installed Purity software versions.
    /// </summary>
    /// <remarks>
    /// Source: commands.txt - Listed as tested command
    /// Similar to pureboot list but focuses on version information
    /// </remarks>
    [SerialCommand("pureversion")]
    public class PureVersionCommand : ISerialCommand
    {
        public string Name => "pureversion";
        public string Synopsis => "Display installed Purity software versions";
        public IReadOnlyList<string> Parameters => new[] { "list" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            if (args.Length == 0 || args[0] == "list")
            {
                HandleList(sim, terminal);
            }
            else
            {
                terminal.WriteLine($"pureversion: unknown subcommand '{args[0]}'");
                terminal.WriteLine("Usage: pureversion list");
            }
        }
        
        private void HandleList(Simulation.SimulationState sim, ISerialOutput terminal)
        {
            var hw = sim.GetHardwareModel();
            var controllers = hw.Controllers;
            
            terminal.WriteLine("Installed Purity Versions:");
            terminal.WriteLine("");
            
            // Get current version from controller
            var primaryCtrl = controllers.Count > 0 ? controllers[0] : null;
            
            if (primaryCtrl != null)
            {
                terminal.WriteLine($"  Current: Purity {primaryCtrl.Version} (202408090136+b967c2f84655)");
                terminal.WriteLine($"  Kernel:  5.15.123+ (202407140227+e4052619b975)");
                terminal.WriteLine($"  Partition: first (/dev/sda3)");
            }
            else
            {
                terminal.WriteLine("  Current: Purity 6.5.8 (202408090136+b967c2f84655)");
                terminal.WriteLine("  Kernel:  5.15.123+ (202407140227+e4052619b975)");
                terminal.WriteLine("  Partition: first (/dev/sda3)");
            }
            
            terminal.WriteLine("");
            terminal.WriteLine("  Alternate: Purity 6.3.12 (202306131136+17d7e05401a0)");
            terminal.WriteLine("  Kernel:    5.4.114+ (202305110433+5a25b35047dc)");
            terminal.WriteLine("  Partition: second (/dev/sda4)");
        }
    }
}
