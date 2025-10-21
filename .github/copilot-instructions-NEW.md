# Pure Simulator — AI Coding Agent Instructions

## Project Overview
**Pure Storage FlashArray Training Simulator** — Unity 6 (C#) 3-D simulator for Pure Storage installation/operations training. Users manipulate 3-D hardware (cables, drives, controllers) and interact via two distinct terminal interfaces.

**Core Stack:** Unity 6, C# 10+, nullable enabled  
**Key Namespaces:** `CrimsofallTechnologies.ServerSimulator` (legacy), `PureSim.{Console|Serial|Simulation}` (new architecture)

---

## Critical Architecture: Dual Console System

### IMPLEMENTATION STATUS (READ FIRST!)

**Current State:** TWO separate console systems exist:

**1. ✅ WORKING: Serial Terminal (`CommandProcessor.cs`)** — Your Main PuTTY Interface
- **Location:** `Assets/Scripts/CommandProcessor.cs`, `OS.cs`, `VirtualFileSystemHandler.cs`
- **UI:** Unity UI `Text` + `InputField` (already in scene, fully working)
- **Purpose:** Simulates serial connection to **Purity OS** — the MAIN user interface
- **Commands:** All operational commands (ls, mount, cat, purearray, purehw, puredrive, etc.)
- **Pipeline:** `CommandProcessor` → `OS.ProcessCommand()` → `VirtualFileSystemHandler` → Output
- **Status:** FULLY IMPLEMENTED AND INTEGRATED — This is what users interact with!

**2. ⚠️ NEW BUT NOT CONNECTED: Quake Console (`ConsoleController.cs`)** — Optional Trainer Overlay
- **Location:** `Assets/Scripts/Console/ConsoleController.cs` and `Commands/*.cs`
- **UI:** TextMeshPro (TMP_InputField, TMP_Text) — NOT YET ADDED TO SCENE
- **Purpose:** Simulator control overlay for trainers/developers (toggle with ` key)
- **Commands:** `jump <step>`, `inject fault`, `usb state`, `steps`, `help`, `clear`
- **Pipeline:** `ConsoleController` → `ConsoleRegistry` → `[ConsoleCommand]` handlers → `SimulationState`/`WorkflowEngine`
- **Status:** CODE EXISTS but needs UI scene setup (optional feature for later)

### Two SEPARATE Terminal Interfaces (Do NOT Mix!)

**When adding operational commands (ls, mount, purearray):**
→ Add to `OS.cs` or `VirtualFileSystemHandler.cs` (routes through `CommandProcessor.cs`)

**When adding simulator control (jump, inject, debug):**
→ Add to `Assets/Scripts/Console/Commands/*.cs` with `[ConsoleCommand]` attribute

**DO NOT create `Assets/Scripts/Serial/` commands** — that's future architecture. Use `OS.cs` for now!

### Separation Checklist
✅ **Serial Terminal:** Operational commands engineers run on Purity OS  
✅ **Quake Console:** Simulator control (workflow, faults, USB injection)  
❌ **Never:** Mix these two — they have zero overlap in commands

---

## Source Material Fidelity (CRITICAL)

**Before implementing ANY command output:**
1. Search `Docs/PuttyLogs/*.log` for real session transcripts
2. Search `Docs/*.pdf` (15+ PDFs: CLI guides, installation manuals, hardware specs)
3. Check `commands.txt` for tested command status
4. **Mirror output exactly** (whitespace, formatting, error messages)
5. **Cite source** in code comments

**Example Citation:**
```csharp
// Source: Docs/PuttyLogs/putty2025-02-18.log L650-653
terminal.WriteLine("mount: special device /dev/sdb1 does not exist");

// Source: Docs/PurityFA_6.4.5_FlashArray_CLIRefGuide.pdf p.42-43
```

**If source missing/unclear:**
```csharp
// TODO: Replace with authoritative output from Docs/PuttyLogs or CLI guide p.XX
```

---

## Virtual Filesystem Implementation

**Core Files:**
- `VirtualFileSystem.cs` — Hierarchical Linux-like filesystem in memory
- `VirtualFileSystemHandler.cs` — Command processor (200+ commands)
- `VirtualHardware.cs` — Virtual drives, NICs, PCIe cards
- `OS.cs` — Static command router integrating with `CommandProcessor.cs`

**Directory Structure:** Standard Linux (`/bin`, `/home`, `/etc`, `/var/log`, `/mnt`, `/proc`)

**Integration Pattern:**
```csharp
// In OS.cs or command handlers
if (fileSystemHandler != null)
{
    string result = fileSystemHandler.HandleLsCommand(splits);
    commandProcessor.Log(result);
    return;
}
// Fallback to legacy logic if filesystem not initialized
```

**Commands Supported:** `ls`, `cd`, `pwd`, `cat`, `touch`, `mkdir`, `rm`, `cp`, `mv`, `find`, `mount`, `umount`, `df`, `lsblk`, `chmod`, `chown`, wildcard expansion (`*.ppkg`)

---

## Hardware Model (3-D Simulation)

**Key Components:**
- `Chassis.cs` — Array/shelf with controllers, drives, ports, power (860 lines, MonoBehaviour)
- `HardDrive.cs`, `USBPort.cs`, `WirePort.cs`, `FlashArray.cs`, `NvRAM.cs`
- `CommandProcessor.cs` — Main terminal emulator (2500+ lines, UI integration)
- `SimulationState.cs` — New architecture: single source of truth for state (USB, faults, power)
- `WorkflowEngine.cs` — Installation/upgrade steps with precondition guards

**State Management:**
- **Legacy:** Direct manipulation via `Chassis` properties (`PSU0On`, `CT1Installed`, `OSFullyRunning`)
- **New:** Go through `SimulationState` events and APIs (`SetUsbInserted()`, `InjectFault()`)
- **Transition:** Both systems coexist; prefer `SimulationState` for new features

---

## Developer Workflows

### Adding New Serial Commands (Operational)
1. Create `Assets/Scripts/Serial/Commands/MyCommand.cs`
2. Implement `ISerialCommand` interface
3. Add `[SerialCommand("mycommand")]` attribute
4. **Search `Docs/PuttyLogs/` for real output examples**
5. Mirror output exactly, cite source in comments
6. Test in `CommandProcessor` terminal

### Adding New Console Commands (Simulator Control)
1. Create `Assets/Scripts/Console/Commands/MyCommand.cs`
2. Implement `IConsoleCommand` interface
3. Add `[ConsoleCommand("mycommand")]` attribute
4. Use `SimulationState` and `WorkflowEngine` APIs only
5. Add unit test in `Assets/Tests/Editor/Console/`

### Testing
- **Editor Tests:** NUnit in `Assets/Tests/Editor/` (guard with `#if UNITY_INCLUDE_TESTS`)
- **PlayMode Tests:** `Assets/Tests/PlayMode/`
- **Golden Transcripts:** Run scripted commands, compare output to `Docs/PuttyLogs/`
- **Run:** Unity Test Runner or `Assets/Editor/` scripts

### Building & Running
- **Scene:** Main training scene with `Chassis`, `CommandProcessor`, `UIManager`
- **Input:** `InputField` in Unity UI captures commands
- **Output:** `Text` component displays terminal output with color coding
- **Keyboard:** History navigation (↑↓), autocomplete (Tab), console toggle (~)

---

## Coding Conventions

**Namespaces:**
- Legacy: `CrimsofallTechnologies.ServerSimulator` (most existing code)
- New: `PureSim.Console.*`, `PureSim.Serial.*`, `PureSim.Simulation.*`
- Folder structure mirrors namespaces

**Style:**
- XML docs on public APIs
- Structured logging (no PII)
- No per-frame allocations in hot paths
- Nullable reference types enabled

**Command Pattern:**
```csharp
[SerialCommand("lsblk")]
public class LsblkCommand : ISerialCommand
{
    public string Name => "lsblk";
    public string Synopsis => "List block devices";
    public IReadOnlyList<string> Parameters => new[] { "[device]" };
    
    public void Execute(SimulationState sim, string[] args, ISerialOutput terminal)
    {
        // Source: Docs/PuttyLogs/putty2025-02-18.log L120-176
        if (!sim.IsUsbInserted())
        {
            terminal.WriteLine("sda      8:0    0  200G  0 disk");
            return;
        }
        terminal.WriteLine("sda      8:0    0  200G  0 disk");
        terminal.WriteLine("sdb      8:16   1   32G  0 disk");
        terminal.WriteLine("└─sdb1   8:17   1   32G  0 part");
    }
}
```

---

## Change Tracking (REQUIRED)

**Every change must append to `change log.MD` (exact case/spacing):**
```markdown
## YYYY-MM-DD HH:MM:SS EDT — Title
**Author:** Copilot / Human  
**Task:** Brief summary

### Files Changed
- `path/to/file.cs` — Description

### Rationale
Why this change was made

### Test Plan
How to verify

### Citations
- Docs/PuttyLogs/file.log L100-150
```

**"Next Steps" System:**
- Update `.copilot/next_steps.json` with new tasks
- Regenerate `Assets/Editor/PureSimTasksMenu.cs` for Unity menu items
- EditorWindow at `Tools/PureSim/Next Steps…` shows open tasks

---

## Key Files Reference

**Must-read for context:**
- `DUAL_CONSOLE_ARCHITECTURE.md` — Console separation rules with examples
- `ARCHITECTURE_DIAGRAM.md` — Visual component flow diagrams
- `VIRTUAL_FILESYSTEM_README.md` — Filesystem API and command catalog
- `commands.txt` — Tested command status (fully working vs experimental)
- `change log.MD` — Recent implementation history

**Critical Interfaces:**
- `ISerialCommand` → `Execute(SimulationState, string[], ISerialOutput)`
- `IConsoleCommand` → `Execute(SimulationState, string[], IConsoleOutput)`
- Attributes: `[SerialCommand("name")]`, `[ConsoleCommand("name")]`

**Entry Points:**
- `CommandProcessor.cs:ProcessCommand()` — Main command router
- `OS.cs:ProcessCommand()` — Static command processor with VFS integration
- `ConsoleRegistry.cs` — Reflection-based command discovery
- `VirtualFileSystemHandler.cs` — 50+ virtual filesystem command handlers

---

## Common Pitfalls

❌ **Don't:** Add operational commands (`mount`, `ls`) to Quake Console  
❌ **Don't:** Add simulator commands (`jump`, `inject`) to Serial Terminal  
❌ **Don't:** Invent command output — always mirror real logs/PDFs  
❌ **Don't:** Mutate state outside `SimulationState`/`WorkflowEngine` (new arch)  
❌ **Don't:** Ship NUnit assemblies in player builds (use Editor folders or guards)

✅ **Do:** Search `Docs/PuttyLogs/` before implementing command output  
✅ **Do:** Cite sources in code comments with file and line numbers  
✅ **Do:** Test both success and error paths (device not found, permission denied, etc.)  
✅ **Do:** Update `change log.MD` with detailed entry for every change  
✅ **Do:** Use `VirtualFileSystemHandler` for file operations where available

---

## Quick Command Lookup

**Serial Terminal (Purity OS):**
- File ops: `ls`, `cd`, `cat`, `touch`, `mkdir`, `rm`, `cp`, `mv`, `find`
- System info: `lsblk`, `df`, `free`, `uptime`, `hostname`, `ifconfig`, `uname`
- Pure CLIs: `purearray`, `purehw`, `puredrive`, `purenetwork`, `puresetup`, `pureboot`
- Mount: `mount /dev/sdb1 /mnt`, `umount /mnt`
- User: `sudo su`, `exit`, `whoami`, `pwd`

**Quake Console (Simulator):**
- Workflow: `jump <step>`, `steps`
- Faults: `inject <fault>`, `clearfault <fault>`, `faults`
- USB: `usb state inserted`, `usb state removed`
- Debug: `diag sim`, `diag power`, `diag ports`
- Util: `help`, `clear`, `history`

---

## Getting Started

**When asked to implement a new feature:**
1. Identify if it's Serial (operational) or Console (simulator control)
2. Search `Docs/` and `Docs/PuttyLogs/` for relevant examples
3. Check existing commands in target namespace for patterns
4. Implement with source citations in comments
5. Add error paths (device not found, not mounted, permission denied)
6. Write tests in appropriate `Assets/Tests/` folder
7. Update `change log.MD` with full details

**When debugging:**
1. Check `Logs/` folder for Unity console output
2. Review `CommandProcessor.cs` for command routing
3. Check `VirtualFileSystemHandler.cs` for filesystem command implementation
4. Verify state in `SimulationState` or legacy `Chassis` properties

**When reading existing code:**
- Legacy code is in root namespace or `CrimsofallTechnologies.ServerSimulator`
- New architecture is in `PureSim.*` namespaces
- Both coexist; prefer new architecture for additions
