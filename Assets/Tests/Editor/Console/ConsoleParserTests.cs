using NUnit.Framework;
using PureSim.Console;
using System.Linq;

namespace PureSim.Tests.Editor.Console
{
    /// <summary>
    /// Tests for console command parsing and registry.
    /// </summary>
    public class ConsoleParserTests
    {
        [Test]
        public void ConsoleRegistry_DiscoverCommands_FindsAllCommands()
        {
            // Arrange
            var registry = new ConsoleRegistry();
            
            // Act
            var commands = registry.GetAllCommandNames();
            
            // Assert
            Assert.IsNotNull(commands);
            Assert.Greater(commands.Count, 0, "Should discover at least one command");
            
            // Verify key commands are present
            Assert.IsTrue(commands.Contains("help"), "Should find help command");
            Assert.IsTrue(commands.Contains("jump"), "Should find jump command");
            Assert.IsTrue(commands.Contains("steps"), "Should find steps command");
        }
        
        [Test]
        public void ConsoleRegistry_TryGetCommand_ReturnsKnownCommand()
        {
            // Arrange
            var registry = new ConsoleRegistry();
            
            // Act
            bool found = registry.TryGetCommand("help", out IConsoleCommand command);
            
            // Assert
            Assert.IsTrue(found, "Should find help command");
            Assert.IsNotNull(command);
            Assert.AreEqual("help", command.Name);
        }
        
        [Test]
        public void ConsoleRegistry_TryGetCommand_ReturnsFalseForUnknownCommand()
        {
            // Arrange
            var registry = new ConsoleRegistry();
            
            // Act
            bool found = registry.TryGetCommand("nonexistent", out IConsoleCommand command);
            
            // Assert
            Assert.IsFalse(found, "Should not find nonexistent command");
            Assert.IsNull(command);
        }
        
        [Test]
        public void ConsoleOutput_WriteLine_AddsLine()
        {
            // Arrange
            var output = new ConsoleOutput();
            
            // Act
            output.WriteLine("test line");
            
            // Assert
            var lines = output.GetLines();
            Assert.AreEqual(1, lines.Count);
            Assert.AreEqual("test line", lines[0]);
        }
        
        [Test]
        public void ConsoleOutput_Clear_RemovesAllLines()
        {
            // Arrange
            var output = new ConsoleOutput();
            output.WriteLine("line 1");
            output.WriteLine("line 2");
            
            // Act
            output.Clear();
            
            // Assert
            var lines = output.GetLines();
            Assert.AreEqual(0, lines.Count);
        }
        
        [Test]
        public void ConsoleOutput_WriteTable_FormatsTable()
        {
            // Arrange
            var output = new ConsoleOutput();
            string[] headers = { "Name", "Status" };
            var rows = new System.Collections.Generic.List<string[]>
            {
                new[] { "Item1", "OK" },
                new[] { "Item2", "Error" }
            };
            
            // Act
            output.WriteTable(headers, rows);
            
            // Assert
            var lines = output.GetLines();
            Assert.Greater(lines.Count, 2, "Should have header, separator, and rows");
        }
    }
}
