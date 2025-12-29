using ComponentsHYBR.Utilities;
using UI.Menu.BaseMenu;

namespace UI.Menu
{
    public class SettingsMenu : MenuElement
    {
        public void SetSoundVolume(bool soundOn)
        {
            Sounds.instance.SetVolume(soundOn ? 1 : 0);
        }
    }
}