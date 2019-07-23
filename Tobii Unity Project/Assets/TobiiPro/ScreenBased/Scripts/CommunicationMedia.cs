﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Tobii.Research.Unity;
using UnityEngine.UI;
using DlibFaceLandmarkDetectorExample;
using Assets.TobiiPro.ScreenBased.Scripts;
using Assets.UDP;

/// <summary>
/// WebCamTexture Example
/// An example of detecting face landmarks in WebCamTexture images.
/// </summary>
public class CommunicationMedia : MonoBehaviour
{
    /// <summary>
    /// Set the name of the device to use.
    /// </summary>
    [SerializeField, TooltipAttribute("Set the name of the device to use.")]
    public string requestedDeviceName = null;

    /// <summary>
    /// Set the width of WebCamTexture.
    /// </summary>
    [SerializeField, TooltipAttribute("Set the width of WebCamTexture.")]
    public int requestedWidth = 640;

    /// <summary>
    /// Set the height of WebCamTexture.
    /// </summary>
    [SerializeField, TooltipAttribute("Set the height of WebCamTexture.")]
    public int requestedHeight = 480;

    /// <summary>
    /// Set FPS of WebCamTexture.
    /// </summary>
    [SerializeField, TooltipAttribute("Set FPS of WebCamTexture.")]
    public int requestedFPS = 30;

    /// <summary>
    /// Set whether to use the front facing camera.
    /// </summary>
    [SerializeField, TooltipAttribute("Set whether to use the front facing camera.")]
    public bool requestedIsFrontFacing = false;

    /// <summary>
    /// The adjust pixels direction toggle.
    /// </summary>
    //public Toggle adjustPixelsDirectionToggle;

    /// <summary>
    /// Determines if adjust pixels direction.
    /// </summary>
    [SerializeField, TooltipAttribute("Determines if adjust pixels direction.")]
    public bool adjustPixelsDirection = true;


    [SerializeField, TooltipAttribute("if it is remote setting.")]
    public bool remoteFlag = false;

    /// <summary>
    /// The webcam texture.
    /// </summary>
    WebCamTexture webCamTexture;

    /// <summary>
    /// The webcam device.
    /// </summary>
    WebCamDevice webCamDevice;

    /// <summary>
    /// The colors.
    /// </summary>
    Color32[] colors;

    /// <summary>
    /// The rotated colors.
    /// </summary>
    Color32[] rotatedColors;

    /// <summary>
    /// Determines if rotates 90 degree.
    /// </summary>
    bool rotate90Degree = false;

    /// <summary>
    /// Indicates whether this instance is waiting for initialization to complete.
    /// </summary>
    bool isInitWaiting = false;

    /// <summary>
    /// Indicates whether this instance has been initialized.
    /// </summary>
    bool hasInitDone = false;

    /// <summary>
    /// The screenOrientation.
    /// </summary>
    ScreenOrientation screenOrientation;

    /// <summary>
    /// The width of the screen.
    /// </summary>
    int screenWidth;

    /// <summary>
    /// The height of the screen.
    /// </summary>
    int screenHeight;

    private DataExchangeSystem dataExchangeSystem;

    float Alpha = 0.3f;

    /// <summary>
    /// The texture.
    /// </summary>
    Texture2D texture;

    /// <summary>
    /// The FPS monitor.
    /// </summary>
    FpsMonitor fpsMonitor;


    /// <summary>
    /// The instance of eye tracker.
    /// </summary>
    EyeTracker eyeTracker;

    RawImage rawImage;

    private IGazeData lastGazePoint = new GazeData();

    Agent agent;

    GazePlotter gazePlotter;



#if UNITY_WEBGL && !UNITY_EDITOR
        IEnumerator getFilePath_Coroutine;
#endif

    // Use this for initialization
    void Start()
    {
        fpsMonitor = GetComponent<FpsMonitor>();
        eyeTracker = EyeTracker.Instance;
        rawImage = GameObject.Find("RawImage").GetComponent<RawImage>();
        agent = new Agent(requestedWidth, requestedHeight);
        gazePlotter = GameObject.Find("[GazePlot]").GetComponent<GazePlotter>();

        var conditionSettings = GameObject.Find("ConditionSettings").GetComponent<ConditionSettings>();
        conditionSettings.OnConditionChange += (media, curser) =>
        {
            switch (media)
            {
                case MediaCondition.A:
                    rawImage.texture = null;
                    rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, 0);
                    break;
                case MediaCondition.F:
                    rawImage.texture = webCamTexture;
                    rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, Alpha);
                    break;
                default:
                    rawImage.texture = null;
                    rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, 0);
                    break;
            }

        };


        //adjustPixelsDirectionToggle.isOn = adjustPixelsDirection;

#if UNITY_WEBGL && !UNITY_EDITOR
#else
        Run();
#endif
    }

    private void Run()
    {
        Initialize();
    }

    /// <summary>
    /// Initializes webcam texture.
    /// </summary>
    private void Initialize()
    {
        if (isInitWaiting)
            return;

#if UNITY_ANDROID && !UNITY_EDITOR
            // Set the requestedFPS parameter to avoid the problem of the WebCamTexture image becoming low light on some Android devices. (Pixel, pixel 2)
            // https://forum.unity.com/threads/android-webcamtexture-in-low-light-only-some-models.520656/
            // https://forum.unity.com/threads/released-opencv-for-unity.277080/page-33#post-3445178
            if (requestedIsFrontFacing) {
                int rearCameraFPS = requestedFPS;
                requestedFPS = 15;
                StartCoroutine (_Initialize ());
                requestedFPS = rearCameraFPS;
            } else {
                StartCoroutine (_Initialize ());
            }
#else
        StartCoroutine(_Initialize());
#endif
    }

    /// <summary>
    /// Initializes webcam texture by coroutine.
    /// </summary>
    private IEnumerator _Initialize()
    {
        if (hasInitDone)
            Dispose();

        isInitWaiting = true;

        // Creates the camera
        if (!String.IsNullOrEmpty(requestedDeviceName))
        {
            int requestedDeviceIndex = -1;
            if (Int32.TryParse(requestedDeviceName, out requestedDeviceIndex))
            {
                if (requestedDeviceIndex >= 0 && requestedDeviceIndex < WebCamTexture.devices.Length)
                {
                    webCamDevice = WebCamTexture.devices[requestedDeviceIndex];
                    webCamTexture = new WebCamTexture(webCamDevice.name, requestedWidth, requestedHeight, requestedFPS);
                }
            }
            else
            {
                for (int cameraIndex = 0; cameraIndex < WebCamTexture.devices.Length; cameraIndex++)
                {
                    if (WebCamTexture.devices[cameraIndex].name == requestedDeviceName)
                    {
                        webCamDevice = WebCamTexture.devices[cameraIndex];
                        webCamTexture = new WebCamTexture(webCamDevice.name, requestedWidth, requestedHeight, requestedFPS);
                        break;
                    }
                }
            }
            if (webCamTexture == null)
                Debug.Log("Cannot find camera device " + requestedDeviceName + ".");
        }

        if (webCamTexture == null)
        {
            // Checks how many and which cameras are available on the device
            for (int cameraIndex = 0; cameraIndex < WebCamTexture.devices.Length; cameraIndex++)
            {
                if (WebCamTexture.devices[cameraIndex].isFrontFacing == requestedIsFrontFacing)
                {
                    webCamDevice = WebCamTexture.devices[cameraIndex];
                    webCamTexture = new WebCamTexture(webCamDevice.name, requestedWidth, requestedHeight, requestedFPS);
                    break;
                }
            }
        }

        if (webCamTexture == null)
        {
            if (WebCamTexture.devices.Length > 0)
            {
                webCamDevice = WebCamTexture.devices[0];
                webCamTexture = new WebCamTexture(webCamDevice.name, requestedWidth, requestedHeight, requestedFPS);
            }
            else
            {
                Debug.LogError("Camera device does not exist.");
                isInitWaiting = false;
                yield break;
            }
        }


        // Starts the camera
        webCamTexture.Play();

        while (true)
        {
            //If you want to use webcamTexture.width and webcamTexture.height on iOS, you have to wait until webcamTexture.didUpdateThisFrame == 1, otherwise these two values will be equal to 16. (http://forum.unity3d.com/threads/webcamtexture-and-error-0x0502.123922/)
#if UNITY_IOS && !UNITY_EDITOR && (UNITY_4_6_3 || UNITY_4_6_4 || UNITY_5_0_0 || UNITY_5_0_1)
                if (webCamTexture.width > 16 && webCamTexture.height > 16) {
#else
            if (webCamTexture.didUpdateThisFrame)
            {
#if UNITY_IOS && !UNITY_EDITOR && UNITY_5_2
                    while (webCamTexture.width <= 16) {
                        webCamTexture.GetPixels32 ();
                        yield return new WaitForEndOfFrame ();
                    } 
#endif
#endif

                Debug.Log("name:" + webCamTexture.deviceName + " width:" + webCamTexture.width + " height:" + webCamTexture.height + " fps:" + webCamTexture.requestedFPS);
                Debug.Log("videoRotationAngle:" + webCamTexture.videoRotationAngle + " videoVerticallyMirrored:" + webCamTexture.videoVerticallyMirrored + " isFrongFacing:" + webCamDevice.isFrontFacing);

                screenOrientation = Screen.orientation;
                screenWidth = Screen.width;
                screenHeight = Screen.height;
                isInitWaiting = false;
                hasInitDone = true;

                OnInited();

                break;
            }
            else
            {
                yield return 0;
            }
        }
    }

    /// <summary>
    /// Releases all resource.
    /// </summary>
    private void Dispose()
    {
        rotate90Degree = false;
        isInitWaiting = false;
        hasInitDone = false;

        if (webCamTexture != null)
        {
            webCamTexture.Stop();
            WebCamTexture.Destroy(webCamTexture);
            webCamTexture = null;
        }
        if (texture != null)
        {
            Texture2D.Destroy(texture);
            texture = null;
        }
    }

    /// <summary>
    /// Raises the webcam texture initialized event.
    /// </summary>
    private void OnInited()
    {
        if (colors == null || colors.Length != webCamTexture.width * webCamTexture.height)
        {
            colors = new Color32[webCamTexture.width * webCamTexture.height];
            rotatedColors = new Color32[webCamTexture.width * webCamTexture.height];
        }

        if (adjustPixelsDirection)
        {
#if !UNITY_EDITOR && !(UNITY_STANDALONE || UNITY_WEBGL)
                if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown) {
                    rotate90Degree = true;
                }else{
                    rotate90Degree = false;
                }
#endif
        }
        if (rotate90Degree)
        {
            texture = new Texture2D(webCamTexture.height, webCamTexture.width, TextureFormat.RGBA32, false);
        }
        else
        {
            texture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);
        }

        gameObject.GetComponent<Renderer>().material.mainTexture = texture;

        dataExchangeSystem = GameObject.Find("DataExchangeSystem").GetComponent<DataExchangeSystem>();

        dataExchangeSystem.OnReceive += RenderingReceivedData;


        var myIpAddr = ScanIPAddr.IP[0];
        var myIP = GameObject.Find("MyIP").GetComponent<InputField>();
        myIP.text = myIpAddr + ":5000";
        

        gameObject.transform.localScale = new Vector3(texture.width, texture.height, 1);
        Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

        if (fpsMonitor != null)
        {
            fpsMonitor.Add("width", texture.width.ToString());
            fpsMonitor.Add("height", texture.height.ToString());
            fpsMonitor.Add("orientation", Screen.orientation.ToString());
        }

        float width = texture.width;
        float height = texture.height;

        float widthScale = (float)Screen.width / width;
        float heightScale = (float)Screen.height / height;
        if (widthScale < heightScale)
        {
            Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
        }
        else
        {
            Camera.main.orthographicSize = height / 2;
        }
    }

    public void RenderingReceivedData(IMediaData data)
    {
        if (dataExchangeSystem.LatestGazeData == null) return;
        switch (data.MediaCondition)
        {
            case MediaCondition.A:
                var agentData = data as AgentMediaData;
                var screenPos = ToScreenPos(dataExchangeSystem.LatestGazeData.GazePoint);
                agent.DrawAgent(texture, agentData.FaceLandmark, screenPos);
                break;
            case MediaCondition.F:
                break;
            default:
                break;
        }
    }

    public void Connect()
    {
        var local = GameObject.Find("MyIP").GetComponent<InputField>();
        var remote = GameObject.Find("RemoteIP").GetComponent<InputField>();
        
        dataExchangeSystem.SetUDP(local.text, remote.text);
    }

    // Update is called once per frame
    void Update()
    {
        if (adjustPixelsDirection)
        {
            // Catch the orientation change of the screen.
            if (screenOrientation != Screen.orientation && (screenWidth != Screen.width || screenHeight != Screen.height))
            {
                Initialize();
            }
            else
            {
                screenWidth = Screen.width;
                screenHeight = Screen.height;
            }
        }

        gazePlotter.UpdateGazePlotter(texture.width, texture.height);

        if (ConditionSettings.MediaCondition == MediaCondition.A)
        {
            if (hasInitDone && webCamTexture.isPlaying && webCamTexture.didUpdateThisFrame)
            {
                Color32[] colors = GetColors();

                if (colors != null)
                {
                    var landmarks = agent.GetLandmarkPoints(colors);

                    var agentData = new AgentMediaData(landmarks);
         
                    dataExchangeSystem.Post(agentData);
                }
            }
        }
        else
        {
            texture.SetPixels32(new Color32[texture.width * texture.height]);
            texture.Apply(false);
        }
    }

    private Vector2 ToScreenPos(Vector3 gazePos)
    {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, gazePos)
            - (new Vector2(Screen.width, Screen.height) - new Vector2(texture.width, texture.height)) / 2;

        return screenPos;
    }

    private List<Vector2> GetPixels(Vector2 point, int thickness)
    {
        var list = new List<Vector2>();
        for (int i = 0; i < thickness; i++)
        {
            for (int j = 0; j < thickness; j++)
            {
                list.Add(point + new Vector2(i, j));
            }
        }
        return list;
    }


    private void AdjustVectors(List<Vector2> vectors, int width, int height, Rect rect)
    {
        int adjx = -(int)rect.xMin + width / 2 - (int)rect.width / 2;
        int adjy = -(int)rect.yMin + height / 2 - (int)rect.height / 2;
        var del = new Vector2(adjx, adjy);

        for (var i = 0; i < vectors.Count; i++)
        {
            vectors[i] += del;
        }
    }

    /// <summary>
    /// Gets the current WebCameraTexture frame that converted to the correct direction.
    /// </summary>
    private Color32[] GetColors()
    {
        webCamTexture.GetPixels32(colors);

        if (adjustPixelsDirection)
        {
            //Adjust an array of color pixels according to screen orientation and WebCamDevice parameter.
            if (rotate90Degree)
            {
                Rotate90CW(colors, rotatedColors, webCamTexture.width, webCamTexture.height);
                FlipColors(rotatedColors, webCamTexture.width, webCamTexture.height);
                return rotatedColors;
            }
            else
            {
                FlipColors(colors, webCamTexture.width, webCamTexture.height);
                return colors;
            }
        }
        return colors;
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        Dispose();
    }

    /// <summary>
    /// Raises the change camera button click event.
    /// </summary>
    public void OnChangeCameraButtonClick()
    {
        if (hasInitDone)
        {
            requestedDeviceName = null;
            requestedIsFrontFacing = !requestedIsFrontFacing;
            Initialize();
        }
    }

    /// <summary>
    /// Raises the adjust pixels direction toggle value changed event.
    /// </summary>
    //public void OnAdjustPixelsDirectionToggleValueChanged()
    //{
    //    if (adjustPixelsDirectionToggle.isOn != adjustPixelsDirection)
    //    {
    //        adjustPixelsDirection = adjustPixelsDirectionToggle.isOn;
    //        Initialize();
    //    }
    //}

    /// <summary>
    /// Flips the colors.
    /// </summary>
    /// <param name="colors">Colors.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    void FlipColors(Color32[] colors, int width, int height)
    {
        int flipCode = int.MinValue;

        if (webCamDevice.isFrontFacing)
        {
            if (webCamTexture.videoRotationAngle == 0)
            {
                flipCode = 1;
            }
            else if (webCamTexture.videoRotationAngle == 90)
            {
                flipCode = 1;
            }
            if (webCamTexture.videoRotationAngle == 180)
            {
                flipCode = 0;
            }
            else if (webCamTexture.videoRotationAngle == 270)
            {
                flipCode = 0;
            }
        }
        else
        {
            if (webCamTexture.videoRotationAngle == 180)
            {
                flipCode = -1;
            }
            else if (webCamTexture.videoRotationAngle == 270)
            {
                flipCode = -1;
            }
        }

        if (flipCode > int.MinValue)
        {
            if (rotate90Degree)
            {
                if (flipCode == 0)
                {
                    FlipVertical(colors, colors, height, width);
                }
                else if (flipCode == 1)
                {
                    FlipHorizontal(colors, colors, height, width);
                }
                else if (flipCode < 0)
                {
                    Rotate180(colors, colors, height, width);
                }
            }
            else
            {
                if (flipCode == 0)
                {
                    FlipVertical(colors, colors, width, height);
                }
                else if (flipCode == 1)
                {
                    FlipHorizontal(colors, colors, width, height);
                }
                else if (flipCode < 0)
                {
                    Rotate180(colors, colors, height, width);
                }
            }
        }
    }

    /// <summary>
    /// Flips vertical.
    /// </summary>
    /// <param name="src">Src colors.</param>
    /// <param name="dst">Dst colors.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    void FlipVertical(Color32[] src, Color32[] dst, int width, int height)
    {
        for (var i = 0; i < height / 2; i++)
        {
            var y = i * width;
            var x = (height - i - 1) * width;
            for (var j = 0; j < width; j++)
            {
                int s = y + j;
                int t = x + j;
                Color32 c = src[s];
                dst[s] = src[t];
                dst[t] = c;
            }
        }
    }

    /// <summary>
    /// Flips horizontal.
    /// </summary>
    /// <param name="src">Src colors.</param>
    /// <param name="dst">Dst colors.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    void FlipHorizontal(Color32[] src, Color32[] dst, int width, int height)
    {
        for (int i = 0; i < height; i++)
        {
            int y = i * width;
            int x = y + width - 1;
            for (var j = 0; j < width / 2; j++)
            {
                int s = y + j;
                int t = x - j;
                Color32 c = src[s];
                dst[s] = src[t];
                dst[t] = c;
            }
        }
    }

    /// <summary>
    /// Rotates 180 degrees.
    /// </summary>
    /// <param name="src">Src colors.</param>
    /// <param name="dst">Dst colors.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    void Rotate180(Color32[] src, Color32[] dst, int height, int width)
    {
        int i = src.Length;
        for (int x = 0; x < i / 2; x++)
        {
            Color32 t = src[x];
            dst[x] = src[i - x - 1];
            dst[i - x - 1] = t;
        }
    }

    /// <summary>
    /// Rotates 90 degrees (CLOCKWISE).
    /// </summary>
    /// <param name="src">Src colors.</param>
    /// <param name="dst">Dst colors.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    void Rotate90CW(Color32[] src, Color32[] dst, int height, int width)
    {
        int i = 0;
        for (int x = height - 1; x >= 0; x--)
        {
            for (int y = 0; y < width; y++)
            {
                dst[i] = src[x + y * height];
                i++;
            }
        }
    }

    /// <summary>
    /// Rotates 90 degrees (COUNTERCLOCKWISE).
    /// </summary>
    /// <param name="src">Src colors.</param>
    /// <param name="dst">Dst colors.</param>
    /// <param name="height">Height.</param>
    /// <param name="width">Width.</param>
    void Rotate90CCW(Color32[] src, Color32[] dst, int width, int height)
    {
        int i = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                dst[i] = src[x + y * width];
                i++;
            }
        }
    }
}