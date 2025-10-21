namespace PureSim.Serial
{
    /// <summary>
    /// Output interface for serial terminal to write command results.
    /// Mimics real terminal behavior.
    /// </summary>
    public interface ISerialOutput
    {
        /// <summary>
        /// Write a line to the terminal
        /// </summary>
        void WriteLine(string message);
        
        /// <summary>
        /// Write text without newline
        /// </summary>
        void Write(string message);
        
        /// <summary>
        /// Get the current prompt string (e.g., "puresetup@PCTFJ243000F4:~$")
        /// </summary>
        string GetPrompt();
    }
}
