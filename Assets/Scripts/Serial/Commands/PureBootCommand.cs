using System.Collections.Generic;
using System.Text;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// pureboot command - Manage boot partitions and reboot operations.
    /// Lists installed Purity versions on first/second partitions and manages reboot behavior.
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs/putty2025-03-03.log L299-304
    /// Shows output with current (*) and next boot (-->) markers
    /// </remarks>
    [SerialCommand("pureboot")]
    public class PureBootCommand : ISerialCommand
    {
        public string Name => "pureboot";
        public string Synopsis => "Manage boot partitions and reboot operations";
        public IReadOnlyList<string> Parameters => new[] { "list", "reboot", "[--primary]", "[--secondary]", "[--offline]" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            if (args.Length == 0)
            {
                terminal.WriteLine("Usage: pureboot <subcommand> [options]");
                terminal.WriteLine("  list                     List boot partitions");
                terminal.WriteLine("  reboot --primary         Reboot to primary partition");
                terminal.WriteLine("  reboot --secondary       Reboot to secondary partition");
                terminal.WriteLine("  reboot --offline         Reboot offline");
                return;
            }
            
            var subcommand = args[0];
            
            switch (subcommand)
            {
                case "list":
                    HandleList(sim, terminal);
                    break;
                case "reboot":
                    HandleReboot(args, terminal);
                    break;
                default:
                    terminal.WriteLine($"pureboot: unknown subcommand '{subcommand}'");
                    break;
            }
        }
        
        private void HandleList(Simulation.SimulationState sim, ISerialOutput terminal)
        {
            // Source: Docs/PuttyLogs/putty2025-03-03.log L299-304
            // Shows current running and next boot partition markers
            terminal.WriteLine("");
            terminal.WriteLine("Marked entry (*) is currently running");
            terminal.WriteLine("Marked entry (-->) will run at next reboot");
            
            var hw = sim.GetHardwareModel();
            var controllers = hw.Controllers;
            
            // Get primary controller for version info
            var primaryCtrl = controllers.Count > 0 ? controllers[0] : null;
            
            if (primaryCtrl != null)
            {
                // Show first partition (primary)
                terminal.WriteLine($"*-->0. Purity {primaryCtrl.Version} (202408090136+b967c2f84655) with kernel 5.15.123+ (202407140227+e4052619b975) on first (/dev/sda3)");
                
                // Show second partition (alternate version)
                terminal.WriteLine($"    1. Purity 6.3.12 (202306131136+17d7e05401a0) with kernel 5.4.114+ (202305110433+5a25b35047dc) on second (/dev/sda4)");
            }
            else
            {
                terminal.WriteLine("*-->0. Purity 6.5.8 (202408090136+b967c2f84655) with kernel 5.15.123+ (202407140227+e4052619b975) on first (/dev/sda3)");
                terminal.WriteLine("    1. Purity 6.3.12 (202306131136+17d7e05401a0) with kernel 5.4.114+ (202305110433+5a25b35047dc) on second (/dev/sda4)");
            }
        }
        
        private void HandleReboot(string[] args, ISerialOutput terminal)
        {
            if (args.Length < 2)
            {
                terminal.WriteLine("Usage: pureboot reboot [--primary|--secondary|--offline]");
                return;
            }
            
            var option = args[1];
            
            switch (option)
            {
                case "--primary":
                    terminal.WriteLine("Scheduling reboot to primary partition...");
                    terminal.WriteLine("Controller will reboot to primary partition.");
                    break;
                case "--secondary":
                    terminal.WriteLine("Scheduling reboot to secondary partition...");
                    terminal.WriteLine("Controller will reboot to secondary partition.");
                    break;
                case "--offline":
                    terminal.WriteLine("Scheduling offline reboot...");
                    terminal.WriteLine("Controller will reboot offline (no takeover).");
                    break;
                default:
                    terminal.WriteLine($"pureboot reboot: unknown option '{option}'");
                    terminal.WriteLine("Valid options: --primary, --secondary, --offline");
                    break;
            }
        }
    }
}
