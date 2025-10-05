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
    private static Chassis chassis => commandProcessor.chassis;
    public static VirtualFileSystemHandler fileSystemHandler;

    public static Color pink;
    public static Color yellow;
    public static Color red;
    public static Color green;
    public static Color blue;

    private static bool switchedUserToRoot = false;
    private static bool switchedToOtherController = false;

    public static void InitializeVirtualFileSystem()
    {
        if (commandProcessor != null && chassis != null)
        {
            // Create virtual file system handler if it doesn't exist
            if (fileSystemHandler == null)
            {
                var handlerObj = new GameObject("VirtualFileSystemHandler");
                fileSystemHandler = handlerObj.AddComponent<VirtualFileSystemHandler>();
                fileSystemHandler.Initialize(commandProcessor, chassis);
            }
        }
    }

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
                commandProcessor.SetLoginText($"{commandProcessor.LoggedInAs}@{chassis.GetComputerName()}-{chassis.selectedController}:~$");
                
                // Update virtual file system context
                if (fileSystemHandler != null)
                {
                    fileSystemHandler.UpdateUserContext();
                }
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

        //is the current command for user on 'root' only?
        if(IsRootCommand(splits) && !switchedUserToRoot)
        {
            commandProcessor.LogError("You must be on 'root' to perform this action!");
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

        //lists all files in folders
        if (splits[0] == "ls" || splits[0] == "ll" || splits[0] == "la")
        {
            // Convert aliases to ls with appropriate flags
            if (splits[0] == "ll")
            {
                // ll is typically ls -la
                string[] newSplits = new string[splits.Length + 1];
                newSplits[0] = "ls";
                newSplits[1] = "-la";
                for (int i = 1; i < splits.Length; i++)
                {
                    newSplits[i + 1] = splits[i];
                }
                splits = newSplits;
            }
            else if (splits[0] == "la")
            {
                // la is typically ls -A
                string[] newSplits = new string[splits.Length + 1];
                newSplits[0] = "ls";
                newSplits[1] = "-A";
                for (int i = 1; i < splits.Length; i++)
                {
                    newSplits[i + 1] = splits[i];
                }
                splits = newSplits;
            }

            // Use virtual file system if available
            if (fileSystemHandler != null)
            {
                string result = fileSystemHandler.HandleLsCommand(splits);
                commandProcessor.Log(result);
                return;
            }

            //find if any files exists anywhere on drives
            if (splits.Length > 1 && splits[1].StartsWith("/"))
            {
                //list folders in this folder...
                string r = chassis.commandsExtension.FindAndListFiles(splits[1]);

                if(r == "*") {
                    commandProcessor.LogError($"No such file or directory exsists: {splits[1]}");
                    return;
                }

                commandProcessor.Log(r);
                return;
            }

            //means files on the controller?
            commandProcessor.Log(chassis.GetFilesOnArray());
            return;
        }

        if (splits[0] == "mount") 
        {
            // Use virtual file system handler if available
            if (fileSystemHandler != null)
            {
                string result = fileSystemHandler.HandleMountCommand(splits);
                if (!string.IsNullOrEmpty(result))
                {
                    commandProcessor.LogError(result);
                }
                return;
            }

            if(splits.Length < 3) {
                commandProcessor.LogError("mount [drive] [folder] - incorrect useage!");
                return;
            }

            if(splits[2] != "/mnt")
            {
                commandProcessor.LogError($"No directory selected to exsists: {splits[1]}!");
                return;
            }

            //is it even a directory user is mounting?
            if (splits[2] == "/mnt" && chassis.DirectoryExsists(splits[1]) && chassis.UsbCorrect())
            {
                //make sure user has mounted to */mnt*
                commandProcessor.Mounted = true;
                return;
            }

            commandProcessor.LogError("No such file or directory exsists!");
            return;
        }

        // Add umount command
        if (splits[0] == "umount")
        {
            if (fileSystemHandler != null)
            {
                string result = fileSystemHandler.HandleUmountCommand(splits);
                if (!string.IsNullOrEmpty(result))
                {
                    commandProcessor.LogError(result);
                }
                return;
            }

            commandProcessor.LogError("umount: command not available in legacy mode");
            return;
        }

        //before copying files make sure to mount the drives!
        if (splits[0] == "cp") 
        {
            // Use virtual file system handler if available
            if (fileSystemHandler != null)
            {
                string result = fileSystemHandler.HandleCpCommand(splits);
                if (!string.IsNullOrEmpty(result))
                {
                    commandProcessor.LogError(result);
                }
                return;
            }

            if (commandProcessor.Mounted) {
                if (!chassis.UsbCorrect()) {
                    return;
                }

                //copy files USB -> controller:
                string sourcePath = splits.Length > 1 ? splits[1] : "";
                
                // Handle wildcards in cp command (e.g., cp /mnt/* . or cp /mnt/6.7.2/* .)
                if (sourcePath.Contains("*"))
                {
                    // For patterns like /mnt/* or /mnt/subdir/*, copy all USB files
                    if (sourcePath.StartsWith("/mnt"))
                    {
                        commandProcessor.CopyMountFiles(chassis.InsertedUsbPort.Dir.Files, 10f * commandProcessor.timeMultiplier);
                        return;
                    }
                }
                
                string[] spls = sourcePath.Split('/'); //used as: cp /mnt/sdb1
                if (spls.Length >= 2 && spls[1] == "mnt") 
                {
                    // Check if it's just /mnt or /mnt/ (copy all)
                    if (spls.Length == 2 || (spls.Length == 3 && spls[2] == ""))
                    {
                        commandProcessor.CopyMountFiles(chassis.InsertedUsbPort.Dir.Files, 10f * commandProcessor.timeMultiplier);
                        return;
                    }
                    
                    // Check if directory path matches USB directory
                    if (chassis.DirectoryExsists(spls[2])) 
                    {
                        commandProcessor.CopyMountFiles(chassis.InsertedUsbPort.Dir.Files, 10f * commandProcessor.timeMultiplier);
                        return;
                    }
                }
            }

            commandProcessor.LogError($"No such file or directory exsists: {splits[1]}");
            return;
        }

        //if (splits[0] == "pureinstall" && splits[1].EndsWith(".ppkg") && commandProcessor.Mounted && chassis.HasFilesOnArray()) 
        if (splits[0] == "pureinstall")
        {
            //install a version of purity:
            if(splits.Length > 1 && commandProcessor.Mounted && chassis.HasFileOnArray(splits[1])) {
                commandProcessor.StartInstallation();
                return;
            }

            commandProcessor.LogError("Package extraction failure!");
            return;
        }

        if (splits[0] == "pureboot" && splits.Length >= 2)
        {
            if (splits[1] == "reboot" && splits[2] == "--offline") 
            {
                commandProcessor.Log("[Errno 111] Connection refused\nGNU GRUB version 2.06");
                commandProcessor.RebootChassis();
                return;
            }

            //test if the primary is active:
            if(splits[1] == "reboot" && splits[2] == "--primary")
            {
                if(chassis.flashArrays[0].State == "primary" || chassis.flashArrays[1].State == "primary") {
                    commandProcessor.RebootChassis();
                }
                else
                    commandProcessor.LogError("Reboot failure! primary controller cannot be found or is not active.");

                return;
            }

            //test if the secondary is active:
            if(splits[1] == "reboot" && splits[2] == "--secondary")
            {
                if(chassis.flashArrays[0].State == "secondary" || chassis.flashArrays[1].State == "secondary") {
                    commandProcessor.RebootChassis();
                }
                else
                    commandProcessor.LogError("Reboot failure! secondary controller cannot be found or is not active.");
                return;
            }

            if (splits[1] == "list") 
            {
                commandProcessor.Log($"Marked entry (*) is currently running\nMarked entry (-->) will run at next reboot\n    0. Purity {chassis.GetSecondPurityPartVersion()} (202404130351+34e2b1e66ad3) with kernel 5.15.123+ (202403191505+d9f0e688c788) on first (/dev/sda3)\n" +
                    $"*-->1. Purity {chassis.GetCurrentPurityVersion()} (202412120507+7a7df3f70616) with kernel 5.15.123+ (202411262041+7e571dbb5a84) on second (/dev/sda4)");
                return;
            }
        }

        if (splits[0] == "puresetup")
        {
            if (chassis.OSInstalled() && splits[1] == "show")
            {
                commandProcessor.ShowArrayInfo();
                return;
            }

            if (splits[1] == "timezone") 
            {
                commandProcessor.ManualChangeTimezone();
                return;
            }

            if (chassis.OSInstalled(chassis.selectedController))
            {
                commandProcessor.LogError("Cannot change/update os now.");
            }
            else
            {
                if (splits[1] == "newarray" && !chassis.OSInstalled(0) && chassis.selectedController == "CT0") 
                    commandProcessor.StartOSSetup(true);

                if (splits[1] == "secondary" && chassis.OSInstalled(0) && chassis.selectedController == "CT1") //make sure first array is first installed before setting secondary!
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
        if (chassis.selectedController == "CT0" && !chassis.OSInstalled(0)) 
        {
            commandProcessor.LogError($"  '{cmd}' is not recognized as an internal or external command.");
            return;
        }

        if (chassis.selectedController == "CT1" && !chassis.OSInstalled(1))
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
            if (splits[1] == "untag" && splits[2] == "--maintenance") {
                commandProcessor.Log(""); // Command acknowledged
                return;
            }
        }

        //switching to root
        if (splits[0] == "sudo" && splits[1] == "su") 
        {
            commandProcessor.LoggedInAs = "root";
            commandProcessor.SetLoginText($"{commandProcessor.LoggedInAs}@{chassis.GetComputerName()}-{chassis.selectedController}:/var/home/pureeng#");
            switchedUserToRoot = true;
            
            // Update virtual file system context
            if (fileSystemHandler != null)
            {
                fileSystemHandler.UpdateUserContext();
            }
            
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
                // Check for --type flag
                if (splits.Length > 2 && splits[2] == "--type")
                {
                    string typeFilter = splits.Length > 3 ? splits[3] : "";
                    if (typeFilter == "pwr")
                    {
                        // Filter for power supplies only
                        commandProcessor.Log("Name          Status  Identify  Slot  Index  Speed       Temperature  Voltage  Details");
                        commandProcessor.Log($"CH0.PWR0      ok      -         -     0      -           -            200 V    -");
                        commandProcessor.Log($"CH0.PWR1      ok      -         -     1      -           -            200 V    -");
                        commandProcessor.Log($"CT0.PWR0      ok      -         -     0      -           -            12 V     -");
                        commandProcessor.Log($"CT1.PWR0      ok      -         -     0      -           -            12 V     -");
                    }
                    else
                    {
                        // For other types, show full list with filter (future enhancement)
                        commandProcessor.Log(chassis.commandsExtension.PureHWList());
                    }
                }
                else
                {
                    commandProcessor.Log(chassis.commandsExtension.PureHWList());
                }
                return;
            }

            if (splits[0] == "hardware_check.py")
            {
                commandProcessor.Log(chassis.commandsExtension.HardwareCheck());
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

                    commandProcessor.Log(chassis.RunningProcesses());
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
                if(chassis.flashArrays[0].arrayName == splits[4]) array = chassis.flashArrays[0];
                if(chassis.flashArrays[1].arrayName == splits[4]) array = chassis.flashArrays[1];
            
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
            commandProcessor.Log("Product Version: " + chassis.PurityVersionInPartition0);
        }

        if (splits[0] == "purenetwork") 
        {
            if (splits[1] == "list" && splits.Length == 2) 
            {
                commandProcessor.Log(chassis.commandsExtension.PureNetworkList());
            }
            else if (splits[1] == "eth" && splits[2] == "list")
            {
                // List Ethernet interfaces
                commandProcessor.Log("Name          Address              Netmask            Gateway            MTU    Enabled");
                commandProcessor.Log($"eth0          192.168.1.100        255.255.255.0      192.168.1.1        1500   True");
                commandProcessor.Log($"eth1          192.168.1.101        255.255.255.0      192.168.1.1        1500   True");
                commandProcessor.Log($"eth2          192.168.2.100        255.255.255.0      192.168.2.1        9000   True");
                commandProcessor.Log($"eth3          -                    -                  -                  1500   False");
            }
            else if (splits[1] == "fc" && splits[2] == "list")
            {
                // List Fibre Channel interfaces
                commandProcessor.Log("Name          WWN                              Status    Speed");
                commandProcessor.Log($"fc0           50:01:43:80:12:34:56:78          up        16Gb/s");
                commandProcessor.Log($"fc1           50:01:43:80:12:34:56:79          up        16Gb/s");
                commandProcessor.Log($"fc2           50:01:43:80:12:34:56:7a          down      -");
                commandProcessor.Log($"fc3           50:01:43:80:12:34:56:7b          down      -");
            }
            return;
        }

        if (splits[0] == "purearray")
        {
            if (splits[1] == "remoteassist" && splits[2] == "--connect")
            {
                commandProcessor.Log("Name        Status        Opened        Expires");
                System.DateTime expires = System.DateTime.Now.AddDays(2);
                commandProcessor.Log($"{chassis.GetComputerName()}{chassis.selectedController}        connecting        {System.DateTime.Now.ToString()}        {expires.ToString()}");
            }
            if (splits[1] == "remoteassist" && splits[2] == "--status")
            {
                commandProcessor.Log("Name        Status        Opened        Expires");
                System.DateTime expires = System.DateTime.Now.AddDays(2);
                commandProcessor.Log($"{chassis.GetComputerName()}{chassis.selectedController}        connected        {System.DateTime.Now.ToString()}        {expires.ToString()}");
            }
            if (splits[1] == "phonehome" && (splits[2] == "--send-today" || splits[2] == "--send-dotoday"))
            {
                commandProcessor.Log("Status  Action");
                commandProcessor.Log("-       -");
            }
            if (splits[1] == "list" && splits.Length == 2)
            {
                commandProcessor.Log(chassis.rack.PurearrayList());
            }
            else if (splits[1] == "list" && splits[2] == "--controller")
            {
                commandProcessor.Log(chassis.commandsExtension.GetControllersList());
            }
            return;
        }

        #endregion

        if (splits[0] == "cat") //see folder contents!
        {
            // Use virtual file system handler if available
            if (fileSystemHandler != null)
            {
                string result = fileSystemHandler.HandleCatCommand(splits);
                commandProcessor.Log(result);
                return;
            }

            if (splits[1] == "/etc/timezone") 
            {
                commandProcessor.Log(chassis.selectedController == "CT0" ? chassis.flashArrays[0].TimeZone : chassis.flashArrays[1].TimeZone);
            }
            return;
        }

        // Add new Linux commands using virtual file system
        if (fileSystemHandler != null)
        {
            string result = "";
            bool handled = true;

            switch (splits[0])
            {
                case "cd":
                    result = fileSystemHandler.HandleCdCommand(splits);
                    // Update command processor prompt if needed
                    if (string.IsNullOrEmpty(result))
                    {
                        string newDir = fileSystemHandler.GetFileSystem().GetCurrentDirectory();
                        string prompt = $"{commandProcessor.LoggedInAs}@{chassis.GetComputerName()}-{chassis.selectedController}:{newDir}";
                        prompt += switchedUserToRoot ? "#" : "$";
                        commandProcessor.SetLoginText(prompt);
                    }
                    break;
                case "pwd":
                    result = fileSystemHandler.HandlePwdCommand();
                    break;
                case "mkdir":
                    result = fileSystemHandler.HandleMkdirCommand(splits);
                    break;
                case "rmdir":
                    result = fileSystemHandler.HandleRmdirCommand(splits);
                    break;
                case "rm":
                    result = fileSystemHandler.HandleRmCommand(splits);
                    break;
                case "mv":
                    result = fileSystemHandler.HandleMvCommand(splits);
                    break;
                case "find":
                    result = fileSystemHandler.HandleFindCommand(splits);
                    break;
                case "touch":
                    result = fileSystemHandler.HandleTouchCommand(splits);
                    break;
                // Add system information commands
                case "df":
                case "lsblk":
                case "ifconfig":
                case "lspci":
                case "free":
                case "iostat":
                case "which":
                case "whoami":
                case "id":
                case "uname":
                case "hostname":
                case "uptime":
                case "date":
                case "history":
                    result = fileSystemHandler.HandleSystemCommand(splits);
                    if (result == null) handled = false;
                    break;
                default:
                    handled = false;
                    break;
            }

            if (handled)
            {
                if (!string.IsNullOrEmpty(result))
                {
                    if (result.Contains("cannot") || result.Contains("missing") || result.Contains("failed"))
                    {
                        commandProcessor.LogError(result);
                    }
                    else
                    {
                        commandProcessor.Log(result);
                    }
                }
                return;
            }
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
                commandProcessor.Log(chassis.GetHardDrivesStatus());
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
            if (splits.Length > 2 && splits[2] == "--open") 
            {
                commandProcessor.Log("ID          Time          Severity          Category          Code          Component          Name          Event          Expected          Action");
                // No messages to display (healthy system)
            }
            return;
        }
        
        if (splits[0] == "pureport" && splits[1] == "list")
        {
            if (splits.Length > 2 && splits[2] == "--initiator")
            {
                commandProcessor.Log("Name          Initiator");
                // Show sample port connections
                commandProcessor.Log($"CT0.ETH0      -");
                commandProcessor.Log($"CT0.ETH1      -");
                commandProcessor.Log($"CT0.FC0       iqn.1993-08.org.debian:01:example");
                commandProcessor.Log($"CT1.ETH0      -");
                commandProcessor.Log($"CT1.FC0       iqn.1993-08.org.debian:01:example");
            }
            return;
        }
        
        if (splits[0] == "puretune" && splits[1] == "--list")
        {
            commandProcessor.Log("Warning: failed to retrieve some tunable status (local puredb-chastity, peer puredb-chastity)");
            commandProcessor.Log("local puredb              - <unset>");
            commandProcessor.Log("peer puredb               - <unset>");
            commandProcessor.Log("local puredb --platform   - <unset>");
            commandProcessor.Log("peer puredb --platform    - <unset>");
            return;
        }
        
        if (splits[0] == "puredb")
        {
            // puredb commands - basic acknowledgment
            commandProcessor.Log(""); // Command acknowledged
            return;
        }
        
        if (splits[0] == "iobalance")
        {
            if (splits.Length > 1 && (splits[1] == "--sampletime" || splits[1].StartsWith("-s")))
            {
                commandProcessor.Log("Sampling I/O balance...");
                commandProcessor.Log("Controller  Read IOPS  Write IOPS  Read BW    Write BW");
                commandProcessor.Log($"CT0         {Random.Range(1000, 5000)}       {Random.Range(2000, 8000)}        {Random.Range(100, 500)}MB/s   {Random.Range(150, 600)}MB/s");
                commandProcessor.Log($"CT1         {Random.Range(1000, 5000)}       {Random.Range(2000, 8000)}        {Random.Range(100, 500)}MB/s   {Random.Range(150, 600)}MB/s");
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
        commandProcessor.LogError($"'{cmd}' is not recognized as an internal or external command.");
    }

    public static bool IsRootCommand(string[] splits)
    {
        if(splits[0] == "purehw") return true;
        if(splits[0] == "hardware_check.py") return true;
        if(splits[0] == "pureadm") return true;
        if(splits[0] == "purewes") return true;
        if(splits[0] == "puresetup" && splits[1] == "show") return true;

        return false;
    }
}
