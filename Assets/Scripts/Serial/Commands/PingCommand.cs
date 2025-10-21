using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// ping command - Send ICMP echo requests to network hosts.
    /// </summary>
    [SerialCommand("ping")]
    public class PingCommand : ISerialCommand
    {
        public string Name => "ping";
        public string Synopsis => "Send ICMP echo requests";
        public IReadOnlyList<string> Parameters => new[] { "<host>", "[-c count]" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            if (args.Length == 0)
            {
                terminal.WriteLine("usage: ping [-c count] <host>");
                return;
            }
            
            // Parse arguments
            string host = null;
            int count = 4;
            
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-c" && i + 1 < args.Length)
                {
                    count = int.Parse(args[i + 1]);
                    i++;
                }
                else if (!args[i].StartsWith("-"))
                {
                    host = args[i];
                }
            }
            
            if (host == null)
            {
                terminal.WriteLine("ping: missing host operand");
                return;
            }
            
            // Simulate ping output
            terminal.WriteLine($"PING {host} (192.168.1.1): 56 data bytes");
            
            for (int i = 0; i < count; i++)
            {
                terminal.WriteLine($"64 bytes from {host}: icmp_seq={i} ttl=64 time=0.{i * 3 + 1} ms");
            }
            
            terminal.WriteLine("");
            terminal.WriteLine($"--- {host} ping statistics ---");
            terminal.WriteLine($"{count} packets transmitted, {count} packets received, 0.0% packet loss");
        }
    }
}
