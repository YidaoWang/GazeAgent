using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebcamOverlay : MonoBehaviour
{
    public int Width = 1920;
    public int Height = 1080;
    public int FPS = 30;
    public int CamDeviceNo = 0;
    public float Alpha = 0.1f;

    void Start()
    {
        RawImage rawimage = this.GetComponents<RawImage>()[0];

        WebCamDevice[] devices = WebCamTexture.devices;
        // display all cameras
        for (var i = 0; i < devices.Length; i++)
        {
            Debug.Log(i.ToString() + ": " + devices[i].name);
        }
        WebCamTexture webcamTexture = new WebCamTexture(devices[CamDeviceNo].name, Width, Height, FPS);

        rawimage.texture = webcamTexture;
        rawimage.material.mainTexture = webcamTexture;
        rawimage.color = new Color(rawimage.color.r, rawimage.color.g, rawimage.color.b, Alpha);

        webcamTexture.Play();
    }
}
