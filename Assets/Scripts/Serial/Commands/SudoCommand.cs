using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// sudo command - Execute commands with root privileges.
    /// In simulation, simply switches context or executes the command directly.
    /// Common usage: sudo su (switch to root user)
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs/*.log showing sudo su usage
    /// </remarks>
    [SerialCommand("sudo")]
    public class SudoCommand : ISerialCommand
    {
        public string Name => "sudo";
        public string Synopsis => "Execute command with root privileges";
        public IReadOnlyList<string> Parameters => new[] { "<command>", "[args...]" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            if (args.Length == 0)
            {
                terminal.WriteLine("usage: sudo <command> [args...]");
                return;
            }
            
            // Handle common sudo su (switch to root)
            if (args[0] == "su")
            {
                // In simulation, this is handled by the terminal prompt change
                // Just acknowledge silently (no output in real logs)
                return;
            }
            
            // For other commands, we'd need to execute them
            terminal.WriteLine($"sudo: executing '{string.Join(" ", args)}' as root");
        }
    }
}
