using System.Collections.Generic;

namespace PureSim.Console.Commands
{
    /// <summary>
    /// Inject command - injects a fault into the simulation for testing error paths.
    /// </summary>
    [ConsoleCommand("inject")]
    public class InjectCommand : IConsoleCommand
    {
        public string Name => "inject";
        public string Synopsis => "Inject a fault into the simulation";
        public IReadOnlyList<string> Parameters => new[] { "<fault_id>", "[description]" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, IConsoleOutput output)
        {
            if (args.Length == 0)
            {
                output.WriteError("Usage: inject <fault_id> [description]");
                output.WriteLine("");
                output.WriteLine("Available fault IDs:");
                output.WriteLine("  usb-not-inserted    - Simulate USB device not inserted");
                output.WriteLine("  usb-wrong-device    - Simulate wrong USB device");
                output.WriteLine("  cable-missing       - Simulate missing cable connection");
                output.WriteLine("  cable-short         - Simulate short/damaged cable");
                output.WriteLine("  controller-mismatch - Simulate controller version mismatch");
                output.WriteLine("  psu-removed         - Simulate PSU removal");
                output.WriteLine("  shelf-power-off     - Simulate shelf powered off");
                output.WriteLine("  wrong-firmware      - Simulate incorrect firmware image");
                return;
            }
            
            string faultId = args[0];
            string description = args.Length > 1 ? string.Join(" ", args, 1, args.Length - 1) : "";
            
            if (string.IsNullOrEmpty(description))
            {
                description = GetDefaultDescription(faultId);
            }
            
            if (sim.HasFault(faultId))
            {
                output.WriteWarning($"Fault '{faultId}' is already active");
                return;
            }
            
            sim.InjectFault(faultId, description);
            output.WriteSuccess($"Injected fault: {faultId}");
            
            if (!string.IsNullOrEmpty(description))
            {
                output.WriteLine($"Description: {description}");
            }
        }
        
        private string GetDefaultDescription(string faultId)
        {
            return faultId switch
            {
                "usb-not-inserted" => "USB device not inserted",
                "usb-wrong-device" => "Wrong USB device inserted",
                "cable-missing" => "Cable connection missing",
                "cable-short" => "Cable is short or damaged",
                "controller-mismatch" => "Controller version mismatch",
                "psu-removed" => "Power supply unit removed",
                "shelf-power-off" => "Shelf is powered off",
                "wrong-firmware" => "Incorrect firmware image",
                _ => "Custom fault"
            };
        }
    }
}
