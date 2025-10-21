using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// puretune command - Manage system tunables and configuration parameters.
    /// Lists consistently and inconsistently set tunables across controllers.
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs/putty2025-03-03.log L430-493
    /// Shows tunable list with values from pureadm and puredb sources
    /// </remarks>
    [SerialCommand("puretune")]
    public class PureTuneCommand : ISerialCommand
    {
        public string Name => "puretune";
        public string Synopsis => "Manage system tunables and configuration parameters";
        public IReadOnlyList<string> Parameters => new[] { "--list" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            if (args.Length == 0 || args[0] == "--list")
            {
                HandleList(terminal);
            }
            else
            {
                terminal.WriteLine($"puretune: unknown option '{args[0]}'");
                terminal.WriteLine("Usage: puretune --list");
            }
        }
        
        private void HandleList(ISerialOutput terminal)
        {
            // Source: Docs/PuttyLogs/putty2025-03-03.log L430-493
            terminal.WriteLine("Warning: failed to retrieve some tunable status (local puredb-chastity, peer puredb-chastity)");
            terminal.WriteLine("Consistently set tunables:");
            terminal.WriteLine("PS_DISABLE_ERADICATION : 1 ('CS0541536')");
            terminal.WriteLine("Inconsistently or unnecessarily set tunables:");
            terminal.WriteLine("FMM_DRIVE_EVENT_RETIRE_SCAN_INTERVAL_MINUTE:");
            terminal.WriteLine("local pureadm             - 30  (PURE-260696)");
            terminal.WriteLine("peer pureadm              - 30  (PURE-260696)");
            terminal.WriteLine("local puredb              - <unset>");
            terminal.WriteLine("peer puredb               - <unset>");
            terminal.WriteLine("local puredb --platform   - <unset>");
            terminal.WriteLine("peer puredb --platform    - <unset>");
            terminal.WriteLine("local puredb --chastity   - <unset>");
            terminal.WriteLine("peer puredb --chastity    - <unset>");
            terminal.WriteLine("PURITY_START_ON_BOOT:");
            terminal.WriteLine("local pureadm             - 1");
            terminal.WriteLine("peer pureadm              - 1");
            terminal.WriteLine("local puredb              - <unset>");
            terminal.WriteLine("peer puredb               - <unset>");
            terminal.WriteLine("local puredb --platform   - <unset>");
            terminal.WriteLine("peer puredb --platform    - <unset>");
            terminal.WriteLine("local puredb --chastity   - <unset>");
            terminal.WriteLine("peer puredb --chastity    - <unset>");
            terminal.WriteLine("PS_FILEGW_SMB_HIDE_DOT_SNAPSHOT:");
            terminal.WriteLine("local pureadm             - 0  (Set during upgrade to 6.3.12 - CLOUD-75192)");
            terminal.WriteLine("peer pureadm              - 0  (Set during upgrade to 6.3.12 - CLOUD-75192)");
            terminal.WriteLine("local puredb              - <unset>");
            terminal.WriteLine("peer puredb               - <unset>");
            terminal.WriteLine("local puredb --platform   - <unset>");
            terminal.WriteLine("peer puredb --platform    - <unset>");
            terminal.WriteLine("local puredb --chastity   - <unset>");
            terminal.WriteLine("peer puredb --chastity    - <unset>");
            terminal.WriteLine("ATOM_5206:");
            terminal.WriteLine("local pureadm             - 1");
            terminal.WriteLine("peer pureadm              - 1");
            terminal.WriteLine("local puredb              - <unset>");
            terminal.WriteLine("peer puredb               - <unset>");
            terminal.WriteLine("local puredb --platform   - <unset>");
            terminal.WriteLine("peer puredb --platform    - <unset>");
            terminal.WriteLine("local puredb --chastity   - <unset>");
            terminal.WriteLine("peer puredb --chastity    - <unset>");
        }
    }
}
