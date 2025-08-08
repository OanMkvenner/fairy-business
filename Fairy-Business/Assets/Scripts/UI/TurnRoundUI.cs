using System.Collections.Generic;
using UnityEngine;

namespace UI
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
    }
}