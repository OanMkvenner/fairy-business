using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu.Buttons
{
    [RequireComponent(typeof(Button))]
    public abstract class BaseButton : MonoBehaviour
    {
        private void Awake()
        {
            if (!TryGetComponent(out Button button))
                return;
            
            button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            if (!TryGetComponent(out Button button))
                return;
            
            button.onClick.RemoveAllListeners();
        }

        protected abstract void OnClick();
    }
}