using System;
using System.IO;
using System.Text;
using UnityEngine;

public class FlashArray : MonoBehaviour
{
    public VirtualDirectory Dir;

    public Material greenMat, amberMat, offMat;
    public MeshRenderer[] Lights;
    public int index = 0;

    [Space]
    public string ID;
    public string StartingModelName = "FA";
    public string ModelName = "FA-X7OR2";

    //ready, not-ready or offline
    public string Status = "ready";

    //primary or secondary only, before install it should be neither
    public string State = "neither";

    [Space]
    public string arrayName;
    public string physicalIp, virtualIp, Netmask, Gateway, DNSServer, DNSDomain, NTPServers, SMTPRelayHost, SMTPSenderDomain, AlertSenderRecipients;

    public string TimeZone, tempTimeZone = "";
    public DateTime currentDateTime;

    private const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
    private Light[] realLights=new Light[0];

    private void Start()
    {
        StringBuilder sb = new StringBuilder();
        System.Random random = new System.Random();
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                sb.Append(chars[random.Next(chars.Length)]);
            }

            if (i < 4)
                sb.Append("-");
        }
        ID = sb.ToString();
        Dir = new();
    }

    public void SetupDefaultConfigs() 
    {
        TimeZone = "Etc/UTC"; //default timezone
        currentDateTime = DateTime.UtcNow;

        physicalIp = index == 0 ? "100.150.145.32" : "100.150.145.33";
        virtualIp = "100.150.144.56";
        Netmask = "255.255.255.0";
        Gateway = "255.255.255.25";
        DNSServer = "10.0.0.9,10.4.5.6";
        DNSDomain = "hello.com";
        NTPServers = "ntp0.hello.com";
        SMTPRelayHost = "mail.hello.com";
        SMTPSenderDomain = "hello.com";
        AlertSenderRecipients = "admin@hello.com,mail@hello.com";
    }

    public void SetLights(bool ok, bool isOn) 
    {
        if(realLights.Length == 0) GetRealLights();

        if (!isOn)
        {
            //all lights off
            Lights[0].material = offMat; //OK light
            Lights[1].material = offMat; //ID light
            Lights[2].material = offMat; //PR1 light

            realLights[0].enabled = false;
            realLights[1].enabled = false;
            realLights[2].enabled = false;
        }
        else 
        {
            //enable some lights - but mind colors
            realLights[0].enabled = true;
            realLights[1].enabled = false;
            realLights[2].enabled = false;

            //OK light
            if (ok) {
                Lights[0].material = greenMat;
                realLights[0].color = greenMat.color;
            }
            else {
                Lights[0].material = amberMat;
                realLights[0].color = amberMat.color;
            }

            Lights[1].material = offMat; //ID light

            //PR1 light
            if(State == "primary") {
                realLights[2].enabled = true;
                Lights[2].material = greenMat;
                realLights[2].color = greenMat.color;
            }
            else {
                Lights[2].material = offMat;
                realLights[2].enabled = false;
            }
        }
    }

    private void GetRealLights()
    {
        realLights = new Light[Lights.Length];
        realLights[0] = Lights[0].GetComponentInChildren<Light>();
        realLights[1] = Lights[1].GetComponentInChildren<Light>();
        realLights[2] = Lights[2].GetComponentInChildren<Light>();
    }
}
