using System.Collections.Generic;
using UnityEngine;

namespace UI.Gameplay
{
    public class TurnRoundUI : MonoBehaviour
    {
        [SerializeField] private GameObject finishedRound;
        [SerializeField] private List<GameObject> turns;

        public void FillTurn(int turnCounter)
        {
            turns[turnCounter].gameObject.SetActive(true);
        }

        public void FillFinishedRound()
        {
            finishedRound.SetActive(true);
        }

        public void ResetUI()
        {
            foreach (GameObject turn in turns)
            {
                turn.SetActive(false);
            }
            
            finishedRound.SetActive(false);
        }
    }
}