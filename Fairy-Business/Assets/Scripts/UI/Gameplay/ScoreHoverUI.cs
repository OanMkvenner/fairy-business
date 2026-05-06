using TMPro;
using UnityEngine;

namespace UI.Gameplay
{
    public class ScoreHoverUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI blueScoreText;
        [SerializeField] private TextMeshProUGUI redScoreText;
        [SerializeField] private TextMeshProUGUI scorePhaseText;

        public void Init(int blueScore, int redScore, int scorePhase)
        {
            blueScoreText.text = blueScore.ToString();
            redScoreText.text = redScore.ToString();
            scorePhaseText.text = scorePhase.ToString();
        }
    }
}