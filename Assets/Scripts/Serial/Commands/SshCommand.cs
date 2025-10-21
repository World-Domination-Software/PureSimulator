using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// ssh command - Connect to remote host (simulated for peer controller).
    /// Common usage: ssh peer (connect to peer controller)
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs showing ssh peer usage
    /// </remarks>
    [SerialCommand("ssh")]
    public class SshCommand : ISerialCommand
    {
        public string Name => "ssh";
        public string Synopsis => "Connect to remote host";
        public IReadOnlyList<string> Parameters => new[] { "<host>" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            if (args.Length == 0)
            {
                terminal.WriteLine("usage: ssh <host>");
                return;
            }
            
            var host = args[0];
            
            if (host == "peer")
            {
                // Simulate successful connection to peer controller
                terminal.WriteLine("Last login: Mon Oct 21 12:00:00 2025 from fe80::26a9:37ff:fe45:6575%eth0");
                terminal.WriteLine("");
                terminal.WriteLine("Mon Oct 21 18:00:00 2025");
                terminal.WriteLine("Welcome root. This is Purity Version 6.5.8 on FlashArray");
                terminal.WriteLine("http://www.purestorage.com/");
                // Prompt change would be handled by terminal
            }
            else
            {
                terminal.WriteLine($"ssh: connect to host {host} port 22: No route to host");
            }
        }
    }
}
