using UnityEngine;
using CrimsofallTechnologies.ServerSimulator;

/// <summary>
/// Example script showing how to configure drive sizes and USB versions
/// This demonstrates the extensibility features added for future UI development
/// </summary>
public class DriveConfigurationExample : MonoBehaviour
{
    [Header("Drive Size Presets (in TB)")]
    public float[] availableSizes = new float[] { 1.92f, 3.84f, 7.68f, 15.36f, 30.72f };
    
    [Header("Purity Version Presets")]
    public string[] availableVersions = new string[] { "6.4.0", "6.5.0", "6.5.8", "6.6.0", "6.6.5", "6.7.0" };
    
    /// <summary>
    /// Example: Set all drives in a chassis to a specific size
    /// </summary>
    public void SetAllDrivesToSize(Chassis chassis, float sizeInTB)
    {
        double sizeInMB = sizeInTB * 1000000; // Convert TB to MB
        
        foreach (var drive in chassis.HardDrives)
        {
            if (drive != null)
            {
                drive.SetStorageSize(sizeInMB);
                Debug.Log($"Set {drive.name} to {sizeInTB}TB");
            }
        }
    }
    
    /// <summary>
    /// Example: Configure individual drive size
    /// </summary>
    public void ConfigureDrive(HardDrive drive, float sizeInTB)
    {
        double sizeInMB = sizeInTB * 1000000;
        drive.SetStorageSize(sizeInMB);
        Debug.Log($"Drive configured: {sizeInTB}TB ({drive.GetFormattedSize()})");
    }
    
    /// <summary>
    /// Example: Set up a typical X20R4 configuration
    /// X20 typically has 10 drive bays with 1.92TB SSDs
    /// </summary>
    public void ConfigureX20R4(Chassis chassis)
    {
        const float driveSize = 1.92f; // TB
        double sizeInMB = driveSize * 1000000;
        
        // Configure first 10 drives (X20 configuration)
        for (int i = 0; i < 10 && i < chassis.HardDrives.Length; i++)
        {
            if (chassis.HardDrives[i] != null)
            {
                chassis.HardDrives[i].SetStorageSize(sizeInMB);
            }
        }
        
        Debug.Log("Configured as X20R4: 10 drives @ 1.92TB each");
    }
    
    /// <summary>
    /// Example: Set up a typical X70R3 configuration
    /// X70 typically has 20+ drive bays with 3.84TB or 7.68TB SSDs
    /// </summary>
    public void ConfigureX70R3(Chassis chassis)
    {
        const float driveSize = 3.84f; // TB
        double sizeInMB = driveSize * 1000000;
        
        // Configure all available drives
        foreach (var drive in chassis.HardDrives)
        {
            if (drive != null)
            {
                drive.SetStorageSize(sizeInMB);
            }
        }
        
        Debug.Log($"Configured as X70R3: {chassis.HardDrives.Length} drives @ {driveSize}TB each");
    }
    
    /// <summary>
    /// Example: Set USB drive to specific Purity version
    /// </summary>
    public void SetUSBPurityVersion(USBPort usbPort, string version)
    {
        usbPort.SetPurityVersion(version);
        Debug.Log($"USB drive now contains Purity {version}");
        Debug.Log($"Files: purity_{version}.ppkg, purity_{version}.ppkg.sha1");
    }
    
    /// <summary>
    /// Example: Create USB drives for multiple versions (testing upgrades)
    /// In a real implementation, you would have multiple USB ports or a UI to select versions
    /// </summary>
    public void PrepareUpgradeScenario(USBPort currentUSB, string currentVersion, string targetVersion)
    {
        Debug.Log($"Preparing upgrade from {currentVersion} to {targetVersion}");
        
        // The USB can be virtually "swapped" by changing its version
        Debug.Log($"Current USB has Purity {currentUSB.GetPurityVersion()}");
        
        // To simulate upgrade, user would:
        // 1. Remove current USB (if any)
        // 2. Insert new USB with target version
        currentUSB.SetPurityVersion(targetVersion);
        
        Debug.Log($"Ready to upgrade to {targetVersion}");
    }
    
    /// <summary>
    /// Example: Get drive capacity information
    /// </summary>
    public void PrintDriveInfo(HardDrive drive)
    {
        Debug.Log($"Drive Information:");
        Debug.Log($"  Formatted Size: {drive.GetFormattedSize()}");
        Debug.Log($"  Size in TB: {drive.GetSizeInTB()}");
        Debug.Log($"  Size in GB: {drive.GetSizeInGB()}");
        Debug.Log($"  Size in MB: {drive.GetSizeInMB()}");
    }
    
    /// <summary>
    /// Example: Calculate total array capacity
    /// </summary>
    public void PrintTotalCapacity(Chassis chassis)
    {
        double totalTB = 0;
        int driveCount = 0;
        
        foreach (var drive in chassis.HardDrives)
        {
            if (drive != null && drive.status != HardDriveStatus.not_inserted)
            {
                totalTB += drive.GetSizeInTB();
                driveCount++;
            }
        }
        
        Debug.Log($"Array Capacity: {driveCount} drives, {totalTB:F2}TB total");
    }
    
    // Example Unity Editor menu items (optional)
    #if UNITY_EDITOR
    [UnityEditor.MenuItem("Pure Simulator/Examples/Configure X20R4")]
    private static void MenuConfigureX20R4()
    {
        var chassis = FindObjectOfType<Chassis>();
        if (chassis != null)
        {
            var example = new DriveConfigurationExample();
            example.ConfigureX20R4(chassis);
        }
        else
        {
            Debug.LogWarning("No Chassis found in scene");
        }
    }
    
    [UnityEditor.MenuItem("Pure Simulator/Examples/Configure X70R3")]
    private static void MenuConfigureX70R3()
    {
        var chassis = FindObjectOfType<Chassis>();
        if (chassis != null)
        {
            var example = new DriveConfigurationExample();
            example.ConfigureX70R3(chassis);
        }
        else
        {
            Debug.LogWarning("No Chassis found in scene");
        }
    }
    #endif
}
