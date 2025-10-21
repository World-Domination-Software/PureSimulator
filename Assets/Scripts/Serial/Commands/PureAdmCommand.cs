using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// pureadm command - Administrative commands for Purity services.
    /// Primary use is checking process status with 'pureadm status'.
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs/putty2025-03-03.log L238-248
    /// Shows process status output with start/running states
    /// </remarks>
    [SerialCommand("pureadm")]
    public class PureAdmCommand : ISerialCommand
    {
        public string Name => "pureadm";
        public string Synopsis => "Administrative commands for Purity services";
        public IReadOnlyList<string> Parameters => new[] { "status" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            if (args.Length == 0)
            {
                terminal.WriteLine("Usage: pureadm <subcommand>");
                terminal.WriteLine("  status    Show Purity service status");
                return;
            }
            
            var subcommand = args[0];
            
            switch (subcommand)
            {
                case "status":
                    HandleStatus(terminal);
                    break;
                default:
                    terminal.WriteLine($"pureadm: unknown subcommand '{subcommand}'");
                    break;
            }
        }
        
        private void HandleStatus(ISerialOutput terminal)
        {
            // Source: Docs/PuttyLogs/putty2025-03-03.log L238-248
            terminal.WriteLine("Process status:");
            terminal.WriteLine("purity start/running");
            terminal.WriteLine("lio-drv start/running");
            terminal.WriteLine("foed start/running, process 17862");
            terminal.WriteLine("platform start/running, process 17790");
            terminal.WriteLine("gui start/running, process 17764");
            terminal.WriteLine("rest start/running, process 21135");
            terminal.WriteLine("monitor start/running, process 17776");
            terminal.WriteLine("iostat start/running, process 46481");
            terminal.WriteLine("statistics start/running, process 17832");
            terminal.WriteLine("wsdd start/running, process 17808");
            terminal.WriteLine("syslogd start/running, process 17806");
        }
    }
}
