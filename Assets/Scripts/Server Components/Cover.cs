using UnityEngine;

public class Cover : MonoBehaviour
{
    public GameObject coverGfx;
    public Material offMat, onMat;
    public Light light_;

    private MeshRenderer mRenderer;

    private void Awake()
    {
        mRenderer = coverGfx.GetComponent<MeshRenderer>();
        SetLights(false);
    }

    private void OnMouseDown()
    {
        coverGfx.SetActive(!coverGfx.activeSelf);
        GetComponent<BoxCollider>().enabled = false;
    }

    public void SetLights(bool value) 
    {
        Material[] mats = mRenderer.materials;
        light_.enabled = value;
        
        if(value) 
            mats[2] = onMat;
        else
            mats[2] = offMat;
        
        light_.color = mats[2].color;
        mRenderer.materials = mats;
    }
}
