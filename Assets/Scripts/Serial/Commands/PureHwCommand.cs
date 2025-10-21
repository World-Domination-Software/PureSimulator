using System.Collections.Generic;
using System.Text;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// purehw command - Lists and manages hardware components.
    /// Shows controllers, drives, fans, power supplies, ports, temperature sensors.
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs/putty2025-02-22-2.txt
    /// Source: Docs/PuttyLogs/putty2025-03-03.log L46-192
    /// Source: Docs/purehw.pdf
    /// </remarks>
    [SerialCommand("purehw")]
    public class PureHwCommand : ISerialCommand
    {
        public string Name => "purehw";
        public string Synopsis => "List and manage hardware components";
        public IReadOnlyList<string> Parameters => new[] { "list", "[--all]", "[--type <type>]" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            // Default action is list
            if (args.Length == 0 || args[0] != "list")
            {
                terminal.WriteLine("Usage: purehw list [--all] [--type <type>]");
                return;
            }
            
            var hardware = sim.GetHardwareModel();
            
            // Check for type filter
            string typeFilter = null;
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "--type" && i + 1 < args.Length)
                {
                    typeFilter = args[i + 1].ToLower();
                    break;
                }
            }
            
            // Output header
            // Source: Docs/PuttyLogs/putty2025-03-03.log L46
            terminal.WriteLine("Name       Status         Identify  Slot  Index  Speed       Temperature  Voltage  Details");
            
            var sb = new StringBuilder();
            
            // List all hardware in order: Chassis, Bays, NVRAMs, Power, Temp, Controllers, Ports, Fans, Temps
            
            // Chassis
            if (typeFilter == null || typeFilter == "chassis")
            {
                foreach (var chassis in hardware.Chassis)
                {
                    AppendHardwareLine(sb, chassis.Name, chassis.Status, chassis.Identify, "-", chassis.Index.ToString());
                }
            }
            
            // Drives (Bays and NVRAM)
            if (typeFilter == null || typeFilter == "bay" || typeFilter == "drive")
            {
                foreach (var drive in hardware.Drives)
                {
                    AppendHardwareLine(sb, drive.Name, drive.Status, drive.Identify, "-", drive.Index.ToString());
                }
            }
            
            // Power supplies
            if (typeFilter == null || typeFilter == "pwr" || typeFilter == "power")
            {
                foreach (var psu in hardware.PowerSupplies)
                {
                    AppendHardwareLine(sb, psu.Name, psu.Status, "-", "-", psu.Index.ToString(), 
                                     voltage: psu.Voltage);
                }
            }
            
            // Chassis temperature sensors (before controllers)
            if (typeFilter == null || typeFilter == "tmp" || typeFilter == "temp")
            {
                foreach (var sensor in hardware.TemperatureSensors)
                {
                    if (sensor.Name.StartsWith("CH"))
                    {
                        AppendHardwareLine(sb, sensor.Name, sensor.Status, "-", "-", sensor.Index.ToString(),
                                         temperature: sensor.Temperature);
                    }
                }
            }
            
            // Controllers
            if (typeFilter == null || typeFilter == "controller")
            {
                foreach (var controller in hardware.Controllers)
                {
                    AppendHardwareLine(sb, controller.Name, controller.Status, controller.Identify, "-", 
                                     controller.Name.Contains("CT0") ? "0" : "1");
                }
            }
            
            // Ethernet ports
            if (typeFilter == null || typeFilter == "eth" || typeFilter == "port")
            {
                foreach (var port in hardware.EthernetPorts)
                {
                    AppendHardwareLine(sb, port.Name, port.Status, "-", "-", port.Index.ToString(),
                                     speed: port.Speed);
                }
            }
            
            // FC ports
            if (typeFilter == null || typeFilter == "fc" || typeFilter == "port")
            {
                foreach (var port in hardware.FCPorts)
                {
                    AppendHardwareLine(sb, port.Name, port.Status, "-", port.Slot.ToString(), 
                                     port.Index.ToString(), speed: port.Speed);
                }
            }
            
            // Fans
            if (typeFilter == null || typeFilter == "fan")
            {
                foreach (var fan in hardware.Fans)
                {
                    AppendHardwareLine(sb, fan.Name, fan.Status, "-", "-", fan.Index.ToString());
                }
            }
            
            // Controller temperature sensors
            if (typeFilter == null || typeFilter == "tmp" || typeFilter == "temp")
            {
                foreach (var sensor in hardware.TemperatureSensors)
                {
                    if (sensor.Name.StartsWith("CT"))
                    {
                        AppendHardwareLine(sb, sensor.Name, sensor.Status, "-", "-", sensor.Index.ToString(),
                                         temperature: sensor.Temperature);
                    }
                }
            }
            
            terminal.WriteLine(sb.ToString().TrimEnd());
        }
        
        /// <summary>
        /// Format a hardware line matching the purehw list output format.
        /// Source: Docs/PuttyLogs/putty2025-03-03.log L47-192
        /// </summary>
        private void AppendHardwareLine(StringBuilder sb, string name, string status, string identify, 
                                       string slot, string index, string speed = "-", 
                                       string temperature = "-", string voltage = "-", string details = "-")
        {
            // Column widths: Name(10) Status(14) Identify(9) Slot(5) Index(6) Speed(11) Temp(12) Voltage(8) Details
            sb.AppendFormat("{0,-10} {1,-14} {2,-9} {3,-5} {4,-6} {5,-11} {6,-12} {7,-8} {8}\n",
                name, status, identify, slot, index, speed, temperature, voltage, details);
        }
    }
}
