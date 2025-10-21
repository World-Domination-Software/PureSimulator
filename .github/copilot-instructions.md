# Pure Simulator — Repository Custom Instructions
*(Save this file as `.github/copilot-instructions.md` at the repo root.)*

## Prime Directive
**Always ground answers and generated code in our repo’s authoritative resources.**  
Search **all** `Resources/**` and `Docs/**` paths (including PDFs) before proposing behavior or emitting text. We have:
- **PDFs** that detail the hardware (arrays, shelves, controllers, PSUs, SFPs, NICs, cables) and physical procedures.
- **A PDF and other transcripts** that enumerate the **commands to interact with the equipment** (Purity + host/Linux tooling).
- **Logs / transcripts / text files** with realistic outputs from real installs and upgrades.

When simulating CLI/console output, **mirror the closest real snippet** you find and cite the file(s)/page(s)/line(s) in code comments.  
If a PDF is the source, cite it as `// Source: Docs/Hardware/<file>.pdf p.<page>` (and page range if helpful).  
If something is ambiguous or you cannot parse a PDF, generate a **clearly marked placeholder** and add:  
`// TODO: replace with authoritative snippet from <pdf or log> p.<page> or <file>:<lines>`

---

## Simulator Modality (3-D + Dual Consoles)
- This is a **full 3-D, dynamically manipulable model** of Pure hardware. Users can (un)seat components, re-cable, power cycle, and observe state changes in real time. The 3-D scene is authoritative for *visualization*; all mutations go through the simulation state/engine.
- We expose **two distinct consoles** with non-overlapping purposes:
  1) **Virtual Serial Terminal (PuTTY-like):** Simulates a serial connection to equipment running **Purity** (our Ubuntu-based OS).  
     **All operational commands live here** (host/Linux utilities, Purity/array CLIs). Outputs **must mirror** logs and the command PDF.
  2) **Quake-Style Developer Console:** An overlay for **simulator control only** (jump between workflow steps, inject faults, sim diagnostics, toggle USB presence). **No array/host commands here.**
- **Never conflate the two.** If a command would exist on a real serial session, put it in the **Serial Terminal**; if it controls the simulator itself, put it in the **Quake console**.

---

## Scope & Non-Goals
We are building a Unity (C#) training simulator that reproduces Pure Storage installation/operations through a **3-D model** and a **Virtual Serial Terminal** whose outputs match real logs/PDFs.  
**Non-goals:** real device I/O, secrets, production credentials, external network calls.

---

## Authoritative References
Before implementing behavior or text, search and cite:
- `Resources/**` (logs, transcripts, text snippets)
- `Docs/**.pdf` (hardware descriptions, physical procedures, **command catalog PDF**)
- Any additional `*.md` in repo with real outputs

**Rule:** When behavior is ambiguous, **mirror the referenced material exactly** (phrasing, spacing, ordering).

---

## Architecture (authoritative)
- **3-D Simulation Core**
  - **SimulationState** – single source of truth for arrays, controllers, shelves, ports, cables, power, media/USB, host devices. Serializable/checkpointable.
  - **HardwareModel** – typed components & relationships; all mutations via explicit APIs.
  - **EventBus** – pub/sub for state changes (hotswap, faults, power events).
  - **WorkflowEngine** – installer/upgrade flows modeled as steps with pre/post conditions and guards (`CanJumpIn/Out`).
  - **Persistence** – save/load checkpoints and scenarios; supports safe “jump to step”.
- **UI Surfaces**
  - **3-D Scene** – visualizes hardware and responds to SimulationState.
  - **Virtual Serial Terminal (PuTTY-like)** – terminal emulator that **emulates a serial session to Purity**; realistic line discipline, echo, history, optional latency.
  - **Quake-Style Developer Console** – overlay toggled by backquote/tilde for simulator control/debug (not a serial session).
- **Separation of Concerns**
  - **Serial Terminal** routes through **SerialCommand** handlers that consult `Resources/**` & `Docs/**.pdf` to render exact output.
  - **Quake Console** routes through **ConsoleCommand** handlers that call **WorkflowEngine**, **FaultInjector**, and **SimulationState** helpers.

---

## Dual Console Contracts (STRICT)
### A) Virtual Serial Terminal (PuTTY-like, the “real” session)
- **Purpose:** Execute operational commands an engineer runs over serial against **Purity** (Ubuntu-based) and array CLIs.
- **Pipeline:** keystrokes → terminal emulator → tokenizer → **SerialParser** → **SerialDispatcher** → **SerialCommand** → **TerminalOutput**.
- **Behavior rules:**
  - **Authoritative Output:** Prior to emitting text, **locate and mirror** the closest log/transcript/PDF snippet in `Resources/**` or `Docs/**.pdf`.
  - Implement common error modes (device not found, permission denied/locked, already mounted, mismatch, not present).
  - **No simulator control** here (no `jump`, `inject`, `steps`, etc.).
- **Representative SerialCommand handlers:**  
  `lsblk`, `mount <dev> <path>`, `umount <path>`, USB presence checks, array/CLI diagnostics (`pure*`), cabling/port summaries, health checks — all grounded in sources above.

### B) Quake-Style Developer Console (Simulator control)
- **Purpose:** Control and introspect the **simulation** (not the array OS).
- **Pipeline:** input → tokenizer → **ConsoleParser** → **ConsoleDispatcher** → **ConsoleCommand** → **ConsoleOutput** (overlay UI).
- **Allowed ConsoleCommand handlers:**  
  `jump <step-id>`, `steps`, `faults`, `inject <fault-id> [...]`, `clearfault <fault-id>`, `usb state [inserted|removed]`, `diag sim`, `diag power`, `diag ports`, `help`, `history`, `clear`.
- **Forbidden:** Any command available on a real serial session (`lsblk`, `purearray list`, `apt`, `mount`, `dmesg`, etc.).

---

## Command Namespacing & File Layout
- **Serial terminal commands (real ops)**  
  Namespace: `PureSim.Serial.*`  
  Paths: `Assets/Scripts/Serial/Commands/*.cs`  
  Attribute: `[SerialCommand("name")]`
- **Quake console commands (sim control)**  
  Namespace: `PureSim.Console.*`  
  Paths: `Assets/Scripts/Console/Commands/*.cs`  
  Attribute: `[ConsoleCommand("name")]`

---

## Mapping Real Commands to the Serial Terminal
For each real-world command present in the command PDF and logs:
1. Create a **SerialCommand** handler with success and common error paths.  
2. **Mirror** format (ordering, spacing, headers) from the source material.  
3. In comments, **cite** the file(s)/page(s)/line(s) mirrored.  
4. Add a **golden transcript** test that runs the command(s) and compares terminal output byte-for-byte to the source.

**Illustrative examples (replace with real citations):**
- `lsblk` – show USB as last block device variant per logs; include missing USB and wrong device path errors.  
  `// Source: Docs/Commands/Purity-Host-Reference.pdf p.42-43; Resources/logs/install_2024-08-14.txt L120-176`
- `mount /dev/sdX1 /mnt/usb` – success, “already mounted”, “not a block device”, “no media present”.  
  `// Source: Resources/transcripts/mount_scenarios.txt L10-68`

---

## Quake Console Requirements (Simulator Control)
- Toggle with backquote/tilde; scrollback, history, autocomplete.
- Commands discovered via `[ConsoleCommand("verb")]` attribute; implement `IConsoleCommand`.
- **All side-effects go through** WorkflowEngine/SimulationState; no direct state mutation in handlers.
- Unit tests for parser, registry, and at least: `help`, `jump`, and one fault command.

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

## Coding Standards
- Unity 6, C# 10+, nullable enabled, XML docs on public APIs.
- Namespaces: `PureSim.{Area}`. Folder layout mirrors namespaces.
- Logging: structured, no PII, no real endpoints.
- **Tests:** NUnit for Editor/PlayMode (ensure no NUnit assemblies ship in player builds).  
  - Keep test assemblies in Editor/Tests folders or guarded with `#if UNITY_INCLUDE_TESTS`.

---

## Build & Test
- Provide editor scripts to run play-mode tests.  
- **Golden transcript** tests: run scripted **Serial Terminal** sessions and compare output to files under `Resources/**` or snippets from `Docs/**.pdf` (transcribed for tests).  
- Ensure automated tests for both consoles and core workflow jumps.

---

## Changelog & “Next Steps” Process (REQUIRED)
- Maintain a single append-only file at repo root named **`change log.MD`** (intentional spacing and case).  
  - Each Copilot-assisted task **must append** a new section with:
    - Title, date/time in **America/New_York**, author (“Copilot” or human), summary.
    - List of files changed (paths), rationale, acceptance criteria, test plan, and any citations used.
    - Follow with the **final console transcript(s)** if relevant.
- Create a developer-visible “Next Steps” system wired into Unity’s **Tools** menu:
  - A menu item `Tools/PureSim/Next Steps…` opening an EditorWindow that lists actionable UI/build tasks.
  - **Source of truth for tasks:** `.copilot/next_steps.json` (text file in repo; Copilot updates it whenever it proposes new UI/workflow tasks).
  - Copilot must **regenerate** `Assets/Editor/PureSimTasksMenu.cs` so it contains a `[MenuItem("Tools/PureSim/Next Steps/<Task Name>")]` for **each open task** (code-gen is acceptable; the menu entries open the Next Steps window focused on that task).
  - The EditorWindow supports: filter by status (Open/Done), copy instructions, and one-click “Create Stub” actions (for scripts/prefabs) when specified by tasks.

---

## Review Checklist
- ✅ All simulated outputs **cite** the resource files or PDFs used.  
- ✅ No world mutations outside WorkflowEngine/SimulationState.  
- ✅ Serial vs Quake console responsibilities are **not mixed**.  
- ✅ New commands include help text, error paths, and tests.  
- ✅ Performance: no per-frame allocations in hot paths; overlay UI idle-cheap.  
- ✅ `change log.MD` updated with comprehensive entry.

---

## Task Pattern for Copilot
When asked to add/modify features, follow this template in your reply **and** in `change log.MD`:
1) **Summary** & acceptance criteria  
2) **Files to create/modify** (with paths)  
3) **References used** (filenames + page/lines)  
4) **Proposed diffs or code blocks**  
5) **Test plan** (unit + golden transcripts)  
6) **Next Steps** items to append to `.copilot/next_steps.json` (each with `id`, `title`, `description`, `type`, `status`)

---

## Reference Interfaces & Attributes (for consistency)
*(These are canonical signatures; generate real files in the tasks below.)*

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
