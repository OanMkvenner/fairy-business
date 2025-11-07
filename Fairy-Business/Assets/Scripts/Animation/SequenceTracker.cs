using System.Collections.Generic;
using Animation.AnimationController;
using DG.Tweening;
using UnityEngine;

namespace Animation
{
    public class SequenceTracker : MonoBehaviour
    {
        [SerializeField] private List<ScanAnimation> scannedAnimations;
        [SerializeField] private NewTurnAnimationController newTurnAnimationController;

        private void Awake()
        {
            GameSession.OnTurnReset += ResetAllUI;
        }

        private void OnDestroy()
        {
            GameSession.OnTurnReset -= ResetAllUI;
        }
    
        private void ResetAllUI()
        {
            int sequencesToWait = 0;
            int sequencesCompleted = 0;

            // Prüfe alle Objekte, wie viele Animations-Sequences aktiv sind
            foreach (ScanAnimation scanAnimation in scannedAnimations)
            {
                if (scanAnimation.Sequence != null && scanAnimation.Sequence.IsActive())
                {
                    sequencesToWait++;

                    // Registriere OnComplete Callback
                    scanAnimation.Sequence.OnComplete(() =>
                    {
                        sequencesCompleted++;

                        // Wenn alle fertig sind, setze alle Sprites
                        if (sequencesCompleted >= sequencesToWait)
                        {
                            StartNewTurnAnimation();
                        }
                    });
                }
            }

            // Wenn keine Sequence aktiv war, direkt setzen
            if (sequencesToWait == 0)
            {
                StartNewTurnAnimation();
            }
        }

        private void StartNewTurnAnimation()
        {
            Sounds.instance.Play("NewTurn");
            
            foreach (ScanAnimation scanAnimation in scannedAnimations)
            {
                scanAnimation.ResetUI();
            }
            
            newTurnAnimationController.StartAnimations();
        }
    }
}