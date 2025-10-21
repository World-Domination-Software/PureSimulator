using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// stty command - Change and print terminal line settings.
    /// Common usage: stty rows 20, stty columns 250
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs showing stty rows/columns commands
    /// </remarks>
    [SerialCommand("stty")]
    public class SttyCommand : ISerialCommand
    {
        public string Name => "stty";
        public string Synopsis => "Change and print terminal line settings";
        public IReadOnlyList<string> Parameters => new[] { "[rows N]", "[columns N]" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            if (args.Length == 0)
            {
                // Show current terminal settings
                terminal.WriteLine("speed 38400 baud; rows 24; columns 80; line = 0;");
                return;
            }
            
            // Handle settings changes
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "rows" && i + 1 < args.Length)
                {
                    // In simulation, this would notify the terminal to adjust
                    i++;
                }
                else if (args[i] == "columns" && i + 1 < args.Length)
                {
                    // In simulation, this would notify the terminal to adjust
                    i++;
                }
            }
            
            // Silent success (no output for set operations)
        }
    }
}
