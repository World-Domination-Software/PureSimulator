# Copilot Context: Pure Storage Simulator
# Copilot Purpose: Define the architecture, features, and development goals for a 3D Pure Storage training simulator.
# Copilot Tags: unity, csharp, simulator, cli, linux, purestorage, training, ai, exam, 3d, hardware

# 🧠 Pure Storage Simulator — Design Document

**Project Owner:** World Domination Software (WDS)  
**Simulator Name:** Pure Storage Simulator  
**Purpose:** Training & certification platform for Pure Storage engineers  
**Platforms:** Unity (C#), Linux CLI Simulation (Python or C# backend)  
**Version:** Draft v1.0  
**Last Updated:** October 2025  

---

## 1. Overview

The **Pure Storage Simulator** is a **3D, interactive training and certification environment** designed to replicate real-world installation, upgrade, and maintenance operations for **Pure Storage FlashArray, FlashBlade, and related hardware**.

The simulator allows engineers and trainees to:
- Assemble and upgrade virtual arrays in a realistic 3D environment.
- Perform simulated CLI and GUI operations identical to **Purity OS**.
- Follow documented procedures from Pure’s official installation and upgrade guides.
- Prepare for the **Pure Storage Implementation Engineer certification** by completing interactive exercises and CLI tasks.

---

## 2. Core Components

### 2.1 3D Hardware Simulator
A fully interactive Unity environment that visually represents Pure hardware:
- **FlashArray //X and //C**
- **FlashBlade**
- **DirectFlash Modules (DFMs)**
- **Controllers, NVRAMs, Power Supplies, Fans, PCIE cards, and Cables**

Each component can be:
- Added, removed, or replaced.
- Inspected for status (green = OK, amber = warning, red = failed).
- Simulated for fault conditions (e.g., drive failure, controller offline).

**Interaction Examples:**
- Click to open chassis or blade slots.
- Drag-and-drop modules into correct slots.
- Hover to show specs and firmware info.
- Tooltips mimic actual Pure UI readouts (e.g., “DFM-1 status: healthy, FW v6.4.1”).

---

### 2.2 CLI / Terminal Simulator (“Putty Emulator”)

A built-in **terminal simulator** replicates SSH access to Purity OS.  
It uses Linux-like syntax and supports:
- Command history, autocompletion (Tab), and color-coded output.
- Emulation of standard Linux tools (`ls`, `cd`, `cat`, `grep`, `find`, `df`, `top`, `ifconfig`, etc.)
- Pure-specific commands:
  - `purearray list`
  - `purearray list --controller`
  - `purevol list`
  - `purehw list`
  - `pureadm status`
  - `pureuser list`
  - `purearray connect`
  - `purearray list --connect --path`

All simulated CLI behavior references real-world logs from the `/References` folder (which contains captured output from real arrays and upgrades).

---

### 2.3 AI Training & Exam Module

A built-in assessment engine called **PureAI** evaluates user actions and command output:
- Tracks correct vs incorrect steps.
- Monitors command sequence timing.
- Generates “exam-style” feedback:  
  *“Controller firmware mismatch detected — re-run `purearray list --controller` on both nodes and confirm version alignment.”*

This module also includes:
- Question pools inspired by **Implementation Engineer Exam** content.
- Scenario-based simulations (e.g., “Upgrade array to 6.4.8”, “Replace failed DFM”).
- Score tracking and progress badges.

---

### 2.4 Reference Integration

A `/References` folder contains:
- Real **installation and upgrade documentation** from Pure.
- **CLI logs** from actual engineer sessions.
- **Vector Storage AI notes** related to PureAI and data models.
- Sample exam questions and correct CLI responses.

These are used by Copilot and the simulator’s internal documentation system to ensure behavior consistency.

---

## 3. System Architecture

| Layer | Component | Description |
|-------|------------|-------------|
| **Front-End** | Unity Engine | 3D visualization and user interaction |
| **Middleware** | C# Interface Layer | Connects Unity front-end with command simulation and AI modules |
| **Back-End** | Python/C# | Processes CLI logic, stores simulation state, runs AI evaluation |
| **Data Layer** | SQLite / JSON | Stores simulated array states, component metadata, and user progress |
| **AI Layer** | Vector/Embedding Model | Maps user commands and Pure documentation for validation & feedback |

---

## 4. Functional Requirements

### 4.1 Hardware Simulation
- ✅ 3D models of all Pure hardware assets.
- ✅ Click, drag, and hover interactivity.
- ✅ Slot validation (e.g., DFM cannot be inserted into wrong slot).
- ✅ Hardware health simulation (power loss, temperature warning, etc.).

### 4.2 CLI / OS Layer
- ✅ Full Linux-like terminal emulation.
- ✅ Purity command set with expected outputs.
- ✅ Log-based realism (responses pulled from `/References/Logs/`).
- ✅ File system operations: mount, cp, mv, cat, rm, grep, find.
- ✅ Autocomplete and command history.

### 4.3 Training Logic
- ✅ Scenario creation system.
- ✅ Progression tracking (save state).
- ✅ Exam scoring and reporting.
- ✅ Support for time-based challenges and replays.

---

## 5. Technical Features

| Feature | Description |
|----------|--------------|
| **Virtual Filesystem** | Mirrors Linux hierarchy (`/etc`, `/pure`, `/mnt/usb`, `/var/log`) |
| **Mounting System** | Allows mounting simulated USB or ISO files containing upgrade packages |
| **Command Parser** | Regex-based interpreter maps CLI input → stored output data |
| **Color Output** | ANSI-colored text for health checks and warnings |
| **Hardware Events** | Random failure simulation triggers alerts (`purehw list` shows “FAILED”) |
| **Exam Scenarios** | Configurable YAML or JSON defining expected steps and outcomes |

---

## 6. Example Scenario — Controller Upgrade

**Objective:** Replace Controller 1 and perform firmware sync.  
**Steps (simulated):**
1. View array state → `purearray list --controller`
2. Confirm peer timezone → `cat /etc/timezone && ssh peer cat /etc/timezone`
3. Sync firmware versions → `purearray list --connect --path`
4. Run `purehw list` to validate component states.
5. Reboot controllers sequentially.
6. Validate health and version match.

The simulator displays:
- CLI outputs pulled from reference logs.
- Realistic time delays and progress animations.
- 3D visualization of controller removal/insertion.

---

## 7. Development Goals

| Phase | Goal | Deliverables |
|-------|------|--------------|
| **Phase 1** | Core Framework | CLI simulator + 3D model integration |
| **Phase 2** | Training System | Scenarios + grading system |
| **Phase 3** | Full Hardware Set | All FlashArray and FlashBlade models |
| **Phase 4** | Exam & AI | Full certification simulation engine |
| **Phase 5** | Web Integration | Optional web viewer for non-Unity playback |

---

## 8. Design Principles

- **Realism:** Outputs and visuals must match real Pure hardware and documentation.
- **Modularity:** Each subsystem (CLI, 3D, AI) runs independently.
- **Extensibility:** New hardware and procedures should be easy to add.
- **Transparency:** All CLI logic and assets live in editable files for traceability.
- **AI Integration:** Copilot and internal LLMs reference `/Docs/DesignDocument.md` and `/References` for contextual accuracy.

---

## 9. File Structure (Proposed)

```
/PureSimulator/
│
├── /Assets/
│   ├── /Models/
│   ├── /Materials/
│   ├── /Scenes/
│   ├── /Scripts/
│   └── /UI/
│
├── /CLI/
│   ├── CommandParser.cs
│   ├── LinuxCommands.json
│   ├── PurityCommands.json
│   └── VirtualFileSystem/
│
├── /Training/
│   ├── Scenarios/
│   ├── Exams/
│   ├── Results/
│   └── AI/
│
├── /References/
│   ├── InstallationGuides/
│   ├── UpgradeLogs/
│   ├── ExamQuestions/
│   └── VectorStorageAI/
│
├── /Docs/
│   ├── DesignDocument.md
│   └── DevelopmentPlan.md
│
└── README.md
```

---

## 10. Integration Notes for GitHub Copilot

- Copilot should use this document as a **context reference** for:
  - Command simulation logic.
  - File structure expectations.
  - Pure hardware behavior.
  - Command output consistency with real logs.
- All generated code must adhere to:
  - Linux terminal syntax.
  - Pure Storage procedure flow.
  - Real-world step validation.

---

### ✅ End of Design Document

