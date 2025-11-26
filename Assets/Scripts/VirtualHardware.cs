using System;
using System.Collections.Generic;
using PureSim.Simulation;
using UnityEngine;

namespace CrimsofallTechnologies.ServerSimulator
{
    [System.Serializable]
    public class VirtualDrive
    {
        public string Name;
        public string DevicePath; // e.g., /dev/sda1, /dev/nvme0n1
        public string FileSystem; // ext4, xfs, ntfs, etc.
        public long SizeBytes;
        public long UsedBytes;
        public bool IsMounted;
        public string MountPoint;
        public string Status; // healthy, degraded, failed
        public string Model;
        public string SerialNumber;
        public DateTime LastChecked;

        public VirtualDrive() {}

        public VirtualDrive(string name, string devicePath, long sizeBytes)
        {
            Name = name;
            DevicePath = devicePath;
            SizeBytes = sizeBytes;
            UsedBytes = 0;
            IsMounted = false;
            MountPoint = "";
            Status = "healthy";
            Model = "Pure Storage Drive";
            SerialNumber = GenerateSerialNumber();
            FileSystem = "xfs";
            LastChecked = DateTime.Now;
        }

        private string GenerateSerialNumber()
        {
            return "PST" + UnityEngine.Random.Range(100000, 999999).ToString();
        }

        public string GetUsagePercentage()
        {
            if (SizeBytes == 0) return "0%";
            return ((UsedBytes * 100) / SizeBytes).ToString() + "%";
        }

        public string GetFormattedSize()
        {
            return FormatBytes(SizeBytes);
        }

        public string GetFormattedUsed()
        {
            return FormatBytes(UsedBytes);
        }

        public string GetFormattedFree()
        {
            return FormatBytes(SizeBytes - UsedBytes);
        }

        private string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB" };
            int suffixIndex = 0;
            double size = bytes;

            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }

            return $"{size:F1}{suffixes[suffixIndex]}";
        }
    }

    [System.Serializable]
    public class VirtualNetworkInterface
    {
        public string Name;
        public string IPAddress;
        public string Netmask;
        public string Gateway;
        public bool IsUp;
        public string Status;
        public long RxBytes;
        public long TxBytes;
        public string MAC;

        public VirtualNetworkInterface(string name)
        {
            Name = name;
            IPAddress = "192.168.1.100";
            Netmask = "255.255.255.0";
            Gateway = "192.168.1.1";
            IsUp = true;
            Status = "UP";
            RxBytes = 0;
            TxBytes = 0;
            MAC = GenerateMAC();
        }

        private string GenerateMAC()
        {
            var random = new System.Random();
            var macBytes = new byte[6];
            random.NextBytes(macBytes);
            var macParts = new string[6];
            for (int i = 0; i < macBytes.Length; i++)
            {
                macParts[i] = macBytes[i].ToString("x2");
            }
            return string.Join(":", macParts);
        }
    }

    [System.Serializable]
    public class VirtualPCIeCard
    {
        public string Name;
        public string BusAddress; // e.g., 0000:03:00.0
        public string DeviceType; // network, storage, gpu, etc.
        public string Status;
        public string Driver;
        public string Vendor;
        public string Device;

        public VirtualPCIeCard(string name, string deviceType, string busAddress)
        {
            Name = name;
            DeviceType = deviceType;
            BusAddress = busAddress;
            Status = "OK";
            Driver = "loaded";
            Vendor = "Pure Storage";
            Device = name;
        }
    }

    public class VirtualHardwareManager : MonoBehaviour
    {
        private List<VirtualDrive> drives;

        //this stores which drive is at which index for easy lookup (for above drives list)
        private Dictionary<string, int> indexedDrives;
        private List<VirtualNetworkInterface> networkInterfaces;
        private List<VirtualPCIeCard> pcieCards;
        private VirtualFileSystemHandler fileSystemHandler;

        public void Initialize(VirtualFileSystemHandler fsHandler)
        {
            fileSystemHandler = fsHandler;
            InitializeHardware();
            CreateHardwareFiles();
        }

        private void InitializeHardware()
        {
            drives = new List<VirtualDrive>();
            networkInterfaces = new List<VirtualNetworkInterface>();
            pcieCards = new List<VirtualPCIeCard>();

            // Create virtual drives
            drives.Add(new VirtualDrive("System Drive", "/dev/sda1", 500L * 1024 * 1024 * 1024)); // 500GB
            drives.Add(new VirtualDrive("Data Drive 1", "/dev/nvme0n1", 2L * 1024 * 1024 * 1024 * 1024)); // 2TB
            drives.Add(new VirtualDrive("Data Drive 2", "/dev/nvme1n1", 2L * 1024 * 1024 * 1024 * 1024)); // 2TB

            //init drive index lookup
            indexedDrives = new Dictionary<string, int>();
            for (int i = 0; i < drives.Count; i++) {
                indexedDrives[drives[i].DevicePath] = i;
            }

            // Create network interfaces
            networkInterfaces.Add(new VirtualNetworkInterface("eth0"));
            networkInterfaces.Add(new VirtualNetworkInterface("eth1"));
            networkInterfaces.Add(new VirtualNetworkInterface("eth2"));
            networkInterfaces.Add(new VirtualNetworkInterface("eth3"));

            // Create PCIe cards
            pcieCards.Add(new VirtualPCIeCard("Pure Storage FC Card", "fc", "0000:06:00.0"));
            pcieCards.Add(new VirtualPCIeCard("Intel Ethernet Controller", "network", "0000:03:00.0"));
            pcieCards.Add(new VirtualPCIeCard("NVMe Controller 1", "storage", "0000:05:00.0"));
            pcieCards.Add(new VirtualPCIeCard("NVMe Controller 2", "storage", "0000:07:00.0"));
        }

        private void CreateHardwareFiles()
        {
            if (fileSystemHandler == null) return;

            var fs = fileSystemHandler.GetFileSystem();

            // Create /proc filesystem entries
            fs.CreateFile("/proc/mounts", GenerateMountsFile());
            fs.CreateFile("/proc/meminfo", GenerateMemInfoFile());
            fs.CreateFile("/proc/partitions", GeneratePartitionsFile());

            // Create /sys filesystem entries
            fs.CreateDirectory("/sys/block");
            fs.CreateDirectory("/sys/class");
            fs.CreateDirectory("/sys/class/net");

            // Create drive entries in /sys/block
            foreach (var drive in drives)
            {
                string devicePath = drive.DevicePath;
                string[] pathParts = devicePath.Split('/');
                string driveName = pathParts[pathParts.Length - 1];
                // Remove partition numbers
                driveName = driveName.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
                fs.CreateDirectory($"/sys/block/{driveName}");
                fs.CreateFile($"/sys/block/{driveName}/size", (drive.SizeBytes / 512).ToString());
                fs.CreateFile($"/sys/block/{driveName}/model", drive.Model);
            }

            // Create network interface entries
            foreach (var netif in networkInterfaces)
            {
                fs.CreateDirectory($"/sys/class/net/{netif.Name}");
                fs.CreateFile($"/sys/class/net/{netif.Name}/address", netif.MAC);
                fs.CreateFile($"/sys/class/net/{netif.Name}/operstate", netif.IsUp ? "up" : "down");
            }

            // Create device files in /dev
            foreach (var drive in drives)
            {
                fs.CreateFile(drive.DevicePath, "[BLOCK DEVICE]");
            }
        }

        private string GenerateMountsFile()
        {
            var lines = new List<string>();
            
            foreach (var drive in drives)
            {
                if (drive.IsMounted && !string.IsNullOrEmpty(drive.MountPoint))
                {
                    lines.Add($"{drive.DevicePath} {drive.MountPoint} {drive.FileSystem} rw,relatime 0 0");
                }
            }

            // Add standard mounts
            lines.Add("/dev/sda1 / xfs rw,relatime 0 0");
            lines.Add("proc /proc proc rw,nosuid,nodev,noexec,relatime 0 0");
            lines.Add("sysfs /sys sysfs rw,nosuid,nodev,noexec,relatime 0 0");
            lines.Add("tmpfs /tmp tmpfs rw,nosuid,nodev 0 0");

            return string.Join("\n", lines);
        }

        private string GenerateMemInfoFile()
        {
            return @"MemTotal:       65871872 kB
MemFree:        32935936 kB
MemAvailable:   58234560 kB
Buffers:         2097152 kB
Cached:         23068672 kB
SwapCached:            0 kB
Active:         16777216 kB
Inactive:       12582912 kB
SwapTotal:       8388608 kB
SwapFree:        8388608 kB";
        }

        private string GeneratePartitionsFile()
        {
            var lines = new List<string>
            {
                "major minor  #blocks  name"
            };

            int majorNum = 8;
            foreach (var drive in drives)
            {
                string devicePath = drive.DevicePath;
                string[] pathParts = devicePath.Split('/');
                string deviceName = pathParts[pathParts.Length - 1];
                long blocks = drive.SizeBytes / 1024;
                lines.Add($"   {majorNum}        1  {blocks,10}  {deviceName}");
                majorNum++;
            }

            return string.Join("\n", lines);
        }

        // Hardware status commands
        public string GetDriveStatus()
        {
            var lines = new List<string>
            {
                "Name                Status    Size       Used      Free      Use%  Mounted on"
            };

            foreach (var drive in drives)
            {
                string line = $"{drive.Name,-20} {drive.Status,-9} {drive.GetFormattedSize(),-10} " +
                             $"{drive.GetFormattedUsed(),-9} {drive.GetFormattedFree(),-9} {drive.GetUsagePercentage(),-5} " +
                             $"{(drive.IsMounted ? drive.MountPoint : "not mounted")}";
                lines.Add(line);
            }

            return string.Join("\n", lines);
        }

        public string GetNetworkStatus()
        {
            var lines = new List<string>
            {
                "Interface  Status  IP Address       MAC Address        RX Bytes    TX Bytes"
            };

            foreach (var netif in networkInterfaces)
            {
                string line = $"{netif.Name,-10} {netif.Status,-7} {netif.IPAddress,-15} {netif.MAC,-18} " +
                             $"{netif.RxBytes,10} {netif.TxBytes,10}";
                lines.Add(line);
            }

            return string.Join("\n", lines);
        }

        public string GetPCIeStatus()
        {
            var lines = new List<string>
            {
                "Bus Address   Device Type  Status  Driver   Vendor/Device"
            };

            foreach (var card in pcieCards)
            {
                string line = $"{card.BusAddress,-13} {card.DeviceType,-12} {card.Status,-7} {card.Driver,-8} " +
                             $"{card.Vendor}/{card.Device}";
                lines.Add(line);
            }

            return string.Join("\n", lines);
        }

        public VirtualDrive GetDrive(string devicePath)
        {
            return drives.Find(d => d.DevicePath == devicePath);
        }

        public List<VirtualDrive> GetAllDrives()
        {
            return new List<VirtualDrive>(drives);
        }

        public VirtualNetworkInterface GetNetworkInterface(string name)
        {
            return networkInterfaces.Find(n => n.Name == name);
        }

        public List<VirtualNetworkInterface> GetAllNetworkInterfaces()
        {
            return new List<VirtualNetworkInterface>(networkInterfaces);
        }

        //useful for inserting USB drives
        public void CreateAndAddDrive(string driveName, VirtualDirectory Dir)
        {
            VirtualDrive drive = new VirtualDrive
            {
                Name = driveName,
                DevicePath = Dir.DirectoryName,
                SizeBytes = 32L * 1024 * 1024 * 1024, //32 GB,
                UsedBytes = 7L * 1024 * 1024 * 1024, //7 GB used
                Status = "Ready",
                FileSystem = "NTFS",
                Model = "USB Drive",
                SerialNumber = "" + UnityEngine.Random.Range(10000000, 99999999),  
            };

            drives.Add(drive);
            indexedDrives[drive.Name] = drives.Count - 1;

            fileSystemHandler.GetFileSystem().CreateFile(Dir.DirectoryName, "[BLOCK DEVICE]");
        }

        //for removing USB drives
        public void RemoveDrive(string driveName)
        {
            if(indexedDrives.ContainsKey(driveName))
            {
                indexedDrives.Remove(driveName, out int index);
                drives.RemoveAt(index);
                fileSystemHandler.GetFileSystem().DeleteFile(drives[index].DevicePath);
            }
        }
    }
}