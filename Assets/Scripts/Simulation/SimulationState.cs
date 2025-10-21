using System;
using System.Collections.Generic;
using UnityEngine;

namespace PureSim.Simulation
{
    /// <summary>
    /// Single source of truth for the entire simulation state.
    /// Tracks arrays, controllers, shelves, ports, cables, power, media/USB, and host devices.
    /// Designed to be serializable and checkpointable.
    /// </summary>
    [Serializable]
    public class SimulationState
    {
        // Hardware model - complete hardware representation
        [SerializeField] private HardwareModel hardwareModel = new HardwareModel();
        
        // Array and workflow state
        [SerializeField] private List<ArrayState> arrays = new List<ArrayState>();
        [SerializeField] private List<ControllerState> controllers = new List<ControllerState>();
        [SerializeField] private List<ShelfState> shelves = new List<ShelfState>();
        
        // Media state
        [SerializeField] private bool usbInserted = false;
        [SerializeField] private string usbDeviceName = "/dev/sdb1";
        [SerializeField] private bool usbMounted = false;
        [SerializeField] private string usbMountPath = "/mnt";
        
        // Power state
        [SerializeField] private Dictionary<string, bool> powerStates = new Dictionary<string, bool>();
        
        // Fault injection
        [SerializeField] private List<ActiveFault> activeFaults = new List<ActiveFault>();
        
        // Events
        public event Action<string> OnStateChanged;
        
        public SimulationState()
        {
            Initialize();
        }
        
        private void Initialize()
        {
            // Initialize with default state
            usbInserted = false;
            usbMounted = false;
        }
        
        // USB Media Management
        public bool IsUsbInserted() => usbInserted;
        
        public void SetUsbInserted(bool inserted)
        {
            if (usbInserted != inserted)
            {
                usbInserted = inserted;
                if (!inserted)
                {
                    usbMounted = false; // Auto-unmount if removed
                }
                OnStateChanged?.Invoke($"USB {(inserted ? "inserted" : "removed")}");
            }
        }
        
        public string GetUsbDeviceName() => usbDeviceName;
        
        public bool IsUsbMounted() => usbMounted;
        
        public void SetUsbMounted(bool mounted)
        {
            if (!usbInserted && mounted)
            {
                throw new InvalidOperationException("Cannot mount USB when not inserted");
            }
            usbMounted = mounted;
            OnStateChanged?.Invoke($"USB {(mounted ? "mounted" : "unmounted")}");
        }
        
        public string GetUsbMountPath() => usbMountPath;
        
        // Fault Management
        public void InjectFault(string faultId, string description)
        {
            if (!activeFaults.Exists(f => f.Id == faultId))
            {
                activeFaults.Add(new ActiveFault { Id = faultId, Description = description });
                OnStateChanged?.Invoke($"Fault injected: {faultId}");
            }
        }
        
        public void ClearFault(string faultId)
        {
            int removed = activeFaults.RemoveAll(f => f.Id == faultId);
            if (removed > 0)
            {
                OnStateChanged?.Invoke($"Fault cleared: {faultId}");
            }
        }
        
        public bool HasFault(string faultId)
        {
            return activeFaults.Exists(f => f.Id == faultId);
        }
        
        public List<ActiveFault> GetActiveFaults() => new List<ActiveFault>(activeFaults);
        
        // Power Management
        public void SetPowerState(string component, bool powered)
        {
            powerStates[component] = powered;
            OnStateChanged?.Invoke($"Power {(powered ? "on" : "off")}: {component}");
        }
        
        public bool GetPowerState(string component)
        {
            return powerStates.TryGetValue(component, out bool state) ? state : true;
        }
        
        // Hardware Model Access
        public HardwareModel GetHardwareModel() => hardwareModel;
        
        // Serialization support
        public string Serialize()
        {
            return JsonUtility.ToJson(this, true);
        }
        
        public static SimulationState Deserialize(string json)
        {
            return JsonUtility.FromJson<SimulationState>(json);
        }
    }
    
    [Serializable]
    public class ArrayState
    {
        public string Name;
        public string Model;
        public string Version;
    }
    
    [Serializable]
    public class ControllerState
    {
        public string Name;
        public string Type;
        public string Mode;
        public string Status;
    }
    
    [Serializable]
    public class ShelfState
    {
        public string Name;
        public int BayCount;
    }
    
    [Serializable]
    public class ActiveFault
    {
        public string Id;
        public string Description;
    }
}
