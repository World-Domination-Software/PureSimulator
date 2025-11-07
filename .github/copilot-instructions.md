# Pure Storage Simulator — Copilot Instructions

## Project Overview

**Pure Storage FlashArray Training Simulator** — Unity 6 (C#) 3-D simulator for Pure Storage installation/operations training and AI certification. Users manipulate 3-D hardware (cables, drives, controllers) and interact via two distinct terminal interfaces for hands-on training and examination.

**Core Stack:** Unity 6, C# 10+, nullable enabled  
**Key Namespaces:** `CrimsofallTechnologies.ServerSimulator` (legacy), `PureSim.{Console|Serial|Simulation}` (new architecture)

**Purpose:** Training & certification platform combining:
- Interactive 3D hardware assembly/manipulation
- Realistic CLI simulation matching Purity OS
- AI-powered training and examination system
- Documented procedures from Pure's official guides

---

## Prime Directive

**Always ground answers and generated code in our repo's authoritative resources.**  

Search **all** `Docs/**` and `PureResources/**` paths (including PDFs) before proposing behavior or emitting text. We have:
- **15+ PDFs** that detail the hardware (arrays, shelves, controllers, PSUs, SFPs, NICs, cables) and physical procedures
- **CLI reference PDFs** that enumerate the **commands to interact with the equipment** (Purity + host/Linux tooling)
- **PuttyLogs/** with realistic outputs from real installs and upgrades
- **Design.md** with project architecture and AI certification mode details

When simulating CLI/console output, **mirror the closest real snippet** you find and cite the file(s)/page(s)/line(s) in code comments.  
If a PDF is the source, cite it as `// Source: Docs/<file>.pdf p.<page>` (and page range if helpful).  
If something is ambiguous or you cannot parse a PDF, generate a **clearly marked placeholder** and add:  
`// TODO: replace with authoritative snippet from <pdf or log> p.<page> or <file>:<lines>`

---

## Current Implementation Status (READ FIRST!)

**CRITICAL: TWO separate console systems exist** with different purposes and implementations:

### 1. ✅ WORKING: Virtual Serial Terminal (PuTTY-like) — Main User Interface
- **Location:** `Assets/Scripts/CommandProcessor.cs`, `OS.cs`, `VirtualFileSystemHandler.cs`
- **UI:** Unity UI `Text` + `InputField` (already in scene, fully working)
- **Purpose:** Simulates serial connection to **Purity OS** — the MAIN operational interface
- **Commands:** All operational commands (ls, mount, cat, purearray, purehw, puredrive, etc.)
- **Pipeline:** `CommandProcessor` → `OS.ProcessCommand()` → `VirtualFileSystemHandler` → Output
- **Status:** FULLY IMPLEMENTED AND INTEGRATED — This is what users interact with!
- **Integration:** When adding operational commands, add to `OS.cs` or `VirtualFileSystemHandler.cs`

### 2. ⚠️ NEW BUT NOT FULLY CONNECTED: Quake-Style Developer Console — Optional Trainer Overlay
- **Location:** `Assets/Scripts/Console/ConsoleController.cs` and `Commands/*.cs`
- **UI:** TextMeshPro (TMP_InputField, TMP_Text) — NOT YET ADDED TO SCENE
- **Purpose:** Simulator control overlay for trainers/developers (toggle with ` key)
- **Commands:** `jump <step>`, `inject fault`, `usb state`, `steps`, `help`, `clear`
- **Pipeline:** `ConsoleController` → `ConsoleRegistry` → `[ConsoleCommand]` handlers → `SimulationState`/`WorkflowEngine`
- **Status:** CODE EXISTS but needs UI scene setup (optional feature for later)
- **Integration:** When adding simulator control, use `Assets/Scripts/Console/Commands/*.cs` with `[ConsoleCommand]` attribute

**DO NOT create new `Assets/Scripts/Serial/` commands for now** — Use `OS.cs` for operational commands unless refactoring to new architecture.

---

## Dual Console Architecture (STRICT SEPARATION)

### Simulator Modality (3-D + Dual Consoles)

This is a **full 3-D, dynamically manipulable model** of Pure hardware. Users can (un)seat components, re-cable, power cycle, and observe state changes in real time. The 3-D scene is authoritative for *visualization*; all mutations go through the simulation state/engine.

We expose **two distinct consoles** with non-overlapping purposes:

#### A) Virtual Serial Terminal (PuTTY-like, the "real" session)

**Purpose:** Execute operational commands an engineer runs over serial against **Purity** (Ubuntu-based) and array CLIs.

**Pipeline:** keystrokes → terminal emulator → tokenizer → **SerialParser** → **SerialDispatcher** → **SerialCommand** → **TerminalOutput**

**Behavior rules:**
- **Authoritative Output:** Prior to emitting text, **locate and mirror** the closest log/transcript/PDF snippet in `Docs/PuttyLogs/` or `Docs/*.pdf`
- Implement common error modes (device not found, permission denied/locked, already mounted, mismatch, not present)
- **No simulator control** here (no `jump`, `inject`, `steps`, etc.)

**Representative commands:**  
`lsblk`, `mount <dev> <path>`, `umount <path>`, USB presence checks, array/CLI diagnostics (`purearray`, `purehw`, `puredrive`, `purenetwork`, `puresetup`, `pureboot`), cabling/port summaries, health checks — all grounded in sources above.

**Current Implementation:**
- Namespace: `PureSim.Serial.*` (new) or root/`CrimsofallTechnologies.ServerSimulator` (legacy)
- Paths: `Assets/Scripts/Serial/Commands/*.cs` (new) or `Assets/Scripts/OS.cs`, `VirtualFileSystemHandler.cs` (legacy)
- Attribute: `[SerialCommand("name")]` (new) or method in `OS.cs` (legacy)

#### B) Quake-Style Developer Console (Simulator control)

**Purpose:** Control and introspect the **simulation** (not the array OS).

**Pipeline:** input → tokenizer → **ConsoleParser** → **ConsoleDispatcher** → **ConsoleCommand** → **ConsoleOutput** (overlay UI)

**Allowed ConsoleCommand handlers:**  
`jump <step-id>`, `steps`, `faults`, `inject <fault-id> [...]`, `clearfault <fault-id>`, `usb state [inserted|removed]`, `diag sim`, `diag power`, `diag ports`, `help`, `history`, `clear`

**Forbidden:** Any command available on a real serial session (`lsblk`, `purearray list`, `apt`, `mount`, `dmesg`, etc.)

**Current Implementation:**
- Namespace: `PureSim.Console.*`
- Paths: `Assets/Scripts/Console/Commands/*.cs`
- Attribute: `[ConsoleCommand("name")]`

### Command Separation Checklist

✅ **Serial Terminal:** Operational commands engineers run on Purity OS  
✅ **Quake Console:** Simulator control (workflow, faults, USB injection)  
❌ **Never:** Mix these two — they have zero overlap in commands

---

## Architecture (Authoritative)

### 3-D Simulation Core

- **SimulationState** — single source of truth for arrays, controllers, shelves, ports, cables, power, media/USB, host devices. Serializable/checkpointable.
- **HardwareModel** — typed components & relationships; all mutations via explicit APIs.
- **EventBus** — pub/sub for state changes (hotswap, faults, power events).
- **WorkflowEngine** — installer/upgrade flows modeled as steps with pre/post conditions and guards (`CanJumpIn/Out`).
- **Persistence** — save/load checkpoints and scenarios; supports safe "jump to step".

### UI Surfaces

- **3-D Scene** — visualizes hardware and responds to SimulationState.
- **Virtual Serial Terminal (PuTTY-like)** — terminal emulator that **emulates a serial session to Purity**; realistic line discipline, echo, history, optional latency.
- **Quake-Style Developer Console** — overlay toggled by backquote/tilde for simulator control/debug (not a serial session).

### Separation of Concerns

- **Serial Terminal** routes through **SerialCommand** handlers (or `OS.cs`) that consult `Docs/PuttyLogs/` & `Docs/*.pdf` to render exact output.
- **Quake Console** routes through **ConsoleCommand** handlers that call **WorkflowEngine**, **FaultInjector**, and **SimulationState** helpers.

### Hardware Model (3-D Simulation)

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

### Virtual Filesystem Implementation

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

## Authoritative References

Before implementing behavior or text, search and cite:
- `Docs/*.pdf` (15+ PDFs: hardware descriptions, physical procedures, **CLI reference guide**)
- `Docs/PuttyLogs/*.log` (real session transcripts)
- `Docs/Design.md` (project architecture, AI certification mode)
- `PureResources/**` (additional reference materials)
- `commands.txt` (tested command status)
- Any additional `*.md` in repo with real outputs

**Rule:** When behavior is ambiguous, **mirror the referenced material exactly** (phrasing, spacing, ordering).

### Source Material Fidelity (CRITICAL)

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

## Workflow Steps (install/upgrade)

Canonical sequence (expand as needed):  
**USB detect → device select → mount → image validate → apply → controller swap → cabling verify → shelf add → health checks.**

- **Quake `jump`** can enter a step only if `CanJumpIn` returns true; when blocked, report failing preconditions clearly.
- The **Serial Terminal** is the **only** surface that executes the operational commands used by those steps (e.g., `mount`).

---

## Fault & Error Simulation

Provide injectable faults with realistic text, mirrored from logs/PDFs where possible:
- USB not inserted, wrong block device, cable missing/short, controller mismatch, PSU removed, shelf power off, wrong firmware image.
- Prefer exact phrasing from sources and cite them in comments.

---

## Developer Workflows

### Adding New Serial Commands (Operational)

**For new architecture:**
1. Create `Assets/Scripts/Serial/Commands/MyCommand.cs`
2. Implement `ISerialCommand` interface
3. Add `[SerialCommand("mycommand")]` attribute
4. **Search `Docs/PuttyLogs/` for real output examples**
5. Mirror output exactly, cite source in comments
6. Test in `CommandProcessor` terminal

**For legacy integration (current approach):**
1. Add method to `OS.cs` or `VirtualFileSystemHandler.cs`
2. Follow existing patterns in those files
3. **Search `Docs/PuttyLogs/` for real output examples**
4. Mirror output exactly, cite source in comments
5. Test in `CommandProcessor` terminal

### Adding New Console Commands (Simulator Control)

1. Create `Assets/Scripts/Console/Commands/MyCommand.cs`
2. Implement `IConsoleCommand` interface
3. Add `[ConsoleCommand("mycommand")]` attribute
4. Use `SimulationState` and `WorkflowEngine` APIs only
5. Add unit test in `Assets/Tests/Editor/Console/`

### Mapping Real Commands to the Serial Terminal

For each real-world command present in the command PDF and logs:
1. Create a **SerialCommand** handler (or `OS.cs` method) with success and common error paths
2. **Mirror** format (ordering, spacing, headers) from the source material
3. In comments, **cite** the file(s)/page(s)/line(s) mirrored
4. Add a **golden transcript** test that runs the command(s) and compares terminal output byte-for-byte to the source

**Illustrative examples (replace with real citations):**
- `lsblk` — show USB as last block device variant per logs; include missing USB and wrong device path errors.  
  `// Source: Docs/PurityFA_6.4.5_FlashArray_CLIRefGuide.pdf p.42-43; Docs/PuttyLogs/putty2025-02-18.log L120-176`
- `mount /dev/sdX1 /mnt/usb` — success, "already mounted", "not a block device", "no media present".  
  `// Source: Docs/PuttyLogs/mount_scenarios.log L10-68`

### Testing

- **Editor Tests:** NUnit in `Assets/Tests/Editor/` (guard with `#if UNITY_INCLUDE_TESTS`)
- **PlayMode Tests:** `Assets/Tests/PlayMode/`
- **Golden Transcripts:** Run scripted **Serial Terminal** sessions and compare output to files under `Docs/PuttyLogs/` or snippets from `Docs/*.pdf` (transcribed for tests)
- **Run:** Unity Test Runner or `Assets/Editor/` scripts

### Building & Running

- **Scene:** Main training scene with `Chassis`, `CommandProcessor`, `UIManager`
- **Input:** `InputField` in Unity UI captures commands
- **Output:** `Text` component displays terminal output with color coding
- **Keyboard:** History navigation (↑↓), autocomplete (Tab), console toggle (~)

---

## Coding Standards

**Namespaces:**
- Legacy: `CrimsofallTechnologies.ServerSimulator` (most existing code)
- New: `PureSim.Console.*`, `PureSim.Serial.*`, `PureSim.Simulation.*`
- Folder structure mirrors namespaces

**Style:**
- Unity 6, C# 10+, nullable enabled, XML docs on public APIs
- Structured logging (no PII, no real endpoints)
- No per-frame allocations in hot paths
- Nullable reference types enabled

**Tests:** NUnit for Editor/PlayMode (ensure no NUnit assemblies ship in player builds)
- Keep test assemblies in Editor/Tests folders or guarded with `#if UNITY_INCLUDE_TESTS`

**Command Pattern (New Architecture):**
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

## Quake Console Requirements (Simulator Control)

- Toggle with backquote/tilde; scrollback, history, autocomplete
- Commands discovered via `[ConsoleCommand("verb")]` attribute; implement `IConsoleCommand`
- **All side-effects go through** WorkflowEngine/SimulationState; no direct state mutation in handlers
- Unit tests for parser, registry, and at least: `help`, `jump`, and one fault command

---

## Change Tracking & Documentation (REQUIRED)

### CHANGELOG.md — Append-Only Change Log

Maintain the repository's **`CHANGELOG.md`** file (note: uppercase, no spaces) at repo root as an append-only log.  

Each Copilot-assisted task **must append** a new section with:
- Title, date/time in **America/New_York**, author ("Copilot" or human), summary
- List of files changed (paths), rationale, acceptance criteria, test plan, and any citations used
- Follow with the **final console transcript(s)** if relevant

**Format:**
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

### Next Steps System

Create a developer-visible "Next Steps" system wired into Unity's **Tools** menu:
- A menu item `Tools/PureSim/Next Steps…` opening an EditorWindow that lists actionable UI/build tasks
- **Source of truth for tasks:** `.copilot/next_steps.json` (text file in repo; Copilot updates it whenever it proposes new UI/workflow tasks)
- Copilot must **regenerate** `Assets/Editor/PureSimTasksMenu.cs` so it contains a `[MenuItem("Tools/PureSim/Next Steps/<Task Name>")]` for **each open task** (code-gen is acceptable; the menu entries open the Next Steps window focused on that task)
- The EditorWindow supports: filter by status (Open/Done), copy instructions, and one-click "Create Stub" actions (for scripts/prefabs) when specified by tasks

---

## Integration with Existing Framework

**IMPORTANT:** When implementing new features, always integrate within the existing framework even if it's less than ideal. This helps developers continue to understand the code.

**Current Approach:**
- Use `OS.cs` and `VirtualFileSystemHandler.cs` for operational commands
- Extend existing methods rather than creating parallel systems
- Follow established patterns in `CommandProcessor.cs`

**If Major Issues Arise:**
- Document the issue clearly
- Propose the change in CHANGELOG.md with detailed rationale
- Get consensus before making breaking architectural changes
- Provide migration path from old to new approach

---

## Common Pitfalls

❌ **Don't:** Add operational commands (`mount`, `ls`) to Quake Console  
❌ **Don't:** Add simulator commands (`jump`, `inject`) to Serial Terminal  
❌ **Don't:** Invent command output — always mirror real logs/PDFs  
❌ **Don't:** Mutate state outside `SimulationState`/`WorkflowEngine` (new arch)  
❌ **Don't:** Ship NUnit assemblies in player builds (use Editor folders or guards)  
❌ **Don't:** Create parallel systems — integrate with existing framework

✅ **Do:** Search `Docs/PuttyLogs/` before implementing command output  
✅ **Do:** Cite sources in code comments with file and line numbers  
✅ **Do:** Test both success and error paths (device not found, permission denied, etc.)  
✅ **Do:** Update `CHANGELOG.md` with detailed entry for every change  
✅ **Do:** Use `VirtualFileSystemHandler` for file operations where available  
✅ **Do:** Integrate within existing framework unless there's a documented reason to change

---

## Quick Command Lookup

### Serial Terminal (Purity OS — Operational)

**File operations:**
- `ls`, `cd`, `cat`, `touch`, `mkdir`, `rm`, `cp`, `mv`, `find`

**System information:**
- `lsblk`, `df`, `free`, `uptime`, `hostname`, `ifconfig`, `uname`

**Pure CLIs:**
- `purearray`, `purehw`, `puredrive`, `purenetwork`, `puresetup`, `pureboot`
- `pureadm`, `purewes`, `puredb`, `pureport`, `purevol`, `pureversion`, `pureinstall`, `puremessage`, `puretune`

**Storage:**
- `mount /dev/sdb1 /mnt`, `umount /mnt`

**User/Auth:**
- `sudo su`, `exit`, `whoami`, `pwd`

**Network:**
- `ping`, `ssh`

**Diagnostics:**
- `dmesg`, `iobalance`, `stty`

### Quake Console (Simulator Control)

**Workflow:**
- `jump <step>`, `steps`

**Faults:**
- `inject <fault>`, `clearfault <fault>`, `faults`

**USB:**
- `usb state inserted`, `usb state removed`

**Debug:**
- `diag sim`, `diag power`, `diag ports`

**Utility:**
- `help`, `clear`, `history`

---

## Key Files Reference

**Must-read for context:**
- `DUAL_CONSOLE_ARCHITECTURE.md` — Console separation rules with examples
- `ARCHITECTURE_DIAGRAM.md` — Visual component flow diagrams
- `VIRTUAL_FILESYSTEM_README.md` — Filesystem API and command catalog
- `Docs/Design.md` — Project architecture and AI certification mode
- `commands.txt` — Tested command status (fully working vs experimental)
- `CHANGELOG.md` — Recent implementation history

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

## Reference Interfaces & Attributes (for consistency)

*(These are canonical signatures; generate real files if needed.)*

```csharp
// Assets/Scripts/Console/IConsoleCommand.cs
namespace PureSim.Console
{
    public interface IConsoleCommand
    {
        string Name { get; }
        string Synopsis { get; }
        IReadOnlyList<string> Parameters { get; }
        void Execute(SimulationState sim, string[] args, IConsoleOutput output);
    }
}

// Assets/Scripts/Console/ConsoleCommandAttribute.cs
using System;
namespace PureSim.Console
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ConsoleCommandAttribute : Attribute
    {
        public string Name { get; }
        public ConsoleCommandAttribute(string name) => Name = name;
    }
}

// Assets/Scripts/Serial/ISerialCommand.cs
namespace PureSim.Serial
{
    public interface ISerialCommand
    {
        string Name { get; }
        string Synopsis { get; }
        IReadOnlyList<string> Parameters { get; }
        void Execute(SimulationState sim, string[] args, ISerialOutput terminal);
    }
}

// Assets/Scripts/Serial/SerialCommandAttribute.cs
using System;
namespace PureSim.Serial
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class SerialCommandAttribute : Attribute
    {
        public string Name { get; }
        public SerialCommandAttribute(string name) => Name = name;
    }
}
```

---

## Review Checklist

- ✅ All simulated outputs **cite** the resource files or PDFs used
- ✅ No world mutations outside WorkflowEngine/SimulationState
- ✅ Serial vs Quake console responsibilities are **not mixed**
- ✅ New commands include help text, error paths, and tests
- ✅ Performance: no per-frame allocations in hot paths; overlay UI idle-cheap
- ✅ `CHANGELOG.md` updated with comprehensive entry
- ✅ Integrated with existing framework unless documented reason to change

---

## Task Pattern for Copilot

When asked to add/modify features, follow this template in your reply **and** in `CHANGELOG.md`:

1. **Summary** & acceptance criteria  
2. **Files to create/modify** (with paths)  
3. **References used** (filenames + page/lines)  
4. **Proposed diffs or code blocks**  
5. **Test plan** (unit + golden transcripts)  
6. **Next Steps** items to append to `.copilot/next_steps.json` (each with `id`, `title`, `description`, `type`, `status`)

---

## Getting Started

### When asked to implement a new feature:

1. Identify if it's Serial (operational) or Console (simulator control)
2. Search `Docs/` and `Docs/PuttyLogs/` for relevant examples
3. Check existing commands in target namespace for patterns
4. Decide: integrate with existing framework (OS.cs) or use new architecture (Serial/Commands/)
5. Implement with source citations in comments
6. Add error paths (device not found, not mounted, permission denied)
7. Write tests in appropriate `Assets/Tests/` folder
8. Update `CHANGELOG.md` with full details

### When debugging:

1. Check `Logs/` folder for Unity console output
2. Review `CommandProcessor.cs` for command routing
3. Check `VirtualFileSystemHandler.cs` for filesystem command implementation
4. Verify state in `SimulationState` or legacy `Chassis` properties

### When reading existing code:

- Legacy code is in root namespace or `CrimsofallTechnologies.ServerSimulator`
- New architecture is in `PureSim.*` namespaces
- Both coexist; prefer integrating with existing framework for now
- Use new architecture when it provides clear benefits and doesn't duplicate effort

---

## Scope & Non-Goals

We are building a Unity (C#) training simulator that reproduces Pure Storage installation/operations through a **3-D model** and a **Virtual Serial Terminal** whose outputs match real logs/PDFs, plus an **AI certification mode** for training and examination.

**Non-goals:** real device I/O, secrets, production credentials, external network calls.
