using TMPro;
using UnityEngine;

namespace UI.Gameplay
{
    public class ScoreHoverUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI blueScoreText;
        [SerializeField] private TextMeshProUGUI redScoreText;
        [SerializeField] private TextMeshProUGUI scorePhaseText;
        
        private const string scoreTextLocalization = "hoverMenu_ScoringPhase";

        public void Init(int blueScore, int redScore, int scorePhase)
        {
            blueScoreText.text = blueScore.ToString();
            redScoreText.text = redScore.ToString();
            scorePhaseText.text = scorePhase + " " + Localizer.instance.TranslateToSpecificLanguage(scoreTextLocalization, Localizer.instance.GetCurrentlySetLanguage());
        }

        public void Clear()
        {
            blueScoreText.text = "";
            redScoreText.text = "";
            scorePhaseText.text = "";
        }
    }
}