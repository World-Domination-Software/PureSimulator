using System.Collections.Generic;
using UnityEngine;

namespace PureSim.Console.Commands
{
    /// <summary>
    /// Steps command - lists all workflow steps and their current status.
    /// </summary>
    [ConsoleCommand("steps")]
    public class StepsCommand : IConsoleCommand
    {
        public string Name => "steps";
        public string Synopsis => "List all workflow steps and their status";
        public IReadOnlyList<string> Parameters => new string[0];
        
        public void Execute(Simulation.SimulationState sim, string[] args, IConsoleOutput output)
        {
            // Get workflow engine from ConsoleController
            var controller = GameObject.FindObjectOfType<ConsoleController>();
            if (controller == null)
            {
                output.WriteError("ConsoleController not found in scene");
                return;
            }
            
            var workflowEngine = controller.GetWorkflowEngine();
            if (workflowEngine == null)
            {
                output.WriteError("WorkflowEngine not initialized");
                return;
            }
            
            var steps = workflowEngine.GetAllSteps();
            var currentStep = workflowEngine.GetCurrentStep();
            int currentIndex = workflowEngine.GetCurrentStepIndex();
            
            output.WriteLine("Workflow Steps:");
            output.WriteLine("");
            
            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                bool isCurrent = (i == currentIndex);
                bool canJump = workflowEngine.CanJumpToStep(step.Id, sim, out _);
                
                string statusIcon;
                if (isCurrent)
                    statusIcon = "►"; // Current step
                else if (i < currentIndex)
                    statusIcon = "✓"; // Completed step
                else if (canJump)
                    statusIcon = "○"; // Available step
                else
                    statusIcon = "✗"; // Blocked step
                
                string line = $"{statusIcon} [{step.Id}] {step.Name}";
                if (isCurrent)
                    output.WriteLine($"<color=cyan>{line}</color>");
                else if (canJump)
                    output.WriteLine($"<color=green>{line}</color>");
                else
                    output.WriteLine($"<color=grey>{line}</color>");
                
                output.WriteLine($"    {step.Description}");
                
                if (step.Preconditions.Count > 0)
                {
                    output.WriteLine($"    Preconditions: {string.Join(", ", step.Preconditions)}");
                }
                
                output.WriteLine("");
            }
            
            output.WriteLine("Legend: ► = current, ✓ = completed, ○ = available, ✗ = blocked");
            output.WriteLine("Use 'jump <step_id>' to jump to a specific step");
        }
    }
}
