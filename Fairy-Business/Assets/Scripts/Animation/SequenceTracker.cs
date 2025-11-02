using System.Collections.Generic;
using Animation.OldSystem;
using DG.Tweening;
using UnityEngine;

namespace Animation
{
    public class SequenceTracker : MonoBehaviour
    {
        [SerializeField] private List<ScanAnimation> scannedAnimations;
        [SerializeField] private List<NewTurnAnimation> newTurnAnimations;
        [SerializeField] private BaseAnimationActivator baseAnimationAktivator;

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
                            SetAllInactive();
                        }
                    });
                }
            }

            // Wenn keine Sequence aktiv war, direkt setzen
            if (sequencesToWait == 0)
            {
                SetAllInactive();
            }
        }

        private void SetAllInactive()
        {
            Sounds.instance.Play("NewTurn");
            
            foreach (ScanAnimation scanAnimation in scannedAnimations)
            {
                scanAnimation.ResetUI();
            }
            
            baseAnimationAktivator.ActivateAnimations();
        }
    }
}