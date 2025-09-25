using System.Collections.Generic;
using UnityEngine;

namespace CrimsofallTechnologies.ServerSimulator
{
    public static class VirtualFileSystemPopulator
    {
        public static void PopulateWithSampleFiles(VirtualFileSystem fs)
        {
            // Add sample files to make the filesystem feel more realistic
            
            // /etc files
            fs.CreateFile("/etc/hosts", "127.0.0.1\tlocalhost\n127.0.1.1\tpurearray-ct0\n::1\tip6-localhost ip6-loopback");
            fs.CreateFile("/etc/resolv.conf", "nameserver 8.8.8.8\nnameserver 8.8.4.4\nsearch pure.local");
            fs.CreateFile("/etc/fstab", "# <file system> <mount point> <type> <options> <dump> <pass>\n/dev/sda1 / xfs defaults 0 1");
            fs.CreateFile("/etc/motd", "Welcome to Pure Storage FlashArray Simulator\nPurity Operating Environment");
            fs.CreateFile("/etc/issue", "Pure Storage Purity \\n \\l");
            fs.CreateFile("/etc/os-release", "NAME=\"Purity\"\nVERSION=\"6.5.8\"\nID=purity\nID_LIKE=ubuntu\nPRETTY_NAME=\"Purity 6.5.8\"");
            
            // /usr/bin executables (simulated)
            var binCommands = new string[] 
            {
                "bash", "sh", "ls", "cp", "mv", "rm", "cat", "grep", "awk", "sed", "vi", "nano",
                "top", "ps", "kill", "killall", "mount", "umount", "df", "du", "free", "lsblk",
                "ifconfig", "ping", "wget", "curl", "ssh", "scp", "rsync", "tar", "gzip", "gunzip",
                "find", "locate", "which", "whoami", "id", "su", "sudo", "chmod", "chown", "chgrp"
            };
            
            foreach (var cmd in binCommands)
            {
                fs.CreateFile($"/usr/bin/{cmd}", $"[EXECUTABLE: {cmd}]");
            }
            
            // /usr/local/bin (Pure Storage specific tools)
            var pureCommands = new string[]
            {
                "purearray", "puredrive", "purenetwork", "purehw", "pureboot", "puresetup",
                "puremessage", "purealert", "pureversion", "pureadm", "purewes"
            };
            
            foreach (var cmd in pureCommands)
            {
                fs.CreateFile($"/usr/local/bin/{cmd}", $"[PURE STORAGE EXECUTABLE: {cmd}]");
            }
            
            // /var/log files
            fs.CreateFile("/var/log/messages", GenerateLogFile("system"));
            fs.CreateFile("/var/log/purity.log", GenerateLogFile("purity"));
            fs.CreateFile("/var/log/syslog", GenerateLogFile("syslog"));
            fs.CreateFile("/var/log/auth.log", GenerateLogFile("auth"));
            fs.CreateFile("/var/log/kern.log", GenerateLogFile("kernel"));
            
            // User home directories with sample files
            PopulateUserHome(fs, "puresetup");
            PopulateUserHome(fs, "pureeng");
            PopulateRootHome(fs);
            
            // /tmp with some temporary files
            fs.CreateFile("/tmp/install.log", "Installation log from last Purity upgrade...");
            fs.CreateFile("/tmp/config.tmp", "Temporary configuration data");
            
            // /opt (Optional software)
            fs.CreateDirectory("/opt/Purity");
            fs.CreateDirectory("/opt/Purity/bin");
            fs.CreateDirectory("/opt/Purity/lib");
            fs.CreateDirectory("/opt/Purity/config");
            
            fs.CreateFile("/opt/Purity/bin/storage_view.py", "[PYTHON SCRIPT: Pure Storage diagnostic tool]");
            fs.CreateFile("/opt/Purity/bin/hardware_check.py", "[PYTHON SCRIPT: Hardware diagnostic tool]");
            fs.CreateFile("/opt/Purity/config/array.conf", "# Array configuration\narray_name=FlashArray-m20\nversion=6.5.8");
            
            // /proc entries (will be dynamically updated by VirtualHardwareManager)
            fs.CreateFile("/proc/uptime", "3600.25 3500.10");
            fs.CreateFile("/proc/loadavg", "0.15 0.25 0.30 1/234 12567");
            fs.CreateFile("/proc/stat", "cpu  123456 0 98765 8765432 4321 0 1234 0 0 0");
            
            // Create some sample data files that users might work with
            fs.CreateDirectory("/data");
            fs.CreateFile("/data/sample.txt", "This is sample data that can be manipulated with Linux commands.");
            fs.CreateFile("/data/config.json", "{\n  \"array_name\": \"FlashArray-m20\",\n  \"version\": \"6.5.8\",\n  \"controllers\": [\"CT0\", \"CT1\"]\n}");
        }
        
        private static void PopulateUserHome(VirtualFileSystem fs, string username)
        {
            string homeDir = $"/home/{username}";
            
            // Common files
            fs.CreateFile($"{homeDir}/.bashrc", GenerateBashrc(username));
            fs.CreateFile($"{homeDir}/.profile", "# ~/.profile: executed by the command interpreter for login shells.");
            fs.CreateFile($"{homeDir}/.bash_history", GenerateBashHistory(username));
            fs.CreateFile($"{homeDir}/.vimrc", "set number\nset autoindent\nsyntax on");
            
            // Create Documents directory
            fs.CreateDirectory($"{homeDir}/Documents");
            fs.CreateFile($"{homeDir}/Documents/README.txt", $"Welcome {username}! This is your documents folder.");
            
            if (username == "pureeng")
            {
                // Engineering-specific files
                fs.CreateFile($"{homeDir}/Documents/troubleshooting.md", "# Troubleshooting Guide\n\n## Common Issues\n- Drive failures\n- Network connectivity\n- Performance tuning");
                fs.CreateDirectory($"{homeDir}/scripts");
                fs.CreateFile($"{homeDir}/scripts/backup.sh", "#!/bin/bash\necho 'Backup script for Pure Storage maintenance'");
                fs.CreateFile($"{homeDir}/scripts/health_check.py", "#!/usr/bin/env python3\n# Health check script for FlashArray");
            }
            else if (username == "puresetup")
            {
                // Setup-specific files
                fs.CreateFile($"{homeDir}/Documents/setup_guide.md", "# Pure Storage Setup Guide\n\n## Initial Configuration\n- Network setup\n- Array initialization\n- User management");
                fs.CreateDirectory($"{homeDir}/configs");
                fs.CreateFile($"{homeDir}/configs/initial_setup.conf", "# Initial setup configuration\narray_type=X20R4\ncontrollers=2");
            }
        }
        
        private static void PopulateRootHome(VirtualFileSystem fs)
        {
            fs.CreateFile("/root/.bashrc", GenerateBashrc("root"));
            fs.CreateFile("/root/.profile", "# ~/.profile: executed by Bourne-compatible login shells.");
            fs.CreateFile("/root/.bash_history", GenerateBashHistory("root"));
            
            // Root-specific files
            fs.CreateDirectory("/root/maintenance");
            fs.CreateFile("/root/maintenance/system_update.log", "System maintenance log...");
            fs.CreateFile("/root/.ssh/authorized_keys", "ssh-rsa AAAAB3... pure-admin@flasharray");
        }
        
        private static string GenerateBashrc(string username)
        {
            return $@"# .bashrc for {username}

# Source global definitions
if [ -f /etc/bashrc ]; then
    . /etc/bashrc
fi

# User specific aliases and functions
alias ll='ls -alF'
alias la='ls -A'
alias l='ls -CF'
alias ..='cd ..'
alias ...='cd ../..'

# Pure Storage specific aliases
alias palist='purearray list'
alias pdlist='puredrive list'
alias phlist='purehw list'

# Set PS1 for better prompt
export PS1='[{username}@\h \W]\$ '

# Add Pure Storage tools to PATH
export PATH=$PATH:/usr/local/bin:/opt/Purity/bin";
        }
        
        private static string GenerateBashHistory(string username)
        {
            var commands = new List<string>();
            
            // Common commands for all users
            commands.AddRange(new string[]
            {
                "ls -la",
                "pwd",
                "cd /home/" + username,
                "cat /etc/hostname",
                "df -h",
                "free -m",
                "ps aux",
                "top",
                "ping 8.8.8.8",
                "ls /var/log/"
            });
            
            if (username == "root")
            {
                commands.AddRange(new string[]
                {
                    "purehw list",
                    "puredrive list",
                    "purearray list --controller",
                    "pureadm status",
                    "hardware_check.py",
                    "systemctl status purity",
                    "tail -f /var/log/purity.log",
                    "mount /dev/sdb1 /mnt",
                    "umount /mnt"
                });
            }
            else if (username == "pureeng")
            {
                commands.AddRange(new string[]
                {
                    "puredrive list",
                    "purearray list",
                    "purenetwork list",
                    "cat /var/log/purity.log",
                    "sudo purehw list",
                    "cd /home/pureeng/scripts",
                    "./health_check.py"
                });
            }
            else if (username == "puresetup")
            {
                commands.AddRange(new string[]
                {
                    "puresetup newarray",
                    "puresetup show",
                    "pureboot list",
                    "pureversion list",
                    "cd /home/puresetup/configs"
                });
            }
            
            return string.Join("\n", commands);
        }
        
        private static string GenerateLogFile(string logType)
        {
            var timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            return logType switch
            {
                "system" => $"{timestamp} purearray-ct0 kernel: [12345.678] Pure Storage FlashArray initialized\n" +
                          $"{timestamp} purearray-ct0 purity: Array health check completed - Status: OK\n" +
                          $"{timestamp} purearray-ct0 systemd: Started Pure Storage services",
                          
                "purity" => $"{timestamp} INFO: Pure Storage FlashArray started successfully\n" +
                          $"{timestamp} INFO: All drives operational\n" +
                          $"{timestamp} INFO: Network interfaces configured\n" +
                          $"{timestamp} DEBUG: Performance monitoring active",
                          
                "syslog" => $"{timestamp} purearray-ct0 sshd[1234]: Accepted publickey for pureeng from 192.168.1.100\n" +
                          $"{timestamp} purearray-ct0 cron[5678]: (root) CMD (run-parts /etc/cron.hourly)\n" +
                          $"{timestamp} purearray-ct0 systemd: Started user session",
                          
                "auth" => $"{timestamp} purearray-ct0 sudo: pureeng : TTY=pts/0 ; PWD=/home/pureeng ; USER=root ; COMMAND=/usr/local/bin/purehw list\n" +
                        $"{timestamp} purearray-ct0 sudo: pam_unix(sudo:session): session opened for user root by pureeng(uid=1001)\n" +
                        $"{timestamp} purearray-ct0 su: (to root) pureeng on pts/0",
                        
                "kernel" => $"{timestamp} purearray-ct0 kernel: [    0.000000] Initializing Pure Storage drivers\n" +
                          $"{timestamp} purearray-ct0 kernel: [    1.234567] NVMe controller detected: /dev/nvme0\n" +
                          $"{timestamp} purearray-ct0 kernel: [    2.345678] Network interface eth0 link up",
                          
                _ => $"{timestamp} Sample log entry for {logType}"
            };
        }
    }
}