using UnityEngine;

// put this on the main Canvas in the scene. Its required for some of TweenAnimator's animations. If you have several mainCanvas'es, you might need to change a few things in code...
public class MainCanvasReferencer : MonoBehaviour
{
    public static Transform mainCanvas = null;
    private void Awake() {
        mainCanvas = this.transform;
    }
}
