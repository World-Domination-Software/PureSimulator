using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region Singleton

    public static UIManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    #endregion

    public GameObject startupUI;
    public float startupUIDisableDelay = 4f;

    [Space]
    public GameObject commandUI;
    public GameObject serverInfoObject;
    public GameObject serverUI, infoPanelUI, popupUI;
    
    [Space]
    public GameObject aiUI;
    public GameObject askQuestionPopup;

    public Animator popupAnimator, serverAnimator, infoPanelAnimator, commandUIAnimator, aiAnimator;
    public InputField field;
    public Text versionText, popupTitleText, popupMessageText;

    [Space, Header("Side Panels")]
    public GameObject[] Sides;
    public Animator[] SideAnimators;

    [Space]
    public Text helperText;
    public GameObject helpTexts; //server helper texts

    [Space]
    public GameObject frontCam;
    public GameObject frontCamButton;
    public GameObject backCam;
    public GameObject backCamButton;
	public InputField aiInputField;

    [Space]
    public Text helpInfoText;
    public float helpTextTime = 0.75f;
    public bool openCommandsOnStart = true;
    public InfoObjectManager infoObjectManager;
    public TutorSetup tutorSetup;

    public GameObject errorUI;
    public Text errorText;
    private bool lastWasFrontCam;

    private void Start()
    {
        //always show welcome screen in builds
        if (!Application.isEditor) {
            startupUI.SetActive(true);
        }

        versionText.text = "COM7 - PuTTY | PureOPS v." + Application.version;
        frontCam.SetActive(true);
        backCam.SetActive(false);

        if (openCommandsOnStart)
        {
            SeeCommands();
        }
        else
        {
            SeeServer();
        }

        Invoke(nameof(CloseStartupUI), startupUIDisableDelay);
    }

    private void CloseStartupUI() 
    {
        startupUI.SetActive(false);

        //when player starts simulator show them that small 'Ask Question Popup'
        if(!aiUI.activeSelf) {
            askQuestionPopup.SetActive(true);
            Invoke(nameof(HideAIQuestionPopup), 3f); //close after a bit of delay!
        }
    }
    
    private void HideAIQuestionPopup() {
        askQuestionPopup.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && serverUI.activeSelf) 
        {
            SeeCommands();
        }
    }

    public void ShowErrorUI(string error)
    {
        CancelInvoke(nameof(HideErrorUI));
        errorText.text = error;
        errorUI.SetActive(true);
        Invoke(nameof(HideErrorUI), 6f);
    }

    public void SetHelperText(string text) 
    {
        helperText.text = text;
    }

    private void HideErrorUI()
    {
        errorUI.SetActive(false);
    }

    public void ShowInfoPanel() 
    {
        helpInfoText.text = "Select an object name tag to get it's description.";

        infoPanelUI.SetActive(true);
        infoPanelAnimator.SetBool("Open", true);
        serverAnimator.SetBool("Open", false);
        commandUIAnimator.SetBool("Open", false);

        StartCoroutine(DelayedCloseUI(serverUI));
        StartCoroutine(DelayedCloseUI(commandUI));
        serverInfoObject.SetActive(true);

        if (frontCam.activeSelf) lastWasFrontCam = true;

        frontCam.SetActive(false);
        backCam.SetActive(false);

        CancelInvoke(nameof(HelpInfoTexts));
        Invoke(nameof(HelpInfoTexts), helpTextTime);
    }

    private void HelpInfoTexts() 
    {
        helpTexts.SetActive(true);
    }

    public void SeeServer() 
    {
        serverUI.SetActive(true);
        serverAnimator.SetBool("Open", true);

        commandUIAnimator.SetBool("Open", false);
        infoPanelAnimator.SetBool("Open", false);
        StartCoroutine(DelayedCloseUI(infoPanelUI));
        StartCoroutine(DelayedCloseUI(commandUI));
        
        serverInfoObject.SetActive(false);
        GlobalVar.commandProOpen = false;
    }

    public void SelectSimulatorType(int index) 
    {
        if(index == 4)
        {
            LoadingUI.Instance.LoadScene("Exam");
            return;
        }

        tutorSetup.StartTutor(index);
        CloseSidePanel(1);
        OpenSidePanel(0);
    }

    public void SeeCommands()
    {
        commandUI.SetActive(true);
        commandUIAnimator.SetBool("Open", true);
        field.ActivateInputField();
        serverInfoObject.SetActive(false);

        serverAnimator.SetBool("Open", false);
        infoPanelAnimator.SetBool("Open", false);
        StartCoroutine(DelayedCloseUI(serverUI));
        StartCoroutine(DelayedCloseUI(infoPanelUI));

        helpTexts.SetActive(false);
        OpenSidePanel(0);

        GlobalVar.commandProOpen = true;
    }

    public void EnableMainCameras() 
    {
        if (lastWasFrontCam)
        {
            frontCam.SetActive(true);
            backCam.SetActive(false);
        }
        else 
        {
            frontCam.SetActive(false);
            backCam.SetActive(true);
        }
    }

    private IEnumerator DelayedCloseUI(GameObject UI) 
    {
        yield return new WaitForSeconds(0.25f);
        UI.SetActive(false);
    }

    public void Exit()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#endif
        Application.Quit();
    }

    public void BackCamera() 
    {
        backCam.SetActive(true);
        frontCam.SetActive(false);

        //backCamButton.SetActive(false);
        //frontCamButton.SetActive(true);
    }

    public void FrontCamera() 
    {
        backCam.SetActive(false);
        frontCam.SetActive(true);

        //backCamButton.SetActive(true);
        //frontCamButton.SetActive(false);
    }

    public void ShowPopup(string title, string message) 
    {
        popupTitleText.text = title;
        popupMessageText.text = message;

        popupUI.SetActive(true);
        popupAnimator.SetBool("Open", true);
    }

    public void HidePopup() 
    {
        popupAnimator.SetBool("Open", false);
        StartCoroutine(DelayedCloseUI(popupUI));
    }

    public void OpenSidePanel(int index) 
    {
        for (int i = 0; i < Sides.Length; i++)
        {
            if (i == index)
            {
                Sides[i].SetActive(true);
                SideAnimators[i].SetBool("Open", true);
            }
            else
            {
                Sides[i].SetActive(false);
                SideAnimators[i].SetBool("Open", false);
            }
        }
    }

    public void CloseSidePanel(int index) 
    {
        SideAnimators[index].SetBool("Open", false);
        StartCoroutine(DelayedCloseUI(Sides[index]));
    }

    #region AI_BUTTONS

    public void OpenAIUI() {
        //check internet connection of user before opening UI
        if(!Application.isEditor && Application.internetReachability == NetworkReachability.NotReachable){
            ShowErrorUI("Unstable internet connection, Make sure your internet connection is stable.");
            return;
        }

        askQuestionPopup.SetActive(false);
        aiUI.SetActive(true);
        aiAnimator.SetBool("Open", true);
		aiInputField.ActivateInputField();
    }

    public void CloseAIUI() {
        aiAnimator.SetBool("Open", false);
        aiInputField.DeactivateInputField();
        StartCoroutine(DelayedCloseUI(aiUI));
    }

    #endregion

    #region HelpObjectsButtonMethods

    public void Harddrive() 
    {
        helpInfoText.text = "DFM (Direct Flash Module):\n\n"+infoObjectManager.dfmInfo;
    }

    public void NvRam() 
    {
        helpInfoText.text = "NvRam (Non-Volatile RAM):\n\n" + infoObjectManager.nvRamInfo;
    }

    public void PowerSupplyUnit() 
    {
        helpInfoText.text = "Power Supply Unit (PSU):\n\n" + infoObjectManager.psuInfo;
    }

    public void Chassis() 
    {
        helpInfoText.text = "Chassis:\n\n" + infoObjectManager.chassisInfo;
    }

    #endregion
}
