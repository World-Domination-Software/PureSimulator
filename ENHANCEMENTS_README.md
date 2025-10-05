# Pure Simulator Enhancements

This document describes the enhancements made to the Pure Storage Simulator based on actual log files from real Pure Storage arrays.

## Overview

The simulator now includes:
1. **Complete command set** from actual Pure Storage logs
2. **USB drive version management** - ability to specify different Purity versions
3. **Configurable hard drive sizes** - extensible system for future drive size selection
4. **Enhanced virtual file system** - including chmod, chown, and other Linux commands

## USB Drive Version Management

### Purpose
The USB port can now simulate different versions of Purity OS, allowing testing of various upgrade scenarios.

### Implementation
The `USBPort` class now includes:

```csharp
public string purityVersionOnDrive = "6.5.8";  // Default version

// Method to change Purity version
public void SetPurityVersion(string version)

// Method to get current version
public string GetPurityVersion()
```

### Usage Example

In Unity editor or at runtime:
```csharp
// Change USB drive to contain Purity 6.6.0
usbPort.SetPurityVersion("6.6.0");

// This will automatically update the files to:
// - purity_6.6.0.ppkg
// - purity_6.6.0.ppkg.sha1
```

### Future Enhancement Path
To add a UI for version selection:
1. Create a dropdown menu in Unity
2. Populate with available versions
3. Call `SetPurityVersion()` when user selects a version
4. The files will automatically update

## Hard Drive Size Configuration

### Purpose
Hard drives now have extensible size configuration, allowing different capacities to be simulated and reflected in all command outputs.

### Implementation
The `HardDrive` class includes:

```csharp
public double StorageSpace;  // Size in MB

// New helper methods:
public void SetStorageSize(double sizeInMB)
public string GetFormattedSize()
public double GetSizeInTB()
public double GetSizeInGB()
public double GetSizeInMB()
```

### Usage Example

```csharp
// Set drive to 2TB
hardDrive.SetStorageSize(2000000);  // 2,000,000 MB = 2TB

// Or use Init() method
hardDrive.Init(chassis, 2000000, 0, "CH0.BAY0", HardDriveStatus.healthy);
```

### Size Propagation
Drive sizes automatically appear in:
- `puredrive list` command output
- `df` command output (via VirtualHardwareManager)
- `lsblk` command output
- Any other command that shows drive information

### Future Enhancement Path
To add a UI for drive size selection:
1. Create a dropdown or slider in Unity for each drive bay
2. Options could include: 1.92TB, 3.84TB, 7.68TB, 15.36TB, etc.
3. Call `SetStorageSize()` when user selects a size
4. All command outputs will automatically reflect the new size

## New Commands Added

### Pure Storage Commands

Based on actual log files, the following commands are now supported:

#### purearray
- `purearray list` - List arrays
- `purearray list --controller` - List controller status
- `purearray phonehome --send-today` - Send phonehome data
- `purearray phonehome --send-dotoday` - Send phonehome data
- `purearray remoteassist --connect` - Connect remote assistance
- `purearray remoteassist --status` - Check remote assistance status

#### purenetwork
- `purenetwork list` - List all network interfaces
- `purenetwork eth list` - List Ethernet interfaces with details
- `purenetwork fc list` - List Fibre Channel interfaces with details

#### purehw
- `purehw list` - List all hardware components
- `purehw list --type pwr` - List power supplies only

#### puremessage
- `puremessage list --open` - List open messages/alerts
- `puremessage list --open --hidden` - List open and hidden messages

#### purealert
- `purealert tag --timeout <time> --maintenance` - Tag system for maintenance
- `purealert untag --maintenance` - Remove maintenance tag

#### pureport
- `pureport list --initiator` - List port initiator connections

#### puretune
- `puretune --list` - List tunable parameters

#### puredb
- `puredb` - Database query commands (basic support)

#### iobalance
- `iobalance --sampletime <seconds>` - Show I/O balance across controllers

#### purevol
- `purevol list` - List volumes
- `purevol list --connect` - List volume connections
- `purevol list --pending` - List pending volumes
- `purevol list --snap` - List snapshots
- `purevol list --protocol-endpoint` - List protocol endpoints

#### puremastership
- `puremastership list` - List mastership status

### Linux File Commands

#### chmod
Change file permissions (basic simulation)
```bash
chmod 755 /path/to/file
chmod +x script.sh
```

#### chown
Change file ownership (requires root)
```bash
chown user:group /path/to/file
chown pureeng:pureeng /home/pureeng/file.txt
```

## Command Output Examples

All command outputs are based on actual Pure Storage array logs, ensuring realistic simulation.

### Example: purenetwork eth list
```
Name          Address              Netmask            Gateway            MTU    Enabled
eth0          192.168.1.100        255.255.255.0      192.168.1.1        1500   True
eth1          192.168.1.101        255.255.255.0      192.168.1.1        1500   True
eth2          192.168.2.100        255.255.255.0      192.168.2.1        9000   True
eth3          -                    -                  -                  1500   False
```

### Example: iobalance --sampletime 30
```
Sampling I/O balance...
Controller  Read IOPS  Write IOPS  Read BW    Write BW
CT0         2453       5671        234MB/s    456MB/s
CT1         2389       5823        228MB/s    467MB/s
```

### Example: puredrive list
Shows actual drive sizes as configured:
```
Name          Type     Status      Size
CH0.BAY0      SSD      healthy     2T
CH0.BAY1      SSD      healthy     2T
CH0.BAY2      SSD      healthy     3.84T
```

## Testing the Enhancements

### Test USB Version Change
1. In Unity, select a USBPort object
2. In Inspector, change `purityVersionOnDrive` to "6.6.0"
3. Insert the USB drive in simulation
4. Run `ls /mnt` after mounting
5. Verify files show: `purity_6.6.0.ppkg` and `purity_6.6.0.ppkg.sha1`

### Test Drive Sizes
1. In Unity, select a HardDrive object
2. In Inspector, change `StorageSpace` to different values:
   - 1920000 for 1.92TB
   - 3840000 for 3.84TB
   - 7680000 for 7.68TB
3. Run simulation and execute `puredrive list`
4. Verify the size shows correctly in the output

### Test New Commands
Run each command and verify output matches expected format:
```bash
purearray phonehome --send-today
purenetwork eth list
purehw list --type pwr
puremessage list --open
pureport list --initiator
iobalance --sampletime 30
purevol list --connect
puremastership list
chmod 755 /tmp/test.sh
chown pureeng:pureeng /tmp/test.sh
```

## Architecture Notes

### Extensibility Design
The implementation is designed for easy extension:

1. **USB Versions**: Simply call `SetPurityVersion()` with any version string
2. **Drive Sizes**: Call `SetStorageSize()` with any size in MB
3. **New Commands**: Add to OS.cs following existing pattern
4. **Command Outputs**: All outputs can be customized in their respective handlers

### Virtual File System Integration
- All Linux commands (chmod, chown, etc.) integrate with VirtualFileSystem
- File permissions and ownership are tracked per file/directory
- Changes persist during the simulation session

### Command Processing Flow
```
User Input → OS.ProcessCommand() → 
  If Pure command: Direct handler in OS.cs
  If Linux command: VirtualFileSystemHandler
  If System command: VirtualHardwareManager
→ Output to CommandProcessor
```

## Future Enhancement Opportunities

### USB Drive Management
- Add UI dropdown for version selection
- Support multiple USB drives with different versions
- Add validation for compatible versions

### Hard Drive Configuration
- Add UI for selecting drive capacities per bay
- Support different drive types (SSD, NVMe, etc.)
- Add drive failure simulation based on size/type

### Command Extensions
- Add more Pure Storage commands from additional logs
- Implement full puredb query support
- Add purepgroup (protection groups)
- Add pureapp (application management)
- Add purepod (pod management)

### Advanced Features
- Save/load drive configurations
- Import/export array configurations
- Replay command sequences from log files
- Generate realistic performance metrics based on drive sizes

## Bug Fixes Included

### VirtualDirectory.GetFilesNames()
Fixed a bug where the loop was always accessing index 1 instead of index i:
```csharp
// Before:
s += "    "+ _Files[1];  // Bug: always index 1

// After:
s += "    "+ _Files[i];  // Correct: use loop variable
```

## Summary

These enhancements make the Pure Storage Simulator more realistic and useful for training and testing:

1. **Complete command coverage** from real arrays
2. **Version management** for upgrade scenario testing
3. **Size configuration** for capacity planning simulation
4. **File operations** for realistic Linux experience
5. **Extensible design** for easy future enhancements

All implementations are based on actual Pure Storage array logs, ensuring authenticity and practical value for training purposes.
