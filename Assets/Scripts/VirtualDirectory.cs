using System.Collections.Generic;
using UnityEngine;

//this is like a virtual directory (e.g. windows directories with files and all)
[System.Serializable]
public class VirtualDirectory
{
    public string DirectoryName = ""; //name of this directory (e.g. MyDesktop, MyDocuments, etc)
    public List<string> _Files = new(); 
    public string[] Files => _Files.ToArray(); 

    public VirtualDirectory(){}

    public void Copy(string[] newFiles)
    {
        for(int i = 0; i < newFiles.Length; i++)
        {
            //replace same named files
            _Files.Add(newFiles[i]);
        }
    }

    public bool Delete(string fileName)
    {
        if(_Files.Contains(fileName)) {
            _Files.Remove(fileName);
        }

        return false;
    }

    public string GetFilesNames()
    {
        string s = "."; //means empty
        
        if(_Files.Count > 0) {
            s =  "    "+ _Files[0];
            for(int i = 1; i < _Files.Count; i++)
            {
                s += "    "+ _Files[i];
            }
        }

        return s;
    }

    public string GetClosestFile(string initials)
    {
        for (int i = 0; i < _Files.Count; i++)
        {
            if (_Files[i].StartsWith(initials)) 
            {
                return _Files[i];
            }
        }

        return "";
    }

    // Get all files matching a wildcard pattern (* and ? supported)
    public List<string> GetMatchingFiles(string pattern)
    {
        var matches = new List<string>();
        
        for (int i = 0; i < _Files.Count; i++)
        {
            if (WildcardMatch(_Files[i], pattern))
            {
                matches.Add(_Files[i]);
            }
        }
        
        return matches;
    }

    // Wildcard matching supporting * (multiple chars) and ? (single char)
    private bool WildcardMatch(string text, string pattern)
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

    public bool FileExsists(string fileName)
    {
        for (int i = 0; i < _Files.Count; i++)
        {
            if(_Files[i] == fileName)
                return true;
        }

        return false;
    }
}
