using UnityEngine;
using UnityEngine.UI;

namespace UI.Blur
{
    [RequireComponent(typeof(Image))]
    public class BlurElement : MonoBehaviour
    {
        private void Start()
        {
            BlurUIController.instance.RegisterBlurImage(this.GetComponent<Image>());
        }
    }
}