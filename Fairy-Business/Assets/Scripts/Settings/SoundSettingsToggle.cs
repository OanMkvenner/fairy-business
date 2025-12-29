using UnityEngine;
using UnityEngine.UI;

namespace Settings
{
    [RequireComponent(typeof(Toggle))]
    public class SoundSettingsToggle : MonoBehaviour
    {
        private Toggle toggle;

		private void Awake()
		{
			toggle = GetComponent<Toggle>();
		}

        private void OnEnable()
        {
            toggle.SetIsOnWithoutNotify(GameSettings.SoundSetting);

            GameSettings.OnSoundSettingChanged += OnSettingChanged;
            
            toggle.onValueChanged.AddListener(OnToggleChanged);
        }

        private void OnDisable()
        {
            GameSettings.OnSoundSettingChanged -= OnSettingChanged;
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }

        private void OnToggleChanged(bool value)
        {
            GameSettings.SoundSetting = value;
        }

        private void OnSettingChanged(bool value)
        {
            toggle.SetIsOnWithoutNotify(value);
        }
    }
}