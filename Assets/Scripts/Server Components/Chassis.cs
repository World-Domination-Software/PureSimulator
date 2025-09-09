using CrimsofallTechnologies.ServerSimulator;
using System.Collections.Generic;
using UnityEngine;

public class Chassis : MonoBehaviour
{
    public bool isShelf = false; //is the chassis an Array or a Shelf?
    public bool OnAsDefault = true; //connects ETH and PSU cables to this array and makes it fully OK with all drives inserted
    public bool DisableWiresConnection = false; //disables connection of any service cables to this chassis

    public Chassis ConnectedShelfChassis = null;

    [Space]
    public int chassisIndex = 0; //chassis index?
    public string selectedController = ""; //either CT0 or CT1
    public string InstallSize_Type = "X20R4";

    public string purityOSVersion = "6.5.8", purityOSVersion1 = "6.5.8";
    public string PurityVersionInPartition0 = "6.5.5", PurityVersionInPartition1 = "6.5.5";

    public CommandProcessor commandProcessor;
    public ServerRack rack;
    public WirePort insertedLaptopPort;
    public Cover Cover;

    [Space]
    public MeshRenderer[] ForeLights;
    public GameObject[] backLCD;
    public MeshRenderer mRenderer;

    [Space]
    public Material offMat;
    public Material greenMat, amberMat;

    public Animator animator { get; private set; }
    private NvRAM[] NvRam;
    public HardDrive[] HardDrives { get; private set; }
    private WirePort[] wirePorts;
    public FlashArray[] flashArrays = new FlashArray[0];
    public ChassisCommandsExtension commandsExtension;

    //files of the controllers
    private List<string> CT0Files = new(), CT1Files = new();

    //PSU power states
    private bool PSU0On = false;
    private bool PSU1On = false;
    private bool CT1Installed = false;
    public bool CT1FirmWareInstalled = false;
    private bool CT0Installed = false;
    public bool CT0FirmWareInstalled = false;
    private bool ETH0Connected = false;
    private bool ETH1Connected = false;
    private bool ETH2Connected = false;
    private bool ETH3Connected = false;
    public bool OSFullyRunning { get; set; }

    public bool ipConfigsDoneOnCT0;
    public bool ShowUpdateCompCT0, ShowUpdateCompCT1;

    public USBPort InsertedUsbPort { get; set; }

    //invokable actions
    public System.Action OnServerBootAction;
    public System.Action OnEthernetCablesConnected;

    public int numInsertedDrives = 0; //number of drives inserted in this array

    //USB Stuff
    private string[] PossibleUsbNames = new string[] { "sda1", "sdc1", "sdd1", "sde1", "sdf1" };
    private List<string> InsertedUSBDriveNames = new List<string>();

    #region USEFUL-VARS

    public FlashArray GetCurrentController() 
    {
        if (selectedController == "CT0") return flashArrays[0];
        if (selectedController == "CT1") return flashArrays[1];

        return null;
    }

    //returns a new random name for USB drives connected to the Machine:
    public string GetNewRandomUSBName()
    {
        //return sdb1 for the first USB inserted!
        if(!InsertedUSBDriveNames.Contains("sdb1")) {
            InsertedUSBDriveNames.Add("sdb1");
            return "sdb1";
        }
        else {
            int i = Random.Range(0, PossibleUsbNames.Length);
            InsertedUSBDriveNames.Add(PossibleUsbNames[i]);
            return PossibleUsbNames[i];
        }
    }

    public string GetSecondPurityPartVersion()
    {
        if (selectedController == "CT1")
            return PurityVersionInPartition1;

        return PurityVersionInPartition0;
    }
    public string GetCurrentPurityVersion(int index = -1)
    {
        if (index != -1) 
        {
            if (index == 0) return purityOSVersion;
            if (index == 1) return purityOSVersion1;
        }

        if (selectedController == "CT1") 
            return purityOSVersion1;

        return purityOSVersion;
    }

    public bool IsOn() 
    {
        if (PSU0On && PSU1On) return true;

        return false;
    }

    public bool CanInsertMoreDrives() 
    {
        if (InstallSize_Type.Contains("20") && numInsertedDrives >= 10) //only 10 drives max!
        {
            return false;
        }

        return true;
    }

    public string GetModel(int index) 
    {
        if (isShelf)
            return "DFSC1";

        if (index == 0) return flashArrays[0].ModelName;
        if (index == 1) return flashArrays[1].ModelName;

        return "unknown";
    }

    public string GetComputerName(int index = -1) 
    {
        if (index != -1) 
        {
            if (index == 0) return flashArrays[0].arrayName;
            if (index == 1) return flashArrays[1].arrayName;
        }

        if (selectedController == "CT0") return flashArrays[0].arrayName;
        if (selectedController == "CT1") return flashArrays[1].arrayName;

        return "UNKNOWN_CONTROLLER";
    }

    public void SetComputerName(string Name, int index) 
    {
        flashArrays[index].arrayName = Name;
    }

    #endregion

    public void ResetOSInstalls() { 
        CT1Installed = false; 
        CT0Installed = false; 
    }

    public void OnRemoveUsb(string Name)
    {
        if(InsertedUSBDriveNames.Contains(Name)) InsertedUSBDriveNames.Remove(Name);
    }

    public void OSInstallationComplete(string osVersion) 
    {
        if (selectedController == "CT0") { 
            CT0Installed = true;
            //purityOSVersion = osVersion;
            ShowUpdateCompCT0 = false;
        }
        if (selectedController == "CT1") { 
            CT1Installed = true;
            //purityOSVersion1 = osVersion;
            ShowUpdateCompCT1 = false;
        }

        //purityOSVersion = osVersion;
        SetChassisLights();
        Debug.Log($"Purity {(selectedController == "CT0" ? purityOSVersion : purityOSVersion1)} installed successfully on {GetComputerName()}-{selectedController}!");

        //update lights on harddrives too!
        for (int i = 0; i < HardDrives.Length; i++)
        {
            if (HardDrives[i].status != HardDriveStatus.not_inserted) //only for harddrives that are really inserted!
            {
                HardDrives[i].status = HardDriveStatus.healthy;
                HardDrives[i].SetLightsStatus();
            }
        }

        if (IsOk()) 
        {
            commandProcessor.tutor.TutorComplete();
        }
    }

    /// <summary>
    /// Returns true if OS is installed and chassis is powered on!
    /// </summary>
    /// <returns></returns>
    public bool IsOk()
    {
        if (OSInstalled() && IsOn())
            return true;

        return false;
    }

    //is the USB inserted in correct USB port?
    public bool UsbCorrect()
    {
        if (InsertedUsbPort != null && InsertedUsbPort.controllerID == selectedController)
            return true;

        return false;
    }

    public bool OSInstalled(int index = -1) 
    {
        if (index != -1) 
        {
            if (index == 0 && CT0Installed) return true;
            if (index == 1 && CT1Installed) return true;

            return false;
        }

        if (CT0Installed && CT1Installed)
            return true;

        return false;
    }

    public bool FirmwareInstalled() 
    {
        if (selectedController == "CT0" && CT0FirmWareInstalled) return true;
        if (selectedController == "CT1" && CT1FirmWareInstalled) return true;

        return false;
    }

    public bool OSInstalled(string controller) 
    {
        if (controller == "CT0" && CT0Installed)
            return true;

        if (controller == "CT1" && CT1Installed)
            return true;

        return false;
    }

    public void Init(bool NewInsert = false)
    {
        //set some default values
        selectedController = "CT0";

        if (OnAsDefault)
        {
            PSU0On = true;
            PSU1On = true;
            CT0Installed = true;
            CT1Installed = true;
            ETH0Connected = true;
            ETH1Connected = true;
            ETH2Connected = true;
            ETH3Connected = true;
            OSFullyRunning = true;
            CT0FirmWareInstalled = true;
            CT1FirmWareInstalled= true;

            Booted = true;
            //EthConnetionInvoked = true;
        }
        
        if(!OnAsDefault || NewInsert)
        {
            PSU0On = false;
            PSU1On = false;
            CT0Installed = false;
            CT1Installed = false;
            ETH0Connected = false;
            ETH1Connected = false;
            ETH2Connected = false;
            ETH3Connected = false;
            OSFullyRunning = false;
            CT0FirmWareInstalled = false;
            CT1FirmWareInstalled = false;

            purityOSVersion = "6.5.5";
            purityOSVersion1 = "6.5.5";
            PurityVersionInPartition0 = "6.5.5";
            PurityVersionInPartition1 = "6.5.5";
        }

        AutoHarddriveFiller[] hardDriveFillers = GetComponentsInChildren<AutoHarddriveFiller>();
        int bay = 0;
        for (int i = 0; i < hardDriveFillers.Length; i++)
        {
            bay = hardDriveFillers[i].Init(bay, "CH" + chassisIndex) + 1;
        }

        if (OnAsDefault && flashArrays.Length > 0)
        {
            flashArrays[0].arrayName = "pure00";
            flashArrays[1].arrayName = "pure01";
            flashArrays[0].SetupDefaultConfigs();
            flashArrays[1].SetupDefaultConfigs();
        }

        animator = GetComponent<Animator>();
        HardDrives = GetComponentsInChildren<HardDrive>();
        wirePorts = GetComponentsInChildren<WirePort>();
        NvRam = GetComponentsInChildren<NvRAM>();
        for (int i = 0; i < NvRam.Length; i++)
        {
            NvRam[i].Init();
            NvRam[i].SetLightColor(greenMat);
        }

        //Turn off lights
        if (isShelf)
        {
            backLCD[0].SetActive(false); //8 on
            backLCD[1].SetActive(true); //8 off

            backLCD[2].SetActive(false); //9 on
            backLCD[3].SetActive(true); //9 off

            Cover.SetLights(false);
        }

        //setup controllers/flash arrays!
        if(flashArrays.Length > 0)
        {
            for (int i = 0; i < flashArrays.Length; i++)
            {
                flashArrays[i].ModelName = InstallSize_Type;
            }

            flashArrays[0].State = "primary";
            flashArrays[1].State = "secondary";
        }

        for (int i = 0; i < wirePorts.Length; i++)
        {
            if (wirePorts[i].portName != "LAPTOP")
            {
                wirePorts[i].Init(this, true);
            }
            else
            {
                wirePorts[i].Init(this, false);
            }

            wirePorts[i].Interactable = !DisableWiresConnection;
        }

        //enable wires for this *installed* chassis!
        if (!NewInsert && OnAsDefault)
        {
            rack.EnableAllCablesForChassis(chassisIndex, isShelf);

            if (!isShelf)
            {
                insertedLaptopPort.Disable(false);
                insertedLaptopPort.otherWireToDisable.Disable(false);
                insertedLaptopPort = null; //make user insert the port by themselves.
            }
        }

        if (NewInsert) 
        {
            OnNewInsert();
        }

        SetChassisLights();
    }

    private void DelayedFirmwareInstall() 
    {
        commandProcessor.InstalledFirmware();
    }

    public void CopySettings(Chassis c) 
    {
        rack = c.rack;
        commandProcessor = c.commandProcessor;
        selectedController = "CT0";
    }

    //this will update all chassis status colors!
    public void SetChassisLights() 
    {
        if (ForeLights.Length <= 0) //means lights are not seperate meshes?
        {
            Material[] mats = mRenderer.materials;
            if (!IsOn())
            {
                mats[1] = offMat; //CT1 material
                mats[3] = offMat; //PS0 material
                mats[4] = offMat; //CT0 material
                mats[5] = offMat; //PS1 material
                mats[2] = offMat; //OK material

                for (int i = 0; i < flashArrays.Length; i++)
                {
                    flashArrays[i].SetLights(false, false);
                }
            }
            else
            {
                mats[1] = CT1Installed ? greenMat : amberMat; //CT1 material
                mats[3] = PSU0On ? greenMat : offMat; //PS0 material
                mats[4] = CT0Installed ? greenMat : amberMat; //CT0 material
                mats[5] = PSU1On ? greenMat : offMat; //PS1 material
                mats[2] = IsOk() ? greenMat : amberMat; //OK material - should only be ok when everything is ok!

                for (int i = 0; i < flashArrays.Length; i++)
                {
                    flashArrays[i].SetLights(IsOk(), true);
                }
            }
            mRenderer.materials = mats;
        }
        else 
        {
            if (!IsOn())
            {
                ForeLights[0].material = offMat; //CT1 material
                ForeLights[1].material = offMat; //PS0 material
                ForeLights[2].material = offMat; //CT0 material
                ForeLights[3].material = offMat; //PS1 material
                ForeLights[4].material = offMat; //OK material

                for (int i = 0; i < flashArrays.Length; i++)
                {
                    flashArrays[i].SetLights(false, false);
                }
            }
            else
            {
                ForeLights[0].material = CT1Installed ? greenMat : amberMat; //CT1 material
                ForeLights[1].material = PSU0On ? greenMat : offMat; //PS0 material
                ForeLights[2].material = CT0Installed ? greenMat : amberMat; //CT0 material
                ForeLights[3].material = PSU1On ? greenMat : offMat; //PS1 material
                ForeLights[4].material = IsOk() ? greenMat : amberMat; //OK material - should only be ok when everything is ok!

                for (int i = 0; i < flashArrays.Length; i++)
                {
                    flashArrays[i].SetLights(IsOk(), true);
                }
            }
        }
    }

    public int GetInsertedDrivesCount() 
    {
        int count = 0;
        for (int i = 0; i < HardDrives.Length; i++)
        {
            if (HardDrives[i].status != HardDriveStatus.not_inserted) 
            {
                count++;
            }
        }
        return count;
    }

    public string GetHardDrivesStatus()
    {
        string result = "Name               Type         Status           Capacity";

        for (int i = 0; i < HardDrives.Length; i++)
        {
            if (HardDrives[i].status != HardDriveStatus.not_inserted)
                result += $"\n{HardDrives[i].GetString()}";
        }

        return result;
    }

    public string GetControllerStatus(int index) 
    {
        if (index == 0 && CT0FirmWareInstalled && CT0Installed) return flashArrays[0].Status;
        if (index == 1 && CT1FirmWareInstalled && CT1Installed) return flashArrays[1].Status;

        return "ERR: unknown-controller-status";
    }

    public string GetControllerState(int index)
    {
        if (index == 0 && CT0FirmWareInstalled && CT0Installed) return flashArrays[0].State;
        if (index == 1 && CT1FirmWareInstalled && CT1Installed) return flashArrays[1].State;

        return "ERR: unknown-controller-state";
    }

    public string GetHardDrivesInstallStatus()
    {
        string str = "Name           Type    updating      0.00       -";
        for (int i = 0; i < HardDrives.Length; i++)
        {
            if (HardDrives[i].settingUp) 
            {
                str += $"\nCH{chassisIndex}.BAY{i}        {HardDrives[i].type}        {HardDrives[i].status}        0.00        {HardDrives[i].GetInstallDetails()}";
            }
        }
        return str;
    }

    public double GetTotalSize()
    {
        double size = 0; //in MB
        for (int i = 0; i < HardDrives.Length; i++)
        {
            if (HardDrives[i].status == HardDriveStatus.healthy) //only measure healthy hard drives!
                size += HardDrives[i].StorageSpace; 
        }
        return size / 1000000;
    }

    private void OnNewInsert() 
    {
        //turn off all lights, remove all drives, disconnect all cables!
        PSU0On = false;
        PSU1On = false;
        CT0Installed = false;
        CT1Installed = false;
        ETH0Connected = false;
        ETH1Connected = false;
        ETH2Connected = false;
        ETH3Connected = false;
        Booted = false;
        //EthConnetionInvoked = false;
        OSFullyRunning = false;

        for (int i = 0; i < NvRam.Length; i++)
        {
            NvRam[i].SetLightColor(offMat);
        }

        for (int i = 0; i < HardDrives.Length; i++)
        {
            HardDrives[i].Disable();
        }

        for (int i = 0; i < flashArrays.Length; i++)
        {
            flashArrays[i].SetLights(false, false);

            //state of flash-array is 'neither' before OS install
            flashArrays[0].State = "neither";
            flashArrays[1].State = "neither";
        }

        for (int i = 0; i < wirePorts.Length; i++)
        {
            wirePorts[i].Init(this, false);
        }

        //Turn off lights
        if (isShelf)
        {
            backLCD[0].SetActive(false); //8 on
            backLCD[1].SetActive(true); //8 off

            backLCD[2].SetActive(false); //9 on
            backLCD[3].SetActive(true); //9 off

            Cover.SetLights(false);
        }

        insertedLaptopPort = null;
        InsertedUsbPort = null;
        rack.DisableAllCablesForChassis(chassisIndex, isShelf);
        SetChassisLights();
    }

    public void AddRandomUsbPort() 
    {
        InsertedUsbPort = GetComponentInChildren<USBPort>();
    }

    public void AddRandomLaptopPort() 
    {
        for (int i = 0; i < wirePorts.Length; i++)
        {
            if (wirePorts[i].NameEquals("LAPTOP"+((selectedController == "CT0") ? "0" : "1")))
            {
                insertedLaptopPort = wirePorts[i];
                break;
            }
        }
    }

    public void SetupInstallSize(string Type) 
    {
        InstallSize_Type = Type;

        //Disable NVRams
        if (Type.Contains("20") || Type.Contains("50")) 
        {
            NvRam[2].gameObject.SetActive(false);
            NvRam[3].gameObject.SetActive(false);
        }

        if (Type.Contains("70")||Type.Contains("90")) 
        {
            
        }
    }

    #region Wires

    private bool Booted = false;
    public void OnWiresChanged(string id, bool connected) 
    {
        if (isShelf)
        {
            if (id == "ETH0")
            {
                ETH0Connected = true;
            }

            if (id == "ETH1")
            {
                ETH1Connected = true;
            }
        }
        else //need all 4 connections 
        {
            if (id == "ETH0")
            {
                ETH0Connected = true;
            }

            if (id == "ETH1")
            {
                ETH1Connected = true;
            }

            if (id == "ETH2")
            {
                ETH2Connected = true;
            }

            if (id == "ETH3")
            {
                ETH3Connected = true;
            }
        }

        if (id == "PSU0")
        {
            PSU0On = connected;
        }

        if (id == "PSU1")
        {
            PSU1On = connected;
        }

        //Use this computer in PuttY - only trigger when wire is first connected!
        //if (IsOn() && Booted && connected) 
        if(connected)
        {
            if (id == "LAPTOP0")
            {
                selectedController = "CT0";
                commandProcessor.ChangePuttyChassis(this);
                rack.selectedChassis = this; //let the rack know which chassis the user connected to

                for (int i = 0; i < wirePorts.Length; i++)
                {
                    if (wirePorts[i].NameEquals(id))
                    {
                        insertedLaptopPort = wirePorts[i];
                        break;
                    }
                }
            }

            if (id == "LAPTOP1")
            {
                selectedController = "CT1";
                commandProcessor.ChangePuttyChassis(this);
                rack.selectedChassis = this; //let the rack know which chassis the user connected to

                for (int i = 0; i < wirePorts.Length; i++)
                {
                    if (wirePorts[i].NameEquals(id))
                    {
                        insertedLaptopPort = wirePorts[i];
                        break;
                    }
                }
            }
        }

        //when all PSU wires are connected trigger the event!
        if (PSU0On && PSU1On && !Booted) 
        {
            Debug.Log("Powering on... Chassis booted successfully!");
            Booted = true;

            //setup harddrives status
            for (int i = 0; i < HardDrives.Length; i++)
            {
                if (HardDrives[i].status != HardDriveStatus.not_inserted) 
                {
                    HardDrives[i].status = HardDriveStatus.chassis_os_empty;
                    HardDrives[i].SetLightsStatus();
                }
            }

            //Turn on lights
            if (isShelf) 
            {
                backLCD[0].SetActive(true); //on
                backLCD[1].SetActive(false); //off

                backLCD[2].SetActive(true); //on
                backLCD[3].SetActive(false); //off

                Cover.SetLights(true);
            }

            if (OnServerBootAction != null) { OnServerBootAction.Invoke(); }

            //make sure to install Firmware after say 15 seconds - do here as when it has power it does this first!
            Invoke(nameof(DelayedFirmwareInstall), 15f);
        }

        //when all Ethernet wires are connected trigger the event!
        /*if (isShelf && ETH0Connected && ETH1Connected && !EthConnetionInvoked)
        {
            Debug.Log("Ethernet cables connected!");
            EthConnetionInvoked = true;
            if (OnEthernetCablesConnected != null) { OnEthernetCablesConnected.Invoke(); }
        }*/

        //need at least 2 cables not all 4.    
        //if (!isShelf && ETH0Connected && ETH1Connected && ETH2Connected && ETH3Connected && !EthConnetionInvoked) 
        /*if (!isShelf && ETH3Connected && ETH1Connected && !EthConnetionInvoked)
        {
            Debug.Log("Ethernet cables connected!");
            EthConnetionInvoked = true;

            if (OnEthernetCablesConnected != null) { OnEthernetCablesConnected.Invoke(); }
        }

        if (!isShelf && ETH2Connected && ETH0Connected && !EthConnetionInvoked)
        {
            Debug.Log("Ethernet cables connected!");
            EthConnetionInvoked = true;

            if (OnEthernetCablesConnected != null) { OnEthernetCablesConnected.Invoke(); }
        }*/

        if (Booted && commandProcessor.chassis != this && insertedLaptopPort != null) //means all cables are connected? make sure to login as the connected controller! 
        {
            commandProcessor.ChangePuttyChassis(this);
        }

        SetChassisLights();
    }

    #endregion

    #region Files_Management

    public void CopyFilesToArray(string[] Files) 
    {
        if(selectedController == "CT0") flashArrays[0].Dir.Copy(Files);
        if(selectedController == "CT1") flashArrays[1].Dir.Copy(Files);
    }

    public string GetFilesOnArray()
    {
        return selectedController == "CT0" ? flashArrays[0].Dir.GetFilesNames() : flashArrays[1].Dir.GetFilesNames();
    }

    public bool HasFileOnArray(string fileName)
    {
        return selectedController == "CT0" ? flashArrays[0].Dir.FileExsists(fileName) : flashArrays[1].Dir.FileExsists(fileName);
    }

    public string GetClosestFileStartingWith(string initials) 
    {
        return selectedController == "CT0" ? flashArrays[0].Dir.GetClosestFile(initials) : flashArrays[1].Dir.GetClosestFile(initials);
    }

    public bool DirectoryExsists(string dir)
    {
        //USB directory at */dev/usb-name*
        string[] spls = dir.Split('/');
        if(InsertedUsbPort != null) {
            if(spls.Length >= 3 && spls[1] == "dev" && spls[2] == InsertedUsbPort.Dir.DirectoryName)
                return true;
            if(spls.Length >= 2 && spls[1] == InsertedUsbPort.Dir.DirectoryName)
                return true;
            if(dir == InsertedUsbPort.Dir.DirectoryName)
                return true;
        }

        //some other directory inside controller?
        if(dir == flashArrays[0].Dir.DirectoryName) return true;
        if(dir == flashArrays[1].Dir.DirectoryName) return true;

        return false;
    }

    #endregion

    #region Processes

    [System.Serializable]
    public class ServerProcess 
    {
        public string Name = "server";
        public string status = "working";
    }
    public ServerProcess[] Processes = new ServerProcess[0];

    public string RunningProcesses() 
    {
        if (Processes.Length <= 0)
            return "";

        string redHex = ColorUtility.ToHtmlStringRGB(Color.red), greenHex = ColorUtility.ToHtmlStringRGB(Color.green);
        
        string s = Processes[0].Name + $" <color=#{(Processes[0].status == "working" ? greenHex : redHex)}>{Processes[0].status}</color>";
        Debug.Log(redHex);

        for (int i = 1; i < Processes.Length; i++)
        {
            s += $"\n{Processes[i].Name} <color=#{(Processes[i].status == "working" ? greenHex : redHex)}>{Processes[i].status}</color>";
        }

        return s;
    }

    #endregion
}
