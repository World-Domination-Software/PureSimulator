# Wildcard and Linux Notation Support

This document describes the wildcard and Linux notation support added to the PureSimulator virtual terminal.

## Supported Wildcards

### Asterisk (`*`) - Multiple Character Wildcard
The asterisk matches zero or more characters in filenames and paths.

**Examples:**
- `ls *.ppkg` - Lists all files ending with `.ppkg`
- `ls file*` - Lists all files starting with "file"
- `ls /dev/sd*` - Lists all devices starting with "sd" (e.g., sda1, sdb1)
- `ls *test*.txt` - Lists all `.txt` files containing "test"

### Question Mark (`?`) - Single Character Wildcard
The question mark matches exactly one character in filenames and paths.

**Examples:**
- `ls file?.txt` - Matches file1.txt, fileA.txt, but not file10.txt
- `ls test?.log` - Matches test1.log, testa.log, etc.
- `ls /dev/sd?1` - Matches sda1, sdb1, sdc1, etc.

## Linux Path Notation

### Current Directory (`.`)
Represents the current working directory.

**Examples:**
- `ls .` - Lists files in the current directory
- `cp /mnt/file.txt .` - Copies file.txt to the current directory

### Parent Directory (`..`)
Represents the parent directory of the current working directory.

**Examples:**
- `cd ..` - Changes to the parent directory
- `ls ..` - Lists files in the parent directory
- `cp ../file.txt .` - Copies a file from the parent directory to current

## Implementation Details

The wildcard support has been implemented in the following components:

### 1. VirtualDirectory.cs
- Added `GetMatchingFiles(string pattern)` method for pattern matching
- Added `WildcardMatch(string text, string pattern)` for `*` and `?` support

### 2. ChassisCommandsExtension.cs
- Enhanced `MatchesWildcard()` to support both `*` and `?`
- Updated `FindAndListFiles()` to handle wildcard patterns in file paths
- Added support for wildcards in `/dev/` directory listings

### 3. VirtualFileSystem.cs
- Updated `FindFilesRecursive()` to use wildcard matching
- Added `WildcardMatch()` method for file searching
- Path normalization already supports `.` and `..` through `NormalizePath()`

### 4. VirtualFileSystemHandler.cs
- Added `HandleWildcardLs()` for ls command with wildcards
- Added `WildcardMatchPublic()` helper method
- Enhanced `HandleLsCommand()` to detect and process wildcards

## Usage in Commands

### ls command
```bash
ls *.ppkg                    # List all .ppkg files
ls /dev/sd*                  # List all sd devices
ls file?.txt                 # List files matching pattern
ls /mnt/*                    # List all files in /mnt
```

### cp command
```bash
cp /mnt/*.ppkg .             # Copy all .ppkg files from /mnt to current dir
cp ../file.txt .             # Copy from parent directory
```

### Tab Completion
Double-tap Tab to autocomplete filenames (existing functionality).
The autocomplete works with partial filenames and paths.

## Wildcard Matching Algorithm

The wildcard matching uses a backtracking algorithm that:
1. Matches `?` to exactly one character
2. Matches `*` to zero or more characters
3. Handles multiple wildcards in the same pattern
4. Is case-sensitive (matching Linux behavior)

## Testing Examples

To test the wildcard support:

1. List devices with wildcards:
   ```bash
   ls /dev/sd*
   ls /dev/sd?1
   ```

2. List files with patterns:
   ```bash
   ls *.ppkg
   ls purity-?.*.*.ppkg
   ```

3. Use parent directory notation:
   ```bash
   cd /home/pureeng
   ls ..
   cd ..
   ```

4. Copy files with wildcards:
   ```bash
   mount /dev/sdb1 /mnt
   cp /mnt/*.ppkg .
   ```
