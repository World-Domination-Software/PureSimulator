# Virtual File System Framework for Pure Storage Simulator

## Overview

This document describes the comprehensive virtual file system framework implemented for the Pure Storage FlashArray simulator. The framework provides a realistic Linux-like environment that simulates the Purity operating system with full command-line interface support.

## Architecture

### Core Components

1. **VirtualFileSystem.cs** - Core file system implementation with hierarchical directory support
2. **VirtualFileSystemHandler.cs** - Command processing and user interaction layer
3. **VirtualHardware.cs** - Virtual hardware abstraction (drives, network interfaces, PCIe cards)
4. **VirtualFileSystemPopulator.cs** - Populates the file system with realistic content
5. **OS.cs** - Updated to integrate with the new virtual file system

### Directory Structure

The virtual file system implements a standard Linux directory hierarchy:

```
/
├── bin/          # System binaries
├── boot/         # Boot files
├── dev/          # Device files
├── etc/          # Configuration files
│   ├── hostname
│   ├── passwd
│   ├── hosts
│   └── timezone
├── home/         # User home directories
│   ├── puresetup/
│   └── pureeng/
├── lib/          # System libraries
├── media/        # Removable media
├── mnt/          # Mount points
├── opt/          # Optional software
│   └── Purity/   # Pure Storage specific tools
├── proc/         # Process information
├── root/         # Root user home
├── run/          # Runtime data
├── sbin/         # System admin binaries
├── sys/          # System information
├── tmp/          # Temporary files
├── usr/          # User programs
│   ├── bin/
│   ├── lib/
│   └── local/
└── var/          # Variable data
    └── log/      # Log files
```

## Supported Commands

### File System Navigation
- `ls` - List directory contents (supports -l, -a flags)
- `ll` - Alias for `ls -la`
- `la` - Alias for `ls -A`
- `cd` - Change directory
- `pwd` - Print working directory

### File Operations
- `cat` - Display file contents
- `touch` - Create empty files
- `cp` - Copy files
- `mv` - Move/rename files
- `rm` - Remove files (supports -r, -f flags)
- `find` - Search for files

### Directory Operations
- `mkdir` - Create directories
- `rmdir` - Remove empty directories

### System Information
- `df` - Display filesystem disk space usage
- `free` - Display memory usage
- `lsblk` - List block devices
- `ifconfig` - Display network interface configuration
- `lspci` - List PCI devices
- `iostat` - Display I/O statistics
- `uptime` - Show system uptime
- `whoami` - Display current username
- `id` - Display user and group IDs
- `uname` - Display system information
- `hostname` - Display system hostname
- `date` - Display current date and time
- `which` - Locate command executables
- `history` - Display command history

### Pure Storage Specific Commands
All existing Pure Storage commands are preserved:
- `purearray`
- `puredrive`
- `purenetwork`
- `purehw`
- `puresetup`
- `pureboot`
- And more...

### Mount Operations
- `mount` - Mount filesystems (enhanced with virtual FS support)
- `umount` - Unmount filesystems

## User Context

The system supports multiple user contexts:

### Users
- **root** - System administrator with full privileges
- **puresetup** - Setup user for initial configuration
- **pureeng** - Engineering user for maintenance and troubleshooting

### User Switching
- `sudo su` - Switch to root user
- `exit` - Return to previous user context

## Virtual Hardware

The framework includes comprehensive virtual hardware simulation:

### Virtual Drives
- System drives (SSD/NVMe)
- Data drives with capacity simulation
- Mount point management
- Health status monitoring

### Network Interfaces
- Multiple Ethernet interfaces (eth0-eth3)
- IP configuration
- Status monitoring
- Traffic statistics

### PCIe Cards
- Fibre Channel cards
- Network controllers
- NVMe controllers
- Device enumeration

## File System Features

### Realistic Content
- System configuration files
- User home directories with personalized content
- Application and system logs
- Executable binaries (simulated)
- Shell configuration files (.bashrc, .profile)
- Command history files

### File Metadata
- Creation and modification timestamps
- File permissions
- Owner and group information
- File sizes

### Dynamic Updates
- System files update based on chassis state
- Hardware information reflects current configuration
- Log files can be dynamically updated

## Integration with Existing Systems

The virtual file system integrates seamlessly with existing Pure Storage simulator components:

1. **Chassis System** - Hardware state reflected in virtual hardware
2. **USB Port Simulation** - USB devices can be mounted and accessed
3. **Command Processor** - All commands routed through enhanced processor
4. **Time Zone Manager** - System timezone reflected in /etc/timezone
5. **Array Management** - Array status visible through various commands

## Usage Examples

### Basic File Operations
```bash
# Navigate and explore
cd /home/pureeng
pwd
ls -la
ll

# Create and manipulate files
mkdir test_dir
touch test_file.txt
echo "Hello World" > test_file.txt
cat test_file.txt
cp test_file.txt backup.txt
mv backup.txt test_dir/
rm test_file.txt
```

### System Information
```bash
# Check system status
df -h
free -m
uptime
uname -a
hostname

# Check hardware
lsblk
lspci
ifconfig
```

### Pure Storage Operations
```bash
# Mount USB drive
mount /dev/sdb1 /mnt
ls /mnt
cp /mnt/*.ppkg .
umount /mnt

# System administration
sudo su
purehw list
puredrive list
exit
```

## Extensibility

The framework is designed to be easily extensible:

1. **New Commands** - Add handlers in VirtualFileSystemHandler.cs
2. **New Hardware** - Extend VirtualHardwareManager
3. **New File Types** - Add to VirtualFileSystemPopulator
4. **Custom Behaviors** - Modify command processing in OS.cs

## Benefits

1. **Realistic Training Environment** - Provides authentic Linux command-line experience
2. **Complete File System** - Full directory hierarchy with realistic content
3. **Hardware Simulation** - Virtual hardware matches real Pure Storage arrays
4. **Safe Learning** - No risk of damaging real systems
5. **Comprehensive Commands** - Supports most common Linux operations
6. **Pure Storage Integration** - Seamlessly works with existing simulator features

This virtual file system framework transforms the Pure Storage simulator into a comprehensive training and demonstration platform that closely mimics real Pure Storage environments while providing a safe, controlled learning experience.