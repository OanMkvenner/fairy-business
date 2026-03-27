using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Gameplay
{
    public class TurnRoundUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI maxCount;
        [SerializeField] private List<Sprite> turnImages;
        [SerializeField] private List<Image> roundBubbles;

        private int roundCount;

        private void Awake()
        {
            maxCount.text = GameSession.instance.MaxRoundCount.ToString();
        }

        public void FillCurrentTurn(int turnCounter)
        {
            turnCounter--;
            
            roundBubbles[roundCount-1].sprite = turnImages[turnCounter];

            Debug.Log(roundCount - 1 + ". Runde, " + turnCounter + ". Zug");
        }

        public void UpdateRoundCount(int count)
        {
            if(count == GameSession.instance.MaxRoundCount)
                return;
            
            roundCount = count;
            
            roundText.text = count.ToString(); 
        }
    }
}