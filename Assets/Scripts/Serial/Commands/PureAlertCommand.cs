using System;
using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// purealert command - Manage alert maintenance windows.
    /// Common usage: purealert tag --timeout 240m --maintenance
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs/putty2025-02-22-2.txt L21-23
    /// </remarks>
    [SerialCommand("purealert")]
    public class PureAlertCommand : ISerialCommand
    {
        public string Name => "purealert";
        public string Synopsis => "Manage alert maintenance windows";
        public IReadOnlyList<string> Parameters => new[] { "tag", "--timeout", "--maintenance" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            if (args.Length == 0)
            {
                terminal.WriteLine("Usage: purealert tag --timeout <time> --maintenance");
                return;
            }
            
            if (args[0] == "tag")
            {
                HandleTag(args, terminal);
            }
            else
            {
                terminal.WriteLine($"purealert: unknown subcommand '{args[0]}'");
            }
        }
        
        /// <summary>
        /// Handle 'purealert tag --timeout 240m --maintenance' command.
        /// Creates a maintenance window to suppress alerts.
        /// Source: Docs/PuttyLogs/putty2025-02-22-2.txt L21-23
        /// </summary>
        private void HandleTag(string[] args, ISerialOutput terminal)
        {
            // Parse timeout
            string timeout = "240m";
            bool maintenance = false;
            
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "--timeout" && i + 1 < args.Length)
                {
                    timeout = args[i + 1];
                    i++;
                }
                else if (args[i] == "--maintenance")
                {
                    maintenance = true;
                }
            }
            
            if (!maintenance)
            {
                terminal.WriteLine("Usage: purealert tag --timeout <time> --maintenance");
                return;
            }
            
            // Calculate expiration time
            int minutes = ParseTimeout(timeout);
            var created = DateTime.Now;
            var expires = created.AddMinutes(minutes);
            
            // Output format from logs
            // Source: Docs/PuttyLogs/putty2025-02-22-2.txt L22-23
            terminal.WriteLine("Name         Created                  Expires");
            terminal.WriteLine($"maintenance  {created:yyyy-MM-dd HH:mm:ss} EST  {expires:yyyy-MM-dd HH:mm:ss} EST");
        }
        
        private int ParseTimeout(string timeout)
        {
            // Parse formats like "240m", "4h", "180m"
            if (timeout.EndsWith("m"))
            {
                return int.Parse(timeout.TrimEnd('m'));
            }
            else if (timeout.EndsWith("h"))
            {
                return int.Parse(timeout.TrimEnd('h')) * 60;
            }
            return 240; // default 4 hours
        }
    }
}
