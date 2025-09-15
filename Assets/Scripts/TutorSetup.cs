using CrimsofallTechnologies.ServerSimulator;
using UnityEngine;

public class TutorSetup : MonoBehaviour
{
    public ServerRack rack;
    public CommandProcessor commandProcessor;
    public UIManager ui;
    public GameObject checklistSide;

    [System.Serializable]
    public class TutorHelpers
    {
        public TextAsset textFile;
        public string pdfFileName; //file name in Application.dataPath directory
    }
    public TutorHelpers[] TutorHelps = new TutorHelpers[0];

    private Chassis selectedChassis;
    private bool tutorRunning=false;
    private int tutorID = -1;
    private string FlashArrayInstallModelName;

    private void Start()
    {
        ui.SetHelperText("No Tutor Active.");
    }

    public void StartTutor(int id)
    {
        if (tutorRunning)
        {
            StopLastTutor(true);
        }

        bool done = false;
        if (id == 0)
        {
            ui.frontCam.GetComponent<CameraController>().ResetPose();
            ui.backCam.GetComponent<CameraController>().ResetPose();

            selectedChassis = rack.AddArray(4, FlashArrayInstallModelName);
            ui.SeeServer();
            ui.ShowErrorUI("Connect PSU and Ethernet cables to turn on the server, then connect USB to install purityOS.");
            ui.SetHelperText(TutorHelps[id].textFile.text);
            done = true;

            checklistSide.SetActive(true);
        }

        if (!done) //feature not implemented? 
        {
            ui.ShowPopup("Unsupported Feature", "This feature will be implemented in a future release.");
        }

        if (done) {
            tutorID = id;
            tutorRunning = true;
        }
    }

    public void SetFlashArrayModelName(string cSize) 
    {
        FlashArrayInstallModelName = cSize;
    }

    public void StopLastTutor(bool changeUI = false) 
    {
        ui.SetHelperText("No Tutor Active.");
        if(tutorID == 0)
            Destroy(selectedChassis.gameObject);
        
        if(changeUI)
            ui.SeeCommands();
        tutorID = -1;
        checklistSide.SetActive(false);
    }

    public void TutorComplete() 
    {
        Debug.Log("Tutorial completed!");
        tutorRunning = false;
        ui.SetHelperText("No Tutor Active.");
        tutorID = -1;
        checklistSide.SetActive(false);
    }

    public void OpenTutorPDFLink()
    {
        Application.OpenURL(Application.dataPath + "/" + TutorHelps[tutorID].pdfFileName);
    }
}
