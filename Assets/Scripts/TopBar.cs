using UnityEngine;
using UnityEngine.UI;

public class TopBar : MonoBehaviour
{
   public GameObject simTypeUI;
   public GameObject newArrayUI, cameraButton, backButton;
   public Text arrayViewText;
   
   private bool watchingFrontCam = true;
   private UIManager ui => UIManager.Instance;

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
        ui.ShowInfoPanel();
        backButton.SetActive(true);
        CloseAllUI();
    }

    public void WatchArray()
    {
        if(!ui.commandUI.activeSelf) { 
            ui.SeeServer();
            arrayViewText.text = "Watch CLI";
            cameraButton.SetActive(true);
        }
        else { 
            ui.SeeCommands(); 
            arrayViewText.text = "Watch Server";
            cameraButton.SetActive(false);
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
    }

    public void CloseAllUI()
    {
        simTypeUI.SetActive(false);
        newArrayUI.SetActive(false);
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

    public void BackButton() {
        if(ui.infoPanelUI.activeSelf) 
            ui.SeeCommands();
        
        ui.EnableMainCameras();
        backButton.SetActive(false);
    }
}
