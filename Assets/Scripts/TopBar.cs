using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TopBar : MonoBehaviour
{
   public GameObject simTypeUI;
   public GameObject newArrayUI, cameraButton, backButton, certificationPanel;
   public Text arrayViewText;
   
   private bool watchingFrontCam = true;
   private UIManager ui => UIManager.Instance;

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
        CloseAllUI();
        certificationPanel.SetActive(true);
    }

    public void MoreInfo()
    {
        ui.ShowInfoPanel();
        backButton.SetActive(true);
        CloseAllUI();
    }

    public void WatchArray()
    {
        if(ui.commandUI.activeSelf) { 
            ui.SeeServer();
            arrayViewText.text = "Watch CLI";
            cameraButton.SetActive(true);
        }
        else 
        { 
            ui.SeeCommands(); 
            arrayViewText.text = "Watch Rack";
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

        ui.SeeServer();
        ui.EnableMainCameras();
        backButton.SetActive(false);
        watchingFrontCam = false;
        ui.BackCamera();

        //update the top bar too!
        arrayViewText.text = "Watch CLI";
        cameraButton.SetActive(true);
    }

    public void CloseAllUI()
    {
        simTypeUI.SetActive(false);
        newArrayUI.SetActive(false);
        certificationPanel.SetActive(false);
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

    public void ChooseCertification(string Type)
    {
        CloseAllUI();
        if(Type == "FA_IE") SceneManager.LoadScene("Exam_FA");
        if(Type == "FB_IE") 
            //SceneManager.LoadScene("Exam_FB");
            ui.ShowPopup("Unsupported Feature", "This feature will be implemented in a future release.");
    }
}
