using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Locations
{
    public class LocationManager : MonoBehaviour
    {
        [SerializeField] private List<FlipButton> flipButtonLocation;
        [SerializeField] private Button randomLocationButton;
        
        [Header("Script References")]
        [SerializeField] private GameSession gameSession;

        private void Awake()
        {
            randomLocationButton.onClick.AddListener(PickRandomLocations);
        }

        private void PickRandomLocations()
        {
            List<FlipButton> shuffledLocations = new List<FlipButton>(flipButtonLocation);
            
            for (int i = 0; i < shuffledLocations.Count; i++)
            {
                int randomIndex = Random.Range(i, shuffledLocations.Count);
                (shuffledLocations[i], shuffledLocations[randomIndex]) = (shuffledLocations[randomIndex], shuffledLocations[i]);
            }
            
            for (int i = 0; i < 3 && i < shuffledLocations.Count; i++)
            {
                gameSession.SetupSelectLocation(shuffledLocations[i]);
                
                //Todo: Marie 14.08 Animation abspielen.
                //shuffledLocations[i].SetSideWithAnim(FlipButton.ActiveSide.back);
            }
        }
    }
}