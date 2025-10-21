# Hardware Model Integration Guide

## Overview

This document explains how to connect the virtual hardware models (`HardwareModel.cs`) with the 3D Unity GameObjects representing Pure Storage equipment.

The hardware model is the **authoritative source of truth** for all hardware state, while the 3D models provide **visualization** of that state.

---

## Architecture

```
┌─────────────────────────────────────────────────────┐
│           SimulationState                            │
│  - Single source of truth for all state             │
└───────────────┬─────────────────────────────────────┘
                │
                │ contains
                ▼
┌─────────────────────────────────────────────────────┐
│           HardwareModel                              │
│  - Controllers (CT0, CT1)                            │
│  - Drives (CH0.BAY0-19, CH0.NVB0-3)                  │
│  - Ethernet Ports (CT*.ETH0-9)                       │
│  - FC Ports (CT*.FC0-9)                              │
│  - Fans, Power Supplies, Temperature Sensors         │
└───────────────┬─────────────────────────────────────┘
                │
                │ observed by
                ▼
┌─────────────────────────────────────────────────────┐
│      3D GameObject Hierarchy                         │
│  - FlashArray (root)                                 │
│    ├─ Controller0 (CT0)                              │
│    │  ├─ EthernetPort0 (ETH0)                        │
│    │  ├─ EthernetPort1 (ETH1)                        │
│    │  └─ ...                                          │
│    ├─ Controller1 (CT1)                              │
│    ├─ Chassis0 (CH0)                                 │
│    │  ├─ Bay0                                         │
│    │  ├─ Bay1                                         │
│    │  └─ ...                                          │
│    └─ ...                                             │
└─────────────────────────────────────────────────────┘
```

---

## Hardware Component Mapping

### Controllers (CT0, CT1)

**Model:**
```csharp
public class Controller
{
    public string Name;        // "CT0", "CT1"
    public string Type;        // "array_controller"
    public string Mode;        // "primary", "secondary"
    public string Model;       // "FA-X70R3"
    public string Version;     // "6.5.8"
    public string Status;      // "ready", "offline", "failed"
    public string Identify;    // "on", "off"
}
```

**3D GameObject:**
- Name: `Controller0` or `Controller1`
- Script: `ControllerVisualizer.cs`
- Visual indicators:
  - Status LED (green=ready, red=failed, yellow=degraded)
  - Identify LED (blue when identify=="on")
  - Label showing mode (Primary/Secondary)

**Connection:**
```csharp
public class ControllerVisualizer : MonoBehaviour
{
    public string controllerName = "CT0";
    private Controller model;
    private Light statusLED;
    private Light identifyLED;
    
    void Update()
    {
        // Get current state from simulation
        model = GameManager.Instance.SimulationState
            .GetHardwareModel()
            .GetController(controllerName);
        
        // Update visuals
        UpdateStatusLED(model.Status);
        UpdateIdentifyLED(model.Identify == "on");
    }
}
```

---

### Drives (CH0.BAY0-19, CH0.NVB0-3)

**Model:**
```csharp
public class Drive
{
    public string Name;        // "CH0.BAY0"
    public string Type;        // "SSD", "NVRAM"
    public string Status;      // "healthy", "failed", "not_installed"
    public string Capacity;    // "7.93T", "7.00G"
    public string Identify;    // "on", "off"
    public int Index;          // Bay index 0-19
}
```

**3D GameObject:**
- Parent: `Chassis0` GameObject
- Children: `Bay0` to `Bay19`, `NVRAMBay0` to `NVRAMBay3`
- Script: `DriveSlotVisualizer.cs`
- Visual indicators:
  - Drive presence (model visible/invisible based on status)
  - Status LED (green=healthy, red=failed, amber=degraded)
  - Identify LED (blue when identify=="on")
  - Capacity label

**Connection:**
```csharp
public class DriveSlotVisualizer : MonoBehaviour
{
    public string driveName = "CH0.BAY0";
    private Drive model;
    public GameObject driveModel;  // 3D model of the drive
    private Light statusLED;
    
    void Update()
    {
        model = GameManager.Instance.SimulationState
            .GetHardwareModel()
            .GetDrive(driveName);
        
        // Show/hide drive based on installation status
        driveModel.SetActive(model.Status != "not_installed");
        
        // Update LED color
        UpdateStatusLED(model.Status);
    }
}
```

---

### Ethernet Ports (CT*.ETH0-9)

**Model:**
```csharp
public class EthernetPort
{
    public string Name;        // "CT0.ETH0"
    public string Status;      // "ok", "failed"
    public int Index;          // 0-9
    public string Speed;       // "1.00 Gb/s", "10.00 Gb/s", "25.00 Gb/s"
    public bool Enabled;
    public string Services;    // "management", "replication", "iscsi"
}
```

**3D GameObject:**
- Parent: `Controller0` or `Controller1`
- Children: `EthernetPort0` to `EthernetPort9`
- Script: `EthernetPortVisualizer.cs`
- Visual indicators:
  - SFP module present/absent
  - Link LED (green when enabled and connected)
  - Activity LED (blinking during traffic)
  - Speed indicator (color-coded: copper=orange, 10G=green, 25G=blue)

**Connection:**
```csharp
public class EthernetPortVisualizer : MonoBehaviour
{
    public string portName = "CT0.ETH0";
    private EthernetPort model;
    public GameObject sfpModule;
    private Light linkLED;
    
    void Update()
    {
        model = GameManager.Instance.SimulationState
            .GetHardwareModel()
            .GetEthernetPort(portName);
        
        // Show SFP if port has speed
        sfpModule.SetActive(model.Speed != "0.00 b/s");
        
        // Update link LED
        linkLED.enabled = model.Enabled;
        linkLED.color = GetSpeedColor(model.Speed);
    }
}
```

---

### FC Ports (CT*.FC0-9)

**Model:**
```csharp
public class FCPort
{
    public string Name;        // "CT1.FC0"
    public string Status;      // "ok", "failed"
    public int Slot;           // PCIe slot
    public int Index;          // Port index
    public string Speed;       // "16.00 Gb/s", "32.00 Gb/s"
}
```

**3D GameObject:**
- Parent: PCIe card GameObject in Controller
- Script: `FCPortVisualizer.cs`
- Visual indicators:
  - SFP module present/absent
  - Link LED (green when connected)
  - Speed indicator

**Connection:**
```csharp
public class FCPortVisualizer : MonoBehaviour
{
    public string portName = "CT1.FC0";
    private FCPort model;
    public GameObject sfpModule;
    
    void Update()
    {
        model = GameManager.Instance.SimulationState
            .GetHardwareModel()
            .GetFCPort(portName);
        
        // Show SFP if port has link
        sfpModule.SetActive(model.Speed != "0.00 b/s");
    }
}
```

---

### Fans, Power Supplies, Temperature Sensors

These components typically don't have individual 3D representations but can:
- Show status in UI overlays
- Display warning indicators when faults occur
- Show activity (fan spinning animation based on status)

---

## Event-Driven Updates

For performance, use event-driven updates instead of polling in `Update()`:

```csharp
public class HardwareVisualizer : MonoBehaviour
{
    void OnEnable()
    {
        // Subscribe to state change events
        GameManager.Instance.SimulationState.OnStateChanged += HandleStateChanged;
    }
    
    void OnDisable()
    {
        GameManager.Instance.SimulationState.OnStateChanged -= HandleStateChanged;
    }
    
    private void HandleStateChanged(string change)
    {
        // Update visuals only when state actually changes
        RefreshVisualization();
    }
}
```

---

## Interactive Components

### Swapping Components

When user interacts with a component in 3D:

```csharp
public void OnDriveClicked(string driveName)
{
    var sim = GameManager.Instance.SimulationState;
    var hardware = sim.GetHardwareModel();
    var drive = hardware.GetDrive(driveName);
    
    if (drive.Status == "not_installed")
    {
        // Install drive
        drive.Status = "healthy";
        sim.OnStateChanged?.Invoke($"Drive {driveName} installed");
    }
    else
    {
        // Remove drive
        drive.Status = "not_installed";
        sim.OnStateChanged?.Invoke($"Drive {driveName} removed");
    }
}
```

### Swapping SFPs

```csharp
public void SwapSFP(string portName, string newSpeed)
{
    var sim = GameManager.Instance.SimulationState;
    var hardware = sim.GetHardwareModel();
    var port = hardware.GetEthernetPort(portName);
    
    // Change SFP speed (e.g., from 10G to 25G)
    port.Speed = newSpeed;
    sim.OnStateChanged?.Invoke($"SFP changed on {portName} to {newSpeed}");
}
```

### Swapping PCIe Cards

For swapping Ethernet/FC cards:

```csharp
public void SwapPCIeCard(string controllerName, int slot, string cardType)
{
    var sim = GameManager.Instance.SimulationState;
    var hardware = sim.GetHardwareModel();
    
    // Remove old ports for this slot
    // Add new ports based on card type
    
    if (cardType == "FC_16G_4PORT")
    {
        // Add 4 FC ports at 16Gb/s
        for (int i = 0; i < 4; i++)
        {
            hardware.FCPorts.Add(new FCPort
            {
                Name = $"{controllerName}.FC{slot * 4 + i}",
                Slot = slot,
                Index = slot * 4 + i,
                Speed = "16.00 Gb/s",
                Status = "ok"
            });
        }
    }
    else if (cardType == "ETH_25G_2PORT")
    {
        // Add 2 Ethernet ports at 25Gb/s
        for (int i = 0; i < 2; i++)
        {
            hardware.EthernetPorts.Add(new EthernetPort
            {
                Name = $"{controllerName}.ETH{slot * 2 + i}",
                Index = slot * 2 + i,
                Speed = "25.00 Gb/s",
                Status = "ok",
                Enabled = false,
                Services = "-"
            });
        }
    }
    
    sim.OnStateChanged?.Invoke($"PCIe card swapped in {controllerName} slot {slot}");
}
```

---

## Hardware Profiles

Define common hardware configurations:

```csharp
public static class HardwareProfiles
{
    public static void ConfigureX70R3(HardwareModel hardware)
    {
        // 2 controllers, 20 SSDs, 4 NVRAM, 10 ETH per controller, FC on CT1
        // (Already default configuration)
    }
    
    public static void ConfigureX90R4(HardwareModel hardware)
    {
        // Different drive count, more ports, etc.
        hardware.Drives.Clear();
        
        for (int i = 0; i < 24; i++)
        {
            hardware.Drives.Add(new Drive
            {
                Name = $"CH0.BAY{i}",
                Type = "SSD",
                Status = "healthy",
                Capacity = "15.36T",  // Larger drives
                Index = i
            });
        }
    }
}
```

---

## Drive Specifications

Different drive models have different capabilities:

```csharp
public class DriveSpecification
{
    public string Model;
    public string Capacity;
    public string MinPurityVersion;  // Minimum Purity version required
    public string Type;              // "SSD", "NVRAM", "DirectFlash"
    
    public static readonly DriveSpecification[] KnownDrives = new[]
    {
        new DriveSpecification 
        { 
            Model = "Micron_5300_7.93T", 
            Capacity = "7.93T", 
            MinPurityVersion = "6.3.0",
            Type = "SSD"
        },
        new DriveSpecification 
        { 
            Model = "Micron_5300_15.36T", 
            Capacity = "15.36T", 
            MinPurityVersion = "6.4.0",
            Type = "SSD"
        },
        new DriveSpecification 
        { 
            Model = "DirectFlash_X_48T", 
            Capacity = "48T", 
            MinPurityVersion = "6.5.0",
            Type = "DirectFlash"
        }
    };
}
```

---

## Port Speed Options

SFPs support different speeds based on hardware:

```csharp
public static class PortSpeedOptions
{
    public static readonly string[] EthernetSpeeds = new[]
    {
        "0.00 b/s",      // Empty/disconnected
        "1.00 Gb/s",     // Copper RJ45 or 1G SFP
        "10.00 Gb/s",    // 10G SFP+
        "25.00 Gb/s",    // 25G SFP28
        "40.00 Gb/s",    // 40G QSFP+
        "100.00 Gb/s"    // 100G QSFP28
    };
    
    public static readonly string[] FCSpeed = new[]
    {
        "0.00 b/s",      // Empty/disconnected
        "8.00 Gb/s",     // 8G FC
        "16.00 Gb/s",    // 16G FC
        "32.00 Gb/s"     // 32G FC
    };
}
```

---

## Testing Hardware State

Example test to verify hardware model is working:

```csharp
[Test]
public void TestHardwareModel()
{
    var sim = new SimulationState();
    var hardware = sim.GetHardwareModel();
    
    // Verify controllers
    Assert.AreEqual(2, hardware.Controllers.Count);
    Assert.AreEqual("CT0", hardware.Controllers[0].Name);
    Assert.AreEqual("secondary", hardware.Controllers[0].Mode);
    
    // Verify drives
    var bay0 = hardware.GetDrive("CH0.BAY0");
    Assert.IsNotNull(bay0);
    Assert.AreEqual("SSD", bay0.Type);
    Assert.AreEqual("healthy", bay0.Status);
    
    // Verify ports
    var eth0 = hardware.GetEthernetPort("CT0.ETH0");
    Assert.IsNotNull(eth0);
    Assert.AreEqual("1.00 Gb/s", eth0.Speed);
}
```

---

## Summary

1. **Hardware Model** (`HardwareModel.cs`) is the authoritative state
2. **3D GameObjects** visualize that state via observer scripts
3. **User interactions** with 3D modify the hardware model
4. **State changes** trigger visual updates via events
5. **Commands** (`purehw`, `puredrive`, etc.) query the hardware model

This separation allows:
- Commands to work without 3D visualization
- 3D to update independently from simulation logic
- Easy testing of hardware state without Unity
- Save/load of hardware configurations as JSON

---

*Last Updated: 2025-10-21*
