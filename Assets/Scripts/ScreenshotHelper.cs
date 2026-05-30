using System.IO;
using UnityEngine;

public class ScreenshotHelper : MonoBehaviour
{
    private string filePath;
    private Camera cam;

    private void Start() 
    {
        cam = GetComponent<Camera>();
        filePath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "_screenshots");
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
    }

    private void Update() 
    {
        if (Input.GetKeyDown(KeyCode.F12)) 
        {
            //get possible file name
            string fileName = $"screenshot{Directory.GetFiles(filePath).Length + 1}.png";
            SaveScreenshot(Path.Combine(filePath, fileName));
        }
    }

    /*public byte[] TakeScreenshot(int width, int height, bool disableUI) 
    {
        if (disableUI && canvas) canvas.enabled = false;

        RenderTexture rt = RenderTexture.GetTemporary(width, height, 24);
        cam.targetTexture = rt;
        cam.Render();
        cam.targetTexture = null;

        if (disableUI && canvas) canvas.enabled = true;

        RenderTexture.active = rt;
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        byte[] bytes = screenshot.EncodeToPNG();
        Destroy(screenshot);
        return bytes;
    }*/

    public void SaveScreenshot(string filePath) 
    {
        ScreenCapture.CaptureScreenshot(filePath);
    }
}
