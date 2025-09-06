using System.IO;
using UnityEngine;

public class LogWriter : MonoBehaviour
{
    public bool EnableLogsInEditor = false;

    private string logCache;
    private string path = Application.dataPath;

    private void Awake()
    {
        logCache = $"=~=~=~=~=~=~=~=~=~=~=~= PuTTY log {System.DateTime.Now} =~=~=~=~=~=~=~=~=~=~=~=\n\nPurity\n";
        if (!Directory.Exists(path)) 
        {
            Directory.CreateDirectory(path);
        }
    }

    public void AddToLog(string text) 
    {
        logCache += text;
    }

    private void OnApplicationQuit()
    {
        if (!EnableLogsInEditor && Application.isEditor) 
            return;

        Debug.Log("Writing log cache...");
        string fileName = $"putty{System.DateTime.Now.Year:0000}-{System.DateTime.Now.Month:00}-{System.DateTime.Now.Day:00}.log";
        File.WriteAllText(path+fileName, logCache);
    }
}
