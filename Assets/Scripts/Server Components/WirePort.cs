using UnityEngine;
using UnityEngine.EventSystems;

public class WirePort : MonoBehaviour
{
    public bool Interactable = true;
    [Range(0, 3)] public int id = 0;
    public string portName;
    public WirePort otherWireToDisable; //the wire to disable when this wire is plugged in.

    private Chassis chassis;
    private bool connected = false;

    public void Init(Chassis ch, bool plugged) 
    {
        chassis = ch;
        connected = plugged;
    }

    private void OnMouseDown()
    {
        if (!Interactable)
            return;

        if (EventSystem.current.IsPointerOverGameObject()) 
        {
            return;
        }

        //do not let user pull cables from a running server!
        if (chassis.OSFullyRunning && portName != "LAPTOP")
        {
            return;
        }

        //only one connection is possible at a time!
        if (portName == "LAPTOP") {
            otherWireToDisable.Disable();
        }

        //plug in this cord!
        if (!connected)
        {
            Enable();
        }
        else if(portName != "LAPTOP") //cannot disconnect all connection cables at once
        {
            Disable();
        }
    }

    public void Enable(bool triggerEvent = true) 
    {
        int i = id;
        if (portName == "PSU" || portName == "ETH_SHELF")
            i = Mathf.Abs(i - 1);

        connected = true;
        chassis.rack.EnableWire(portName, i, chassis.chassisIndex, triggerEvent);
    }

    public void Disable(bool triggerEvent = true) 
    {
        int i = id;
        if (portName == "PSU" || portName == "ETH_SHELF")
            i = Mathf.Abs(i - 1);

        connected = false;
        chassis.rack.DisableWire(portName, i, chassis.chassisIndex, triggerEvent);
    }

    public bool NameEquals(string wireName) 
    {
        if (wireName == portName + id) 
        {
            return true;
        }

        return false;
    }
}
