using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    public int editorFramerate = 45;

    public Camera[] Cameras;
    public bool UsePostProcessing;
    public bool UseHighestQuality;

    private void Start()
    {
        if (Application.isEditor)
        {
            Application.targetFrameRate = editorFramerate;
        }
        else 
        {
            UseHighestQuality = true;
            UsePostProcessing = true;
            Application.targetFrameRate = 60;
        }

        if (UsePostProcessing) 
        {
            for (int i = 0; i < Cameras.Length; i++)
            {
                Cameras[i].GetComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;
            }
        }

        if(UseHighestQuality) QualitySettings.SetQualityLevel(1, true); //max quality level!
    }
}
