using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PureSim.Console
{
    /// <summary>
    /// Main controller for the Quake-style developer console overlay.
    /// Handles input, command parsing, dispatch, history, and autocomplete.
    /// Toggle with backquote/tilde key.
    /// </summary>
    public class ConsoleController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject consolePanel;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text outputText;
        [SerializeField] private ScrollRect scrollRect;
        
        [Header("Settings")]
        [SerializeField] private int maxHistorySize = 100;
        [SerializeField] private int maxOutputLines = 500;
        [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote;
        
        private bool isVisible = false;
        private ConsoleRegistry registry;
        private ConsoleOutput output;
        private List<string> commandHistory = new List<string>();
        private int historyIndex = -1;
        private string currentInput = "";
        
        // Simulation references (injected or found)
        private Simulation.SimulationState simulationState;
        private Simulation.WorkflowEngine workflowEngine;
        
        private void Awake()
        {
            // Initialize console system
            registry = new ConsoleRegistry();
            output = new ConsoleOutput(OnNewOutputLine);
            
            // Initialize simulation state if not provided
            if (simulationState == null)
            {
                simulationState = new Simulation.SimulationState();
            }
            
            if (workflowEngine == null)
            {
                workflowEngine = new Simulation.WorkflowEngine();
            }
            
            // Setup UI
            if (consolePanel != null)
            {
                consolePanel.SetActive(false);
            }
            
            if (inputField != null)
            {
                inputField.onSubmit.AddListener(OnSubmitCommand);
            }
            
            // Welcome message
            output.WriteLine("=== PureSim Developer Console ===");
            output.WriteLine("Type 'help' for available commands");
            output.WriteLine("");
            
            UpdateOutputDisplay();
        }
        
        private void Update()
        {
            // Toggle console visibility
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleConsole();
            }
            
            // Handle history navigation when console is visible
            if (isVisible && inputField != null && inputField.isFocused)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    NavigateHistory(-1);
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    NavigateHistory(1);
                }
                else if (Input.GetKeyDown(KeyCode.Tab))
                {
                    AutoComplete();
                }
            }
        }
        
        private void ToggleConsole()
        {
            isVisible = !isVisible;
            
            if (consolePanel != null)
            {
                consolePanel.SetActive(isVisible);
            }
            
            if (isVisible && inputField != null)
            {
                inputField.text = "";
                inputField.ActivateInputField();
                inputField.Select();
            }
        }
        
        private void OnSubmitCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                // Re-focus for next command
                if (inputField != null)
                {
                    inputField.text = "";
                    inputField.ActivateInputField();
                }
                return;
            }
            
            // Add to history
            commandHistory.Add(input);
            if (commandHistory.Count > maxHistorySize)
            {
                commandHistory.RemoveAt(0);
            }
            historyIndex = commandHistory.Count;
            
            // Echo command
            output.WriteLine($"> {input}");
            
            // Parse and execute
            ExecuteCommand(input);
            
            // Update display
            UpdateOutputDisplay();
            ScrollToBottom();
            
            // Clear and re-focus input
            if (inputField != null)
            {
                inputField.text = "";
                inputField.ActivateInputField();
            }
        }
        
        private void ExecuteCommand(string input)
        {
            var parts = ParseCommand(input);
            if (parts.Length == 0)
                return;
            
            string commandName = parts[0].ToLowerInvariant();
            string[] args = parts.Skip(1).ToArray();
            
            if (registry.TryGetCommand(commandName, out IConsoleCommand command))
            {
                try
                {
                    command.Execute(simulationState, args, output);
                }
                catch (Exception e)
                {
                    output.WriteError($"Command execution failed: {e.Message}");
                    Debug.LogException(e);
                }
            }
            else
            {
                output.WriteError($"Unknown command: {commandName}");
                output.WriteLine("Type 'help' for available commands");
            }
        }
        
        private string[] ParseCommand(string input)
        {
            // Simple tokenization - split by whitespace, respect quotes
            var tokens = new List<string>();
            bool inQuotes = false;
            var currentToken = new System.Text.StringBuilder();
            
            foreach (char c in input)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (currentToken.Length > 0)
                    {
                        tokens.Add(currentToken.ToString());
                        currentToken.Clear();
                    }
                }
                else
                {
                    currentToken.Append(c);
                }
            }
            
            if (currentToken.Length > 0)
            {
                tokens.Add(currentToken.ToString());
            }
            
            return tokens.ToArray();
        }
        
        private void NavigateHistory(int direction)
        {
            if (commandHistory.Count == 0)
                return;
            
            // Save current input if moving from current
            if (historyIndex == commandHistory.Count)
            {
                currentInput = inputField.text;
            }
            
            historyIndex += direction;
            historyIndex = Mathf.Clamp(historyIndex, 0, commandHistory.Count);
            
            if (inputField != null)
            {
                if (historyIndex < commandHistory.Count)
                {
                    inputField.text = commandHistory[historyIndex];
                }
                else
                {
                    inputField.text = currentInput;
                }
                
                // Move caret to end
                inputField.caretPosition = inputField.text.Length;
            }
        }
        
        private void AutoComplete()
        {
            if (inputField == null)
                return;
            
            string input = inputField.text;
            if (string.IsNullOrWhiteSpace(input))
                return;
            
            var parts = ParseCommand(input);
            if (parts.Length == 0)
                return;
            
            string prefix = parts[0].ToLowerInvariant();
            var matches = registry.GetAllCommandNames()
                .Where(cmd => cmd.StartsWith(prefix))
                .ToList();
            
            if (matches.Count == 1)
            {
                // Single match - complete it
                inputField.text = matches[0];
                inputField.caretPosition = inputField.text.Length;
            }
            else if (matches.Count > 1)
            {
                // Multiple matches - show them
                output.WriteLine($"Matching commands: {string.Join(", ", matches)}");
                UpdateOutputDisplay();
            }
        }
        
        private void OnNewOutputLine(string line)
        {
            // Called when new output is added
        }
        
        private void UpdateOutputDisplay()
        {
            if (outputText == null)
                return;
            
            var lines = output.GetLines();
            
            // Limit output lines
            if (lines.Count > maxOutputLines)
            {
                lines = lines.Skip(lines.Count - maxOutputLines).ToList();
            }
            
            outputText.text = string.Join("\n", lines);
        }
        
        private void ScrollToBottom()
        {
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }
        
        // Public API for external access
        public void SetSimulationState(Simulation.SimulationState state)
        {
            simulationState = state;
        }
        
        public void SetWorkflowEngine(Simulation.WorkflowEngine engine)
        {
            workflowEngine = engine;
        }
        
        public Simulation.WorkflowEngine GetWorkflowEngine() => workflowEngine;
        
        public void Show()
        {
            if (!isVisible)
                ToggleConsole();
        }
        
        public void Hide()
        {
            if (isVisible)
                ToggleConsole();
        }
    }
}
