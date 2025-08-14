using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode.UiStateGraph;
using UnityEditor;
using DG.Tweening;

[ExecuteInEditMode]
public class UiManager : MonoBehaviour
{
    static public UiManager instance;
    public StateGraph mainUigraph;
    
    public Dictionary<string, WaitForCallbackNode> callbackHashmap = new();
    public List<StateGraph> cachedConnectedStateGraphs;
		// initialization function without parameters (needed elsewhere for callback reasons)
    bool errorCalledAlready = false;
    public void UpdateConnectedStateGraphList(){
        if (mainUigraph == null){
            if (!errorCalledAlready){
                Debug.LogError($"MainUiGraph not set in UiManger in scene. Please create a Create->Graph->UiStateGraph in your Assets/UiGraphs folder (or anywhere in the project) and assign it to the UiManger in your scene!");
                errorCalledAlready = true;
            }
            cachedConnectedStateGraphs.Clear();
            return;
        } else {
            errorCalledAlready = false;
        }
        cachedConnectedStateGraphs = mainUigraph.GetConnectedStateGraphList();
    }

    private void Awake() {
        instance = this;
    }

    void Start()
    {
        // initialize with the preferences set in DOTween's Utility Panel
        DOTween.Init();
        // (alternatively) initialize with custom settings, and set capacities immediately if needed
        //DOTween.Init(true, true, LogBehaviour.Verbose).SetCapacity(200, 10);

        //EditorApplication.playModeStateChanged += PlayModeChangedTrigger;

        // postpone initial start until one frame later, because "Start()" functions happen in random order
        if (mainUigraph == null) return;

		StartCoroutine(CustomCoroutines.OneFrameDelay(InitializeAllGraphs));
    }

    void InitializeAllGraphs(){
        UpdateConnectedStateGraphList();
        List<WaitForCallbackNode> allCallbackNodes = new();
        foreach (var graph in cachedConnectedStateGraphs)
        {
            graph.InitGraph();
            allCallbackNodes.AddRange(graph.GetAllNodesOfType<WaitForCallbackNode>());
        }
        foreach (var callbackNode in allCallbackNodes)
        {
            var portNames = callbackNode.GetAllCallbackPortNames();
            foreach (var portName in portNames)
            {
                if (portName == "passThrough") continue;
                if (callbackHashmap.ContainsKey(portName)){
                    Debug.LogError($"found multiple WaitForCallbackNode's with the same portname {portName}! This is not allowed. Always choose unique Portnames in CallbackNodes");
                } else {
                    callbackHashmap[portName] = callbackNode;
                }
            }
        }
        List<ViewNode> allViewNodes = new();
        foreach (var graph in cachedConnectedStateGraphs)
        {
            allViewNodes.AddRange(graph.GetAllNodesOfType<ViewNode>());
        }
        foreach (var viewNode in allViewNodes)
        {
            var viewNodeRef = viewNode.assignedCanvasRef.gameObject.GetComponent<ViewNodeReferencer>();
            if (viewNodeRef == null) viewNodeRef = viewNode.assignedCanvasRef.gameObject.AddComponent<ViewNodeReferencer>();
            viewNodeRef.AddTargetViewNode(viewNode);
        }
        if (Application.isPlaying){
		    StartCoroutine(CustomCoroutines.OneFrameDelay(DoAllOnStarts)); // delayed, because they should start after above initializations are finished on ALL graphs
        }
    }
    void DoAllOnStarts(){
        foreach (var graph in cachedConnectedStateGraphs)
        {
            graph.OnStart();
        }
    }

    static public void CallbackUiEvent(string callbackPort, bool onlyCallIfNodeIsActive = false, bool executeImmideately = false){
        if (instance.callbackHashmap.ContainsKey(callbackPort)){
            instance.callbackHashmap[callbackPort].callCustomUiEvent(callbackPort, onlyCallIfNodeIsActive, executeImmideately);
        } else {
            Debug.LogError($"CallCustomUiEvent: {callbackPort} not found in any UIGraph! Check naming (dont use spaces!) and check if the CallbackNode still exists!");
        }
    }
    static public void CallbackUiEventExecImmideately(string callbackPort, bool onlyCallIfNodeIsActive = false){
        CallbackUiEvent(callbackPort, onlyCallIfNodeIsActive, true);
    }

    void OnEnable(){
        #if (UNITY_EDITOR)
        EditorApplication.update += UpdateConnectedStateGraphList;
        EditorApplication.update += postProcessOperations;
        #endif // End #if (UNITY_EDITOR)
    }

    void OnDisable(){
        #if (UNITY_EDITOR)
        EditorApplication.update -= UpdateConnectedStateGraphList;
        EditorApplication.update -= postProcessOperations;
        #endif // End #if (UNITY_EDITOR)
    }

    private void postProcessOperations()
    {
        foreach (var graph in cachedConnectedStateGraphs)
        {
            if (graph != null) {
                graph.PostProcessDropOperations();
            }
        }

        handleQueuedRedraws();
    }
    private void handleQueuedRedraws(){
        #if (UNITY_EDITOR)
        if (XNodeEditor.NodeEditorWindow.current){
            StateGraph currentGraph = XNodeEditor.NodeEditorWindow.current.graph as StateGraph;
            if (currentGraph != null) {
                if (currentGraph.queuedRepaint){
                    currentGraph.queuedRepaint = false;
                        XNodeEditor.NodeEditorWindow.current.graphEditor.window.Repaint();
                }
            }
        }
        #endif // End #if (UNITY_EDITOR)
    }
    void Update()
    {
        if (mainUigraph == null) return;


        #if (UNITY_EDITOR)
        UpdateConnectedStateGraphList();
        postProcessOperations();
        #endif // End #if (UNITY_EDITOR)

        foreach (var graph in cachedConnectedStateGraphs)
        {
            graph.OnUpdate();
        }
    }
}
