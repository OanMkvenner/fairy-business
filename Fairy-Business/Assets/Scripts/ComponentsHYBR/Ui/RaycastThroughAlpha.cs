using UnityEngine;
using UnityEngine.UI;

public class RaycastThroughAlpha : MonoBehaviour
{
    public float minimumAlphaThreshold = 0.5f;
    void Start()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = minimumAlphaThreshold;
    }
}
