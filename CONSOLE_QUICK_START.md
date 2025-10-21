# Quick Start Guide: PureSim Console Commands

## For Developers

### Adding a New Console Command (Simulator Control)

```csharp
// File: Assets/Scripts/Console/Commands/MyCommand.cs
using System.Collections.Generic;

namespace PureSim.Console.Commands
{
    [ConsoleCommand("mycommand")]
    public class MyCommand : IConsoleCommand
    {
        public string Name => "mycommand";
        public string Synopsis => "Brief description of what it does";
        public IReadOnlyList<string> Parameters => new[] { "<required>", "[optional]" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, IConsoleOutput output)
        {
            // Validate arguments
            if (args.Length < 1)
            {
                output.WriteError("Usage: mycommand <required> [optional]");
                return;
            }
            
            // Interact with simulation state
            if (sim.IsUsbInserted())
            {
                output.WriteSuccess("USB is present");
            }
            
            // Write formatted output
            output.WriteLine("Normal text");
            output.WriteWarning("Warning message");
            output.WriteError("Error message");
        }
    }
}
```

Command auto-registers on startup via reflection.

---

### Adding a New Serial Command (Operational)

```csharp
// File: Assets/Scripts/Serial/Commands/MySerialCommand.cs
using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// Brief description of command.
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs/putty2025-XX-XX.log LXX-YY
    /// </remarks>
    [SerialCommand("mycmd")]
    public class MySerialCommand : ISerialCommand
    {
        public string Name => "mycmd";
        public string Synopsis => "Brief description";
        public IReadOnlyList<string> Parameters => new[] { "<arg1>", "[arg2]" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            // 1. Search Docs/PuttyLogs for real output
            // 2. Mirror exact format from logs
            // 3. Cite source in comments
            
            // Example success case (cite source):
            // Source: Docs/PuttyLogs/putty2025-02-18.log L123
            terminal.WriteLine("/dev/sda1  /dev/sdb1");
            
            // Example error case (cite source):
            // Source: Standard Linux error messages
            if (!sim.IsUsbInserted())
            {
                terminal.WriteLine("mycmd: device not found");
            }
        }
    }
}
```

---

## Console Commands Quick Reference

### Workflow Control
- `steps` — List all workflow steps and status
- `jump <step_id>` — Jump to specific step (with precondition validation)

### Fault Injection
- `faults` — List active faults
- `inject <fault_id> [description]` — Inject a fault
- `clearfault <fault_id>|all` — Clear faults

### State Control
- `usb state <inserted|removed>` — Control USB presence

### Meta
- `help [command]` — Show help
- `clear` — Clear console output

---

## Serial Commands Quick Reference

### Device Management
- `ls /dev/sd*1` — List block devices
- `mount <device> <mountpoint>` — Mount filesystem
- `umount <mountpoint>` — Unmount filesystem

---

## Testing Your Commands

### Unit Test (Console)
```csharp
// File: Assets/Tests/Editor/Console/MyCommandTests.cs
using NUnit.Framework;
using PureSim.Console;
using PureSim.Console.Commands;

public class MyCommandTests
{
    [Test]
    public void MyCommand_Execute_ProducesExpectedOutput()
    {
        // Arrange
        var sim = new Simulation.SimulationState();
        var output = new ConsoleOutput();
        var command = new MyCommand();
        
        // Act
        command.Execute(sim, new[] { "arg1" }, output);
        
        // Assert
        var lines = output.GetLines();
        Assert.Greater(lines.Count, 0);
    }
}
```

### Golden Transcript (Serial)
```
# File: Assets/Tests/PlayMode/Serial/Golden_MyCommand.txt
# Source: Docs/PuttyLogs/putty2025-XX-XX.log LXX-YY

## Test: Happy path
Command: mycmd arg1
Expected Output:
expected output line 1
expected output line 2

## Test: Error path
Command: mycmd
Expected Output:
mycmd: missing argument
```

---

## Common Patterns

### Checking Simulation State
```csharp
// USB state
if (sim.IsUsbInserted()) { }
if (sim.IsUsbMounted()) { }

// Faults
if (sim.HasFault("usb-not-inserted")) { }
var faults = sim.GetActiveFaults();

// Power
if (sim.GetPowerState("CT0")) { }
```

### Modifying Simulation State
```csharp
// USB
sim.SetUsbInserted(true);
sim.SetUsbMounted(true);

// Faults
sim.InjectFault("cable-missing", "Cable not connected");
sim.ClearFault("cable-missing");

// Power
sim.SetPowerState("CT0", true);
```

### Workflow Engine
```csharp
// Get controller reference
var controller = GameObject.FindObjectOfType<ConsoleController>();
var workflow = controller.GetWorkflowEngine();

// Check if can jump
if (workflow.CanJumpToStep("mount", sim, out var failed)) {
    workflow.JumpToStep("mount", sim);
}

// List all steps
var steps = workflow.GetAllSteps();
var current = workflow.GetCurrentStep();
```

---

## Troubleshooting

### Command Not Found
1. Check attribute: `[ConsoleCommand("name")]` or `[SerialCommand("name")]`
2. Check interface: `IConsoleCommand` or `ISerialCommand`
3. Check namespace: `PureSim.Console.*` or `PureSim.Serial.*`
4. Rebuild project in Unity

### Tests Not Running
1. Ensure assembly references: `PureSim.Runtime`
2. Check `defineConstraints`: `UNITY_INCLUDE_TESTS`
3. Use Unity Test Runner window (Window > General > Test Runner)

### Output Not Showing
1. Console: Check `ConsoleController` is in scene
2. Serial: Check `ISerialOutput` implementation
3. Verify `WriteLine()` calls are being made

---

## Best Practices

### DO:
✅ Mirror real command output exactly (spacing, order, phrasing)  
✅ Cite source logs in comments (`// Source: Docs/PuttyLogs/...`)  
✅ Implement multiple error paths (happy + at least 2 errors)  
✅ Add unit tests for all commands  
✅ Use `SimulationState` as single source of truth  

### DON'T:
❌ Mix console and serial commands  
❌ Make up command output (always use real logs)  
❌ Directly mutate state (go through `SimulationState` APIs)  
❌ Add operational commands to Console  
❌ Add simulator control commands to Serial  

---

## Resources

- **Full Architecture Doc:** `DUAL_CONSOLE_ARCHITECTURE.md`
- **Change Log:** `change log.MD`
- **Repository Instructions:** `.github/copilot-instructions.md`
- **Real Command Logs:** `Docs/PuttyLogs/*.log`
- **Command PDFs:** `Docs/*.pdf`

---

*Need help? Check the boundary tests in `Assets/Tests/Editor/ConsoleBoundaryTests.cs` for examples of proper separation.*
