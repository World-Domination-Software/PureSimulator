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
                s += "    "+ _Files[1];
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
