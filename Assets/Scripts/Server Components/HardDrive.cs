using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CrimsofallTechnologies.ServerSimulator
{
    public class HardDrive : MonoBehaviour
    {
        public GameObject GFX;

        [Tooltip("In MB")]
        public double StorageSpace;
        public HardDriveStatus status;
        public HardDriveType type = HardDriveType.SSD;

        public Animator animator;
        public MeshRenderer mRenderer;
        public Material greenMat, amberMat, offMat;

        private string InstalledLocation;
        private bool firmwareInstalled;
        public bool settingUp { get; private set; }
        public int myIndex { get; set; }

        private string installStatus;
        private Chassis myChassis;
        private bool inserting;

        public void Init(Chassis chassis, double size, int index, string installLocation, HardDriveStatus _status, bool isInserted = true)
        {
            if (isInserted)
            {
                status = _status;
                settingUp = false;
                firmwareInstalled = true;
            }
            else
            {
                status = HardDriveStatus.not_inserted;
            }

            myChassis = chassis;
            InstalledLocation = installLocation;
            StorageSpace = size;
            myIndex = index;
            GFX.SetActive(isInserted);
            SetLightsStatus();
        }
        
        public string GetString()
        {
            string space = "";

            if (myIndex <= 9) space = " ";

            if (firmwareInstalled == false) 
            {
                return $"{InstalledLocation}{space}          -            unknown          -  ";
            }

            if (status == HardDriveStatus.unused)
                return $"{InstalledLocation}{space}          -            {status.ToString()}          -  ";
           
            return $"{InstalledLocation}{space}          {type.ToString()}          {status.ToString()}          {StorageSpace/1000000}T";
        }

        public void SetLightsStatus() 
        {
            Material[] mats = mRenderer.materials;
            if (!myChassis.IsOn()) 
            {
                mats[1] = offMat;
                mats[2] = offMat;
                mRenderer.materials = mats;
                return;
            }

            if (status == HardDriveStatus.healthy)
            {
                mats[1] = greenMat;
                mats[2] = greenMat;
                mRenderer.materials = mats;
                return;
            }

            if(status == HardDriveStatus.unhealthy) 
            {
                mats[1] = amberMat;
                mats[2] = greenMat;
                mRenderer.materials = mats;
                return;
            }

            if (status == HardDriveStatus.chassis_os_empty) 
            {
                mats[1] = offMat;
                mats[2] = greenMat;
                mRenderer.materials = mats;
                return;
            }

            mats[1] = offMat;
            mats[2] = offMat;
            mRenderer.materials = mats;
        }

        public string GetInstallDetails() 
        {
            return installStatus;
        }

        //pressed on a collider? Remove or Insert it!
        public void OnMouseDown()
        {
            if (!UIManager.Instance.serverUI.activeSelf) //only pull/insert hard-drives when server UI is open!
                return;

            if (inserting)
                return;

            //only wait for chassis that has OS installed and ready!
            if (GlobalVar.lastHarddriveSettingUp && myChassis.IsOk())
            {
                UIManager.Instance.ShowErrorUI("You cannot do that now! Wait 30 seconds before inserting a new drive.");
                return;
            }

            //Can the server add this drive here - check if not removing?
            if (!myChassis.CanInsertMoreDrives() && status == HardDriveStatus.not_inserted) 
            {
                Debug.Log("cannot insert more...");
                return;
            }

            if (status != HardDriveStatus.not_inserted)
            {
                status = HardDriveStatus.not_inserted;
                SetLightsStatus();

                Invoke(nameof(DisableGfx), 2.25f);
                animator.SetTrigger("Remove");
                myChassis.numInsertedDrives--;
            }
            else
            {
                firmwareInstalled = false;
                settingUp = true;
                myChassis.numInsertedDrives ++;

                if (myChassis.IsOk())
                {
                    installStatus = $"Not accessible from {myChassis.selectedController}";
                    status = HardDriveStatus.unhealthy;
                    GlobalVar.lastHarddriveSettingUp = true;
                    Invoke(nameof(SettingUp), 5f);
                }
                else 
                {
                    status = HardDriveStatus.chassis_os_empty;
                }

                Invoke(nameof(EnableGfx), 0.25f);
                Invoke(nameof(SetLightsStatus), 2.25f);
                animator.SetTrigger("Insert");
            }
        }

        public void Disable() 
        {
            status = HardDriveStatus.not_inserted;
            SetLightsStatus();
            DisableGfx();
        }

        private void SettingUp() 
        {
            installStatus = "WSSD tuneup in progress (https://go/w2)";
            status = HardDriveStatus.recovering;
            Invoke(nameof(SettingUp2), 5f);
        }

        private void SettingUp2() 
        {
            installStatus = "firmware upgrade in progress. Current firmware: 2.0.30(https://go/w2)";
            Invoke(nameof(UpgradedFirmware), 8f);
        }

        private void UpgradedFirmware() 
        {
            installStatus = "waiting for user";
            status = HardDriveStatus.unadmitted;
            Invoke(nameof(UserWait), 4f);
        }

        private void UserWait() {

            status = HardDriveStatus.updating;
            installStatus = "-";
            Invoke(nameof(InstalledFirmware), 8f);
        }

        public void InstalledFirmware() 
        {
            installStatus = "Installed";
            firmwareInstalled = true;
            status = HardDriveStatus.healthy;
            SetLightsStatus();
            settingUp = false;
            GlobalVar.lastHarddriveSettingUp = false;
        }

        private void EnableGfx() { GFX.SetActive(true); }
        private void DisableGfx() { GFX.SetActive(false); }

        //animation events!
        public void OnAnim_Insert() { inserting = true; }
        public void OnAnim_Complete() { inserting = false; }
    }
}

public enum HardDriveStatus 
{
    healthy, unused, unknown, unhealthy, not_inserted, updating, recovering, unadmitted, chassis_os_empty
}

public enum HardDriveType 
{
    SSD, HDD
}