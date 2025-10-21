using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// clear command - Clear terminal screen.
    /// </summary>
    [SerialCommand("clear")]
    public class ClearTerminalCommand : ISerialCommand
    {
        public string Name => "clear";
        public string Synopsis => "Clear terminal screen";
        public IReadOnlyList<string> Parameters => new string[0];
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            // Signal to terminal to clear screen
            // Terminal implementation should handle this appropriately
            terminal.WriteLine("\x1b[2J\x1b[H");  // ANSI clear screen + home cursor
        }
    }
}
