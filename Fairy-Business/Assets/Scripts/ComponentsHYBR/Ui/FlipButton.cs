using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

/// <summary>
/// Verwaltet einen umklappbaren Button mit Vorder- und Rückseite.
/// Unterstützt Animationen und sofortige Zustandsänderungen.
/// </summary>
public class FlipButton : MonoBehaviour
{
    // Aufzählung für die aktiven Seiten des Buttons
    public enum ActiveSide
    {
        undefined = -1, // Undefinierter Zustand
        front = 0,      // Vorderseite ist aktiv
        back = 1        // Rückseite ist aktiv
    }

    // Referenzen zu UI-Elementen
    [Header("UI References")]
    [Tooltip("Inhalt der Vorderseite")]
    public GameObject FrontContent;
    [Tooltip("Inhalt der Rückseite")]
    public GameObject BackContent;
    [Tooltip("Bild der Vorderseite")]
    public Image FrontImage;
    [Tooltip("Bild der Rückseite")]
    public Image BackImage;
    [Tooltip("Text der Vorderseite")]
    public TMP_Text FrontText;
    [Tooltip("Text der Rückseite")]
    public TMP_Text BackText;

    // Einstellungen
    [Header("Settings")]
    [Tooltip("Standardmäßig aktive Seite")]
    public ActiveSide activeSide = ActiveSide.front;
    [Tooltip("Easing-Funktion für die Flip-Animation")]
    public Ease easingFunction;

    // Interne Variablen
    private ActiveSide visibleSide = ActiveSide.front; // Aktuell sichtbare Seite
    private Tweener currentTween; // Aktuelle Tween-Animation

    /// <summary>
    /// Initialisiert den Button beim Start
    /// </summary>
    private void Start()
    {
        // Verzögerte Initialisierung, um Übersetzungen zu ermöglichen
        Invoke(nameof(LateStart), 0f);

        // Click-Event registrieren
        //GetComponent<Button>().onClick.AddListener(ButtonClicked);
    }

    /// <summary>
    /// Verzögerte Initialisierung für korrekte Übersetzungen
    /// </summary>
    private void LateStart()
    {
        // Sofortige Seitenauswahl setzen
        SetSideInstant(activeSide);

        // Sichtbaren Inhalt basierend auf der Rotation aktualisieren
        UpdateVisibleContentByRotation(true);
    }

    /// <summary>
    /// Aktualisiert den sichtbaren Inhalt basierend auf der aktuellen Rotation
    /// </summary>
    /// <param name="initialUpdate">Ob es sich um das initiale Update handelt</param>
    private void UpdateVisibleContentByRotation(bool initialUpdate = false)
    {
        // Berechnet den nach vorne zeigenden Vektor
        Vector3 frontFacingVector = transform.rotation * Vector3.forward;

        // Wenn die Rückseite nach vorne zeigt
        if (frontFacingVector.z < 0 && (visibleSide == ActiveSide.front || initialUpdate))
        {
            visibleSide = ActiveSide.back;
            if (initialUpdate) activeSide = ActiveSide.back;

            FrontContent.SetActive(false);
            BackContent.SetActive(true);
        }
        // Wenn die Vorderseite nach vorne zeigt
        else if (frontFacingVector.z > 0 && (visibleSide == ActiveSide.back || initialUpdate))
        {
            visibleSide = ActiveSide.front;
            if (initialUpdate) activeSide = ActiveSide.front;

            FrontContent.SetActive(true);
            BackContent.SetActive(false);
        }
    }

    /// <summary>
    /// Setzt die Seite sofort ohne Animation
    /// </summary>
    /// <param name="side">Die gewünschte Seite</param>
    public void SetSideInstant(ActiveSide side)
    {
        // Aktive Seite aktualisieren
        activeSide = side;

        // Eventuelle laufende Animation beenden
        KillCurrentTween();

        // Rotation sofort setzen
        float targetRotation = activeSide == ActiveSide.front ? 0 : 180;
        transform.rotation = Quaternion.AngleAxis(targetRotation, Vector3.right);
    }

    /// <summary>
    /// Behandelt den Button-Klick
    /// </summary>
    public void ButtonClicked()
    {
        // Bestimme die Zielseite (umgekehrt zur aktuellen aktiven Seite)
        ActiveSide targetSide = (activeSide == ActiveSide.front) ? ActiveSide.back : ActiveSide.front;

        // Seite mit Animation wechseln
        SetSideWithAnim(targetSide);

        // LocationManager über den Klick informieren
    }

    /// <summary>
    /// Setzt die Seite mit Animation
    /// </summary>
    /// <param name="desiredSide">Die gewünschte Seite (optional)</param>
    public void SetSideWithAnim(ActiveSide desiredSide = ActiveSide.undefined)
    {
        /*// Wenn keine Seite angegeben, die andere Seite wählen
        if (desiredSide == ActiveSide.undefined)
        {
            activeSide = activeSide == ActiveSide.front ? ActiveSide.back : ActiveSide.front;
        }
        else
        {
            activeSide = desiredSide;
        }*/

        // Eventuelle laufende Animation beenden
        KillCurrentTween();

        // Zielrotation bestimmen
        float targetRotation = activeSide == ActiveSide.front ? 0 : 180;

        // Neue Animation starten
        currentTween = transform.DORotateQuaternion(
                Quaternion.AngleAxis(targetRotation, Vector3.right),
                1.0f)
            .SetRelative(false)
            .SetEase(easingFunction);
    }

    /// <summary>
    /// Beendet die aktuelle Tween-Animation falls vorhanden
    /// </summary>
    private void KillCurrentTween()
    {
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
        }
    }

    /// <summary>
    /// Wird jeden Frame aufgerufen, um den sichtbaren Inhalt zu aktualisieren
    /// </summary>
    private void Update()
    {
        //UpdateVisibleContentByRotation();
    }
}