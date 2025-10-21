# PureSim Dual Console Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         PureSim Unity Application                        │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                  ┌─────────────────┴─────────────────┐
                  │                                   │
    ┌─────────────▼──────────────┐    ┌──────────────▼─────────────┐
    │  Quake-Style Dev Console   │    │  Virtual Serial Terminal   │
    │   (Simulator Control)      │    │   (Operational Commands)   │
    └─────────────┬──────────────┘    └──────────────┬─────────────┘
                  │                                   │
    ┌─────────────▼──────────────┐    ┌──────────────▼─────────────┐
    │   ConsoleController        │    │   SerialTerminal           │
    │   - Input buffer           │    │   - TTY emulation          │
    │   - History (↑↓)           │    │   - Line discipline        │
    │   - Autocomplete (Tab)     │    │   - Echo & prompt          │
    │   - Toggle (~)             │    │   - Command history        │
    └─────────────┬──────────────┘    └──────────────┬─────────────┘
                  │                                   │
    ┌─────────────▼──────────────┐    ┌──────────────▼─────────────┐
    │   ConsoleRegistry          │    │   SerialDispatcher         │
    │   (Reflection Discovery)   │    │   (Command Routing)        │
    └─────────────┬──────────────┘    └──────────────┬─────────────┘
                  │                                   │
                  │                                   │
    ┌─────────────▼──────────────┐    ┌──────────────▼─────────────┐
    │   [ConsoleCommand]         │    │   [SerialCommand]          │
    │   Attribute Discovery      │    │   Attribute Discovery      │
    └─────────────┬──────────────┘    └──────────────┬─────────────┘
                  │                                   │
         ┌────────┴────────┐                 ┌───────┴────────┐
         │                 │                 │                │
    ┌────▼─────┐  ┌───────▼──────┐    ┌────▼─────┐  ┌──────▼──────┐
    │  Help    │  │    Jump      │    │  Lsblk   │  │   Mount     │
    │  Clear   │  │    Steps     │    │  (ls)    │  │   Umount    │
    │  Faults  │  │    Inject    │    └──────────┘  └─────────────┘
    │  Usb     │  │  ClearFault  │
    └──────────┘  └──────────────┘
         │                 │                        │
         └────────┬────────┘                        │
                  │                                 │
    ┌─────────────▼──────────────┐    ┌────────────▼─────────────┐
    │   SimulationState          │◄───│  Logs/PDFs (Source)      │
    │   - USB state              │    │  - Docs/PuttyLogs/*.log  │
    │   - Fault tracking         │    │  - Docs/*.pdf            │
    │   - Power management       │    └──────────────────────────┘
    │   - Event system           │
    └─────────────┬──────────────┘
                  │
    ┌─────────────▼──────────────┐
    │   WorkflowEngine           │
    │   - Step management        │
    │   - Precondition guards    │
    │   - CanJumpIn/CanJumpOut   │
    │   - Event notifications    │
    └────────────────────────────┘
```

## Component Responsibilities

### Console System (Left Path)
**Purpose:** Simulator control and debugging  
**Input:** Backquote (~) toggles overlay  
**Commands:** Workflow control, fault injection, state changes  
**Output:** Formatted text with colors and tables  
**Namespace:** `PureSim.Console.*`

### Serial System (Right Path)
**Purpose:** Operational command execution  
**Input:** Serial connection simulation  
**Commands:** Linux utilities, Purity commands  
**Output:** Authentic output from real logs  
**Namespace:** `PureSim.Serial.*`

### Simulation Core (Bottom)
**Purpose:** Single source of truth  
**State:** USB, faults, power, hardware  
**Workflow:** Steps with preconditions  
**Events:** Pub/sub notifications

---

## Data Flow Examples

### Console Command Flow
```
User types "jump mount" in Console
          ↓
ConsoleController parses input
          ↓
ConsoleRegistry finds JumpCommand
          ↓
JumpCommand.Execute(sim, args, output)
          ↓
Checks WorkflowEngine.CanJumpToStep()
          ↓
Validates preconditions in SimulationState
          ↓
Either: Jumps to step (success)
    Or: Reports failed preconditions (error)
          ↓
Output written to ConsoleOutput
          ↓
User sees formatted result
```

### Serial Command Flow
```
User types "mount /dev/sdb1 /mnt" in Terminal
          ↓
SerialTerminal processes input
          ↓
SerialDispatcher finds MountCommand
          ↓
MountCommand.Execute(sim, args, terminal)
          ↓
Checks SimulationState.IsUsbInserted()
          ↓
Checks SimulationState.IsUsbMounted()
          ↓
Validates device path exists
          ↓
Either: Mounts successfully (silent)
    Or: Writes error message (mirrors real Linux)
          ↓
Updates SimulationState if successful
          ↓
Terminal shows output (or none)
```

---

## Boundary Enforcement

### Interface Level
```
IConsoleCommand  ←→  ✗  ←→  ISerialCommand
(Simulator)              (Operational)
```

### Namespace Level
```
PureSim.Console.*  ←→  ✗  ←→  PureSim.Serial.*
```

### Attribute Level
```
[ConsoleCommand]  ←→  ✗  ←→  [SerialCommand]
```

### Test Level
```
ConsoleBoundaryTests enforces separation at compile time
```

---

## Command Registration Flow

### Console Commands
```
1. Create class implementing IConsoleCommand
2. Add [ConsoleCommand("name")] attribute
3. Place in PureSim.Console namespace
4. ConsoleRegistry discovers via reflection
5. Command auto-registers on startup
6. Available via "help" command
```

### Serial Commands
```
1. Create class implementing ISerialCommand
2. Add [SerialCommand("name")] attribute
3. Search Docs/PuttyLogs for authentic output
4. Cite sources in comments
5. Implement happy path + error paths
6. Add to PureSim.Serial namespace
7. Command auto-registers (when dispatcher ready)
```

---

## State Management Flow

```
                    ┌─────────────────────┐
                    │  SimulationState    │
                    │  (Single Source)    │
                    └──────────┬──────────┘
                               │
                 ┌─────────────┼─────────────┐
                 │             │             │
        ┌────────▼────┐  ┌────▼─────┐  ┌───▼──────┐
        │ USB State   │  │ Faults   │  │  Power   │
        │ - Inserted  │  │ - Active │  │  - CT0   │
        │ - Mounted   │  │ - Inject │  │  - CT1   │
        │ - Device    │  │ - Clear  │  │  - SH*   │
        └─────────────┘  └──────────┘  └──────────┘
                 │             │             │
                 └─────────────┼─────────────┘
                               │
                    ┌──────────▼──────────┐
                    │   Event System      │
                    │   (Notifications)   │
                    └─────────────────────┘
```

---

## Workflow Engine Structure

```
┌──────────────────────────────────────────────────────┐
│              Workflow Engine                         │
├──────────────────────────────────────────────────────┤
│  Steps:                                              │
│  1. [usb-detect]     USB Detection                   │
│  2. [device-select]  Device Selection                │
│  3. [mount]          Mount USB          ← Current    │
│  4. [validate]       Image Validation                │
│  5. [apply]          Apply Firmware                  │
│  6. [controller-swap] Controller Swap                │
│  7. [health-check]   Health Check                    │
├──────────────────────────────────────────────────────┤
│  Guards:                                             │
│  - CanJumpIn(step, state)  → bool                    │
│  - CanJumpOut(step, state) → bool                    │
│  - GetFailedPreconditions(step, state) → list        │
└──────────────────────────────────────────────────────┘
```

---

## Testing Architecture

```
┌─────────────────────────────────────────────┐
│          Test Infrastructure                 │
├─────────────────────────────────────────────┤
│  Edit Mode Tests:                           │
│  - ConsoleParserTests                       │
│  - ConsoleBoundaryTests                     │
│  - Registry discovery tests                 │
│  - Output formatting tests                  │
├─────────────────────────────────────────────┤
│  Play Mode Tests:                           │
│  - Golden transcript tests (future)         │
│  - Integration tests (future)               │
│  - End-to-end workflow tests (future)       │
├─────────────────────────────────────────────┤
│  Golden Transcripts:                        │
│  - Happy path: USB detect → mount           │
│  - Fault path: USB not inserted             │
│  - Error paths: All command errors          │
└─────────────────────────────────────────────┘
```

---

## File Organization

```
Assets/
├── Editor/
│   ├── PureSimNextStepsWindow.cs      # Task management UI
│   ├── PureSimTasksMenu.cs            # Generated menu items
│   └── PureSimTasksMenuGenerator.cs   # Menu generation
├── Scripts/
│   ├── Console/                       # Simulator control
│   │   ├── Commands/                  # 8 console commands
│   │   ├── IConsoleCommand.cs
│   │   ├── ConsoleController.cs
│   │   └── ConsoleRegistry.cs
│   ├── Serial/                        # Operational commands
│   │   ├── Commands/                  # 3 serial commands
│   │   ├── ISerialCommand.cs
│   │   └── SerialCommandAttribute.cs
│   └── Simulation/                    # State management
│       ├── SimulationState.cs
│       └── WorkflowEngine.cs
└── Tests/
    ├── Editor/                        # Edit mode tests
    │   └── Console/
    │       ├── ConsoleParserTests.cs
    │       └── ConsoleBoundaryTests.cs
    └── PlayMode/                      # Play mode tests
        └── Serial/
            └── Golden_Lsblk_and_Mount.txt
```

---

*This diagram represents the implemented architecture as of 2025-10-21.*
