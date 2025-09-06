using CrimsofallTechnologies.ServerSimulator;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TimeZoneManager : MonoBehaviour
{
    public CommandProcessor commandProcessor;
    public List<string> GeographicRegions = new();

    //Used as: *Geographic Region, Locations*
    //public Dictionary<string, List<string>> TimeZoneNames = new Dictionary<string, List<string>>();

    [Serializable]
    public class CFTimeZoneInfo 
    {
        public string geoArea;
        public TextAsset textAsset;

        public string[] AreaNames;

        public void Init() 
        {
            AreaNames = textAsset.text.Split(',');
        }
    }

    public List<CFTimeZoneInfo> TimeZoneNames = new List<CFTimeZoneInfo>();

    private string selectedGeoArea;
    //private DateTime dateTime;

    private void Start()
    {
        for (int i = 0; i < TimeZoneNames.Count; i++)
        {
            TimeZoneNames[i].Init();
        }
    }

    /*private void Start()
    {
        //cache everything
        //dateTime = DateTime.UtcNow;

        string[] lines = zonesText.text.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string[] line = lines[i].Split(',');
            string key = line[1].Split('/')[0];
            
            if (!TimeZoneNames.ContainsKey(key))  
                TimeZoneNames.Add(key, new());

            TimeZoneNames[key].Add(line[1].Split('/')[1]);
            LinuxWindowsTimes.Add(line[1], line[0]);
        }
    }*/

    /*public void SetTimeZone(string location)
    {
        fullZone = LinuxWindowsTimes[$"{selectedGeoArea}/{location}"];
        Debug.Log($"Setting time for: {selectedGeoArea}/{location} -> {fullZone}");
        TimeZoneInfo timeInfo = TimeZoneInfo.FindSystemTimeZoneById(fullZone);

        //apply time zone to the new controller!
        if (commandProcessor.chassis.selectedController == "CT0")
        {
            commandProcessor.chassis.flashArrays[0].TimeZone = fullZone;
            Debug.Log("Time-zones are not yet implemented, will use UTC.");

        }
        if (commandProcessor.chassis.selectedController == "CT1")
        {
            commandProcessor.chassis.flashArrays[1].TimeZone = fullZone;
            Debug.Log("Time-zones are not yet implemented, will use UTC.");
        }
    }*/

    public bool HasTimezonesUpto(int index) 
    {
        if(index <= 0)
            return false;

        for (int i = 0; i < TimeZoneNames.Count; i++)
        {
            if (selectedGeoArea == TimeZoneNames[i].geoArea)
            {
                if(index > TimeZoneNames[i].AreaNames.Length)
                    return false;
            }
        }

        return true;
    }

    public string GetTimeNow() 
    {
        return commandProcessor.chassis.GetCurrentController().currentDateTime.ToString();
    }

    public void ShowTimeZones(string geoArea) 
    {
        /*if (!TimeZoneNames.ContainsKey(geoArea)) 
        {
            Debug.Log("No geographic area known named: " + geoArea);
            return;
        }*/

        selectedGeoArea = geoArea;

        //2 lists are drawn and hence a half of real values is used!
        //if (selectedGeoArea != "US")
        //{
        for (int i = 0; i < TimeZoneNames.Count - 1; i++)
        {
            if (TimeZoneNames[i].geoArea == selectedGeoArea)
            {
                commandProcessor.LogDualColumns(TimeZoneNames[i].AreaNames);
                break;
                /*for (int n = 0; n < TimeZoneNames[i].AreaNames.Length; n++)
                {
                    if (n + 1 < TimeZoneNames[i].AreaNames.Length)
                        commandProcessor.Log($"  {n + 1}. {TimeZoneNames[i].AreaNames[n]}  {n + 2}. {TimeZoneNames[i].AreaNames[n + 1]}");
                    else
                        commandProcessor.Log($"  {n + 1}. {TimeZoneNames[i].AreaNames[n]}");
                    n++; //always skip one since 2 are shown every-time!
                }
                break;*/
            }
        }
        //}

        /*if (selectedGeoArea == "US") 
        {
            commandProcessor.Log("  1. Alaska      4. Central       7. Starke Country (Indiana)  10. Pacific Ocean" +
                               "\n  2. Aleutian    5. Eastern       8. Michigan                  11. Samoa" +
                               "\n  3. America     6. Hawaii        9. Mountain");
        }

        if(selectedGeoArea == "None of the above")
        {
            //choose for GMT +
        }*/
    }

    public void ShowGeographicAreas() 
    {
        //commandProcessor.Log("  1. Africa      4. Australia     7. Atlantic Ocean  10. Pacific Ocean\n  2. America     5. Arctic Ocean  8. Europe          11. US\n" +
        //    "  3. Antarctica  6. Asia          9. Indian Ocean    12. None of the above");

        commandProcessor.Log("  1. Africa      4. Australia     7. Atlantic Ocean  10. Pacific Ocean" +
                           "\n  2. America     5. Arctic Ocean  8. Europe          11. US" +
                           "\n  3. Antarctica  6. Asia          9. Indian Ocean    12. None of the above");
    }

    public string GetAreaNameIndexed(int index) 
    {
        for (int i = 0; i < TimeZoneNames.Count; i++)
        {
            if (selectedGeoArea == TimeZoneNames[i].geoArea) 
            {
                return TimeZoneNames[i].AreaNames[index];
            }
        }

        return "err.";
    }
}
