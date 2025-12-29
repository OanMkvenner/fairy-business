using System;

namespace Settings
{
    public static class GameSettings
    {
        private static bool soundSetting = true;

        public static event Action<bool> OnSoundSettingChanged;

        public static bool SoundSetting
        {
            get => soundSetting;
            set
            {
                if (soundSetting == value)
                    return;

                soundSetting = value;
                OnSoundSettingChanged?.Invoke(soundSetting);
            }
        }
    }
}