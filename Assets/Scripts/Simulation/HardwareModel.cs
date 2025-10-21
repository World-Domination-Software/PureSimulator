using System;
using System.Collections.Generic;
using UnityEngine;

namespace PureSim.Simulation
{
    /// <summary>
    /// Hardware model representing Pure Storage FlashArray components.
    /// This models the physical and logical hardware that commands like purehw, puredrive,
    /// purenetwork query and manipulate.
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs/*.log showing purehw list, puredrive list outputs
    /// Source: Docs/purehw.pdf, Docs/puredrive.pdf
    /// </remarks>
    [Serializable]
    public class HardwareModel
    {
        [SerializeField] private List<Controller> controllers = new List<Controller>();
        [SerializeField] private List<Chassis> chassis = new List<Chassis>();
        [SerializeField] private List<Drive> drives = new List<Drive>();
        [SerializeField] private List<EthernetPort> ethernetPorts = new List<EthernetPort>();
        [SerializeField] private List<FCPort> fcPorts = new List<FCPort>();
        [SerializeField] private List<Fan> fans = new List<Fan>();
        [SerializeField] private List<PowerSupply> powerSupplies = new List<PowerSupply>();
        [SerializeField] private List<TemperatureSensor> temperatureSensors = new List<TemperatureSensor>();
        
        public IReadOnlyList<Controller> Controllers => controllers;
        public IReadOnlyList<Chassis> Chassis => chassis;
        public IReadOnlyList<Drive> Drives => drives;
        public IReadOnlyList<EthernetPort> EthernetPorts => ethernetPorts;
        public IReadOnlyList<FCPort> FCPorts => fcPorts;
        public IReadOnlyList<Fan> Fans => fans;
        public IReadOnlyList<PowerSupply> PowerSupplies => powerSupplies;
        public IReadOnlyList<TemperatureSensor> TemperatureSensors => temperatureSensors;
        
        public HardwareModel()
        {
            InitializeDefaultHardware();
        }
        
        /// <summary>
        /// Initialize with default hardware configuration for a FlashArray.
        /// Based on logs showing typical X70R3 configuration.
        /// </summary>
        /// <remarks>
        /// Source: Docs/PuttyLogs/putty2025-03-03.log showing FA-X70R3 configuration
        /// </remarks>
        private void InitializeDefaultHardware()
        {
            // Create chassis with bays
            var chassis0 = new Chassis { Name = "CH0", Status = "ok", Identify = "off", Index = 0 };
            chassis.Add(chassis0);
            
            // Create controllers
            controllers.Add(new Controller 
            { 
                Name = "CT0", 
                Type = "array_controller",
                Mode = "secondary", 
                Model = "FA-X70R3", 
                Version = "6.5.8",
                Status = "ready",
                Identify = "off"
            });
            
            controllers.Add(new Controller 
            { 
                Name = "CT1", 
                Type = "array_controller",
                Mode = "primary", 
                Model = "FA-X70R3", 
                Version = "6.5.8",
                Status = "ready",
                Identify = "off"
            });
            
            // Create drives in chassis bays (20 SSDs + 4 NVRAM)
            // Source: Docs/PuttyLogs/putty2025-03-03.log L35-59
            for (int i = 0; i < 20; i++)
            {
                drives.Add(new Drive
                {
                    Name = $"CH0.BAY{i}",
                    Type = "SSD",
                    Status = i < 12 ? "healthy" : "not_installed",
                    Capacity = "7.93T",
                    Identify = "off",
                    Index = i,
                    Details = "-"
                });
            }
            
            // Add NVRAM drives
            for (int i = 0; i < 4; i++)
            {
                drives.Add(new Drive
                {
                    Name = $"CH0.NVB{i}",
                    Type = "NVRAM",
                    Status = i < 2 ? "healthy" : "not_installed",
                    Capacity = "7.00G",
                    Identify = "off",
                    Index = i,
                    Details = "-"
                });
            }
            
            // Create ethernet ports for controllers
            // Source: Docs/PuttyLogs/putty2025-03-03.log L79-88, L134-143
            for (int ct = 0; ct <= 1; ct++)
            {
                for (int i = 0; i < 10; i++)
                {
                    var speed = i == 0 ? "1.00 Gb/s" : (i == 2 || i == 4) ? "25.00 Gb/s" : "0.00 b/s";
                    ethernetPorts.Add(new EthernetPort
                    {
                        Name = $"CT{ct}.ETH{i}",
                        Status = "ok",
                        Index = i,
                        Speed = speed,
                        Enabled = i == 0 || i == 2 || i == 4,
                        Services = i == 0 ? "management" : (i == 2 || i == 4 ? "replication" : "-")
                    });
                }
            }
            
            // Create FC ports for CT1 (Fibre Channel)
            // Source: Docs/PuttyLogs/putty2025-03-03.log L150-157
            var fcPortDefs = new[] {
                (0, 0, "16.00 Gb/s"), (0, 1, "16.00 Gb/s"), (0, 2, "0.00 b/s"), (0, 3, "0.00 b/s"),
                (1, 4, "16.00 Gb/s"), (1, 5, "16.00 Gb/s"), (2, 8, "0.00 b/s"), (2, 9, "0.00 b/s")
            };
            
            foreach (var (slot, index, speed) in fcPortDefs)
            {
                fcPorts.Add(new FCPort
                {
                    Name = $"CT1.FC{index}",
                    Status = "ok",
                    Slot = slot,
                    Index = index,
                    Speed = speed
                });
            }
            
            // Create fans for controllers
            for (int ct = 0; ct <= 1; ct++)
            {
                for (int i = 0; i < 6; i++)
                {
                    fans.Add(new Fan
                    {
                        Name = $"CT{ct}.FAN{i}",
                        Status = "ok",
                        Index = i
                    });
                }
            }
            
            // Create power supplies for chassis
            for (int i = 0; i < 2; i++)
            {
                powerSupplies.Add(new PowerSupply
                {
                    Name = $"CH0.PWR{i}",
                    Status = "ok",
                    Index = i,
                    Voltage = i == 0 ? "207 V" : "204 V"
                });
            }
            
            // Create temperature sensors
            // Controllers have many temperature sensors
            for (int ct = 0; ct <= 1; ct++)
            {
                for (int i = 0; i < 27; i++)
                {
                    temperatureSensors.Add(new TemperatureSensor
                    {
                        Name = $"CT{ct}.TMP{i}",
                        Status = "ok",
                        Index = i,
                        Temperature = $"{UnityEngine.Random.Range(29, 55)} C"
                    });
                }
            }
            
            // Chassis temperature sensor
            temperatureSensors.Add(new TemperatureSensor
            {
                Name = "CH0.TMP0",
                Status = "ok",
                Index = 0,
                Temperature = "20 C"
            });
        }
        
        // Hardware component getters
        public Controller GetController(string name) => controllers.Find(c => c.Name == name);
        public Drive GetDrive(string name) => drives.Find(d => d.Name == name);
        public EthernetPort GetEthernetPort(string name) => ethernetPorts.Find(p => p.Name == name);
        public FCPort GetFCPort(string name) => fcPorts.Find(p => p.Name == name);
    }
    
    // Hardware component definitions
    [Serializable]
    public class Controller
    {
        public string Name;
        public string Type;
        public string Mode;      // primary, secondary
        public string Model;     // FA-X70R3, etc.
        public string Version;   // Purity version
        public string Status;    // ready, offline, etc.
        public string Identify;  // on, off
    }
    
    [Serializable]
    public class Chassis
    {
        public string Name;
        public string Status;
        public string Identify;
        public int Index;
    }
    
    [Serializable]
    public class Drive
    {
        public string Name;      // CH0.BAY0, CH0.NVB0
        public string Type;      // SSD, NVRAM
        public string Status;    // healthy, failed, not_installed
        public string Capacity;  // 7.93T, 7.00G
        public string Identify;  // on, off
        public int Index;
        public string Details;
    }
    
    [Serializable]
    public class EthernetPort
    {
        public string Name;      // CT0.ETH0
        public string Status;    // ok, failed
        public int Index;
        public string Speed;     // 1.00 Gb/s, 10.00 Gb/s, 25.00 Gb/s, 100.00 Gb/s, 0.00 b/s
        public bool Enabled;
        public string Services;  // management, replication, iscsi, -
    }
    
    [Serializable]
    public class FCPort
    {
        public string Name;      // CT1.FC0
        public string Status;    // ok, failed
        public int Slot;         // PCIe slot
        public int Index;
        public string Speed;     // 8.00 Gb/s, 16.00 Gb/s, 32.00 Gb/s, 0.00 b/s
    }
    
    [Serializable]
    public class Fan
    {
        public string Name;      // CT0.FAN0
        public string Status;    // ok, failed
        public int Index;
    }
    
    [Serializable]
    public class PowerSupply
    {
        public string Name;      // CH0.PWR0
        public string Status;    // ok, failed
        public int Index;
        public string Voltage;   // 207 V, 204 V
    }
    
    [Serializable]
    public class TemperatureSensor
    {
        public string Name;      // CT0.TMP0
        public string Status;    // ok, warning
        public int Index;
        public string Temperature; // 20 C, 55 C
    }
}
