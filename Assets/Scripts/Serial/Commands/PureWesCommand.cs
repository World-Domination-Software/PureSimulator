using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// purewes command - Wide-area Ethernet Services for controller management.
    /// Primary use is swapping controller modes between primary and secondary.
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs/ny2pure04.log - Multiple instances of controller setattr
    /// Source: commands.txt L7 - purewes controller setattr --verify-array pure00 ct1 --mode secondary
    /// </remarks>
    [SerialCommand("purewes")]
    public class PureWesCommand : ISerialCommand
    {
        public string Name => "purewes";
        public string Synopsis => "Wide-area Ethernet Services for controller management";
        public IReadOnlyList<string> Parameters => new[] { "controller", "setattr", "--verify-array", "--mode" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            if (args.Length == 0)
            {
                terminal.WriteLine("Usage: purewes controller setattr --verify-array <array-name> <controller> --mode <primary|secondary>");
                return;
            }
            
            if (args.Length >= 2 && args[0] == "controller" && args[1] == "setattr")
            {
                HandleControllerSetAttr(sim, args, terminal);
            }
            else
            {
                terminal.WriteLine($"purewes: unknown subcommand '{args[0]}'");
            }
        }
        
        private void HandleControllerSetAttr(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            // Parse arguments
            string arrayName = null;
            string controller = null;
            string mode = null;
            
            for (int i = 2; i < args.Length; i++)
            {
                if (args[i] == "--verify-array" && i + 2 < args.Length)
                {
                    arrayName = args[i + 1];
                    controller = args[i + 2];
                    i += 2;
                }
                else if (args[i] == "--mode" && i + 1 < args.Length)
                {
                    mode = args[i + 1];
                    i++;
                }
            }
            
            if (string.IsNullOrEmpty(arrayName) || string.IsNullOrEmpty(controller) || string.IsNullOrEmpty(mode))
            {
                terminal.WriteLine("Usage: purewes controller setattr --verify-array <array-name> <controller> --mode <primary|secondary>");
                return;
            }
            
            // Source: Docs/PuttyLogs/ny2pure04.log showing pre-check validation
            terminal.WriteLine("Run checks before making changes ...");
            terminal.WriteLine("Check peer purity status...... SUCCESS");
            terminal.WriteLine("Check pureport list........... SUCCESS");
            terminal.WriteLine("Check iobalance............... Sampling I/O for 60 seconds");
            terminal.WriteLine("SUCCESS");
            terminal.WriteLine("");
            terminal.WriteLine($"Setting {controller} mode to {mode}...");
            terminal.WriteLine($"Controller {controller} on array {arrayName} is now {mode}");
            terminal.WriteLine("");
            terminal.WriteLine("Note: The controller mode change may require a reboot to take full effect.");
            
            // Update simulation state if controller exists
            var hw = sim.GetHardwareModel();
            var ctrl = hw.GetController(controller.ToUpper());
            if (ctrl != null)
            {
                ctrl.Mode = mode;
            }
        }
    }
}
