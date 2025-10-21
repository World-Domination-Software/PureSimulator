using System;
using System.Collections.Generic;
using UnityEngine;

namespace PureSim.Simulation
{
    /// <summary>
    /// Manages workflow execution for installation and upgrade procedures.
    /// Steps have pre/post conditions and guards for safe jumping between steps.
    /// </summary>
    public class WorkflowEngine
    {
        private List<WorkflowStep> steps = new List<WorkflowStep>();
        private int currentStepIndex = -1;
        
        public event Action<WorkflowStep> OnStepChanged;
        
        public WorkflowEngine()
        {
            InitializeDefaultWorkflow();
        }
        
        private void InitializeDefaultWorkflow()
        {
            // Define standard installation workflow
            steps.Add(new WorkflowStep
            {
                Id = "usb-detect",
                Name = "USB Detection",
                Description = "Detect USB media containing firmware image",
                Preconditions = new List<string>(),
                Postconditions = new List<string> { "usb-inserted" }
            });
            
            steps.Add(new WorkflowStep
            {
                Id = "device-select",
                Name = "Device Selection",
                Description = "Select correct device for firmware",
                Preconditions = new List<string> { "usb-inserted" },
                Postconditions = new List<string> { "device-selected" }
            });
            
            steps.Add(new WorkflowStep
            {
                Id = "mount",
                Name = "Mount USB",
                Description = "Mount USB device to access firmware",
                Preconditions = new List<string> { "usb-inserted", "device-selected" },
                Postconditions = new List<string> { "usb-mounted" }
            });
            
            steps.Add(new WorkflowStep
            {
                Id = "validate",
                Name = "Image Validation",
                Description = "Validate firmware image integrity",
                Preconditions = new List<string> { "usb-mounted" },
                Postconditions = new List<string> { "image-validated" }
            });
            
            steps.Add(new WorkflowStep
            {
                Id = "apply",
                Name = "Apply Firmware",
                Description = "Apply firmware to controller",
                Preconditions = new List<string> { "image-validated" },
                Postconditions = new List<string> { "firmware-applied" }
            });
            
            steps.Add(new WorkflowStep
            {
                Id = "controller-swap",
                Name = "Controller Swap",
                Description = "Swap to upgraded controller",
                Preconditions = new List<string> { "firmware-applied" },
                Postconditions = new List<string> { "controller-swapped" }
            });
            
            steps.Add(new WorkflowStep
            {
                Id = "health-check",
                Name = "Health Check",
                Description = "Verify system health after upgrade",
                Preconditions = new List<string> { "controller-swapped" },
                Postconditions = new List<string> { "health-verified" }
            });
        }
        
        public List<WorkflowStep> GetAllSteps() => new List<WorkflowStep>(steps);
        
        public WorkflowStep GetCurrentStep()
        {
            if (currentStepIndex >= 0 && currentStepIndex < steps.Count)
                return steps[currentStepIndex];
            return null;
        }
        
        public int GetCurrentStepIndex() => currentStepIndex;
        
        public bool CanJumpToStep(string stepId, SimulationState state, out List<string> failedPreconditions)
        {
            failedPreconditions = new List<string>();
            
            var step = steps.Find(s => s.Id == stepId);
            if (step == null)
            {
                failedPreconditions.Add($"Step '{stepId}' not found");
                return false;
            }
            
            // Check preconditions based on simulation state
            foreach (var precondition in step.Preconditions)
            {
                if (!IsPreconditionMet(precondition, state))
                {
                    failedPreconditions.Add(precondition);
                }
            }
            
            return failedPreconditions.Count == 0;
        }
        
        private bool IsPreconditionMet(string precondition, SimulationState state)
        {
            switch (precondition)
            {
                case "usb-inserted":
                    return state.IsUsbInserted();
                case "usb-mounted":
                    return state.IsUsbMounted();
                case "device-selected":
                    // Simplified: assume selected if USB is inserted
                    return state.IsUsbInserted();
                case "image-validated":
                    // Simplified: assume validated if mounted
                    return state.IsUsbMounted();
                case "firmware-applied":
                case "controller-swapped":
                case "health-verified":
                    // These would be tracked in more complex state
                    return false;
                default:
                    return false;
            }
        }
        
        public bool JumpToStep(string stepId, SimulationState state)
        {
            if (CanJumpToStep(stepId, state, out var failedPreconditions))
            {
                int index = steps.FindIndex(s => s.Id == stepId);
                if (index >= 0)
                {
                    currentStepIndex = index;
                    OnStepChanged?.Invoke(steps[index]);
                    return true;
                }
            }
            return false;
        }
        
        public bool AdvanceToNextStep(SimulationState state)
        {
            if (currentStepIndex < steps.Count - 1)
            {
                var nextStep = steps[currentStepIndex + 1];
                return JumpToStep(nextStep.Id, state);
            }
            return false;
        }
        
        public WorkflowStep GetStepById(string stepId)
        {
            return steps.Find(s => s.Id == stepId);
        }
    }
    
    [Serializable]
    public class WorkflowStep
    {
        public string Id;
        public string Name;
        public string Description;
        public List<string> Preconditions = new List<string>();
        public List<string> Postconditions = new List<string>();
    }
}
