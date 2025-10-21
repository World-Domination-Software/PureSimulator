using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PureSim.Console
{
    /// <summary>
    /// Implementation of IConsoleOutput that formats text with color codes and supports tables.
    /// </summary>
    public class ConsoleOutput : IConsoleOutput
    {
        private readonly List<string> lines = new List<string>();
        private readonly Action<string> onNewLine;
        
        public ConsoleOutput(Action<string> onNewLine = null)
        {
            this.onNewLine = onNewLine;
        }
        
        public void WriteLine(string message)
        {
            string line = message ?? string.Empty;
            lines.Add(line);
            onNewLine?.Invoke(line);
        }
        
        public void WriteWarning(string message)
        {
            WriteLine($"<color=yellow>Warning: {message}</color>");
        }
        
        public void WriteError(string message)
        {
            WriteLine($"<color=red>Error: {message}</color>");
        }
        
        public void WriteSuccess(string message)
        {
            WriteLine($"<color=green>{message}</color>");
        }
        
        public void WriteTable(string[] headers, List<string[]> rows)
        {
            if (headers == null || headers.Length == 0)
                return;
            
            // Calculate column widths
            int[] widths = new int[headers.Length];
            for (int i = 0; i < headers.Length; i++)
            {
                widths[i] = headers[i]?.Length ?? 0;
            }
            
            foreach (var row in rows)
            {
                for (int i = 0; i < Math.Min(row.Length, widths.Length); i++)
                {
                    if (row[i] != null && row[i].Length > widths[i])
                        widths[i] = row[i].Length;
                }
            }
            
            // Build header
            var sb = new StringBuilder();
            for (int i = 0; i < headers.Length; i++)
            {
                sb.Append(headers[i].PadRight(widths[i] + 2));
            }
            WriteLine(sb.ToString());
            
            // Build separator
            sb.Clear();
            for (int i = 0; i < headers.Length; i++)
            {
                sb.Append(new string('-', widths[i] + 2));
            }
            WriteLine(sb.ToString());
            
            // Build rows
            foreach (var row in rows)
            {
                sb.Clear();
                for (int i = 0; i < headers.Length; i++)
                {
                    string value = i < row.Length ? (row[i] ?? "") : "";
                    sb.Append(value.PadRight(widths[i] + 2));
                }
                WriteLine(sb.ToString());
            }
        }
        
        public void Clear()
        {
            lines.Clear();
        }
        
        public List<string> GetLines() => new List<string>(lines);
        
        public string GetText() => string.Join("\n", lines);
    }
}
