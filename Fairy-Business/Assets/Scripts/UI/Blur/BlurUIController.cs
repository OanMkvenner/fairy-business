using System.Collections.Generic;
using UI.Menu;
using UI.Menu.BaseMenu;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Blur
{
    public class BlurUIController : MonobheaviourSingletonCustom<BlurUIController>
    {
        [SerializeField] private Material blurMaterial;
        private readonly List<Image> blurImages = new List<Image>();

        private void Awake()
        {
            MenuManager.OnMenuOpened += AddBlurMaterial;
            MenuElement.OnMenuClosed += RemoveBlurMaterial;
        }

        private void OnDestroy()
        {
            MenuManager.OnMenuOpened -= AddBlurMaterial;
            MenuElement.OnMenuClosed -= RemoveBlurMaterial;
        }

        public void RegisterBlurImage(Image image)
        {
            if(!blurImages.Contains(image))
                blurImages.Add(image);
        }
        
        private void AddBlurMaterial()
        {
            foreach (Image image in blurImages)
            {
                image.material = new Material(blurMaterial);
            }
        }

        private void RemoveBlurMaterial()
        {
            foreach (Image image in blurImages)
            {
                image.material = null;
            }
        }
    }
}