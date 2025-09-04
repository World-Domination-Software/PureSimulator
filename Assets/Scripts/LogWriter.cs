using System.IO;
using UnityEngine;

public class LogWriter : MonoBehaviour
{
    public bool EnableLogsInEditor = false;

    private string logCache;
    private string path = Application.dataPath+"/Logs/";

    private void Awake()
    {
        logCache = $"=~=~=~=~=~=~=~=~=~=~=~= PuTTY log {System.DateTime.Now.ToString()} =~=~=~=~=~=~=~=~=~=~=~=\n\nPurity\n";
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
        {
            return;
        }

        Debug.Log("Writing log cache...");
        //string fileName = $"log_{System.DateTime.Now.Day}_{System.DateTime.Now.Month}_{System.DateTime.Now.Year}-{System.DateTime.Now.Hour}_{System.DateTime.Now.Minute}.log";
        string fileName = $"putty{System.DateTime.Now.Year.ToString("0000")}-{System.DateTime.Now.Month.ToString("00")}-{System.DateTime.Now.Day.ToString("00")}.log";
        File.WriteAllText(path+fileName, logCache);
    }
}
