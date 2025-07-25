using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI; // needed to apply video feed to "Image" type object, wich is part of UI
using System;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Mathematics;
using System.Threading.Tasks;
using System.Timers;
using Cysharp.Threading.Tasks;


/*
// This is used 

*/
public struct ComparatorStorage
{
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
unsafe public struct ComparisonReturnStruct
{
    public char* resultName;
    public float posX;
    public float posY;
	public float cardRotation;
	public float cardScale;
    public float matchRating;
    public float* keypointDataPtr;
    public int keypointDataLength;
	public int subResultDepth; // 0: a main result, 1: sub result of previous main result, 2: subresult of previous 1st subresult, ...

    //public ComparisonReturnStruct(string first, string last)
    //{
    //    //this.First = first;
    //    //this.Last = last;
    //    this.posX = 0;
    //    this.posY = 0;
    //}
}
struct Keypoint
{
    public float x;
    public float y;
    public Color color;
    public Keypoint(float x, float y, Color color){
        this.x = x;
        this.y = y;
        this.color = color;
    }
}

#if (UNITY_EDITOR) && !UNITY_IOS && !UNITY_EDITOR_OSX
    // This code is used to DYNAMICALLY link to the DLL in the windows
    // Unity Editor - so i dont have to restart the editor on every DLL update.
    // For production use the below [DLLImport("OTLib")] way of doing it instead!
    [fts.PluginAttr("../../Packages/com.hybrgames.camerapackage/Plugins/OTLib")]
    public static class Comparator
    {
        [fts.PluginFunctionAttr("createImageComparator")] 
        public static createImageComparatorDelegate createImageComparator = null;
        public unsafe delegate ComparatorStorage* createImageComparatorDelegate();
        [fts.PluginFunctionAttr("deleteImageComparator")] 
        public static deleteImageComparatorDelegate deleteImageComparator = null;
        public unsafe delegate void deleteImageComparatorDelegate(ref ComparatorStorage* comparator_ptr);
        [fts.PluginFunctionAttr("testImageComparatorCreation")] 
        public static testImageComparatorCreationDelegate testImageComparatorCreation = null;
        public unsafe delegate float testImageComparatorCreationDelegate(ComparatorStorage* comparator_ptr);
        [fts.PluginFunctionAttr("findImage")] 
        public static findImageDelegate findImage = null;
        public unsafe delegate IntPtr findImageDelegate(ComparatorStorage* comparator_ptr, ref void* rawImage, int width, int height, bool isColoredPicture, bool debugImageOutput);
        [fts.PluginFunctionAttr("findGameBoard")] 
        public static findGameBoardDelegate findGameBoard = null;
        public unsafe delegate void findGameBoardDelegate(ComparatorStorage* comparator_ptr, ref void* rawImage, int width, int height, int cameraAngle, float heightMult, [In, Out] ref IntPtr returnStruct, ref int returnStructArraySize, bool debugImageOutput);
        [fts.PluginFunctionAttr("findImageWithReturnstructs")] 
        public static findImageWithReturnstructsDelegate findImageWithReturnstructs = null;
        public unsafe delegate void findImageWithReturnstructsDelegate(ComparatorStorage* comparator_ptr, ref void* rawImage, int width, int height, [In, Out] ref IntPtr returnStruct, ref int returnStructArraySize, bool isFlippedHorizontally, bool isColoredPicture, bool debugImageOutput);
        [fts.PluginFunctionAttr("initializeImages")] 
        public static initializeImagesDelegate initializeImages = null;
        public unsafe delegate int initializeImagesDelegate(ComparatorStorage* comparator_ptr, string file_path);
        [fts.PluginFunctionAttr("setCustomValues")] 
        public static setCustomValuesDelegate setCustomValues = null;
        public unsafe delegate void setCustomValuesDelegate(ComparatorStorage* comparator_ptr, string valueTypeUtfString, float value1, float value2);
        [fts.PluginFunctionAttr("callCommand")] 
        public static callCommandDelegate callCommand = null;
        public unsafe delegate long callCommandDelegate(ComparatorStorage* comparator_ptr, string commandTypeUtfString, string value);
        [fts.PluginFunctionAttr("initializeImagesDirectly")] 
        public static initializeImagesDirectlyDelegate initializeImagesDirectly = null;
        public unsafe delegate int initializeImagesDirectlyDelegate(ComparatorStorage* comparator_ptr, string file_path, string[] allFiles, int amountOfFiles);
    }
#else
    struct Comparator 
    {
        #if UNITY_IOS
            // On iOS plugins are statically linked into
            // the executable, so we have to use __Internal as the
            // library name.
            [DllImport("__Internal")]
            public static unsafe extern ComparatorStorage* createImageComparator();
            [DllImport("__Internal")]
            public static unsafe extern void deleteImageComparator(ref ComparatorStorage* comparator_ptr);
            [DllImport("__Internal")]
            public static unsafe extern float testImageComparatorCreation(ComparatorStorage* comparator_ptr);
            [DllImport("__Internal")]
            public static unsafe extern IntPtr findImage(ComparatorStorage* comparator_ptr, ref void* rawImage, int width, int height, bool isColoredPicture, bool debugImageOutput);
            [DllImport("__Internal")]
            public static unsafe extern void findGameBoard(ComparatorStorage* comparator_ptr, ref void* rawImage, int width, int height, int cameraAngle, float heightMult, [In, Out] ref IntPtr returnStruct, ref int returnStructArraySize, bool debugImageOutput);
            [DllImport("__Internal")]
            public static unsafe extern void findImageWithReturnstructs(ComparatorStorage* comparator_ptr, ref void* rawImage, int width, int height, [In, Out] ref IntPtr returnStruct, ref int returnStructArraySize, bool isFlippedHorizontally, bool isColoredPicture, bool debugImageOutput);
            [DllImport("__Internal")]
            public static unsafe extern int initializeImages(ComparatorStorage* comparator_ptr, string file_path);
            [DllImport("__Internal")]
            public static unsafe extern void setCustomValues(ComparatorStorage* comparator_ptr, string valueTypeUtfString, float value1, float value2);
            [DllImport("__Internal")]
            public static unsafe extern long callCommand(ComparatorStorage* comparator_ptr, string commandTypeUtfString, string value);
            [DllImport("__Internal")]
            public static unsafe extern int initializeImagesDirectly(ComparatorStorage* comparator_ptr, string file_path, string[] allFiles, int amountOfFiles);
        #else
            // Other platforms load plugins dynamically, so pass the
            // name of the plugin's dynamic library.
            //[DllImport ("PluginName")]   
            [DllImport("OTLib")]
            public static unsafe extern ComparatorStorage* createImageComparator();
            [DllImport("OTLib")]
            public static unsafe extern void deleteImageComparator(ref ComparatorStorage* comparator_ptr);
            [DllImport("OTLib")]
            public static unsafe extern float testImageComparatorCreation(ComparatorStorage* comparator_ptr);
            [DllImport("OTLib")]
            public static unsafe extern IntPtr findImage(ComparatorStorage* comparator_ptr, ref void* rawImage, int width, int height, bool isColoredPicture, bool debugImageOutput);
            [DllImport("OTLib")]
            public static unsafe extern void findGameBoard(ComparatorStorage* comparator_ptr, ref void* rawImage, int width, int height, int cameraAngle, float heightMult, [In, Out] ref IntPtr returnStruct, ref int returnStructArraySize, bool debugImageOutput);
            [DllImport("OTLib")]
            public static unsafe extern void findImageWithReturnstructs(ComparatorStorage* comparator_ptr, ref void* rawImage, int width, int height, [In, Out] ref IntPtr returnStruct, ref int returnStructArraySize, bool isFlippedHorizontally, bool isColoredPicture, bool debugImageOutput);
            [DllImport("OTLib")]
            public static unsafe extern int initializeImages(ComparatorStorage* comparator_ptr, string file_path);
            [DllImport("OTLib")]
            public static unsafe extern void setCustomValues(ComparatorStorage* comparator_ptr, string valueTypeUtfString, float value1, float value2);
            [DllImport("OTLib")]
            public static unsafe extern long callCommand(ComparatorStorage* comparator_ptr, string commandTypeUtfString, string value);
            [DllImport("OTLib")]
            public static unsafe extern int initializeImagesDirectly(ComparatorStorage* comparator_ptr, string file_path, string[] allFiles, int amountOfFiles);
        #endif
    } 
#endif

public class ScanResult
{
    public string name;
    public string subName;
    public float2 pos;
    public float rotation;
    public float scale;
    public float matchRating;
    public float2 camResolution; // cam res could change, keep resolution of the time the result was taken
    public int subResultDepth = 0; // 0: a main result, 1: sub result of previous main result, 2: subresult of previous 1st subresult, ...
    public List<ScanResult> subResults = new();
}

public class CameraOpencvLib : MonoBehaviour
{
    public static CameraOpencvLib instance { get; private set; }
    private static unsafe ComparatorStorage* comparator_obj;

    // //public Image CameraImage;
    // public GameObject camOutput1;
    // public GameObject camOutput2;
    // public Slider debugWidthSlider;
    // public Slider debugHeightSlider;
    // private Texture2D cameraTextureOriginal;
    // private Texture2D cameraTextureModified;
    // public TMPro.TMP_Text debugOutput1;
    // public TMPro.TMP_Text debugOutput2;
    
#if ENABLE_UNITY_COLLECTIONS_CHECKS
    private AtomicSafetyHandle m_Safety;
#endif
    List<Keypoint> keypoints = new List<Keypoint>();
    List<Keypoint> cardPositions = new List<Keypoint>();
    public bool scanningPaused = true;
    public bool isFlippedHorizontally = false;
    public bool mergeSubresultNames = true;

    VideoKit.Devices.Outputs.PixelBufferOutput imageBuffer;
    CameraHybr cameraHybr;

    private void Awake() {
        instance = this;
        scanningPaused = true;

#if UNITY_EDITOR && !UNITY_IOS && !UNITY_EDITOR_OSX
        var newGo = new GameObject();
        newGo.AddComponent<fts.NativePluginLoader>();
#endif
    }
    unsafe int loadFromPictures(string originFolder){
            // WARNING needs "read/write" as 'enabled' and "Compression" as 'NONE' on all pictures that are to be scanned
            int i = 0;
            var loadedObjects = Resources.LoadAll(originFolder, typeof(Texture2D)).Cast<Texture2D>(); // requires that you include the "using System.Linq;"
            string[] names = new string[loadedObjects.Count()];
            var dirPath = System.IO.Path.Combine(Application.persistentDataPath , originFolder);
            foreach(var loadedTexture in loadedObjects)
            {
                byte[] itemBGRytes = loadedTexture.EncodeToPNG();
                if (!System.IO.Directory.Exists(dirPath))
                {
                    System.IO.Directory.CreateDirectory(dirPath);
                }
                var finalPath = System.IO.Path.Combine(dirPath, loadedTexture.name + ".png");
                System.IO.File.WriteAllBytes(finalPath, itemBGRytes);

                names[i] = loadedTexture.name + ".png";
                i++;
            }
            return Comparator.initializeImagesDirectly(comparator_obj, dirPath, names, i);
    }

    unsafe int LoadKeypointsFromPictures(){
        return loadFromPictures("ScannerPictures");
    }

    unsafe int  LoadKeypointsFromFile(){
        // load from keypoints-file
        TextAsset keypoint_file = Resources.Load<TextAsset>("Keypoints");
        if (!keypoint_file){
            Debug.LogError("Keypoints.txt file not found. Please put one in 'Assets/Resources'");
        }
        string m_Path = System.IO.Path.Combine(Application.persistentDataPath, "Keypoints");
        System.IO.File.WriteAllText(m_Path, keypoint_file.text);
        int initialized_amount = Comparator.initializeImages(comparator_obj, m_Path);
        return initialized_amount;
    }

    async void Start() {
        cameraHybr = GetComponent<CameraHybr>();
        LoadingManager.AddExpectedLoadValue(3.0f, "initCameraLib");
        await UniTask.Yield();
        InitCameraLib();
    }

    public unsafe void InitCameraLib(){
        LoadingManager.AddLoadedValue(0.0f, "initCameraLib");
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        m_Safety = AtomicSafetyHandle.Create();
#endif

        imageBuffer = new VideoKit.Devices.Outputs.PixelBufferOutput();
        imageBuffer.orientation = ScreenOrientation.Portrait;

        // load keypoints directly from pictures
        LoadingManager.AddLoadedValue(1.0f, "initCameraLib");
#if (UNITY_EDITOR) && !UNITY_IOS && !UNITY_EDITOR_OSX
        if (Comparator.createImageComparator == null) { 
            Debug.LogError("Loading 'Comparator' library has failed. Maybe missing a dll or NativePluginLoader?");
            return;
        };
#endif
        comparator_obj = Comparator.createImageComparator();
        LoadingManager.AddLoadedValue(1.0f, "initCameraLib");

        //int filesLoaded = LoadKeypointsFromPictures();
        int filesLoaded = LoadKeypointsFromFile();
        Debug.LogWarning("loaded " + filesLoaded.ToString());
        LoadingManager.AddLoadedValue(1.0f, "initCameraLib");
    }

    Color convertToColor(float value)
    {
        value = (1.0f - value) * 3.0f;
        return new Color(2.0f * value, 2.0f * (1 - value), 0);
    }

    async void Update(){
        if (cameraStopOrdered || cameraStartingOrdered)
        {
            if (cameraStateChangeDelay > 0)
            {
                cameraStateChangeDelay -= Time.unscaledDeltaTime;
            }
        } else {
            cameraStateChangeDelay = 0;
        }
        await CheckCameraStartOrdered();
        await CheckCameraStopOrdered();
    }

    bool cameraStopOrdered = false;
    bool cameraStartingOrdered = false;
    float cameraStateChangeDelay = 0;
    public async UniTask CheckCameraStopOrdered(){
        // doing it this way to avoid several multithread racing conditions
        if (cameraStopOrdered && !processingCard && cameraStateChangeDelay <= 0) {
            cameraStopOrdered = false;
            cameraStateChangeDelay = 0;
            await cameraHybr.StopCamera();
        }
    }
    public async UniTask CheckCameraStartOrdered(){
        // doing it this way to avoid several multithread racing conditions
        if (cameraStartingOrdered && cameraStateChangeDelay <= 0) {
            cameraStartingOrdered = false;
            await cameraHybr.StartCamera();
        }
    }
    
    public void _StoptScanning()
    {
        CameraOpencvLib.StopScanning();
    }
    public void _StartScanning()
    {
        CameraOpencvLib.StartScanning();
    }
    static public void StartScanning()
    {
        // if we ordered a camera stop, no point in actually stopping now!
        instance.cameraStopOrdered = false;
        instance.cameraStateChangeDelay = 0.5f;
        instance.cameraStartingOrdered = true;
        instance.scanningPaused = false;
    }
    static public void StopScanning()
    {
        // cant stop Camera while last image ist sill being processed! (would release used buffer)
        instance.cameraStopOrdered = true;
        instance.cameraStateChangeDelay = 0.5f;
        instance.cameraStartingOrdered = false;
        instance.scanningPaused = true;
    }
    bool pausedScannerBecauseAppPaused = false;
    private void OnApplicationPause(bool pause) {
        if (pause) {
            // App is paused
            if (!scanningPaused)
            {
                // check if camera was currently running and pause it if so (remember that fact)
                Debug.LogWarning("Pausing Camera because of App minimizing");
                pausedScannerBecauseAppPaused = true;
                StopScanning();
            }
        } else {
            // App resumed
            if (pausedScannerBecauseAppPaused)
            {
                pausedScannerBecauseAppPaused = false;
                // if we stopped camera because of app pausing, resume it now
                Debug.LogWarning("Resuming Camera after App minimizing");
                StartScanning();
            }
        }
    }

    // if at lest one Allowance is set, all non-allowed are not being used anymore! add multiple allowances to have multile groups active
    unsafe static public long SetFilterAddAllowance(string groupName){
        return Comparator.callCommand(comparator_obj, "addCardGroupAllowance", groupName);
    }
    // if a disallowance is set, this group is excluded. Add multiple to exclude multiple groups
    unsafe static public long SetFilterAddDisallowed(string groupName){

        return Comparator.callCommand(comparator_obj, "addCardGroupDisallowance", groupName);
    }
    // resets all disallowances and allowances and activates all groups
    unsafe static public long ResetFilter(){

        return Comparator.callCommand(comparator_obj, "resetCardFilters", "");
    }

    private static ScanResult receivedScanResult = null;
    public static ScanResult GetNewScanResult(){
        var result = receivedScanResult;
        receivedScanResult = null;
        return result;
    }
    unsafe void OnResultFound(ScanResult result){
        receivedScanResult = result;
    }

    int currentProcessingWidth = 0;
    int currentProcessingHeight = 0;
    bool frontCameraValueBuffer = false; // used to provide a thread-safe buffered value for the scanning thread. DONT USE ANYHWHERE ELSE!
    System.Threading.Tasks.Task currentFindCardTask = null;

    private void OnApplicationQuit() {
        // wait for task to finishe before closing down
        currentFindCardTask?.Wait();
        imageBuffer.Dispose();
    }

    void AsyncFindCard(){

            //var texture = GetComponent<CameraHybr>().tgtTexture;
            //if (texture.width != currentProcessingWidth || texture.height != currentProcessingHeight)
            //{
            //    texture.Resize(currentProcessingWidth, currentProcessingHeight);
            //    GetComponent<CameraHybr>().rawImage.texture = texture;
            //}
            //for (int j = 0; j < currentProcessingHeight; j++)
            //{
            //    for (int i = 0; i < currentProcessingWidth; i++)
            //    {
            //        Color32 color = new Color32();
            //        color.r = imageBuffer.pixelBuffer[4 * (j * currentProcessingWidth + i) + 0];
            //        color.g = imageBuffer.pixelBuffer[4 * (j * currentProcessingWidth + i) + 1];
            //        color.b = imageBuffer.pixelBuffer[4 * (j * currentProcessingWidth + i) + 2];
            //        
            //        color.a = 255;
            //        Color converted = color;
            //        GetComponent<CameraHybr>().tgtTexture.SetPixel(i, j, converted);
            //    }
            //}
            //GetComponent<CameraHybr>().tgtTexture.Apply();

            
        // code for converting a static texture into nativeArray<byte>
        //NativeArray<byte> resultArray = new NativeArray<byte>(pixels.Count() * 4, Allocator.Persistent);
        //
        //for (int j = 0; j < currentProcessingHeight; j++)
        //{
        //    for (int i = 0; i < currentProcessingWidth; i++)
        //    {
        //        resultArray[4 * (j * currentProcessingWidth + i) + 0] = pixels[(j * currentProcessingWidth + i)].r;
        //        resultArray[4 * (j * currentProcessingWidth + i) + 1] = pixels[(j * currentProcessingWidth + i)].g;
        //        resultArray[4 * (j * currentProcessingWidth + i) + 2] = pixels[(j * currentProcessingWidth + i)].b;
        //        resultArray[4 * (j * currentProcessingWidth + i) + 3] = pixels[(j * currentProcessingWidth + i)].a;
        //    }
        //}
        //    Debug.LogError("3");
        //FindCardWithExtraData(resultArray, currentProcessingWidth, currentProcessingHeight);
        //Debug.LogError("itsa worka");
        //resultArray.Dispose();
        

        FindCardWithExtraData(imageBuffer.pixelBuffer, currentProcessingWidth, currentProcessingHeight, frontCameraValueBuffer);
        processingCard = false;
    }

    bool processingCard = false;
    public unsafe void NewCameraImage(VideoKit.Devices.CameraImage cameraImage){
        if (scanningPaused){
            return;
        }

        if (!processingCard)
        {
            processingCard = true;
            imageBuffer.Update(cameraImage);
            currentProcessingWidth = imageBuffer.width;
            currentProcessingHeight = imageBuffer.height;
            frontCameraValueBuffer = cameraHybr.frontcamera;
#if UNITY_STANDALONE || UNITY_EDITOR 
            frontCameraValueBuffer = false; // unity probably always uses a camera that behaves like a back-Cam!
#endif
            currentFindCardTask = Task.Run(AsyncFindCard);
        }

        //imageBuffer.Update(cameraImage);
        //buffer.Dispose();
        //CameraHybr frontNatCam = GetComponent<CameraHybr>();
//
        //if (frontNatCam.previewTexture){
        //    if (frontNatCam.IsRunning() && frontNatCam.previewTexture != null && stopToDebug == -1)
        //    {
        //        var rawImage = frontNatCam.previewTexture.GetPixels32();
        //        //FindCardAsString(rawImage);
        //    }
        //}
    }

    unsafe void FindCardAsString(NativeArray<byte> byteArray, int width, int height){
        CameraHybr frontNatCam = GetComponent<CameraHybr>();
        void* rawImagePtr = NativeArrayUnsafeUtility.GetUnsafePtr(byteArray);
        IntPtr returnValue = Comparator.findImage(comparator_obj, ref rawImagePtr, width, height, true, false);
        string valueString = Marshal.PtrToStringAnsi((IntPtr)returnValue);

        if (valueString != "")
        {
            ScanResult result = new ScanResult();
            // use valueString
            result.name = valueString;
            OnResultFound(result);
        }
    }

    unsafe ScanResult GenerateScanResultFromReturnStruct(ComparisonReturnStruct mainResult, int width, int height, bool frontCamera){
        ScanResult ret = new ScanResult();
        ret.name = Marshal.PtrToStringAnsi((IntPtr)mainResult.resultName);
        ret.pos = new float2(mainResult.posX, mainResult.posY);
        ret.rotation = mainResult.cardRotation;
        ret.scale = mainResult.cardScale;
        ret.matchRating = mainResult.matchRating;
        ret.camResolution = new float2(width, height);
        ret.subResultDepth = mainResult.subResultDepth;

        // postprocess Scales. Multiplying with diameter of camera (divide diameter of placeable object [has to be same xy resolution as original] with this new scale value to get proper size in camwindow!)
        ret.scale *= Mathf.Sqrt(width * width + height * height);

        // postprocess position to be relative to center instead of realtive to top left, and flip y-axis
        ret.pos = ret.pos - 0.5f;
        ret.pos.y *= -1;

        if (!frontCamera)
        {
            ret.rotation = -ret.rotation;
        }
        return ret;
    }
    unsafe List<ScanResult> GatherResultsFromIndexList(List<int> indexList, int width, int height, bool frontCamera, Unity.Collections.NativeArray<ComparisonReturnStruct> returnStructs){
        List<ScanResult> scanResults = new();
        if (indexList.Count <= 0) {
            Debug.LogError("Empty indexList as scan result");
            return scanResults;
        }

        ScanResult mainResult = new();
        foreach (int idx in indexList)
        {
            ComparisonReturnStruct result = returnStructs[idx];
            ScanResult ret = GenerateScanResultFromReturnStruct(result, width, height, frontCamera);
            if (ret.subResultDepth == 0){
                scanResults.Add(ret);
                mainResult = ret;
            } else {
                mainResult.subResults.Add(ret);
            }

            // add all found (and returned) keypoints and positions (for debugging)
            for (int i = 0; i < result.keypointDataLength; i += 3)
            {
                Color color = convertToColor(result.keypointDataPtr[i+2]);
                keypoints.Add(new Keypoint(x: result.keypointDataPtr[i], y: result.keypointDataPtr[i+1], color : color));
                //Debug.LogWarning($"keypoint {i}:  {result.keypointDataPtr[i + 2]}");
                //Debug.LogWarning($"keypoint {i}: {pe.keypointDataPtr[i]}, {pe.keypointDataPtr[i + 1]}");
                //Vector3 pos = new Vector3(pe.keypointDataPtr[i], pe.keypointDataPtr[i+1], 0);
                //Debug.DrawLine(pos - new Vector3(1, 1, 0), pos + new Vector3(1, 1, 0), Color.red);
            }
            cardPositions.Add(new Keypoint(x: result.posX, y: result.posY, color : Color.red));
        }

        return scanResults;
    }

    // common values: 0.1 - 1.5; set to -1 if no searchMask is desired!
    public static unsafe void SetSearchMaskRadiusPercent(float searchMaskRadiusPercent)
    {
        Comparator.setCustomValues(comparator_obj, "setSearchMaskRadiusPercent", searchMaskRadiusPercent, 0);
    }
    // default: 400. higher: scan further away. lower: scan closer.
    public static unsafe void SetScanResolution(float scanResolution)
    {
        if (scanResolution < 100)
        {
            Debug.LogError("scanResolution < 100 is not allowed. If this is actually desired i'd have to check if its safe to go this low first!");
        } else if (scanResolution > 2000){
            Debug.LogError("warning: doing scanResolution above 1200 can result in very high calculation times. Resetting to 1200 for now!");
        }
        scanResolution = Mathf.Clamp(scanResolution, 100, 1200);
        Comparator.setCustomValues(comparator_obj, "setScanResolution", scanResolution, 0);
    }
    unsafe void FindCardWithExtraData(NativeArray<byte> byteArray, int width, int height, bool frontCamera)
    {
        //cameraTextureOriginal.SetPixels32(rawImage);
        //cameraTextureOriginal.Apply();

        //float heightMult = debugHeightSlider.value;
        //float widthMult  = debugWidthSlider.value;

        IntPtr ptr = (IntPtr)0;

        //double timeStart = Time.realtimeSinceStartup;
        int outputLength = 0;
        isFlippedHorizontally = !frontCamera;
        void* rawImagePtr = NativeArrayUnsafeUtility.GetUnsafePtr(byteArray);
        Comparator.findImageWithReturnstructs(comparator_obj, ref rawImagePtr, width, height, ref ptr, ref outputLength, isFlippedHorizontally, isColoredPicture: true, debugImageOutput: false);
        //double timeTaken = Time.realtimeSinceStartup - timeStart;
        
        Unity.Collections.NativeArray<ComparisonReturnStruct> returnStructs = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<ComparisonReturnStruct>((void*)ptr, outputLength, Unity.Collections.Allocator.None);

        
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle safety = m_Safety;
    
        AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(safety);
        AtomicSafetyHandle.UseSecondaryVersion(ref safety);
        AtomicSafetyHandle.SetAllowSecondaryVersionWriting(safety, false);
    
        NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref returnStructs, safety);
#endif

        //Debug.LogWarning($"Persons array after call:{outputLength}");

        keypoints.Clear();
        cardPositions.Clear();
        int depth = -1;
        List<int> depthIndexes = new List<int>();
        List<List<int>> resultIndices = new List<List<int>>();
        for (int peIdx = 0; peIdx < returnStructs.Count(); peIdx++)
        {
            ComparisonReturnStruct pe  = returnStructs[peIdx];
            
            // handle subResult depth
            if (pe.subResultDepth > depth)
            {
                depth = pe.subResultDepth;
                depthIndexes.Add(peIdx);
            } else {
                // same or lower depth, this means the previous was a final result!
                resultIndices.Add(depthIndexes);

                // handle depth change
                if (pe.subResultDepth < depth)
                {
                    int difference = depth - pe.subResultDepth;
                    depth = pe.subResultDepth;
                    depthIndexes.RemoveRange(depthIndexes.Count - difference, difference);

                }
                depthIndexes[depthIndexes.Count - 1] = peIdx;
            }
        }
        // if we have at least one result, add the last depthIndexes list as result as well.
        if (returnStructs.Count() > 0)
        {
            resultIndices.Add(depthIndexes);
        }

        List<ScanResult> scanResults = new List<ScanResult>();
        foreach (var idxList in resultIndices)
        {
            var newScanResults = GatherResultsFromIndexList(idxList, width, height, frontCamera, returnStructs);
            if (mergeSubresultNames && newScanResults.Count > 0){
                var mainScan = newScanResults[0];
                string mergedName = mainScan.name;
                foreach (var item in mainScan.subResults)
                {
                    mergedName += item.name;
                }
                mainScan.name = mergedName;
            }
            scanResults.AddRange(newScanResults);
        }

        foreach (var result in scanResults)
        {
            OnResultFound(result);
        }
    }
    void LateUpdate()
    {
        
    }
}