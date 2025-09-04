using UnityEngine;
using System.Collections.Generic;
using CrimsofallTechnologies.ServerSimulator;
using static UnityEngine.EventSystems.EventTrigger;

[DefaultExecutionOrder(0)]
public class ServerRack : MonoBehaviour
{
    [System.Serializable]
    public class WiresGroup 
    {
        public GameObject[] powerCables;
        public GameObject[] ethArrayCables;
        //public GameObject[] ethShelfCables;
        public GameObject[] ConnectionCables;
        
        [Space]
        public GameObject wallSocketPlug;

        public void EnableWire(string wireName, int id) 
        {
            if (wireName == "ETH")
            {
                ethArrayCables[id].SetActive(true);
            }

            if (wireName == "LAPTOP") 
            {
                ConnectionCables[id].SetActive(true);
            }

            /*if (wireName == "ETH_SHELF") 
            {
                ethShelfCables[id].SetActive(true);
            }*/

            if (wireName == "PSU") 
            {
                powerCables[id].SetActive(true);
                wallSocketPlug.SetActive(true);
            }
        }

        public void DisableWire(string wireName, int id) 
        {
            if (wireName == "ETH")
            {
                ethArrayCables[id].SetActive(false);
            }

            if (wireName == "LAPTOP")
            {
                ConnectionCables[id].SetActive(false);
            }

            /*if (wireName == "ETH_SHELF")
            {
                ethShelfCables[id].SetActive(false);
            }*/

            if (wireName == "PSU")
            {
                powerCables[id].SetActive(false);

                if (!powerCables[0].activeSelf && !powerCables[1].activeSelf) //when both PSU wires are removed then plug is removed from wall socket too!
                    wallSocketPlug.SetActive(false);
            }
        }

        public void DisableAllWires() 
        {
            for (int i = 0; i < powerCables.Length; i++) powerCables[i].SetActive(false);
            for (int i = 0; i < ethArrayCables.Length; i++) ethArrayCables[i].SetActive(false);
            //for (int i = 0; i < ethShelfCables.Length; i++) ethShelfCables[i].SetActive(false);
            for (int i = 0; i < ConnectionCables.Length; i++) ConnectionCables[i].SetActive(false);

            wallSocketPlug.SetActive(false);
        }

        public void EnableAllWires(bool isShelf)
        {
            for (int i = 0; i < powerCables.Length; i++) powerCables[i].SetActive(true);
            if (!isShelf)
            {
                for (int i = 0; i < ethArrayCables.Length; i++) ethArrayCables[i].SetActive(true);
                for (int i = 0; i < ConnectionCables.Length; i++) ConnectionCables[i].SetActive(true);
            }
            //else
             //   for (int i = 0; i < ethShelfCables.Length; i++) ethShelfCables[i].SetActive(true);

            wallSocketPlug.SetActive(true);
        }
    }
    public WiresGroup[] Wires = new WiresGroup[0];
    public Chassis[] Servers = new Chassis[8];
    public GameObject[] Covers = new GameObject[0];

    public float arrayYDifference = 4.54f;
    public float shelfYDifference = 4.54f;
    public GameObject arrayPrefab, shelfPrefab; //always make sure this is the lowest chassis in the rack!

    public CameraController[] CamerasToFocus; //these camera's will be focuses on the new inserted array!

    public Chassis selectedChassis;

    private static string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ";

    private void Awake()
    {
        for (int i = 0; i < Wires.Length; i++) 
        {
            Wires[i].DisableAllWires();
        }

        for (int i = 0; i < Servers.Length; i++)
        {
            if (Servers[i] != null)
            {
                Servers[i].Init();
                Covers[i].SetActive(false);
            }
        }
    }

    //turns on all cables for given chassis
    public void EnableAllCablesForChassis(int index, bool isShelf) 
    {
        Wires[index].EnableAllWires(isShelf);
    }

    //turns off all cables for given chassis
    public void DisableAllCablesForChassis(int index, bool isShelf)
    {
        Wires[index].DisableAllWires();
    }

    public void EnableWire(string wireName, int id, int chassisId, bool triggerEvent = true) 
    {
        Wires[chassisId].EnableWire(wireName, id);

        if(triggerEvent)
            Servers[chassisId].OnWiresChanged(wireName+id, true);
    }

    public void DisableWire(string wireName, int id, int chassisId, bool triggerEvent = true) 
    {
        Wires[chassisId].DisableWire(wireName, id);

        if(triggerEvent)
            Servers[chassisId].OnWiresChanged(wireName + id, false);
    }

    public Chassis AddArray(int id, string ModelName)
    {
        Vector3 pos = Servers[0].transform.position + new Vector3(0f, arrayYDifference * id, 0f);
        Chassis c = Instantiate(arrayPrefab, pos, Quaternion.identity, transform).GetComponent<Chassis>();
        c.chassisIndex = id;
        c.CopySettings(Servers[0]);
        Servers[id] = c;
        Covers[id].SetActive(false);
        c.name = "ARRAY" + id;
        c.transform.SetAsFirstSibling();
        c.Init(true);

        if(c.flashArrays.Length == 0)
        {
            c.flashArrays = c.GetComponentsInChildren<FlashArray>();
        }

        c.flashArrays[0].ModelName = ModelName;
        c.flashArrays[1].ModelName = ModelName;

        c.SetComputerName($"PCTFJ{Random.Range(0, 9999999).ToString("0000000")}{alphabet[Random.Range(0, alphabet.Length)]}", 0);
        c.SetComputerName($"PCTFJ{Random.Range(0, 9999999).ToString("0000000")}{alphabet[Random.Range(0, alphabet.Length)]}", 1);
        c.SetupInstallSize(ModelName);
        for (int i = 0; i < CamerasToFocus.Length; i++)
        {
            CamerasToFocus[i].SetCameraToFocusOnChassis(c.transform);
        }
        return c;
    }

    public Chassis AddShelf(int id, string ModelName)
    {
        Vector3 pos = Servers[1].transform.position + new Vector3(0f, shelfYDifference * id, 0f);
        Chassis c = Instantiate(shelfPrefab, pos, Quaternion.identity, transform).GetComponent<Chassis>();
        c.chassisIndex = id;
        c.CopySettings(Servers[1]);
        //c.SetComputerName($"PCTFJ{Random.Range(0, 9999999).ToString("0000000")}{alphabet[Random.Range(0, alphabet.Length)]}", 0);
        //c.SetComputerName($"PCTFJ{Random.Range(0, 9999999).ToString("0000000")}{alphabet[Random.Range(0, alphabet.Length)]}", 1);
        Servers[id] = c;
        Covers[id].SetActive(false);
        c.name = "SHELF" + id;
        c.transform.SetAsFirstSibling();

        c.flashArrays[0].ModelName = ModelName;
        c.flashArrays[1].ModelName = ModelName;
        
        c.Init(true);
        for (int i = 0; i < CamerasToFocus.Length; i++)
        {
            CamerasToFocus[i].SetCameraToFocusOnChassis(c.transform);
        }
        return c;
    }

    public string PurearrayList() 
    {
        if (Servers.Length == 0)
            return "no arrays connected!";

        string g = "Name                 ID                                    OS                      Version";
        if (selectedChassis != null)
        {
            g += $"\n{selectedChassis.GetComputerName(0)}               {selectedChassis.flashArrays[0].StartingModelName+"-"+selectedChassis.flashArrays[0].ID}" +
                $"         Purity//FA              {selectedChassis.purityOSVersion}";
            g += $"\n{selectedChassis.GetComputerName(1)}               {selectedChassis.flashArrays[1].StartingModelName+"-"+selectedChassis.flashArrays[1].ID}" +
                $"         Purity//FA              {selectedChassis.purityOSVersion1}";
        }
        return g;
    }
}
