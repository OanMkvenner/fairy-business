using System.Collections.Generic;
using System.Linq;
using ComponentsHYBR.Utilities;
using UI.Menu.BaseMenu;
using UnityEngine;

namespace UI.Menu
{
    public class WinScreenMenu : MenuElement
    {
        [SerializeField] private List<EndScreenInfo> redPlayerScores;
        [SerializeField] private List<EndScreenInfo> bluePlayerScores;

        public override void OpenMenu()
        {
            base.OpenMenu();
            Sounds.instance.Play("WinScreen");
            InitializeUI();
        }

        private void InitializeUI()
        {
            Dictionary<int, (int, int)> foo = GameSession.instance.VictoryPointCountsPerPhase;

            int i = 0;

            foreach (KeyValuePair<int, (int, int)> item in foo.Skip(1))
            {
                bluePlayerScores[i].Initialize(item.Value.Item1, item.Value.Item1 > item.Value.Item2);
                redPlayerScores[i].Initialize(item.Value.Item2, item.Value.Item1 < item.Value.Item2);
                i++;
            }
        }
    }
}