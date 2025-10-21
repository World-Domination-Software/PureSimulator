using UnityEngine;
// using UnityEngine.Rendering.Universal; // Removed - not needed or add URP package if required

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

        if (UsePostProcessing && Cameras != null)
        {
            // Try to enable URP post-processing if UniversalAdditionalCameraData is available.
            // Use reflection so this script does not require the Universal RP package at compile time.
            var uacType = System.Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData");

            for (int i = 0; i < Cameras.Length; i++)
            {
                var cam = Cameras[i];
                if (cam == null) continue;

                if (uacType != null)
                {
                    // Try to get existing component
                    var comp = cam.GetComponent(uacType);
                    if (comp == null)
                    {
                        // If the type exists in the loaded assemblies, attempt to add it at runtime
                        try
                        {
                            comp = cam.gameObject.AddComponent(uacType);
                        }
                        catch
                        {
                            comp = null;
                        }
                    }

                    if (comp != null)
                    {
                        var prop = uacType.GetProperty("renderPostProcessing");
                        if (prop != null && prop.CanWrite)
                        {
                            try { prop.SetValue(comp, true, null); } catch { }
                        }
                    }
                }
                // If Universal RP is not installed, skip silently — no compile-time dependency required.
            }
        }

        if(UseHighestQuality) QualitySettings.SetQualityLevel(1, true); //max quality level!
    }
}
