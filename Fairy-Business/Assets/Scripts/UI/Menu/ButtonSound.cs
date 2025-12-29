using ComponentsHYBR.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu
{
    [RequireComponent(typeof(Button))]
    public class ButtonSound : MonoBehaviour
    {
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(PlaySound);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveAllListeners();
        }

        private void PlaySound()
        {
            Sounds.instance.Play("BasicButton");
        }
    }
}