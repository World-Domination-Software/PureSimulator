using System.Collections.Generic;
using System.Linq;

namespace PureSim.Console.Commands
{
    /// <summary>
    /// Help command - lists all available commands or provides details about a specific command.
    /// </summary>
    [ConsoleCommand("help")]
    public class HelpCommand : IConsoleCommand
    {
        public string Name => "help";
        public string Synopsis => "Display help information for console commands";
        public IReadOnlyList<string> Parameters => new[] { "[command_name]" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, IConsoleOutput output)
        {
            var registry = new ConsoleRegistry();
            
            if (args.Length == 0)
            {
                // List all commands
                output.WriteLine("Available Console Commands:");
                output.WriteLine("");
                
                var commands = registry.GetAllCommands().OrderBy(c => c.Name).ToList();
                
                foreach (var cmd in commands)
                {
                    string paramText = cmd.Parameters.Count > 0 
                        ? " " + string.Join(" ", cmd.Parameters)
                        : "";
                    output.WriteLine($"  {cmd.Name}{paramText}");
                    output.WriteLine($"    {cmd.Synopsis}");
                    output.WriteLine("");
                }
                
                output.WriteLine("Use 'help <command>' for detailed information about a specific command.");
            }
            else
            {
                // Show help for specific command
                string commandName = args[0].ToLowerInvariant();
                
                if (registry.TryGetCommand(commandName, out IConsoleCommand command))
                {
                    output.WriteLine($"Command: {command.Name}");
                    output.WriteLine($"Synopsis: {command.Synopsis}");
                    
                    if (command.Parameters.Count > 0)
                    {
                        output.WriteLine($"Usage: {command.Name} {string.Join(" ", command.Parameters)}");
                    }
                    else
                    {
                        output.WriteLine($"Usage: {command.Name}");
                    }
                }
                else
                {
                    output.WriteError($"Unknown command: {commandName}");
                }
            }
        }
    }
}
