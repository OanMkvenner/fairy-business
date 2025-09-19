using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI.Gameplay
{
    public class TurnRoundUI : MonoBehaviour
    {
        [SerializeField] private List<TurnWrapper> turns;
        [SerializeField] private TextMeshProUGUI roundText;

        [SerializeField] private List<GameObject> pastRounds;
        [SerializeField] private List<GameObject> futureRounds;

        public void FillCurrentTurn(int turnCounter)
        {
            turns[turnCounter].CurrentTurnOn();

            if (turnCounter > 0)
            {
                turns[turnCounter - 1].PastTurnOn();
            }
        }

        public void SetRoundCount(int count)
        {
            roundText.text = count.ToString(); 
        }

        public void NewRound(int count)
        {
            foreach (TurnWrapper turn in turns)
            {
                turn.FutureTurnOn();
            }
            
            for (int i = 0; i < count; i++)
            {
                futureRounds[i].SetActive(false);
                pastRounds[i].SetActive(true);
            }
        }

        public void ResetRoundUI()
        {
            foreach (GameObject futureRound in futureRounds)
            {
                futureRound.SetActive(true);
            }

            foreach (GameObject pastRound in pastRounds)
            {
                pastRound.SetActive(false);
            }
        }
    }
}