# Dual Console Architecture

## Overview

PureSim implements a **dual-console architecture** with strict separation of concerns:

1. **Virtual Serial Terminal (PuTTY-like)** — Simulates a serial connection to Purity OS
2. **Quake-Style Developer Console** — Overlay for simulator control and debugging

**These two consoles have non-overlapping purposes and must never be conflated.**

---

## Virtual Serial Terminal (Operational Commands)

### Purpose
Simulates a real serial connection to Pure Storage equipment running **Purity** (Ubuntu-based OS). All operational commands that would exist on a real serial session belong here.

### Characteristics
- Emulates TTY device with line discipline, echo, and history
- Commands execute against simulated Purity OS
- Outputs **must mirror** real logs from `Docs/PuttyLogs/` and command PDFs
- Examples: `mount`, `umount`, `ls`, `purearray`, `purehw`, `puresetup`

### Implementation
- **Namespace:** `PureSim.Serial.*`
- **Location:** `Assets/Scripts/Serial/`
- **Interface:** `ISerialCommand`
- **Attribute:** `[SerialCommand("name")]`

### Example Command
```csharp
[SerialCommand("mount")]
public class MountCommand : ISerialCommand
{
    public void Execute(SimulationState sim, string[] args, ISerialOutput terminal)
    {
        // Source: Docs/PuttyLogs/putty2025-02-18.log L650-653
        if (!sim.IsUsbInserted() && device.Contains("sdb"))
        {
            terminal.WriteLine("mount: special device /dev/sdb1 does not exist");
            return;
        }
        // ... implementation mirrors real behavior
    }
}
```

---

## Quake-Style Developer Console (Simulator Control)

### Purpose
Overlay console for **simulator control only**. Controls workflow steps, fault injection, simulation diagnostics, and USB state toggling. **No operational/array commands here.**

### Characteristics
- Toggles with backquote/tilde (`~`)
- Overlay UI with scrollback, history, and autocomplete
- Commands manipulate `SimulationState` and `WorkflowEngine`
- Examples: `jump`, `steps`, `inject`, `clearfault`, `usb state`

### Implementation
- **Namespace:** `PureSim.Console.*`
- **Location:** `Assets/Scripts/Console/`
- **Interface:** `IConsoleCommand`
- **Attribute:** `[ConsoleCommand("name")]`

### Example Command
```csharp
[ConsoleCommand("jump")]
public class JumpCommand : IConsoleCommand
{
    public void Execute(SimulationState sim, string[] args, IConsoleOutput output)
    {
        if (workflowEngine.CanJumpToStep(stepId, sim, out var failed))
        {
            workflowEngine.JumpToStep(stepId, sim);
            output.WriteSuccess($"Jumped to step: {stepId}");
        }
        else
        {
            output.WriteError("Cannot jump - missing preconditions:");
            // ... report failed preconditions
        }
    }
}
```

---

## Command Separation Rules

### Console Commands (Simulator Control)
✅ **Allowed:**
- Workflow control: `jump`, `steps`
- Fault injection: `inject`, `clearfault`, `faults`
- State control: `usb state inserted`, `usb state removed`
- Diagnostics: `diag sim`, `diag power`, `diag ports`
- Meta: `help`, `clear`, `history`

❌ **Forbidden:**
- Any command that would exist on real Purity OS
- Examples: `mount`, `lsblk`, `purearray`, `purehw`, `apt`, `dmesg`

### Serial Commands (Operational)
✅ **Allowed:**
- Linux utilities: `ls`, `mount`, `umount`, `lsblk`, `dmesg`
- Purity commands: `purearray`, `purehw`, `puredrive`, `puresetup`
- Package management: `apt`, file operations, networking

❌ **Forbidden:**
- Simulator control commands
- Examples: `jump`, `inject`, `steps`, `usb state`

---

## Enforcement

Boundary separation is enforced through:

1. **Interface Separation**
   - `IConsoleCommand` vs `ISerialCommand` are distinct interfaces
   - Commands implement only one interface

2. **Namespace Separation**
   - Console: `PureSim.Console.*`
   - Serial: `PureSim.Serial.*`

3. **Attribute Separation**
   - Console: `[ConsoleCommand("name")]`
   - Serial: `[SerialCommand("name")]`

4. **Test Enforcement**
   - `ConsoleBoundaryTests.cs` verifies no cross-contamination
   - Tests fail if operational commands appear in console registry

---

## Testing

### Unit Tests
```bash
# Run in Unity Test Runner (Edit Mode)
Assets/Tests/Editor/Console/ConsoleParserTests.cs
Assets/Tests/Editor/ConsoleBoundaryTests.cs
```

### Golden Transcripts
Serial commands include golden transcript tests with expected output:
```
Assets/Tests/PlayMode/Serial/Golden_Lsblk_and_Mount.txt
```

Each test references source logs:
```
# Source: Docs/PuttyLogs/putty2025-02-18.log L648-653
Command: ls /dev/sd*1
Expected Output:
/dev/sda1  /dev/sdb1
```

---

## Adding New Commands

### Adding a Console Command (Simulator Control)

1. Create file in `Assets/Scripts/Console/Commands/`
2. Implement `IConsoleCommand` interface
3. Add `[ConsoleCommand("name")]` attribute
4. Implement command logic using `SimulationState` and `WorkflowEngine`
5. Command auto-registers via reflection

### Adding a Serial Command (Operational)

1. Create file in `Assets/Scripts/Serial/Commands/`
2. Implement `ISerialCommand` interface
3. Add `[SerialCommand("name")]` attribute
4. **Search logs/PDFs for authentic output**
5. Add source citations in code comments
6. Implement happy path and error paths
7. Add golden transcript test case

---

## Architecture Components

### Simulation Core
- **SimulationState** — Single source of truth (arrays, USB, faults, power)
- **WorkflowEngine** — Workflow steps with precondition guards
- **EventBus** — Pub/sub for state changes (future)
- **HardwareModel** — Typed components and relationships (future)

### Console System
- **ConsoleController** — Input, parsing, dispatch, history, autocomplete
- **ConsoleRegistry** — Reflection-based command discovery
- **ConsoleOutput** — Formatted output with colors and tables

### Serial System
- **SerialTerminal** — Terminal emulator with line discipline (future)
- **SerialDispatcher** — Command routing (future)
- **SerialOutput** — Terminal output interface

---

## Examples

### Console Session (Simulator Control)
```
> help
Available Console Commands:
  jump <step_id>
    Jump to a specific workflow step
  
  inject <fault_id> [description]
    Inject a fault into the simulation

> steps
Workflow Steps:
► [usb-detect] USB Detection
  ○ [device-select] Device Selection
  ✗ [mount] Mount USB (blocked: usb-inserted)

> usb state inserted
USB inserted
Device: /dev/sdb1

> jump mount
Jumped to step: Mount USB (mount)
```

### Serial Session (Operational)
```
puresetup@PCTFJ243000F4:~$ ls /dev/sd*1
/dev/sda1  /dev/sdb1

puresetup@PCTFJ243000F4:~$ mount /dev/sdb1 /mnt

puresetup@PCTFJ243000F4:~$ ls /mnt
purity_6.5.9_202412120507+7a7df3f70616.ppkg
purity_6.5.9_202412120507+7a7df3f70616.ppkg.sha1
```

---

## References

- **Repository Instructions:** `.github/copilot-instructions.md`
- **Change Log:** `change log.MD`
- **Authoritative Logs:** `Docs/PuttyLogs/*.log`
- **Command PDFs:** `Docs/*.pdf`
- **Design Document:** `Docs/Design.md`

---

## FAQ

**Q: Can I add a `reboot` command to the Console?**  
A: No. `reboot` is an operational command that exists on real systems. It belongs in the Serial Terminal. Console commands are for simulator control only (e.g., `jump`, `inject`).

**Q: Can I add a `debug` command to the Serial Terminal?**  
A: Only if it exists on real Purity systems. If you want simulator debugging, add it to Console instead (e.g., `diag sim`).

**Q: How do I test my new command?**  
A: Add unit tests to `Assets/Tests/Editor/` for logic, and golden transcript tests to `Assets/Tests/PlayMode/` for Serial commands.

**Q: Where do I find authentic command output?**  
A: Search `Docs/PuttyLogs/*.log` files and reference PDFs in `Docs/`. Always cite sources in comments.

**Q: The console command isn't showing up?**  
A: Ensure you have:
1. `[ConsoleCommand("name")]` attribute
2. `IConsoleCommand` interface implemented
3. Public parameterless constructor
4. Correct namespace (`PureSim.Console`)

---

*Last Updated: 2025-10-21*
