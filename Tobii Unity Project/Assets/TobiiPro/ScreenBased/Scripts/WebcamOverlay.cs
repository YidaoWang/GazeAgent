using DlibFaceLandmarkDetectorExample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebcamOverlay : MonoBehaviour
{
    public float Alpha = 0.3f;

    void Start()
    {
        RawImage rawimage = this.GetComponents<RawImage>()[0];

        WebCamDevice[] devices = WebCamTexture.devices;
        // display all cameras
        for (var i = 0; i < devices.Length; i++)
        {
            Debug.Log(i.ToString() + ": " + devices[i].name);
        }
        WebCamTexture webcamTexture = null;

        rawimage.texture = webcamTexture;
        //rawimage.material.mainTexture = webcamTexture;
        rawimage.color = new Color(rawimage.color.r, rawimage.color.g, rawimage.color.b, Alpha);

        //webcamTexture.Play();

        var conditionSettings = GameObject.Find("ConditionSettings").GetComponent<ConditionSettings>();
        conditionSettings.OnConditionChange += (media, cursor) => {
            var rawImage = GameObject.Find("RawImage").GetComponent<RawImage>();
            if (media == MediaCondition.F)
            {
                rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, Alpha);
            }
            else
            {
                rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, 0);
            }
        };
    }
}
