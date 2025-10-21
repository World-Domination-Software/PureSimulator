using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// cat command - Display file contents.
    /// Common usage: cat /etc/timezone
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs showing cat /etc/timezone usage
    /// </remarks>
    [SerialCommand("cat")]
    public class CatCommand : ISerialCommand
    {
        public string Name => "cat";
        public string Synopsis => "Display file contents";
        public IReadOnlyList<string> Parameters => new[] { "<file>" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            if (args.Length == 0)
            {
                terminal.WriteLine("cat: missing file operand");
                return;
            }
            
            var filename = args[0];
            
            // Common files in Purity systems
            switch (filename)
            {
                case "/etc/timezone":
                    terminal.WriteLine("America/New_York");
                    break;
                case "/etc/purity-version":
                    terminal.WriteLine("6.5.8");
                    break;
                case "/proc/version":
                    terminal.WriteLine("Linux version 5.4.0-pure (pure@build) (gcc version 9.3.0)");
                    break;
                default:
                    terminal.WriteLine($"cat: {filename}: No such file or directory");
                    break;
            }
        }
    }
}
