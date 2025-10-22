using System.Collections.Generic;

namespace PureSim.Serial.Commands
{
    /// <summary>
    /// iobalance command - Monitor and display host I/O balance across controllers.
    /// Shows which hosts have balanced or unbalanced I/O between CT0 and CT1.
    /// </summary>
    /// <remarks>
    /// Source: Docs/PuttyLogs/putty2025-03-03.log L1136-1192
    /// Shows sampling output with host names and I/O counts per controller
    /// </remarks>
    [SerialCommand("iobalance")]
    public class IobalanceCommand : ISerialCommand
    {
        public string Name => "iobalance";
        public string Synopsis => "Monitor and display host I/O balance across controllers";
        public IReadOnlyList<string> Parameters => new[] { "--sampletime", "<seconds>" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, ISerialOutput terminal)
        {
            int sampleTime = 30; // default
            
            // Parse --sampletime argument
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--sampletime" && i + 1 < args.Length)
                {
                    if (int.TryParse(args[i + 1], out int parsed))
                    {
                        sampleTime = parsed;
                    }
                    break;
                }
            }
            
            terminal.WriteLine($"sampling host I/O for {sampleTime} seconds...");
            
            // Source: Docs/PuttyLogs/putty2025-03-03.log L1136-1192
            // Display sample I/O balance data
            terminal.WriteLine($"{"Host",-30}  {"CT0 I/O Count",-15}  {"CT1 I/O Count",-15}");
            
            // Generate some sample host data
            var hosts = new[]
            {
                ("host-001-ll", 375.00, 362.00, false),
                ("host-002-ll", 136.00, 112.00, false),
                ("host-003-ll", 43.00, 47.00, false),
                ("host-004-hl", 1020.00, 594.00, true),    // unbalanced
                ("host-004-ll", 25360.00, 23590.00, false),
                ("host-005-hl", 6770.00, 5640.00, false),
                ("host-005-ll", 15270.00, 28250.00, true), // unbalanced
                ("host-006-hl", 16170.00, 13380.00, false),
                ("host-007-ll", 160.00, 43.00, true),       // unbalanced
                ("host-008-ll", 120.00, 121.00, false),
                ("host-009-hl", 15060.00, 16340.00, false),
                ("host-010-ll", 46750.00, 41450.00, false),
                ("host-011-hl", 25950.00, 19970.00, false),
                ("host-012-ll", 14500.00, 12840.00, false)
            };
            
            foreach (var (host, ct0, ct1, unbalanced) in hosts)
            {
                string ct0Str = FormatIOCount(ct0);
                string ct1Str = FormatIOCount(ct1);
                string status = unbalanced ? "  ! unbalanced I/O" : "";
                terminal.WriteLine($"{host,-30}  {ct0Str,-15}  {ct1Str,-15}{status}");
            }
        }
        
        private string FormatIOCount(double count)
        {
            if (count >= 1000)
            {
                return $"{count / 1000.0:F2}K";
            }
            return $"{count:F2}";
        }
    }
}
