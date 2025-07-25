/* 
*   NatDevice
*   Copyright (c) 2021 Yusuf Olokoba.
*/


using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VideoKit;
using UnityEngine.PlayerLoop;
using Cysharp.Threading.Tasks;
using VideoKit.Devices;
using UnityEngine.Events;
using System.Threading.Tasks;

public class CameraHybr : MonoBehaviour {
    public static CameraHybr instance { get; private set; }

    #region --Inspector--
    public bool startScannerOnStartup;
    public bool frontcamera;
    //public AspectRatioFitter aspectFitter;
    #endregion
    

    #region --Setup--

    VideoKitCameraManager cameraManager = null;
    //MediaDeviceQuery query;
    //CameraDevice device;
    CameraOpencvLib openCvLib;
    bool cameraInitialized = false;

    void Awake() {
        instance = this;
        openCvLib = GetComponent<CameraOpencvLib>();
        cameraManager = GetComponent<VideoKitCameraManager>();
        if (cameraManager == null) Debug.LogError("VideoKitCameraManager not found on GameObject of CameraHybr. Please add one");
    }
    async void Start() {
        LoadingManager.AddExpectedLoadValue(1.0f, "InitCamera");
        await UniTask.Yield();
        InitCamera();
        if (startScannerOnStartup) CameraOpencvLib.StartScanning();
    }

    public async void InitCamera(){
        LoadingManager.AddLoadedValue(0.0f, "InitCamera");
        if (openCvLib == null) Debug.LogError("CameraOpencvLib not found on GameObject of CameraHybr. Please add one");
        // Request camera permissions
        var status = await CameraDevice.CheckPermissions();
        TryFinalizeInit();
    }
    int retries = 0;
    public async void TryFinalizeInit(){
        retries++;
        if (retries < 50)
        {
            var status = await CameraDevice.CheckPermissions();
            if (status != MediaDevice.PermissionStatus.Authorized) {
                Debug.LogError("User did not grant camera permissions, lets retry");
            } else {
                await SetupCamera();
            }
        } else {
            Debug.LogError("Too many retries. No Camera permissions available.");
        }
    }
    #endregion

    async Task SetupCamera(){
        var status = await CameraDevice.CheckPermissions();
        retries = 0;
        if (status != MediaDevice.PermissionStatus.Authorized) {
            TryFinalizeInit();
            return;
        }
        /*
        // Create a device query for device cameras
        query = new MediaDeviceQuery(MediaDeviceCriteria.CameraDevice);
        // Take first available device
        device = query.current as CameraDevice;
        // And replace it with the first front facing camera, if available
        foreach (CameraDevice camera in query)
        {
            if (frontcamera && camera.frontFacing)
            {
                device = camera;   
                break;
            }
            if (!frontcamera && !camera.frontFacing)
            {
                device = camera;   
                break;
            }
        }

        device.frameRate = 10;
        //device.orientation = ScreenOrientation.Portrait;
        device.previewResolution = (400, 400);
*/

        cameraManager.OnCameraImage += new System.Action<CameraImage>(OnCameraImage);
        if (!cameraInitialized)
        {
            LoadingManager.AddLoadedValue(1.0f, "InitCamera");
            cameraInitialized = true;
        }
    }

    async UniTask StartDevice(){
        
        Debug.Log("Starting Camera...");
        // Start camera preview
        await UniTask.Yield();
        await cameraManager.StartRunning();
        //device.StartRunning(OnCameraImage);
        await UniTask.Yield();
        var (minBias, maxBias) = cameraManager.device.exposureBiasRange;
        cameraManager.device.exposureBias = 0.2f * maxBias;
        //if (device.exposureLockSupported)
            //device.exposureLock = true;
        /*
        device.exposureBias = maxBias;
        if (device.exposureLockSupported)
            device.exposureLock = true;
        if (device.exposurePointSupported)
            device.exposurePoint = (0.5f, 0.5f);
        */
        //NatSuite.Devices.
        
        if (cameraManager.device.exposurePointSupported){
            Debug.LogWarning("exposurePointSupported");
            cameraManager.device.SetExposurePoint(0.5f, 0.5f);
        }
    }

    #region --UI Handlers--

    /*
    public async UniTask SwitchCamera () {
        Debug.Log("Switching Camera...");
        // Check that there is another camera to switch to
        if (query.count < 2)
            return;
        // Stop current camera
        var device = query.current as CameraDevice;
        await UniTask.Yield();
        device.StopRunning();
        await UniTask.Yield();
        // Advance to next available camera
        query.Advance();
        // Start new camera
        device = query.current as CameraDevice;
        await UniTask.Yield();
        device.StartRunning(OnCameraImage);
        await UniTask.Yield();
    }*/

    public async UniTask StopCamera(){
        Debug.Log("Stopping Camera...");
        await UniTask.Yield();
		if (!Application.isPlaying) return;
        if (cameraManager.device != null)
        {
            if (cameraManager.device.running)
            {
                await UniTask.Yield();
                cameraManager.StopRunning();
                Debug.Log("Camera Stopped");
                await UniTask.Yield();
            } else {
                Debug.Log("StopCamera: device isnt running");
            }
        } else if (cameraManager.device == null)
        {
            Debug.LogError("StopCamera: stopping failed. device == null");
        }
    }
    public async UniTask StartCamera(){
		if (!Application.isPlaying) return;
        
        if (cameraManager.device != null)
        {
            if (!cameraManager.device.running)
            {
                await StartDevice();
                Debug.Log("Camera Started");
            } else {
                Debug.Log("StartCamera: device already running");
            }
        } else if (cameraManager.device == null)
        {
            Debug.LogError("StartCamera: starting failed. device == null");
            Debug.LogError("Trying Camera setup again");
            await SetupCamera();

            if (cameraInitialized)
            {
                await StartDevice();
            } else {
                Debug.LogError("StartCamera: starting failed. Again, SetupCamera doesnt work!");
            }
        }
    }
    
    private void Update() {
        //if (renderOutputActive)
        //{
        //    rawImage.texture = renderOutput.texture;
        //}
        if (!cameraInitialized)
        {
            TryFinalizeInit();
        }
    }


    void OnCameraImage(CameraImage cameraImage) {
        // update visual output if required
        //if (renderOutputActive)
        //{
        //    renderOutput.Update(cameraImage);
        //}
        // process picture
        openCvLib.NewCameraImage(cameraImage);
    }
    


    public void FocusCamera (BaseEventData e) {
        /*
        // Check if focus is supported
        var device = query.current as CameraDevice;
        if (!device.focusPointSupported)
            return;
        // Get the touch position in viewport coordinates
        var eventData = e as PointerEventData;
        var transform = eventData.pointerPress.GetComponent<RectTransform>();
        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(
            transform,
            eventData.pressPosition,
            eventData.pressEventCamera,
            out var worldPoint
        ))
            return;
        var corners = new Vector3[4];
        transform.GetWorldCorners(corners);
        var point = worldPoint - corners[0];
        var size = new Vector2(corners[3].x, corners[1].y) - (Vector2)corners[0];
        // Focus camera at point
        device.SetFocusPoint(point.x / size.x, point.y / size.y);
        */
    }
    
    #endregion

    public bool IsRunning(){
        if (cameraManager.device == null)
        {
            return false;
        }
        return cameraManager.running;
    }
    
    #region --Operations--

    void OnDisable () {
        if (cameraManager != null)
        {
            if (cameraManager.device != null)
            {
                if (cameraManager.running)
                    cameraManager.StopRunning();
            }
        }
    }
    #endregion

}