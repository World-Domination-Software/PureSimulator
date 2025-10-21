using System.Collections.Generic;

namespace PureSim.Console.Commands
{
    /// <summary>
    /// UsbState command - toggles USB media presence in the simulation.
    /// This controls whether USB is inserted/removed for testing workflows.
    /// </summary>
    [ConsoleCommand("usb")]
    public class UsbStateCommand : IConsoleCommand
    {
        public string Name => "usb";
        public string Synopsis => "Control USB media state (inserted/removed)";
        public IReadOnlyList<string> Parameters => new[] { "state", "<inserted|removed>" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, IConsoleOutput output)
        {
            if (args.Length < 2)
            {
                output.WriteError("Usage: usb state <inserted|removed>");
                output.WriteLine("");
                output.WriteLine($"Current state: USB {(sim.IsUsbInserted() ? "inserted" : "removed")}");
                output.WriteLine($"Device: {sim.GetUsbDeviceName()}");
                output.WriteLine($"Mounted: {(sim.IsUsbMounted() ? "yes at " + sim.GetUsbMountPath() : "no")}");
                return;
            }
            
            if (args[0].ToLowerInvariant() != "state")
            {
                output.WriteError("Usage: usb state <inserted|removed>");
                return;
            }
            
            string newState = args[1].ToLowerInvariant();
            
            if (newState == "inserted" || newState == "insert")
            {
                if (sim.IsUsbInserted())
                {
                    output.WriteWarning("USB is already inserted");
                }
                else
                {
                    sim.SetUsbInserted(true);
                    output.WriteSuccess("USB inserted");
                    output.WriteLine($"Device: {sim.GetUsbDeviceName()}");
                }
            }
            else if (newState == "removed" || newState == "remove")
            {
                if (!sim.IsUsbInserted())
                {
                    output.WriteWarning("USB is already removed");
                }
                else
                {
                    sim.SetUsbInserted(false);
                    output.WriteSuccess("USB removed");
                    if (sim.IsUsbMounted())
                    {
                        output.WriteLine("(USB was auto-unmounted)");
                    }
                }
            }
            else
            {
                output.WriteError("Invalid state. Use 'inserted' or 'removed'");
            }
        }
    }
}
