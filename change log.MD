# Pure Simulator — Change Log

This is an append-only changelog documenting all Copilot-assisted and manual changes to the repository.

---

## 2025-10-21 16:59:54 EDT — Initial Repository Plumbing Setup
**Author:** Copilot  
**Task:** Task 1 — Repo plumbing: Changelog + Next Steps Editor tooling

### Summary
Created foundational infrastructure for change tracking and task management within Unity Editor. This includes an append-only changelog, a JSON-based task tracking system, and Unity Editor tools for managing development tasks.

### Files Created
- `/change log.MD` — This append-only changelog file
- `/.copilot/next_steps.json` — JSON array of task objects for tracking development work
- `/Assets/Editor/PureSimNextStepsWindow.cs` — EditorWindow for viewing and managing tasks
- `/Assets/Editor/PureSimTasksMenu.cs` — Code-generated menu items for quick task access

### Acceptance Criteria
✅ Changelog file created with proper structure and initial entry  
⏳ Next Steps JSON file created with task tracking schema  
⏳ EditorWindow implemented for task management with UI features  
⏳ Menu generation system implemented for Tools/PureSim menu  

### Test Plan
- Manual verification: Open Unity Editor and verify Tools/PureSim menu exists
- Verify EditorWindow opens and displays tasks from JSON file
- Test task filtering, copying instructions, and stub creation features
- Verify menu items update when tasks change status

### References Used
- Repository structure exploration
- `.github/copilot-instructions.md` — Architecture guidance and coding standards
- Unity 6 EditorWindow API documentation

### Next Steps
- Complete EditorWindow implementation
- Implement menu code generation system
- Begin Quake-Style Developer Console implementation

---

## 2025-10-21 17:30:00 EDT — Quake Console & Serial Command Infrastructure
**Author:** Copilot  
**Task:** Task 2 & 3 — Quake Console + Serial Terminal Command Stubs

### Summary
Implemented complete dual-console architecture with strict separation between simulator control (Quake console) and operational commands (Serial terminal). Created core simulation infrastructure, console command system with reflection-based discovery, and serial command stubs with authentic output mirrored from real Purity logs.

### Files Created

**Simulation Core:**
- `/Assets/Scripts/Simulation/SimulationState.cs` — Single source of truth for simulation state (USB, faults, power)
- `/Assets/Scripts/Simulation/WorkflowEngine.cs` — Workflow step management with precondition guards

**Console System (Simulator Control):**
- `/Assets/Scripts/Console/IConsoleCommand.cs` — Interface for console commands
- `/Assets/Scripts/Console/ConsoleCommandAttribute.cs` — Attribute for command discovery
- `/Assets/Scripts/Console/IConsoleOutput.cs` — Output interface
- `/Assets/Scripts/Console/ConsoleOutput.cs` — Output implementation with color and tables
- `/Assets/Scripts/Console/ConsoleRegistry.cs` — Reflection-based command discovery
- `/Assets/Scripts/Console/ConsoleController.cs` — Main controller with input, history, autocomplete

**Console Commands:**
- `/Assets/Scripts/Console/Commands/HelpCommand.cs` — List and describe commands
- `/Assets/Scripts/Console/Commands/JumpCommand.cs` — Jump to workflow step with precondition validation
- `/Assets/Scripts/Console/Commands/StepsCommand.cs` — List all workflow steps and status
- `/Assets/Scripts/Console/Commands/FaultsCommand.cs` — List active faults
- `/Assets/Scripts/Console/Commands/InjectCommand.cs` — Inject faults for testing
- `/Assets/Scripts/Console/Commands/ClearFaultCommand.cs` — Clear faults
- `/Assets/Scripts/Console/Commands/UsbStateCommand.cs` — Control USB insertion state
- `/Assets/Scripts/Console/Commands/ClearCommand.cs` — Clear console output

**Serial System (Operational Commands):**
- `/Assets/Scripts/Serial/ISerialCommand.cs` — Interface for serial commands
- `/Assets/Scripts/Serial/SerialCommandAttribute.cs` — Attribute for command discovery
- `/Assets/Scripts/Serial/ISerialOutput.cs` — Terminal output interface

**Serial Commands:**
- `/Assets/Scripts/Serial/Commands/LsblkCommand.cs` — List block devices (ls /dev/sd*)
- `/Assets/Scripts/Serial/Commands/MountCommand.cs` — Mount filesystems with error paths
- `/Assets/Scripts/Serial/Commands/UmountCommand.cs` — Unmount filesystems

**Tests:**
- `/Assets/Tests/Editor/Console/ConsoleParserTests.cs` — Unit tests for console parsing and registry
- `/Assets/Tests/Editor/ConsoleBoundaryTests.cs` — Tests ensuring Console/Serial separation
- `/Assets/Tests/PlayMode/Serial/Golden_Lsblk_and_Mount.txt` — Golden transcript for serial commands
- `/Assets/Tests/Editor/PureSim.Tests.Editor.asmdef` — Editor test assembly
- `/Assets/Tests/PlayMode/PureSim.Tests.PlayMode.asmdef` — PlayMode test assembly

**Assembly Definitions:**
- `/Assets/Scripts/PureSim.Runtime.asmdef` — Main runtime assembly

### Acceptance Criteria
✅ Dual console architecture implemented with strict separation  
✅ Console commands control simulator only (no operational commands)  
✅ Serial commands mirror real Purity output from logs/PDFs  
✅ Reflection-based command discovery system working  
✅ Console controller with history, autocomplete, and toggle support  
✅ Workflow engine with step preconditions and jump guards  
✅ USB state management integrated  
✅ Fault injection system implemented  
✅ Unit tests for console and boundary separation  
✅ Golden transcript files created with real log citations  
⏳ ConsoleOverlayUI prefab (requires Unity Editor)  

### Test Plan
- Run Console unit tests in Unity Test Runner (Edit Mode)
- Run boundary tests to verify Console/Serial separation
- Test console command discovery finds all commands
- Test console output formatting (plain text, tables, colors)
- Test workflow step jumping with precondition validation
- Test serial mount command with all error paths:
  - Success (clean mount)
  - Success (with unclean filesystem warning)
  - Error: USB not inserted
  - Error: Already mounted
  - Error: Wrong device path
  - Error: Not a block device

### References Used
- **Docs/PuttyLogs/putty2025-02-18.log** L648-653 — Device listing and mount commands
- **Docs/PuttyLogs/putty2025-02-18.log** L2229, L4046 — Clean mount examples
- **Docs/PuttyLogs/putty2025-03-03.log** — Additional device listing patterns
- **.github/copilot-instructions.md** — Architecture and coding standards
- Unity 6 EditorWindow and reflection APIs

### Design Decisions
1. **Strict Interface Separation**: IConsoleCommand vs ISerialCommand ensures no cross-contamination
2. **Reflection-Based Discovery**: Commands auto-register via attributes, no manual registration needed
3. **Authentic Output**: Serial commands mirror exact phrasing from real logs, including spacing and error messages
4. **Simulation State**: Single source of truth pattern for all hardware/USB/fault state
5. **Precondition Guards**: WorkflowEngine validates preconditions before allowing step jumps
6. **Quote Support**: Console parser respects quotes for arguments with spaces

### Known Limitations
- ConsoleOverlayUI prefab needs to be created in Unity Editor (scripted approach not feasible for UI)
- PlayMode golden transcript tests need custom test harness (deferred)
- Some workflow preconditions are simplified stubs (e.g., firmware-applied, health-verified)

---
