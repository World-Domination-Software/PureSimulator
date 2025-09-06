using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class USBPort : MonoBehaviour
{
    public VirtualDirectory Dir;

    public string controllerID = "CT0";
    public Animator animator;
    public GameObject flashDriveModel;
    public Chassis chassis;

    public string purityVersionOnDrive = "6.5.8";

    private bool driveInserted;
    /*public string[] Files = new string[] 
    {
        "purity_6.5.8.ppkg", "purity_6.5.8.ppkg.sha1"
    };*/

    private void Start()
    {
        flashDriveModel.SetActive(false);
        Dir.DirectoryName = "None";
    }

    //universal serial bus port?
    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        //do not add/remove USB while user is setting up OS!
        if (chassis.commandProcessor.isSettingUpOS) 
        {
            return;
        }

        //do not let player plug flash-drives when OS is ok!
        if (chassis.OSInstalled() && !driveInserted)
        {
            return;
        }

        if (chassis.InsertedUsbPort == this) 
        {
            RemoveDrive();
            return;
        }

        if (chassis.InsertedUsbPort != null)
        {
            chassis.InsertedUsbPort.RemoveDrive();
            Invoke(nameof(InsertDrive), 1f);
            return;
        }

        InsertDrive();
    }

    public void InsertDrive() 
    {
        driveInserted = true;
        animator.SetBool("Insert", true);
        flashDriveModel.SetActive(true);
        chassis.InsertedUsbPort = this;

        Dir.DirectoryName = chassis.GetNewRandomUSBName();
    }

    public void RemoveDrive() 
    {
        driveInserted = false;
        animator.SetBool("Insert", false);
        chassis.InsertedUsbPort = null;
        chassis.OnRemoveUsb(Dir.DirectoryName);
        Dir.DirectoryName = "None";

        Invoke(nameof(SetDriveFalse), 1f);
    }

    public void SetDriveFalse() 
    {
        flashDriveModel.SetActive(false);
    }
}
