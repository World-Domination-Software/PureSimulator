using CrimsofallTechnologies.ServerSimulator;
using UnityEngine;

[DefaultExecutionOrder(0)]
public class AutoHarddriveFiller : MonoBehaviour
{
    public Chassis chassis;
    public GameObject harddrivePrefab;
    public Vector3 seperator = new Vector3(0,0,0.55f);
    public int amount = 3;
    public bool harddrivesInserted = true;

    public int Init(int index, string installLocation)
    {
        int driveIndex = index;
        Transform initialHardDrive = transform.GetChild(0);
        initialHardDrive.GetComponent<HardDrive>().Init(chassis, GetRandomSize(), driveIndex, $"{installLocation}.BAY" + driveIndex, HardDriveStatus.healthy, harddrivesInserted);

        for (int i = 1; i < amount + 1; i++)
        {
            Vector3 pos = initialHardDrive.position - (seperator * i);
            driveIndex++;
            Instantiate(harddrivePrefab, pos, initialHardDrive.rotation, transform).GetComponent<HardDrive>().Init(chassis, GetRandomSize(), driveIndex, $"{installLocation}.BAY" + driveIndex, HardDriveStatus.healthy, harddrivesInserted);
        }
        return driveIndex;
    }

    public double GetRandomSize() 
    {
        //1TB = 1,000,000 MB
        return 1740000;
    }
}
