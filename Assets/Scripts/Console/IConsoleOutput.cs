using System.Collections.Generic;

namespace PureSim.Console
{
    /// <summary>
    /// Output interface for console commands to write results, errors, and formatted data.
    /// </summary>
    public interface IConsoleOutput
    {
        /// <summary>
        /// Write a normal output line
        /// </summary>
        void WriteLine(string message);
        
        /// <summary>
        /// Write a warning message
        /// </summary>
        void WriteWarning(string message);
        
        /// <summary>
        /// Write an error message
        /// </summary>
        void WriteError(string message);
        
        /// <summary>
        /// Write a success message
        /// </summary>
        void WriteSuccess(string message);
        
        /// <summary>
        /// Write a table of data
        /// </summary>
        void WriteTable(string[] headers, List<string[]> rows);
        
        /// <summary>
        /// Clear all output
        /// </summary>
        void Clear();
    }
}
