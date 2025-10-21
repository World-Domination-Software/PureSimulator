using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PureSim.Console.Commands
{
    /// <summary>
    /// Jump command - jumps to a specific workflow step if preconditions are met.
    /// Guards against invalid jumps and reports missing preconditions clearly.
    /// </summary>
    [ConsoleCommand("jump")]
    public class JumpCommand : IConsoleCommand
    {
        public string Name => "jump";
        public string Synopsis => "Jump to a specific workflow step";
        public IReadOnlyList<string> Parameters => new[] { "<step_id>" };
        
        public void Execute(Simulation.SimulationState sim, string[] args, IConsoleOutput output)
        {
            if (args.Length == 0)
            {
                output.WriteError("Usage: jump <step_id>");
                output.WriteLine("Use 'steps' command to see available step IDs");
                return;
            }
            
            string stepId = args[0];
            
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
            
            // Check if step exists
            var step = workflowEngine.GetStepById(stepId);
            if (step == null)
            {
                output.WriteError($"Step '{stepId}' not found");
                output.WriteLine("Use 'steps' command to see available steps");
                return;
            }
            
            // Check if we can jump to this step
            if (workflowEngine.CanJumpToStep(stepId, sim, out List<string> failedPreconditions))
            {
                // Perform the jump
                if (workflowEngine.JumpToStep(stepId, sim))
                {
                    output.WriteSuccess($"Jumped to step: {step.Name} ({step.Id})");
                    output.WriteLine($"Description: {step.Description}");
                }
                else
                {
                    output.WriteError("Failed to jump to step (unknown error)");
                }
            }
            else
            {
                // Report blocked jump with clear precondition failures
                output.WriteError($"Cannot jump to step '{step.Name}' ({stepId})");
                output.WriteLine("");
                output.WriteLine("Missing preconditions:");
                
                foreach (var precondition in failedPreconditions)
                {
                    output.WriteLine($"  • {precondition}");
                }
                
                output.WriteLine("");
                output.WriteLine("Resolve these conditions before jumping to this step.");
                output.WriteLine("Use simulator commands to change state (e.g., 'usb state inserted')");
            }
        }
    }
}
