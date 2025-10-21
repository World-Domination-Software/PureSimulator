using NUnit.Framework;
using PureSim.Console;
using PureSim.Serial;
using System.Linq;

namespace PureSim.Tests.Editor
{
    /// <summary>
    /// Tests to verify that Console and Serial commands remain separate and don't cross boundaries.
    /// This ensures operational commands (Serial) don't appear in simulator control (Console).
    /// </summary>
    public class ConsoleBoundaryTests
    {
        [Test]
        public void ConsoleRegistry_ShouldNotContainSerialCommands()
        {
            // Arrange
            var consoleRegistry = new ConsoleRegistry();
            var consoleCommands = consoleRegistry.GetAllCommandNames();
            
            // Serial command names that should NOT appear in console
            string[] serialCommandNames = { "mount", "umount", "ls", "lsblk" };
            
            // Assert
            foreach (var serialCmd in serialCommandNames)
            {
                Assert.IsFalse(
                    consoleCommands.Contains(serialCmd),
                    $"Console registry should not contain serial command '{serialCmd}'. " +
                    "Operational commands belong in Serial Terminal only."
                );
            }
        }
        
        [Test]
        public void ConsoleCommands_ShouldOnlyBeSimulatorControl()
        {
            // Arrange
            var registry = new ConsoleRegistry();
            var commands = registry.GetAllCommandNames();
            
            // Expected console command categories (simulator control only)
            string[] expectedConsoleCommands = { 
                "help", "clear",           // Meta
                "jump", "steps",            // Workflow
                "faults", "inject", "clearfault",  // Fault injection
                "usb"                       // State control
            };
            
            // Assert - all console commands should be for simulator control
            foreach (var cmd in commands)
            {
                bool isExpected = expectedConsoleCommands.Contains(cmd);
                Assert.IsTrue(
                    isExpected,
                    $"Unexpected command '{cmd}' in console registry. " +
                    "Console should only contain simulator control commands."
                );
            }
        }
        
        [Test]
        public void SerialCommands_ShouldImplementISerialCommand()
        {
            // Verify that mount and ls implement ISerialCommand, not IConsoleCommand
            var mountType = typeof(Serial.Commands.MountCommand);
            var lsType = typeof(Serial.Commands.LsblkCommand);
            
            Assert.IsTrue(typeof(ISerialCommand).IsAssignableFrom(mountType),
                "MountCommand should implement ISerialCommand");
            Assert.IsTrue(typeof(ISerialCommand).IsAssignableFrom(lsType),
                "LsblkCommand should implement ISerialCommand");
                
            Assert.IsFalse(typeof(IConsoleCommand).IsAssignableFrom(mountType),
                "MountCommand should NOT implement IConsoleCommand");
            Assert.IsFalse(typeof(IConsoleCommand).IsAssignableFrom(lsType),
                "LsblkCommand should NOT implement IConsoleCommand");
        }
        
        [Test]
        public void ConsoleCommands_ShouldImplementIConsoleCommand()
        {
            // Verify that console commands implement IConsoleCommand, not ISerialCommand
            var helpType = typeof(Console.Commands.HelpCommand);
            var jumpType = typeof(Console.Commands.JumpCommand);
            
            Assert.IsTrue(typeof(IConsoleCommand).IsAssignableFrom(helpType),
                "HelpCommand should implement IConsoleCommand");
            Assert.IsTrue(typeof(IConsoleCommand).IsAssignableFrom(jumpType),
                "JumpCommand should implement IConsoleCommand");
                
            Assert.IsFalse(typeof(ISerialCommand).IsAssignableFrom(helpType),
                "HelpCommand should NOT implement ISerialCommand");
            Assert.IsFalse(typeof(ISerialCommand).IsAssignableFrom(jumpType),
                "JumpCommand should NOT implement ISerialCommand");
        }
    }
}
