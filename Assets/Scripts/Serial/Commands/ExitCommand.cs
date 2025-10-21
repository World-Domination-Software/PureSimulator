using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// exit command - Exit current shell or session.
    /// </summary>
    [SerialCommand("exit")]
    [SerialCommand("quit")]
    [SerialCommand("logout")]
    public class ExitCommand : ISerialCommand
    {
        public string Name => "exit";
        public string Synopsis => "Exit current shell or session";
        public IReadOnlyList<string> Parameters => new string[0];
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            // In simulation, this might trigger a prompt change or session end
            // For now, just acknowledge
            terminal.WriteLine("logout");
        }
    }
}
