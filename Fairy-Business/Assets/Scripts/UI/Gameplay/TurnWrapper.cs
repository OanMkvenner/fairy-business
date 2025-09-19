using UnityEngine;

namespace UI.Gameplay
{
    public class TurnWrapper : MonoBehaviour
    {
        [SerializeField] private GameObject pastTurn;
        [SerializeField] private GameObject futureTurn;
        [SerializeField] private GameObject currentTurn;

        public void FutureTurnOn()
        {
            futureTurn.SetActive(true);
            pastTurn.SetActive(false);
            currentTurn.SetActive(false);
        }
        
        public void PastTurnOn()
        {
            futureTurn.SetActive(false);
            pastTurn.SetActive(true);
            currentTurn.SetActive(false);
        }
        
        public void CurrentTurnOn()
        {
            futureTurn.SetActive(false);
            pastTurn.SetActive(false);
            currentTurn.SetActive(true);
        }
    }
}