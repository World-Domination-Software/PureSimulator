using CrimsofallTechnologies.ServerSimulator;
using UnityEngine;

//this is the script that handles commands entered to the System (after installation that is)!

//SCHEME (to add your own commands):
/*
 
if (splits[(int)where in the split command after spacing] == "some-prewritten-command (e.g. help)") {
    method to run...
    return; --> insert *always* to let the computer know this is a valid command otherwise it will log the unknown command exception!
}

 */

public static class OS
{
    public static CommandProcessor commandProcessor;

    public static Color pink;
    public static Color yellow;
    public static Color red;
    public static Color green;
    public static Color blue;

    private static bool switchedUserToRoot = false;
    private static bool switchedToOtherController = false;

    public static void ProcessCommand(string cmd)
    {
        string[] splits = cmd.Split(' ');
        //splits = AddSpaces(splits);

        if (!commandProcessor.setupPausedOnDataErase && (splits[0] == "exit" || splits[0] == "quit"))
        {
            if (switchedUserToRoot) //switch out of root!
            {
                commandProcessor.Log(commandProcessor.LoginText + " " + cmd);
                commandProcessor.LoggedInAs = "pureeng";
                switchedUserToRoot = false;
                commandProcessor.SetLoginText($"{commandProcessor.LoggedInAs}@{commandProcessor.chassis.GetComputerName()}-{commandProcessor.chassis.selectedController}:~$");
            }
            else if (switchedToOtherController) //switch back to original controller!
            {
                commandProcessor.SwitchChassis();
                switchedToOtherController = false;
            }
            else //log out of current session at last lol.
            {
                commandProcessor.LogOut(true);
            }

            return;
        }

        if (commandProcessor.LoggedIn && !commandProcessor.isSettingUpOS)
        {
            commandProcessor.Log(commandProcessor.LoginText + " " + cmd);
        }

        //do not allow commands unless player is fully logged in!
        if (!commandProcessor.LoggedIn) 
        {
            commandProcessor.LogError($"  '{cmd}' is not recognized as an internal or external command.*");
            return;
        }

        //COMMANDS DETECTION BELOW:

        if(IsRootCommand(splits) && !switchedUserToRoot)
        {
            commandProcessor.Log("You must be on 'root' to execute this action!");
            return;
        }

        #region INSTALLING/UPGRADING

        if (commandProcessor.wantsToInputPassword) 
        {
            if (splits[0] == "continue")
                commandProcessor.ContinuePassword();
            if(splits[0] == "skip")
                commandProcessor.SkipPassword();
            return;
        }

        if (splits[0] == "ls") //lists all files in folders
        {
            if (!commandProcessor.chassis.UsbCorrect())
            {
                commandProcessor.LogError("No such directory exists to mount");
                return;
            }

            //find if any files exists anywhere on drives
            if (splits.Length > 1 && splits[1].StartsWith("/"))
            {
                //list folders in this folder...
                commandProcessor.Log(commandProcessor.chassis.commandsExtension.FindAndListFiles(splits[1]));
                return;
            }

            //means files on the controller?
            commandProcessor.Log(commandProcessor.chassis.GetFilesOnArray());
            return;
        }

        if (splits[0] == "mount") 
        {
            if (!commandProcessor.chassis.UsbCorrect())
            {
                commandProcessor.LogError("No such directory exists to mount: " + splits[1]);
                return;
            }

            //make sure user has mounted to */mnt*
            if (splits[1].Split('/').Length >= 2)
            {
                if (splits.Length > 1 && splits[2] == "/mnt")
                    commandProcessor.Mounted = true;
                else 
                {
                    commandProcessor.LogError("No such directory exists to mount: " + splits[1]);
                }
                return;
            }
        }

        //before copying files make sure to mount the drives!
        if (splits[0] == "cp") 
        {
            if (commandProcessor.Mounted) {
                if (!commandProcessor.chassis.UsbCorrect())
                {
                    commandProcessor.LogError("No such directory exists to mount: " + splits[1]);
                    return;
                }

                //copy all files in folder! 
                //if (splits[1] == "/mnt/6.5.9/*") 
                if (splits[1].Split('/').Length >= 2)
                {
                    commandProcessor.CopyMountFiles(10f, commandProcessor.chassis.InsertedUsbPort.Files);
                }
            }
            return;
        }

        //if (splits[0] == "pureinstall" && splits[1].EndsWith(".ppkg") && commandProcessor.Mounted && commandProcessor.chassis.HasFilesOnArray()) 
        if (splits[0] == "pureinstall" && commandProcessor.Mounted && commandProcessor.chassis.HasFileOnArray(splits[1]))
        {
            //install a version of it!
            Debug.Log("Installing new purity version...");
            commandProcessor.StartInstallation();
            return;
        }

        if (splits[0] == "pureboot")
        {
            if (splits[1] == "reboot" && splits[2] == "--offline") 
            {
                commandProcessor.Log("[Errno 111] Connection refused\nGNU GRUB version 2.06");
                commandProcessor.RebootChassis();
                return;
            }

            if (splits[1] == "list") 
            {
                commandProcessor.Log($"Marked entry (*) is currently running\nMarked entry (-->) will run at next reboot\n    0. Purity {commandProcessor.chassis.GetSecondPurityPartVersion()} (202404130351+34e2b1e66ad3) with kernel 5.15.123+ (202403191505+d9f0e688c788) on first (/dev/sda3)\n" +
                    $"*-->1. Purity {commandProcessor.chassis.GetCurrentPurityVersion()} (202412120507+7a7df3f70616) with kernel 5.15.123+ (202411262041+7e571dbb5a84) on second (/dev/sda4)");
                return;
            }
        }

        if (splits[0] == "puresetup")
        {
            if (commandProcessor.chassis.OSInstalled() && splits[1] == "show")
            {
                commandProcessor.ShowArrayInfo();
                return;
            }

            if (splits[1] == "timezone") 
            {
                commandProcessor.ManualChangeTimezone();
                return;
            }

            if (commandProcessor.chassis.OSInstalled(commandProcessor.chassis.selectedController))
            {
                commandProcessor.LogError("Cannot change/update os now.");
            }
            else
            {
                if (splits[1] == "newarray" && !commandProcessor.chassis.OSInstalled(0) && commandProcessor.chassis.selectedController == "CT0") 
                    commandProcessor.StartOSSetup(true);

                if (splits[1] == "secondary" && commandProcessor.chassis.OSInstalled(0) && commandProcessor.chassis.selectedController == "CT1") //make sure first array is first installed before setting secondary!
                    commandProcessor.StartOSSetup(true);
            }

            return;
        }

        if (commandProcessor.isSettingUpOS && commandProcessor.setupPausedOnDataErase) 
        {
            //choose default selection
            if (splits[0] == "" || splits[0] == "\n") 
                commandProcessor.ExitSetup();

            if (splits[0].StartsWith("continue")) commandProcessor.ContinueSetup();
            if (splits[0].StartsWith("exit")) commandProcessor.ExitSetup();
            return;
        }

        if (commandProcessor.applyConfigToArray) 
        {
            if (splits[0] == "y") commandProcessor.ApplyConfigToArray();
            if (splits[0] == "n") commandProcessor.ReenterArrayConfigs();
            return;
        }

        if (commandProcessor.isSettingUpOS && commandProcessor.setupPausedOnRapidDataLock)
        {
            //choose default selection
            if (splits[0] == "" || splits[0] == "\n")
                commandProcessor.ContinueSetup2("n");

            if (splits[0] == "y") commandProcessor.ContinueSetup2("y");
            if (splits[0] == "n") commandProcessor.ContinueSetup2("n");
            return;
        }

        if (commandProcessor.waitingForTimezone)
        {
            if (splits[0] == "y") commandProcessor.ChangeTimeZone();
            if (splits[0] == "n") commandProcessor.SkipTimeZone();
            return;
        }

        //do not allow more commands while installing!
        if (commandProcessor.chassis.selectedController == "CT0" && !commandProcessor.chassis.OSInstalled(0)) 
        {
            commandProcessor.LogError($"  '{cmd}' is not recognized as an internal or external command.");
            return;
        }

        if (commandProcessor.chassis.selectedController == "CT1" && !commandProcessor.chassis.OSInstalled(1))
        {
            commandProcessor.LogError($"  '{cmd}' is not recognized as an internal or external command.");
            return;
        }

        #endregion

        if (splits[0] == "purealert") 
        {
            if (splits[1] == "tag" && splits[2] == "--timeout" && splits.Length == 5) {
                commandProcessor.Log("Name          Created          Expires");
                int.TryParse(splits[3].TrimEnd('m'), out int minutes);
                System.DateTime expires = System.DateTime.Now.AddHours((double)(minutes / 60));
                commandProcessor.Log($"{splits[4].TrimStart('-', '-')}          {System.DateTime.Now.ToString()}          {expires.ToString()}");
                return;
            }
        }

        //switching to root
        if (splits[0] == "sudo" && splits[1] == "su") 
        {
            commandProcessor.LoggedInAs = "root";
            commandProcessor.SetLoginText($"{commandProcessor.LoggedInAs}@{commandProcessor.chassis.GetComputerName()}-{commandProcessor.chassis.selectedController}:/var/home/pureeng#");
            switchedUserToRoot = true;
            return;
        }

        //switching to other controller without logging in
        if (splits[0] == "ssh" && splits[1] == "peer") 
        {
            switchedToOtherController = true;
            commandProcessor.SwitchChassis();
            return;
        }

        #region HEALTH CHECKS

        if (switchedUserToRoot) 
        {
            //if (splits[0] == "purehw" && splits[1] == "list" && splits[2] == "--all")
            if(splits[0] == "purehw" && splits[1] == "list")
            {
                commandProcessor.Log(commandProcessor.chassis.commandsExtension.PureHWList());
                return;
            }

            if (splits[0] == "hardware_check.py")
            {
                commandProcessor.Log(commandProcessor.chassis.commandsExtension.HardwareCheck());
                return;
            }

            if (splits[0] == "pureadm")
            {
                if (splits[1] == "status") 
                {
                    commandProcessor.Log("Process Status:");
                    commandProcessor.Log("purity start/running\nlio-drv start/running\nfoed start/running, process 2944\nplatform start/running, process 2907\n" +
                        "gui start/running, process 2866\nrest start/running, process 4003\nmonitor stop/waiting\niostat start/running. process 7315" +
                        "\nstatistics stop/waiting\nmiddleware start/running, process 4896\nvasa start/running, process 4897");

                    commandProcessor.Log(commandProcessor.chassis.RunningProcesses());
                }
                return;
            }
        
            //switch primary and secondary controllers (as instructed)
            if(splits[0] == "purewes" && splits[1] == "controller" && splits[2] == "setattr" && splits[3] == "--verify-array")
            {
                //simple check
                if(splits.Length < 8 || (splits[7] != "primary" && splits[7] != "secondary"))
                {
                    commandProcessor.LogError($"  '{cmd}' is not recognized as an internal or external command.");
                    return;
                }

                //4th part is controller name, 5th is ct1 or ct0, 6th is --mode ande 7th is primary or secondary
                FlashArray array = null;
                if(commandProcessor.chassis.flashArrays[0].arrayName == splits[4]) array = commandProcessor.chassis.flashArrays[0];
                if(commandProcessor.chassis.flashArrays[1].arrayName == splits[4]) array = commandProcessor.chassis.flashArrays[1];
            
                if(array != null && (splits[5] == "ct1" || splits[5] == "ct0") && splits[6] == "--mode") 
                {
                    //make sure it is not already primary or secondary, throw a error!
                    if(array.State == splits[7]) 
                    {
                        commandProcessor.LogError("Controller state is already [" + splits[7]+"]");
                    }
                    else
                    {
                        //change state
                        commandProcessor.ChangeControllerState(array, splits[7]);
                    }
                }
                return;
            }
        }

        if(splits[0] == "pureversion" && splits[1] == "list")
        {
            commandProcessor.Log("Product Version: " + commandProcessor.chassis.PurityVersionInPartition0);
        }

        if (splits[0] == "purenetwork") 
        {
            if (splits[1] == "list") 
            {
                commandProcessor.Log(commandProcessor.chassis.commandsExtension.PureNetworkList());
            }
            return;
        }

        if (splits[0] == "purearray")
        {
            if (splits[1] == "remoteassist" && splits[2] == "--connect")
            {
                commandProcessor.Log("Name        Status        Opened        Expires");
                System.DateTime expires = System.DateTime.Now.AddDays(2);
                commandProcessor.Log($"{commandProcessor.chassis.GetComputerName()}{commandProcessor.chassis.selectedController}        connecting        {System.DateTime.Now.ToString()}        {expires.ToString()}");
            }
            if (splits[1] == "remoteassist" && splits[2] == "--status")
            {
                commandProcessor.Log("Name        Status        Opened        Expires");
                System.DateTime expires = System.DateTime.Now.AddDays(2);
                commandProcessor.Log($"{commandProcessor.chassis.GetComputerName()}{commandProcessor.chassis.selectedController}        connected        {System.DateTime.Now.ToString()}        {expires.ToString()}");
            }
            if (splits[1] == "list" && splits[2] == "--controller")
            {
                commandProcessor.Log(commandProcessor.chassis.commandsExtension.GetControllersList());
            }
            if (splits[1] == "list" && splits.Length == 2)
            {
                commandProcessor.Log(commandProcessor.chassis.rack.PurearrayList());
            }
            return;
        }

        #endregion

        if (splits[0] == "cat") //see folder contents!
        {
            if (splits[1] == "/etc/timezone") 
            {
                commandProcessor.Log(commandProcessor.chassis.selectedController == "CT0" ? commandProcessor.chassis.flashArrays[0].TimeZone : commandProcessor.chassis.flashArrays[1].TimeZone);
            }
            return;
        }

        //Easter EGGS!
        #region EASTER_EGGS

        //no help
        if (splits[0].Contains("help")) 
        {
            commandProcessor.Log("There is no help!", red); return;
        }

        //neverwards
        if (splits[0].Contains("neverwards")) 
        {
            commandProcessor.Log("EasterEgg: Neverwards - The ARPG developed by the developers!", green); return;
        }

        //devs
        if (splits[0] == "credits" || splits[0] == "devs") 
        {
            commandProcessor.Log("EasterEgg: Crimsofall Technologies & Smart Like Rocks!", green); return;
        }

        #endregion

        if (splits[0] == "puredrive")
        {
            if (splits[1] == "list")
            {
                commandProcessor.Log(commandProcessor.chassis.GetHardDrivesStatus());
            }

            return;
        }

        if (splits[0] == "ping")
        {
            commandProcessor.Log($"ping {Random.Range(10, 20)} ms");
            return;
        }

        if (splits[0] == "puremessage" && splits[1] == "list")
        {
            if (splits[2] == "--open" && splits[3] == "--hidden") 
            {
                commandProcessor.Log("ID          Time          Severity          Category          Code          Component          Name          Event          Expected          Action");
                
            }
            return;
        }

        if (splits[0] == "storage_view.py" && splits[1] == "config" && splits[3] == "ssh" && splits[4] == "peer") 
        {
            commandProcessor.Log(System.DateTime.Now.ToString());
            commandProcessor.Log("\nCommand line: ['/opt/Purity/bin/storage_view.py', 'config']\nVersion: 0.999.1\nWorking at ct0 to look over cables between ct0 and shelves");
            commandProcessor.WorkToFindServerCableErrors();
            return;
        }

        if (splits[0] == "watch" && switchedUserToRoot) 
        {
            if (splits[1] == "puredrive" && splits[2] == "list" && splits[3] == "--pack")
            {
                commandProcessor.WatchPuredriveList(cmd);
            }

            if (splits[1] == "purearray" && splits[2] == "list")
            {
                commandProcessor.WatchArray(cmd);
            }
        }

        //return an error!
        commandProcessor.LogError($"  '{cmd}' is not recognized as an internal or external command.");
    }

    public static bool IsRootCommand(string[] splits)
    {
        if(splits[0] == "purehw") return true;
        if(splits[0] == "hardware_check.py") return true;
        if(splits[0] == "pureadm") return true;
        if(splits[0] == "purewes") return true;

        return false;
    }
}
