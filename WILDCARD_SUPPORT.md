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
5. Supports complex patterns like `purity-?.*.*.ppkg`

## Edge Cases and Limitations

### Supported:
- ✓ Multiple wildcards in one pattern: `*test*.log`
- ✓ Mixed wildcards: `file?.tx*`
- ✓ Wildcards at any position: `*`, `test*`, `*test`, `*test*`
- ✓ Empty matches with `*`: `*` matches everything including empty
- ✓ Directory navigation with `.` and `..`
- ✓ Relative and absolute paths

### Notes:
- Wildcard matching is case-sensitive (Linux standard behavior)
- The `?` wildcard requires exactly one character (won't match zero characters)
- Pattern `*.ppkg` matches `file.ppkg` and `.ppkg` (including hidden files starting with `.`)
- Patterns are matched against filenames only, not full paths (except in specific commands)

### Examples of Edge Cases:
```bash
# Pattern: file?.txt
file1.txt    → matches (? = 1)
fileA.txt    → matches (? = A)
file.txt     → no match (? requires exactly 1 character)
file10.txt   → no match (? matches only 1 character, not 10)

# Pattern: *.ppkg
test.ppkg    → matches
.ppkg        → matches (. at start is a character)
ppkg         → no match (no . before ppkg)

# Pattern: *
everything   → matches (including empty if listing directories)
```

## Testing Examples

To test the wildcard support in the PureSimulator terminal:

### 1. List devices with wildcards:
```bash
ls /dev/sd*          # Lists sda1, sdb1, etc.
ls /dev/sd?1         # Lists sda1, sdb1, sdc1 (but not sdab1)
ls /dev/s*           # Lists all devices starting with 's'
```

### 2. List files with patterns:
```bash
ls *.ppkg                # Lists all .ppkg files
ls purity-?.*.*.ppkg     # Lists purity-1.2.3.ppkg, purity-6.5.4.ppkg, etc.
ls file?.txt             # Lists file1.txt, fileA.txt (but not file10.txt)
ls *test*                # Lists all files containing 'test'
```

### 3. Use current and parent directory notation:
```bash
cd /home/pureeng
ls .                     # Lists current directory
ls ..                    # Lists parent directory (/home)
cd ..                    # Changes to parent directory
cp ../file.txt .         # Copies file from parent to current
```

### 4. Copy files with wildcards:
```bash
mount /dev/sdb1 /mnt
cp /mnt/*.ppkg .         # Copies all .ppkg files from /mnt to current directory
cp /mnt/purity-6.*.ppkg .   # Copies specific version patterns
cp /mnt/file?.txt .      # Copies files matching single-char pattern
```

### 5. Complex wildcard patterns:
```bash
ls purity-[0-9].*.*.ppkg    # Using wildcards for version matching
ls *config*.xml             # Files containing 'config' with .xml extension
cp /mnt/*backup* .          # Copy all files with 'backup' in the name
```

### 6. Tab completion:
```bash
# Type partial filename and double-tap Tab to autocomplete
ls puri<Tab><Tab>        # Autocompletes to purity-*.ppkg files
cp /mnt/fil<Tab><Tab>    # Autocompletes to file names starting with 'fil'
```

### 7. Navigating with . and .. :
```bash
cd /opt/Purity/bin
cd ../../etc            # Go up two levels, then into etc
ls ../../../home        # List files three levels up, then in home
pwd                     # Shows current directory after navigation
```

## Real-World Usage Scenarios

### Scenario 1: Installing Purity from USB
```bash
# Mount the USB drive
mount /dev/sdb1 /mnt

# List available Purity versions
ls /mnt/purity-*.ppkg

# Install a specific version pattern
pureinstall /mnt/purity-6.5.*.ppkg

# Copy all installation packages for backup
cp /mnt/*.ppkg .
```

### Scenario 2: Managing Log Files
```bash
# View all log files
ls /var/log/*.log

# View logs from current day
ls /var/log/*$(date +%Y%m%d)*.log

# Clean up old logs
rm /var/log/old_*.log
```

### Scenario 3: Working with Configuration Files
```bash
# List all config files
ls /etc/*.conf

# Backup configuration files
cp /etc/*.conf /root/backup/

# Find specific config file
ls /etc/*network*.conf
```

### Scenario 4: Troubleshooting with Wildcards
```bash
# Check all drive statuses
ls /dev/sd*

# View device information
cat /dev/sd?1

# List all mounted filesystems
df | grep /dev/sd*
```
