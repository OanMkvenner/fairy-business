using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Gameplay
{
    public class TurnRoundUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI maxCount;
        [SerializeField] private List<GameObject> turns;

        private void Awake()
        {
            maxCount.text = GameSession.instance.MaxRoundCount.ToString();
        }

        public void FillCurrentTurn(int turnCounter)
        {
            turns[turnCounter].SetActive(true);

            if (turnCounter <= 0)
                return;
            
            turns[turnCounter - 1].SetActive(false);
        }

        public void UpdateRoundCount(int count)
        {
            if(count == GameSession.instance.MaxRoundCount)
                return;
            
            roundText.text = count.ToString(); 
        }
    }
}