using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CrimsofallTechnologies.ServerSimulator
{
    [System.Serializable]
    public class VirtualFileSystemNode
    {
        public string Name;
        public bool IsDirectory;
        public string Content = "";
        public DateTime CreatedTime;
        public DateTime ModifiedTime;
        public string Permissions = "755";
        public string Owner = "root";
        public string Group = "root";
        public long Size = 0;
        public Dictionary<string, VirtualFileSystemNode> Children;

        public VirtualFileSystemNode(string name, bool isDirectory = false)
        {
            Name = name;
            IsDirectory = isDirectory;
            CreatedTime = DateTime.Now;
            ModifiedTime = DateTime.Now;
            if (isDirectory)
            {
                Children = new Dictionary<string, VirtualFileSystemNode>();
            }
        }

        public VirtualFileSystemNode GetChild(string name)
        {
            if (!IsDirectory || Children == null) return null;
            Children.TryGetValue(name, out VirtualFileSystemNode child);
            return child;
        }

        public void AddChild(VirtualFileSystemNode child)
        {
            if (!IsDirectory) return;
            if (Children == null) Children = new Dictionary<string, VirtualFileSystemNode>();
            Children[child.Name] = child;
            ModifiedTime = DateTime.Now;
        }

        public bool RemoveChild(string name)
        {
            if (!IsDirectory || Children == null) return false;
            bool removed = Children.Remove(name);
            if (removed) ModifiedTime = DateTime.Now;
            return removed;
        }

        public List<VirtualFileSystemNode> GetChildren()
        {
            if (!IsDirectory || Children == null) return new List<VirtualFileSystemNode>();
            return Children.Values.ToList();
        }
    }

    [System.Serializable]
    public class VirtualFileSystem
    {
        private VirtualFileSystemNode root;
        private string currentDirectory = "/";
        private string currentUser = "pureeng";

        public VirtualFileSystem()
        {
            InitializeFileSystem();
        }

        private void InitializeFileSystem()
        {
            root = new VirtualFileSystemNode("/", true);

            // Create standard Linux directory structure
            CreateDirectory("/bin");
            CreateDirectory("/boot");
            CreateDirectory("/dev");
            CreateDirectory("/etc");
            CreateDirectory("/home");
            CreateDirectory("/home/puresetup");
            CreateDirectory("/home/pureeng");
            CreateDirectory("/lib");
            CreateDirectory("/media");
            CreateDirectory("/mnt");
            CreateDirectory("/opt");
            CreateDirectory("/proc");
            CreateDirectory("/root");
            CreateDirectory("/run");
            CreateDirectory("/sbin");
            CreateDirectory("/srv");
            CreateDirectory("/sys");
            CreateDirectory("/tmp");
            CreateDirectory("/usr");
            CreateDirectory("/usr/bin");
            CreateDirectory("/usr/lib");
            CreateDirectory("/usr/local");
            CreateDirectory("/usr/sbin");
            CreateDirectory("/var");
            CreateDirectory("/var/log");
            CreateDirectory("/var/tmp");

            // Create some standard files
            CreateFile("/etc/hostname", "purearray-ct0");
            CreateFile("/etc/timezone", "America/Los_Angeles");
            CreateFile("/etc/passwd", "root:x:0:0:root:/root:/bin/bash\npuresetup:x:1000:1000:Pure Setup:/home/puresetup:/bin/bash\npureeng:x:1001:1001:Pure Engineer:/home/pureeng:/bin/bash");
            CreateFile("/proc/version", "Linux version 5.15.123+ (purity@pure) (gcc version 9.4.0) #1 SMP");
            CreateFile("/proc/cpuinfo", "processor\t: 0\nvendor_id\t: GenuineIntel\nmodel name\t: Intel(R) Xeon(R) CPU");

            // Set proper ownership for user directories
            var puresetupHome = GetNode("/home/puresetup");
            if (puresetupHome != null)
            {
                puresetupHome.Owner = "puresetup";
                puresetupHome.Group = "puresetup";
            }

            var pureengHome = GetNode("/home/pureeng");
            if (pureengHome != null)
            {
                pureengHome.Owner = "pureeng";
                pureengHome.Group = "pureeng";
            }
        }

        public void SetCurrentUser(string user)
        {
            currentUser = user;
        }

        public string GetCurrentUser()
        {
            return currentUser;
        }

        public void SetCurrentDirectory(string directory)
        {
            if (DirectoryExists(directory))
            {
                currentDirectory = NormalizePath(directory);
            }
        }

        public string GetCurrentDirectory()
        {
            return currentDirectory;
        }

        public string GetUserHomeDirectory()
        {
            return currentUser switch
            {
                "root" => "/root",
                "puresetup" => "/home/puresetup",
                "pureeng" => "/home/pureeng",
                _ => "/home/" + currentUser
            };
        }

        private string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "/";
            
            if (!path.StartsWith("/"))
            {
                // Relative path - combine with current directory
                path = currentDirectory.TrimEnd('/') + "/" + path;
            }

            // Split and process path components
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var normalizedParts = new List<string>();

            foreach (var part in parts)
            {
                if (part == "." || part == "") continue;
                if (part == "..")
                {
                    if (normalizedParts.Count > 0)
                        normalizedParts.RemoveAt(normalizedParts.Count - 1);
                }
                else
                {
                    normalizedParts.Add(part);
                }
            }

            var result = "/" + string.Join("/", normalizedParts);
            return result == "" ? "/" : result;
        }

        public VirtualFileSystemNode GetNode(string path)
        {
            path = NormalizePath(path);
            if (path == "/") return root;

            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var current = root;

            foreach (var part in parts)
            {
                if (current?.IsDirectory != true) return null;
                current = current.GetChild(part);
                if (current == null) return null;
            }

            return current;
        }

        public bool CreateDirectory(string path)
        {
            path = NormalizePath(path);
            if (path == "/") return false; // Root already exists

            var parentPath = path.Substring(0, path.LastIndexOf('/'));
            if (parentPath == "") parentPath = "/";

            var parent = GetNode(parentPath);
            if (parent == null || !parent.IsDirectory) return false;

            var dirName = path.Substring(path.LastIndexOf('/') + 1);
            if (parent.GetChild(dirName) != null) return false; // Already exists

            var newDir = new VirtualFileSystemNode(dirName, true)
            {
                Owner = currentUser,
                Group = currentUser
            };
            parent.AddChild(newDir);
            return true;
        }

        public bool CreateFile(string path, string content = "")
        {
            path = NormalizePath(path);
            
            var parentPath = path.Substring(0, path.LastIndexOf('/'));
            if (parentPath == "") parentPath = "/";

            var parent = GetNode(parentPath);
            if (parent == null || !parent.IsDirectory) return false;

            var fileName = path.Substring(path.LastIndexOf('/') + 1);
            
            var existingFile = parent.GetChild(fileName);
            if (existingFile != null)
            {
                // File exists, update content
                if (!existingFile.IsDirectory)
                {
                    existingFile.Content = content;
                    existingFile.Size = content.Length;
                    existingFile.ModifiedTime = DateTime.Now;
                    return true;
                }
                return false; // Can't overwrite directory
            }

            var newFile = new VirtualFileSystemNode(fileName, false)
            {
                Content = content,
                Size = content.Length,
                Owner = currentUser,
                Group = currentUser
            };
            parent.AddChild(newFile);
            return true;
        }

        public bool DirectoryExists(string path)
        {
            var node = GetNode(path);
            return node?.IsDirectory == true;
        }

        public bool FileExists(string path)
        {
            var node = GetNode(path);
            return node != null && !node.IsDirectory;
        }

        public bool DeleteFile(string path)
        {
            path = NormalizePath(path);
            var parentPath = path.Substring(0, path.LastIndexOf('/'));
            if (parentPath == "") parentPath = "/";

            var parent = GetNode(parentPath);
            if (parent == null || !parent.IsDirectory) return false;

            var fileName = path.Substring(path.LastIndexOf('/') + 1);
            var target = parent.GetChild(fileName);
            
            if (target == null || target.IsDirectory) return false;
            
            return parent.RemoveChild(fileName);
        }

        public bool DeleteDirectory(string path, bool recursive = false)
        {
            path = NormalizePath(path);
            if (path == "/") return false; // Can't delete root

            var node = GetNode(path);
            if (node == null || !node.IsDirectory) return false;

            if (!recursive && node.GetChildren().Count > 0) return false; // Directory not empty

            var parentPath = path.Substring(0, path.LastIndexOf('/'));
            if (parentPath == "") parentPath = "/";

            var parent = GetNode(parentPath);
            if (parent == null) return false;

            var dirName = path.Substring(path.LastIndexOf('/') + 1);
            return parent.RemoveChild(dirName);
        }

        public string ListDirectory(string path, bool longFormat = false, bool showHidden = false)
        {
            var node = GetNode(path);
            if (node == null || !node.IsDirectory) return null;

            var children = node.GetChildren();
            if (children.Count == 0) return "";

            var result = new List<string>();

            foreach (var child in children.OrderBy(c => c.Name))
            {
                if (!showHidden && child.Name.StartsWith(".")) continue;

                if (longFormat)
                {
                    var permissions = child.IsDirectory ? "d" + child.Permissions : "-" + child.Permissions;
                    var size = child.IsDirectory ? "4096" : child.Size.ToString();
                    var modTime = child.ModifiedTime.ToString("MMM dd HH:mm");
                    
                    result.Add($"{permissions} 1 {child.Owner} {child.Group} {size,8} {modTime} {child.Name}");
                }
                else
                {
                    result.Add(child.Name);
                }
            }

            return longFormat ? string.Join("\n", result) : string.Join("    ", result);
        }

        public string ReadFile(string path)
        {
            var node = GetNode(path);
            if (node == null || node.IsDirectory) return null;
            return node.Content;
        }

        public bool CopyFile(string sourcePath, string destPath)
        {
            var sourceNode = GetNode(sourcePath);
            if (sourceNode == null || sourceNode.IsDirectory) return false;

            return CreateFile(destPath, sourceNode.Content);
        }

        public bool MoveFile(string sourcePath, string destPath)
        {
            if (!CopyFile(sourcePath, destPath)) return false;
            return DeleteFile(sourcePath);
        }

        public List<string> FindFiles(string pattern, string searchPath = "/")
        {
            var results = new List<string>();
            FindFilesRecursive(GetNode(searchPath), searchPath, pattern, results);
            return results;
        }

        private void FindFilesRecursive(VirtualFileSystemNode node, string currentPath, string pattern, List<string> results)
        {
            if (node == null) return;

            if (!node.IsDirectory)
            {
                if (node.Name.Contains(pattern) || pattern == "*")
                {
                    results.Add(currentPath);
                }
                return;
            }

            foreach (var child in node.GetChildren())
            {
                var childPath = currentPath == "/" ? "/" + child.Name : currentPath + "/" + child.Name;
                FindFilesRecursive(child, childPath, pattern, results);
            }
        }
    }
}