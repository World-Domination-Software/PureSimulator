using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class USBPort : MonoBehaviour
{
    public string controllerID = "CT0";
    public Animator animator;
    public GameObject flashDriveModel;
    public Chassis chassis;

    public string purityVersionOnDrive = "6.5.8";

    private bool driveInserted;
    public string[] Files = new string[] 
    {
        "purity_6.5.8.ppkg", "purity_6.5.8.ppkg.sha1"
    };

    public string[] Folders = new string[]
    {
        "/sda1", "/sdb1"
    };

    private void Start()
    {
        flashDriveModel.SetActive(false);
    }

    /*public string GetFoldersName() 
    {
        if (driveInserted)
        {
            string s = 
            for (int i = 0; i < Folders.Length; i++)
            {

            }
        }
        else
        {
            return ".";
        };
    }*/

    //returns packages on this USB drive
    public string GetFilesInside() 
    {
        string f = Files[0];
        for (int i = 1; i < Files.Length; i++)
        {
            f += "    " + Files[i];
        }
        return f;
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
    }

    public void RemoveDrive() 
    {
        driveInserted = false;
        animator.SetBool("Insert", false);
        chassis.InsertedUsbPort = null;
        Invoke(nameof(SetDriveFalse), 1f);
    }

    public void SetDriveFalse() 
    {
        flashDriveModel.SetActive(false);
    }

    public string GetFolder(int i)
    {
        return "/dev/" + Folders[i];
    }
}
