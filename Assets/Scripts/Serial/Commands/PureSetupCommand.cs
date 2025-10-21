using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// puresetup command - Initial array setup and configuration.
    /// Common subcommands: show, timezone, newarray, secondaryarray
    /// </summary>
    /// <remarks>
    /// Source: commands.txt L7-21
    /// Source: Docs/getting_started_with_flasharray_purity_user_info__puresetup_2025-10-21-17-16-39.pdf
    /// </remarks>
    [SerialCommand("puresetup")]
    public class PureSetupCommand : ISerialCommand
    {
        public string Name => "puresetup";
        public string Synopsis => "Array setup and configuration";
        public IReadOnlyList<string> Parameters => new[] { "show", "timezone", "newarray", "secondaryarray" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            if (args.Length == 0)
            {
                terminal.WriteLine("Usage: puresetup <subcommand>");
                terminal.WriteLine("  show                    Show current configuration");
                terminal.WriteLine("  timezone                Set timezone");
                terminal.WriteLine("  newarray                Configure new array");
                terminal.WriteLine("  secondaryarray          Configure as secondary array");
                return;
            }
            
            var subcommand = args[0];
            
            switch (subcommand)
            {
                case "show":
                    HandleShow(sim, terminal);
                    break;
                case "timezone":
                    HandleTimezone(args, terminal);
                    break;
                case "newarray":
                    HandleNewArray(args, terminal);
                    break;
                case "secondaryarray":
                    HandleSecondaryArray(args, terminal);
                    break;
                default:
                    terminal.WriteLine($"puresetup: unknown subcommand '{subcommand}'");
                    break;
            }
        }
        
        private void HandleShow(Simulation.SimulationState sim, ISerialOutput terminal)
        {
            var hardware = sim.GetHardwareModel();
            
            terminal.WriteLine("Array Configuration:");
            terminal.WriteLine($"  Name: {hardware.Controllers[0].Model}");
            terminal.WriteLine($"  Version: {hardware.Controllers[0].Version}");
            terminal.WriteLine("  Timezone: America/New_York");
            terminal.WriteLine("  Controllers: 2");
            terminal.WriteLine($"  Drives: {hardware.Drives.Count}");
        }
        
        private void HandleTimezone(string[] args, ISerialOutput terminal)
        {
            if (args.Length < 2)
            {
                terminal.WriteLine("Current timezone: America/New_York");
                terminal.WriteLine("Usage: puresetup timezone <timezone>");
                return;
            }
            
            var timezone = args[1];
            terminal.WriteLine($"Timezone set to: {timezone}");
        }
        
        private void HandleNewArray(string[] args, ISerialOutput terminal)
        {
            bool skipTests = false;
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "--skip-connectivity-tests")
                    skipTests = true;
            }
            
            terminal.WriteLine("Configuring new array...");
            if (skipTests)
                terminal.WriteLine("Skipping connectivity tests");
            terminal.WriteLine("Array configuration complete");
        }
        
        private void HandleSecondaryArray(string[] args, ISerialOutput terminal)
        {
            bool skipTests = false;
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "--skip-connectivity-tests")
                    skipTests = true;
            }
            
            terminal.WriteLine("Configuring secondary array...");
            if (skipTests)
                terminal.WriteLine("Skipping connectivity tests");
            terminal.WriteLine("Secondary array configuration complete");
        }
    }
}
