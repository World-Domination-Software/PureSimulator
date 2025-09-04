using System;
using UnityEngine;

public class NvRAM : MonoBehaviour
{
    public double Size = 7000; //In MB
    private MeshRenderer mRenderer;

    public void Init() 
    {
        mRenderer = GetComponent<MeshRenderer>();
    }

    public void SetLightColor(Material mat)
    {
        Material[] mats = mRenderer.materials;
        mats[1] = mat;
        mRenderer.materials = mats;
    }
}
