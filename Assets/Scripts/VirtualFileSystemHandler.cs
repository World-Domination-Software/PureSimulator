using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CrimsofallTechnologies.ServerSimulator
{
    public class VirtualFileSystemHandler : MonoBehaviour
    {
        private VirtualFileSystem fileSystem;
        private CommandProcessor commandProcessor;
        private Chassis chassis;
        public VirtualHardwareManager hardwareManager;

        public void Initialize(CommandProcessor processor, Chassis chassisRef)
        {
            commandProcessor = processor;
            chassis = chassisRef;
            fileSystem = new VirtualFileSystem();
            
            // Initialize virtual hardware
            hardwareManager = gameObject.AddComponent<VirtualHardwareManager>();
            hardwareManager.Initialize(this);
            
            // Set initial user and directory based on login
            UpdateUserContext();
        }

        public void UpdateUserContext()
        {
            if (commandProcessor != null)
            {
                fileSystem.SetCurrentUser(commandProcessor.LoggedInAs);
                
                // Set current directory to user's home if we're not already there
                string homeDir = fileSystem.GetUserHomeDirectory();
                if (fileSystem.GetCurrentDirectory() == "/")
                {
                    fileSystem.SetCurrentDirectory(homeDir);
                }
            }
        }

        public VirtualFileSystem GetFileSystem()
        {
            return fileSystem;
        }

        // Handle ls command with various options
        public string HandleLsCommand(string[] args)
        {
            bool longFormat = false;
            bool showHidden = false;
            string targetPath = fileSystem.GetCurrentDirectory();

            // Parse arguments
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i].StartsWith("-"))
                {
                    if (args[i].Contains("l")) longFormat = true;
                    if (args[i].Contains("a")) showHidden = true;
                }
                else
                {
                    targetPath = args[i];
                }
            }

            // Handle wildcards in the target path
            if (targetPath.Contains("*") || targetPath.Contains("?"))
            {
                return HandleWildcardLs(targetPath, longFormat, showHidden);
            }

            var result = fileSystem.ListDirectory(targetPath, longFormat, showHidden);
            if (result == null)
            {
                return $"ls: cannot access '{targetPath}': No such file or directory";
            }

            return result;
        }

        // Handle ls with wildcard patterns
        private string HandleWildcardLs(string pattern, bool longFormat, bool showHidden)
        {
            // Determine directory and pattern
            string directory = fileSystem.GetCurrentDirectory();
            string filePattern = pattern;
            
            if (pattern.Contains("/"))
            {
                int lastSlash = pattern.LastIndexOf('/');
                directory = pattern.Substring(0, lastSlash);
                if (string.IsNullOrEmpty(directory)) directory = "/";
                filePattern = pattern.Substring(lastSlash + 1);
            }

            // Get all files in the directory
            var dirNode = fileSystem.GetNode(directory);
            if (dirNode == null || !dirNode.IsDirectory)
            {
                return $"ls: cannot access '{pattern}': No such file or directory";
            }

            var matchingFiles = new System.Collections.Generic.List<string>();
            foreach (var child in dirNode.GetChildren())
            {
                if (!showHidden && child.Name.StartsWith(".")) continue;
                
                if (WildcardMatchPublic(child.Name, filePattern))
                {
                    matchingFiles.Add(child.Name);
                }
            }

            if (matchingFiles.Count == 0)
            {
                return $"ls: cannot access '{pattern}': No such file or directory";
            }

            // Format output
            if (longFormat)
            {
                var output = new System.Collections.Generic.List<string>();
                foreach (var fileName in matchingFiles)
                {
                    var fullPath = directory == "/" ? "/" + fileName : directory + "/" + fileName;
                    var node = fileSystem.GetNode(fullPath);
                    if (node != null)
                    {
                        string permissions = node.IsDirectory ? "d" + node.Permissions : "-" + node.Permissions;
                        output.Add($"{permissions} {node.Owner} {node.Group} {node.Size,8} {node.ModifiedTime:MMM dd HH:mm} {node.Name}");
                    }
                }
                return string.Join("\n", output);
            }
            else
            {
                return string.Join("  ", matchingFiles);
            }
        }

        // Public wildcard match helper
        private bool WildcardMatchPublic(string text, string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return string.IsNullOrEmpty(text);
            if (pattern == "*") return true;
            
            int textIdx = 0;
            int patternIdx = 0;
            int starIdx = -1;
            int matchIdx = 0;
            
            while (textIdx < text.Length)
            {
                if (patternIdx < pattern.Length && (pattern[patternIdx] == '?' || pattern[patternIdx] == text[textIdx]))
                {
                    textIdx++;
                    patternIdx++;
                }
                else if (patternIdx < pattern.Length && pattern[patternIdx] == '*')
                {
                    starIdx = patternIdx;
                    matchIdx = textIdx;
                    patternIdx++;
                }
                else if (starIdx != -1)
                {
                    patternIdx = starIdx + 1;
                    matchIdx++;
                    textIdx = matchIdx;
                }
                else
                {
                    return false;
                }
            }
            
            while (patternIdx < pattern.Length && pattern[patternIdx] == '*')
            {
                patternIdx++;
            }
            
            return patternIdx == pattern.Length;
        }

        // Handle cd command
        public string HandleCdCommand(string[] args)
        {
            string targetPath = args.Length > 1 ? args[1] : fileSystem.GetUserHomeDirectory();

            if (fileSystem.DirectoryExists(targetPath))
            {
                fileSystem.SetCurrentDirectory(targetPath);
                return "";
            }
            else
            {
                return $"cd: {targetPath}: No such file or directory";
            }
        }

        // Handle pwd command
        public string HandlePwdCommand()
        {
            return fileSystem.GetCurrentDirectory();
        }

        // Handle mkdir command
        public string HandleMkdirCommand(string[] args)
        {
            if (args.Length < 2)
            {
                return "mkdir: missing operand";
            }

            bool success = true;
            var errors = new List<string>();

            for (int i = 1; i < args.Length; i++)
            {
                if (!fileSystem.CreateDirectory(args[i]))
                {
                    errors.Add($"mkdir: cannot create directory '{args[i]}': File exists or parent directory not found");
                    success = false;
                }
            }

            return success ? "" : string.Join("\n", errors);
        }

        // Handle rmdir command
        public string HandleRmdirCommand(string[] args)
        {
            if (args.Length < 2)
            {
                return "rmdir: missing operand";
            }

            var errors = new List<string>();

            for (int i = 1; i < args.Length; i++)
            {
                if (!fileSystem.DeleteDirectory(args[i], false))
                {
                    errors.Add($"rmdir: failed to remove '{args[i]}': Directory not empty or does not exist");
                }
            }

            return errors.Count > 0 ? string.Join("\n", errors) : "";
        }

        // Handle rm command
        public string HandleRmCommand(string[] args)
        {
            if (args.Length < 2)
            {
                return "rm: missing operand";
            }

            bool recursive = false;
            bool force = false;
            var filesToDelete = new List<string>();

            // Parse arguments
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i].StartsWith("-"))
                {
                    if (args[i].Contains("r") || args[i].Contains("R")) recursive = true;
                    if (args[i].Contains("f")) force = true;
                }
                else
                {
                    filesToDelete.Add(args[i]);
                }
            }

            var errors = new List<string>();

            foreach (var path in filesToDelete)
            {
                var node = fileSystem.GetNode(path);
                if (node == null)
                {
                    if (!force) errors.Add($"rm: cannot remove '{path}': No such file or directory");
                    continue;
                }

                bool success = false;
                if (node.IsDirectory)
                {
                    if (recursive)
                    {
                        success = fileSystem.DeleteDirectory(path, true);
                    }
                    else
                    {
                        errors.Add($"rm: cannot remove '{path}': Is a directory");
                        continue;
                    }
                }
                else
                {
                    success = fileSystem.DeleteFile(path);
                }

                if (!success && !force)
                {
                    errors.Add($"rm: cannot remove '{path}': Operation failed");
                }
            }

            return errors.Count > 0 ? string.Join("\n", errors) : "";
        }

        // Handle cp command
        public string HandleCpCommand(string[] args)
        {
            if (args.Length < 3)
            {
                return "cp: missing file operand";
            }

            string source = args[1];
            string dest = args[2];

            // Special handling for mounting from /mnt to current directory
            if (source.StartsWith("/mnt/") && chassis != null)
            {
                return HandleMountCopy(source, dest);
            }

            if (!fileSystem.FileExists(source))
            {
                return $"cp: cannot stat '{source}': No such file or directory";
            }

            if (fileSystem.CopyFile(source, dest))
            {
                return "";
            }
            else
            {
                return $"cp: cannot create regular file '{dest}': Operation failed";
            }
        }

        // Handle mv command
        public string HandleMvCommand(string[] args)
        {
            if (args.Length < 3)
            {
                return "mv: missing file operand";
            }

            string source = args[1];
            string dest = args[2];

            if (!fileSystem.FileExists(source))
            {
                return $"mv: cannot stat '{source}': No such file or directory";
            }

            if (fileSystem.MoveFile(source, dest))
            {
                return "";
            }
            else
            {
                return $"mv: cannot move '{source}' to '{dest}': Operation failed";
            }
        }

        // Handle cat command
        public string HandleCatCommand(string[] args)
        {
            if (args.Length < 2)
            {
                return "cat: missing file operand";
            }

            var results = new List<string>();

            for (int i = 1; i < args.Length; i++)
            {
                string content = fileSystem.ReadFile(args[i]);
                if (content == null)
                {
                    results.Add($"cat: {args[i]}: No such file or directory");
                }
                else
                {
                    results.Add(content);
                }
            }

            return string.Join("\n", results);
        }

        // Handle find command
        public string HandleFindCommand(string[] args)
        {
            string searchPath = "/";
            string pattern = "*";

            if (args.Length > 1)
            {
                searchPath = args[1];
            }
            if (args.Length > 2 && args[args.Length - 1] != "-name")
            {
                pattern = args[args.Length - 1];
            }

            var results = fileSystem.FindFiles(pattern, searchPath);
            return string.Join("\n", results);
        }

        // Handle touch command
        public string HandleTouchCommand(string[] args)
        {
            if (args.Length < 2)
            {
                return "touch: missing file operand";
            }

            var errors = new List<string>();

            for (int i = 1; i < args.Length; i++)
            {
                if (!fileSystem.CreateFile(args[i], ""))
                {
                    errors.Add($"touch: cannot touch '{args[i]}': Operation failed");
                }
            }

            return errors.Count > 0 ? string.Join("\n", errors) : "";
        }

        // Handle mount command with enhanced functionality
        public string HandleMountCommand(string[] args)
        {
            if (args.Length < 3)
            {
                return "mount: missing arguments\nUsage: mount <device> <mountpoint>";
            }

            string device = args[1];
            string mountpoint = args[2];

            // Check if device exists (USB drive)
            if (chassis != null && chassis.DirectoryExsists(device))
            {
                if (mountpoint == "/mnt" && chassis.UsbCorrect())
                {
                    // Create the mount point in our virtual filesystem
                    if (!fileSystem.DirectoryExists("/mnt"))
                    {
                        fileSystem.CreateDirectory("/mnt");
                    }

                    // Create a symbolic reference to the USB files
                    if (chassis.InsertedUsbPort != null)
                    {
                        var usbFiles = chassis.InsertedUsbPort.Dir.Files;
                        foreach (var file in usbFiles)
                        {
                            fileSystem.CreateFile($"/mnt/{file}", $"[USB FILE: {file}]");
                        }
                    }

                    commandProcessor.Mounted = true;
                    return "";
                }
            }

            return $"mount: {device}: No such device or operation not permitted";
        }

        // Handle umount command
        public string HandleUmountCommand(string[] args)
        {
            if (args.Length < 2)
            {
                return "umount: missing arguments";
            }

            string mountpoint = args[1];

            if (mountpoint == "/mnt" && commandProcessor.Mounted)
            {
                // Clear mounted files from virtual filesystem
                var mntNode = fileSystem.GetNode("/mnt");
                if (mntNode != null && mntNode.IsDirectory)
                {
                    var children = mntNode.GetChildren().ToList();
                    foreach (var child in children)
                    {
                        if (!child.IsDirectory)
                        {
                            fileSystem.DeleteFile($"/mnt/{child.Name}");
                        }
                    }
                }

                commandProcessor.Mounted = false;
                return "";
            }

            return $"umount: {mountpoint}: not mounted";
        }

        // Handle copying from mounted USB to local filesystem
        private string HandleMountCopy(string source, string dest)
        {
            if (!commandProcessor.Mounted || chassis.InsertedUsbPort == null)
            {
                return "cp: source not mounted";
            }

            // Extract filename from source path
            string fileName = source.Substring(source.LastIndexOf('/') + 1);
            
            // Check if file exists on USB
            if (!chassis.InsertedUsbPort.Dir.FileExsists(fileName))
            {
                return $"cp: cannot stat '{source}': No such file or directory";
            }

            // Copy to destination in virtual filesystem
            string destPath = dest == "." ? fileSystem.GetCurrentDirectory() + "/" + fileName : dest;
            
            // Create the file with simulated content
            fileSystem.CreateFile(destPath, $"[COPIED FROM USB: {fileName}]");

            // Also copy to the chassis system if needed
            if (chassis != null)
            {
                chassis.CopyFilesToArray(new string[] { fileName });
            }

            return "";
        }

        // Add special files to virtual filesystem based on system state
        public void UpdateSystemFiles()
        {
            if (chassis == null) return;

            // Update hostname
            fileSystem.CreateFile("/etc/hostname", $"{chassis.GetComputerName()}-{chassis.selectedController}");
            
            // Update timezone from chassis
            string timeZone = chassis.selectedController == "CT0" ? 
                chassis.flashArrays[0].TimeZone : chassis.flashArrays[1].TimeZone;
            fileSystem.CreateFile("/etc/timezone", timeZone);

            // Update version info
            fileSystem.CreateFile("/etc/purity-version", chassis.GetCurrentPurityVersion());
        }

        // Handle additional system commands
        public string HandleSystemCommand(string[] args)
        {
            if (hardwareManager == null && !IsSimpleCommand(args[0])) 
                return "Hardware manager not initialized";

            switch (args[0])
            {
                case "df":
                    return HandleDfCommand(args);
                case "lsblk":
                    return HandleLsblkCommand(args);
                case "ifconfig":
                    return HandleIfconfigCommand(args);
                case "lspci":
                    return HandleLspciCommand(args);
                case "free":
                    return HandleFreeCommand(args);
                case "iostat":
                    return HandleIostatCommand(args);
                case "which":
                    return HandleWhichCommand(args);
                case "whoami":
                    return HandleWhoamiCommand(args);
                case "id":
                    return HandleIdCommand(args);
                case "uname":
                    return HandleUnameCommand(args);
                case "hostname":
                    return HandleHostnameCommand(args);
                case "uptime":
                    return HandleUptimeCommand(args);
                case "date":
                    return HandleDateCommand(args);
                case "history":
                    return HandleHistoryCommand(args);
                default:
                    return null; // Command not handled
            }
        }

        private bool IsSimpleCommand(string cmd)
        {
            return new string[] { "which", "whoami", "id", "uname", "hostname", "uptime", "date", "history" }.Contains(cmd);
        }

        private string HandleDfCommand(string[] args)
        {
            return hardwareManager.GetDriveStatus();
        }

        private string HandleLsblkCommand(string[] args)
        {
            var drives = hardwareManager.GetAllDrives();
            var lines = new List<string>
            {
                "NAME   MAJ:MIN RM   SIZE RO TYPE MOUNTPOINT"
            };

            foreach (var drive in drives)
            {
                string devicePath = drive.DevicePath;
                string[] pathParts = devicePath.Split('/');
                string name = pathParts[pathParts.Length - 1];
                string mountPoint = drive.IsMounted ? drive.MountPoint : "";
                lines.Add($"{name,-6} 8:1    0  {drive.GetFormattedSize(),-6} 0 disk {mountPoint}");
            }

            return string.Join("\n", lines);
        }

        private string HandleIfconfigCommand(string[] args)
        {
            return hardwareManager.GetNetworkStatus();
        }

        private string HandleLspciCommand(string[] args)
        {
            return hardwareManager.GetPCIeStatus();
        }

        private string HandleFreeCommand(string[] args)
        {
            return @"              total        used        free      shared  buff/cache   available
Mem:       65871872     8388608    32935936     1048576    24547328    58234560
Swap:       8388608           0     8388608";
        }

        private string HandleIostatCommand(string[] args)
        {
            return @"Linux 5.15.123+ (purearray-ct0)     " + DateTime.Now.ToString("MM/dd/yyyy") + @"     _x86_64_    (8 CPU)

avg-cpu:  %user   %nice %system %iowait  %steal   %idle
           2.15    0.00    1.23    0.84    0.00   95.78

Device:            tps    kB_read/s    kB_wrtn/s    kB_read    kB_wrtn
sda              15.23       123.45       234.56     123456     234567
nvme0n1          89.12       456.78       567.89     456789     567890
nvme1n1          87.34       445.67       556.78     445678     556789";
        }

        private string HandleWhichCommand(string[] args)
        {
            if (args.Length < 2)
            {
                return "which: missing argument";
            }

            string command = args[1];
            
            // Check common locations for executables
            string[] paths = { "/usr/bin", "/usr/local/bin", "/bin", "/sbin", "/usr/sbin", "/opt/Purity/bin" };
            
            foreach (string path in paths)
            {
                if (fileSystem.FileExists($"{path}/{command}"))
                {
                    return $"{path}/{command}";
                }
            }

            return $"which: no {command} in ({string.Join(":", paths)})";
        }

        private string HandleWhoamiCommand(string[] args)
        {
            return fileSystem.GetCurrentUser();
        }

        private string HandleIdCommand(string[] args)
        {
            string user = fileSystem.GetCurrentUser();
            return user switch
            {
                "root" => "uid=0(root) gid=0(root) groups=0(root)",
                "puresetup" => "uid=1000(puresetup) gid=1000(puresetup) groups=1000(puresetup),4(adm),24(cdrom),27(sudo)",
                "pureeng" => "uid=1001(pureeng) gid=1001(pureeng) groups=1001(pureeng),4(adm),24(cdrom),27(sudo)",
                _ => $"uid=1002({user}) gid=1002({user}) groups=1002({user})"
            };
        }

        private string HandleUnameCommand(string[] args)
        {
            if (args.Length > 1 && args[1] == "-a")
            {
                return "Linux purearray-ct0 5.15.123+ #1 SMP " + DateTime.Now.ToString("ddd MMM dd HH:mm:ss UTC yyyy") + " x86_64 x86_64 x86_64 GNU/Linux";
            }
            return "Linux";
        }

        private string HandleHostnameCommand(string[] args)
        {
            if (chassis != null)
            {
                return $"{chassis.GetComputerName()}-{chassis.selectedController}";
            }
            return "purearray-ct0";
        }

        private string HandleUptimeCommand(string[] args)
        {
            var uptime = TimeSpan.FromSeconds(UnityEngine.Random.Range(3600, 86400)); // Random uptime between 1 hour and 1 day
            int users = 1;
            return $" {DateTime.Now.ToString("HH:mm:ss")} up {uptime.Days} days, {uptime.Hours}:{uptime.Minutes:D2}, {users} user, load average: 0.15, 0.25, 0.30";
        }

        private string HandleDateCommand(string[] args)
        {
            return DateTime.Now.ToString("ddd MMM dd HH:mm:ss UTC yyyy");
        }

        private string HandleHistoryCommand(string[] args)
        {
            // Read the user's bash history file
            string user = fileSystem.GetCurrentUser();
            string historyFile = $"/home/{user}/.bash_history";
            if (user == "root") historyFile = "/root/.bash_history";

            string historyContent = fileSystem.ReadFile(historyFile);
            if (historyContent == null)
            {
                return "bash: history: command not available";
            }

            // Add line numbers
            var lines = historyContent.Split('\n');
            var numberedLines = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrEmpty(lines[i]))
                {
                    numberedLines.Add($"  {i + 1,3}  {lines[i]}");
                }
            }

            return string.Join("\n", numberedLines);
        }
        
        public string HandleChmodCommand(string[] args)
        {
            if (args.Length < 3)
            {
                return "chmod: missing operand\nUsage: chmod [MODE] [FILE]";
            }
            
            string mode = args[1];
            string path = args[2];
            
            var node = fileSystem.GetNode(path);
            if (node == null)
            {
                return $"chmod: cannot access '{path}': No such file or directory";
            }
            
            // Update permissions (simplified - just acknowledge the command)
            node.Permissions = mode.TrimStart('+');
            return ""; // Success, no output
        }
        
        public string HandleChownCommand(string[] args)
        {
            if (args.Length < 3)
            {
                return "chown: missing operand\nUsage: chown [OWNER]:[GROUP] [FILE]";
            }
            
            string ownerGroup = args[1];
            string path = args[2];
            
            var node = fileSystem.GetNode(path);
            if (node == null)
            {
                return $"chown: cannot access '{path}': No such file or directory";
            }
            
            // Check if user has permission (only root can chown)
            if (fileSystem.GetCurrentUser() != "root")
            {
                return $"chown: changing ownership of '{path}': Operation not permitted";
            }
            
            // Parse owner:group
            string[] parts = ownerGroup.Split(':');
            if (parts.Length > 0 && !string.IsNullOrEmpty(parts[0]))
            {
                node.Owner = parts[0];
            }
            if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
            {
                node.Group = parts[1];
            }
            else if (parts.Length == 1)
            {
                // If only owner specified, set group to same as owner
                node.Group = parts[0];
            }
            
            return ""; // Success, no output
        }
    }
}