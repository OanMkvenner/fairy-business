using UnityEngine;


public class SceneReferences : MonoBehaviour
{
    public static SceneReferences instance { get; private set; }
    public GameObject mainCanvas;
    static public GameObject mainCanvasStatic;

    private void Awake() {
        if (instance == null) { instance = this; }
    }
}