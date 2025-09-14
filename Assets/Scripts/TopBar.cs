using UnityEngine;

public class TopBar : MonoBehaviour
{
   public GameObject simTypeUI;
   public GameObject newArrayUI;

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
        CloseAllUI();
    }

    public void WatchArray()
    {
        ui.SeeServer();
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
}
