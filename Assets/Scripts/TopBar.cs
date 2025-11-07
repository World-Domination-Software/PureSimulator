using UnityEngine;
using UnityEngine.UI;

public class TopBar : MonoBehaviour
{
   public GameObject simTypeUI;
   public GameObject newArrayUI, cameraButton, backButton;
   public Text arrayViewText;
   
   private bool watchingFrontCam = true;
   private UIManager ui => UIManager.Instance;

   private bool wasCommandsUIOpen = true;

    private void Start()
    {
        arrayViewText.text = "Watch Rack";   
    }

    public void SimType()
    {
        simTypeUI.SetActive(!simTypeUI.activeSelf);
        if(!simTypeUI.activeSelf) newArrayUI.SetActive(false);
    }

    public void Exam()
    {
        LoadingUI.Instance.LoadScene("Exam");
        CloseAllUI();
    }

    public void MoreInfo()
    {
        CloseAllUI(false);
        ui.ShowInfoPanel();
        backButton.SetActive(true);
    }

    public void WatchArray()
    {
        if(ui.infoPanelUI.activeSelf)
        {
            ui.SeeServer();
            arrayViewText.text = "Watch CLI";
            cameraButton.SetActive(true);

            wasCommandsUIOpen = false;
            backButton.SetActive(false);
            CloseAllUI();
            return;
        }

        if(ui.commandUI.activeSelf) { 
            ui.SeeServer();
            arrayViewText.text = "Watch CLI";
            cameraButton.SetActive(true);

            wasCommandsUIOpen = false;
        }
        else 
        { 
            ui.SeeCommands(); 
            arrayViewText.text = "Watch Rack";
            cameraButton.SetActive(false);

            wasCommandsUIOpen = true;
        }
        CloseAllUI();
    }

    public void NewArray()
    {
        newArrayUI.SetActive(!newArrayUI.activeSelf);
    }

    public void SelectSim(int i)
    {
        ui.SelectSimulatorType(i);
        CloseAllUI();

        ui.SeeServer();
        ui.EnableMainCameras();
        backButton.SetActive(false);
        watchingFrontCam = false;
        ui.BackCamera();

        //update the top bar too!
        arrayViewText.text = "Watch CLI";
        cameraButton.SetActive(true);
    }

    public void CloseAllUI(bool hideInfoPanel = true)
    {
        simTypeUI.SetActive(false);
        newArrayUI.SetActive(false);

        if(hideInfoPanel)
        {
            //close more info UI too!
            ui.EnableMainCameras();
            ui.HideInfoPanel();
        }
    }

    public void BackView()
    {
        watchingFrontCam = !watchingFrontCam;
        if(!watchingFrontCam) {
            ui.BackCamera();
        }
        else {
            ui.FrontCamera();
        }
    }

    public void BackButton() 
    {
        if(wasCommandsUIOpen) {
            ui.SeeCommands();
        }
        else
        {
            ui.SeeServer();
        }
        
        ui.EnableMainCameras();
        backButton.SetActive(false);
    }
}
