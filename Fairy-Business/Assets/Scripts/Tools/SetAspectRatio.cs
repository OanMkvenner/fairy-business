using UnityEngine;
using UnityEngine.UI;

namespace Tools
{
    [RequireComponent(typeof(AspectRatioFitter))]
    public class SetAspectRatio : MonoBehaviour
    {
        private AspectRatioFitter aspectRatioFitter;

        private void Awake()
        {
            aspectRatioFitter = GetComponent<AspectRatioFitter>();
        }

        private void Start()
        {
            if (DeviceClassifier.IsTablet())
            {
                Debug.Log("IsTablet");
                aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
            }
            else
            {
                Debug.Log("IsPhone");
                aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
            }
        }
    }
}