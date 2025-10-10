using System.Collections.Generic;
using Animation;
using UnityEngine;
using DG.Tweening;

public class SequenceTracker : MonoBehaviour
{
    [SerializeField] private List<ScanAnimation> allObjects;

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
        foreach (ScanAnimation scanAnimation in allObjects)
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
        foreach (ScanAnimation scanAnimation in allObjects)
        {
            scanAnimation.ResetUI();
        }
    }
}