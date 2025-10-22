# Pure Simulator — Commands Implementation Status

This document tracks the implementation status of all Pure Storage and Linux commands used in the simulator.

**Last Updated:** 2025-10-21 19:26:25 EDT

---

## Pure Storage Commands

### Implemented ✅

| Command | Subcommands | Description | Source |
|---------|------------|-------------|--------|
| `purearray` | list, list --controller, phonehome --send-today, remoteassist --connect | Array and controller management | Docs/PuttyLogs/putty2025-03-03.log |
| `pureboot` | list, reboot --primary, reboot --secondary, reboot --offline | Boot partition management | Docs/PuttyLogs/putty2025-03-03.log L299-304 |
| `puredrive` | list | Drive listing and management | Docs/PuttyLogs/putty2025-03-03.log |
| `purehw` | list, list --all, list --type | Hardware component listing | Docs/PuttyLogs/putty2025-03-03.log L46-192 |
| `purenetwork` | list, eth list, fc list | Network interface management | Docs/PuttyLogs/putty2025-03-03.log |
| `purealert` | tag --timeout --maintenance | Alert and maintenance window management | Docs/PuttyLogs/putty2025-02-22-2.txt |
| `puremessage` | list --open | System message listing | Docs/PuttyLogs/putty2025-03-03.log |
| `puresetup` | show, timezone, newarray --skip-connectivity-tests, secondaryarray --skip-connectivity-tests | Initial array configuration | commands.txt, Docs/getting_started PDF |
| `pureversion` | list | Purity version listing | commands.txt |
| `pureinstall` | <package.ppkg> | Purity software installation | Docs/PuttyLogs/putty2025-03-03.log L1201-1226 |
| `pureadm` | status | Purity service status | Docs/PuttyLogs/putty2025-03-03.log L238-248 |
| `purewes` | controller setattr --verify-array | Controller mode switching | Docs/PuttyLogs/ny2pure04.log |
| `puredb` | prefer CT0/CT1, npiv status | Database preferences | Docs/PuttyLogs/putty2025-03-03.log L1122-1196 |
| `iobalance` | --sampletime N | Host I/O balance monitoring | Docs/PuttyLogs/putty2025-03-03.log L1136-1192 |
| `puretune` | --list | System tunable management | Docs/PuttyLogs/putty2025-03-03.log L430-493 |
| `pureport` | list | Port connection listing | Docs/PuttyLogs/ny2pure04.log |
| `purevol` | list, list --connect | Volume management | Docs/PuttyLogs/putty2025-03-03.log L428 |

### Not Yet Implemented ⏳

- `pureapp` - Application/pod management
- `purehost` - Host management
- `puresnap` - Snapshot management
- `purepgroup` - Protection group management
- `purecert` - Certificate management
- `purelog` - Log management
- Additional subcommands for existing commands

---

## Linux Utility Commands

### Implemented ✅

| Command | Options | Description | Source |
|---------|---------|-------------|--------|
| `ls` / `lsblk` | (basic) | List block devices | Docs/PuttyLogs/putty2025-02-18.log |
| `mount` | <dev> <path> | Mount filesystems | Docs/PuttyLogs/putty2025-02-18.log |
| `umount` | <path> | Unmount filesystems | Docs/PuttyLogs/putty2025-02-18.log |
| `sudo` | su | Execute as root | Docs/PuttyLogs/putty2025-03-03.log |
| `cat` | <file> | Display file contents | Docs/PuttyLogs/putty2025-03-03.log |
| `clear` | (none) | Clear terminal screen | Common Linux command |
| `exit` / `quit` / `logout` | (none) | Exit shell | Common Linux command |
| `ssh` | <host>, peer | Connect to remote hosts | Docs/PuttyLogs/putty2025-03-03.log |
| `ping` | -c N <host> | Send ICMP echo requests | Common Linux command |
| `df` | -h | Disk space usage | Common Linux command |
| `dmesg` | (basic) | Kernel ring buffer | Docs/PuttyLogs/putty2025-02-18.log |
| `stty` | rows N, cols N | Terminal settings | Common Linux command |

### Not Yet Implemented ⏳

**File Operations:**
- `cp` - Copy files/directories
- `mv` - Move/rename files
- `rm` - Remove files/directories
- `mkdir` - Create directories
- `rmdir` - Remove directories
- `chmod` - Change file permissions
- `chown` - Change file ownership
- `ln` - Create links

**Text Processing:**
- `grep` - Search text patterns
- `awk` - Pattern scanning and processing
- `sed` - Stream editor
- `tail` - Output last part of files
- `head` - Output first part of files
- `less` / `more` - Page through text
- `vi` / `nano` - Text editors

**Process Management:**
- `ps` - Process status
- `top` - System monitor
- `kill` - Terminate processes
- `killall` - Kill by name
- `pkill` - Pattern kill

**Network Utilities:**
- `ifconfig` / `ip` - Network interface configuration
- `netstat` - Network statistics
- `route` - Routing table
- `traceroute` - Trace network path

**System Information:**
- `uname` - System information
- `uptime` - System uptime
- `free` - Memory usage
- `du` - Disk usage

---

## Diagnostic Scripts

### Implemented ✅

| Script | Description | Source |
|--------|-------------|--------|
| `hardware_check.py` | System hardware verification | Docs/PuttyLogs/putty2025-03-03.log L194-218 |

### Not Yet Implemented ⏳

- `storage_view.py` - Storage view and analysis
- `cobalt_check.py` - Cobalt cluster health check
- Other diagnostic/monitoring scripts

---

## Hardware Model

### Architecture ✅

**PCIe Card Slots:**
- Each controller has 3 PCIe slots (1, 2, 3)
- Each slot can hold a 4-port card (FC or Ethernet)
- Cards can be swapped between FC and ETH types

**Built-in Ports (not on PCIe cards):**
- ETH0: Management (1 Gb/s)
- ETH2: Replication (25 Gb/s)
- ETH3: Replication (25 Gb/s)

**Default Configuration:**
- Slot 1: 4-port FC card (FC0-FC3)
- Slot 2: 4-port FC card (FC4-FC7)
- Slot 3: 4-port FC card (FC8-FC11)

**Port Naming:**
- FC ports: CT0.FC0-FC11, CT1.FC0-FC11
- ETH ports: ct0.eth0-eth15, ct1.eth0-eth15 (lowercase in purenetwork)

### Features Implemented ✅

- PCIeCard class with CardType (None, FibreChannel, Ethernet)
- Slot tracking for all ports
- Port generation based on PCIe card configuration
- purenetwork list shows both FC and ETH ports
- purehw list shows all hardware with slot info

### Not Yet Implemented ⏳

- Console command to swap PCIe cards
- 3D visualizers for PCIe card slots
- Interactive card swapping UI
- SFP module swapping
- Hardware profiles for different array models (X70R3, X90R4, C60)

---

## Testing Status

### Unit Tests ✅

- Console parser tests
- Console command registry tests
- Boundary tests (Console vs Serial separation)

### Integration Tests ⏳

- Golden transcript tests (comparing command output to real logs)
- Workflow engine tests
- Hardware model tests
- PCIe card swapping tests

---

## Command Usage Examples

### Pure Storage Commands

```bash
# Boot management
pureboot list
pureboot reboot --secondary

# Version management
pureversion list
pureinstall purity_6.5.8_202408090136+b967c2f84655.ppkg

# Service status
pureadm status

# Controller management
purewes controller setattr --verify-array pure00 ct1 --mode secondary
puredb prefer CT1

# Monitoring
iobalance --sampletime 30
puretune --list

# Network and ports
purenetwork list
pureport list
purevol list --connect
```

### Linux Commands

```bash
# Device and filesystem
lsblk
mount /dev/sdb1 /mnt/usb
umount /mnt/usb

# System
sudo su
cat /etc/timezone
dmesg | grep -i usb
df -h

# Network
ssh peer
ping -c 5 8.8.8.8

# Scripts
hardware_check.py
```

---

## Priority for Next Implementation

1. **High Priority:**
   - PCIe card swapping console command
   - Linux file operations (cp, mv, rm, mkdir)
   - Process management (ps, top, kill)

2. **Medium Priority:**
   - Text processing (grep, tail, head)
   - Diagnostic scripts (storage_view.py, cobalt_check.py)
   - Additional Pure commands (purehost, puresnap)

3. **Low Priority:**
   - Text editors (vi, nano)
   - Network utilities (ifconfig, netstat)
   - Hardware profiles for different array models
